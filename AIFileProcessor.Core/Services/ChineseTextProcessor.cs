//-----------------------------------------------------------------------
// <copyright file="ChineseTextProcessor.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes Chinese text blocks by chunking and executing them in parallel.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="aiServices">The AI services to execute prompts against.</param>
    public class ChineseTextProcessor(ILogger<ChineseTextProcessor> logger, IEnumerable<IAiService> aiServices) : IChineseTextProcessor
    {
        // Define the separators as a static readonly field
        private static readonly string[] _lineSeparators = ["\r\n", "\r", "\n"];

        private static readonly JsonSerializerOptions _defaultSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            MaxDepth = ProcessingLimits.MaxJsonDepth,
        };

        /// <summary>
        /// Splits Chinese text into chunks of the given size, processes them in parallel,
        /// tracks their original index to maintain order, and merges them.
        /// </summary>
        /// <param name="modelId">The ID of the AI model to use for processing.</param>
        /// <param name="prompt">The prompt to execute for each chunk of text.</param>
        /// <param name="data">The block of Chinese text to process.</param>
        /// <param name="chunking">The chunking configuration (lines per chunk and max concurrency).</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>A merged AiApiResponse containing the combined results from all chunks.</returns>
        public async Task<AiApiResponse> ProcessChineseTextAsync(string modelId, string prompt, string data, ChunkingOptions chunking, CancellationToken ct = default)
        {
            return await ProcessInternalAsync(modelId, prompt, data, providerSettings: null, chunking, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<AiApiResponse> ProcessChineseTextAsync(
            string modelId,
            string prompt,
            string data,
            IDictionary<string, IAiServiceSettings> providerSettings,
            ChunkingOptions chunking,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(providerSettings);
            return await ProcessInternalAsync(modelId, prompt, data, providerSettings, chunking, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Internal implementation that handles chunking, parallel execution,
        /// and merging. When <paramref name="providerSettings"/> is non-null,
        /// the per-call settings overload of <see cref="IAiService.ProcessTextAsync(string, string, string, IAiServiceSettings, CancellationToken)"/>
        /// is used; otherwise the default (constructor-injected) overload is used.
        /// </summary>
        private async Task<AiApiResponse> ProcessInternalAsync(
            string modelId,
            string prompt,
            string data,
            IDictionary<string, IAiServiceSettings>? providerSettings,
            ChunkingOptions chunking,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return new AiApiResponse();
            }

            // The 'logger' and 'aiServices' parameters from the primary constructor
            // are in scope and can be used directly here.
            logger.LogInformation("Processing text with model: {ModelId}", modelId);

            // Grab the IAiService that supports the specified modelId
            IAiService? aiService = null;
            string? providerKey = null;
            foreach (var service in aiServices)
            {
                if (service.SupportsModel(modelId))
                {
                    aiService = service;
                    providerKey = AiModelCatalog.GetProviderKey(service);
                    break;
                }
            }

            if (aiService == null)
            {
                throw new NotSupportedException($"No registered IAiService supports the model '{modelId}'.");
            }

            // Resolve per-call settings for this provider, if available
            IAiServiceSettings? perCallSettings = null;
            if (providerSettings != null && providerKey != null && providerSettings.TryGetValue(providerKey, out var settingsObj))
            {
                perCallSettings = settingsObj;
            }

            // 1. Split text into individual lines
            string[] lines = data.Split(_lineSeparators, StringSplitOptions.None);

            // 2. Break down into chunks of chunkSize lines and associate each with its original index
            var chunks = new List<(int Index, string Text)>();

            for (int i = 0; i < lines.Length; i += chunking.ChunkSize)
            {
                int end = Math.Min(i + chunking.ChunkSize, lines.Length);
                string chunkText = string.Join(Environment.NewLine, lines[i..end]);
                chunks.Add((Index: i / chunking.ChunkSize, Text: chunkText));
            }

            // 3. Configure JsonSerializerOptions (Mapping "SP" to "Scratchpad" and "Sentences" to "ProcessedSentenceRows")
            var serializerOptions = _defaultSerializerOptions;

            // Use a semaphore to limit concurrency, preventing rate-limit violations
            // and connection pool exhaustion when processing many chunks.
            using var semaphore = new SemaphoreSlim(chunking.MaxConcurrentChunks, chunking.MaxConcurrentChunks);

            // Capture run start for aggregate metrics
            var runStart = DateTime.UtcNow;

            // Prepare a list to capture per-chunk metrics as they complete
            var chunkMetrics = new System.Collections.Concurrent.ConcurrentBag<Telemetry.ChunkMetrics>();

            // 4. Start all tasks concurrently (throttled by the semaphore), capturing the index of each chunk.
            var tasks = chunks.Select(async chunk =>
            {
                var queueStart = DateTime.UtcNow;
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                var queueDelayMs = (long)(DateTime.UtcNow - queueStart).TotalMilliseconds;
                try
                {
                    // Execute prompt asynchronously with chunk-level retry — use per-call settings if available.
                    // The chunk-level retry wraps the service-level retry (already inside each AiServiceBase).
                    // Chunk retry handles failures that exhaust the service-level retry, giving the chunk
                    // a fresh service-level attempt cycle for each chunk-level retry.
                    int chunkRetryAttempts = 0;
                    AiCallResult aiResult = await TransientRetryPolicy.ExecuteAsync(
                        async cancellationToken =>
                        {
                            return perCallSettings != null
                                ? await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, perCallSettings, cancellationToken).ConfigureAwait(false)
                                : await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, cancellationToken).ConfigureAwait(false);
                        },
                        ex => ex is not OperationCanceledException,
                        ProcessingLimits.ChunkMaxRetries,
                        ProcessingLimits.ChunkBaseDelaySeconds,
                        ProcessingLimits.ChunkMaxDelaySeconds,
                        logger,
                        $"chunk {chunk.Index} (model {modelId})",
                        ct,
                        onAttempt: attempt => chunkRetryAttempts = attempt - 1).ConfigureAwait(false);

                    // Parse directly into a AiApiResponse using the specified constructor
                    var response = new AiApiResponse(aiResult.Text, serializerOptions);

                    // Compute this chunk's line range
                    int startLine = (chunk.Index * chunking.ChunkSize) + 1;
                    int endLine = Math.Min(lines.Length, (chunk.Index + 1) * chunking.ChunkSize);

                    // Record per-chunk metrics (telemetry object may be null)
                    var cm = new Telemetry.ChunkMetrics
                    {
                        ChunkIndex = chunk.Index,
                        StartLine = startLine,
                        EndLine = endLine,
                        QueueDelayMs = queueDelayMs,
                        RetryAttempts = chunkRetryAttempts,
                        Telemetry = aiResult.Telemetry ?? new Telemetry.CallTelemetry { StartUtc = DateTime.UtcNow, EndUtc = DateTime.UtcNow, Attempts = 1 }
                    };

                    chunkMetrics.Add(cm);

                    return (Index: chunk.Index, Response: response, Exception: (Exception?)null);
                }
                catch (OperationCanceledException)
                {
                    // Propagate cancellation so the entire process is aborted and the caller/UI is notified.
                    throw;
                }
                catch (Exception ex)
                {
                    // Log the failure for this specific chunk but return an empty response
                    // so that other chunks' results are preserved.
                    logger.LogError(ex, "Chunk {ChunkIndex} failed during processing.", chunk.Index);
                    return (Index: chunk.Index, Response: new AiApiResponse(), Exception: ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            // 5. Await all tasks and sort them by original index to ensure correct order
            var results = await Task.WhenAll(tasks);

            // Check if any chunks failed. Since partial failure is a silent data-loss risk,
            // we fail fast with a descriptive error specifying the exact lines of the first failed chunk.
            var failedResults = results.Where(r => r.Exception != null).ToList();
            if (failedResults.Any())
            {
                var firstFailure = failedResults[0];
                int startLine = (firstFailure.Index * chunking.ChunkSize) + 1;
                int endLine = Math.Min(lines.Length, (firstFailure.Index + 1) * chunking.ChunkSize);

                throw new InvalidOperationException(
                    $"Processing failed for lines {startLine}-{endLine} of the file. Details: {firstFailure.Exception!.Message}",
                    firstFailure.Exception);
            }

            var orderedResponses = results
                .OrderBy(r => r.Index)
                .Select(r => r.Response)
                .ToList();

            // 6. Merge all processed results into a single AiApiResponse
            var mergedResponse = new AiApiResponse
            {
                // Concatenate Scratchpad (SP) with newlines
                Scratchpad = string.Join(
                    Environment.NewLine,
                    orderedResponses.Select(r => r.Scratchpad).Where(s => !string.IsNullOrEmpty(s))),

                // Combine all corrections into a flat list
                Corrections = [.. orderedResponses.SelectMany(r => r.Corrections ?? [])],

                // Combine all sentence rows in order
                ProcessedSentenceRows = [.. orderedResponses.SelectMany(r => r.ProcessedSentenceRows ?? [])],
            };

            // Aggregate run-level processing metrics from per-chunk metrics and attach to the merged response.
            var runEnd = DateTime.UtcNow;
            var elapsedMs = (long)(runEnd - runStart).TotalMilliseconds;

            var orderedChunks = chunkMetrics.OrderBy(c => c.ChunkIndex).ToList();

            int chunkCount = orderedChunks.Count;
            int succeeded = orderedChunks.Count; // failures are thrown earlier
            int failed = 0;

            int? totalPrompt = orderedChunks.Any(c => c.Telemetry?.PromptTokens != null)
                ? orderedChunks.Sum(c => c.Telemetry?.PromptTokens ?? 0)
                : (int?)null;

            int? totalCompletion = orderedChunks.Any(c => c.Telemetry?.CompletionTokens != null)
                ? orderedChunks.Sum(c => c.Telemetry?.CompletionTokens ?? 0)
                : (int?)null;

            double effectiveParallelism = elapsedMs > 0
                ? Math.Round(orderedChunks.Sum(c => c.Telemetry?.ElapsedMs ?? 0) / (double)elapsedMs, 3)
                : 0.0;

            double linesPerSecond = elapsedMs > 0
                ? Math.Round(lines.Length / (elapsedMs / 1000.0), 3)
                : 0.0;

            var processingMetrics = new Telemetry.ProcessingMetrics
            {
                StartUtc = runStart,
                EndUtc = runEnd,
                ElapsedMs = elapsedMs,
                ChunkCount = chunkCount,
                Succeeded = succeeded,
                Failed = failed,
                TotalPromptTokens = totalPrompt,
                TotalCompletionTokens = totalCompletion,
                EffectiveParallelism = effectiveParallelism,
                LinesPerSecond = linesPerSecond,
                Chunks = orderedChunks,
            };

            mergedResponse.Metrics = processingMetrics;

            return mergedResponse;
        }
    }
}