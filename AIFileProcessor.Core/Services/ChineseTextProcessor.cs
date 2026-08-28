//-----------------------------------------------------------------------
// <copyright file="ChineseTextProcessor.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Exceptions;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes Chinese text blocks by chunking and executing them in parallel.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="aiServices">The AI services to execute prompts against.</param>
    /// <param name="processingLimits">The hot-reloadable processing limits.</param>
    public class ChineseTextProcessor(
        ILogger<ChineseTextProcessor> logger,
        IEnumerable<IAiService> aiServices,
        ProcessingLimits processingLimits) : IChineseTextProcessor
    {
        // Define the separators as a static readonly field
        private static readonly string[] _lineSeparators = ["\r\n", "\r", "\n"];

        private readonly JsonSerializerOptions _defaultSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            MaxDepth = processingLimits.MaxJsonDepth,
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
        /// Builds the aggregate run-level metrics from the per-chunk metrics
        /// collected during the run. Used by both the success path (attaches to
        /// the merged response) and the failure path (attaches to the
        /// <see cref="Exceptions.ChunkProcessingException"/>.
        /// </summary>
        /// <param name="chunkMetrics">Per-chunk metrics captured as chunks completed (any order).</param>
        /// <param name="runStartUtc">The wall-clock UTC timestamp when the run started.</param>
        /// <param name="elapsedMs">The monotonic wall-clock duration of the run in milliseconds.</param>
        /// <param name="lineCount">The total number of source lines in the run.</param>
        /// <returns>Aggregate metrics with chunks ordered by index; token totals are null when no chunk reported usage.</returns>
        private static Telemetry.ProcessingMetrics BuildRunMetrics(
            System.Collections.Concurrent.ConcurrentBag<Telemetry.ChunkMetrics> chunkMetrics,
            DateTime runStartUtc,
            long elapsedMs,
            int lineCount)
        {
            var runEnd = runStartUtc.AddMilliseconds(elapsedMs);

            var orderedChunks = chunkMetrics.OrderBy(c => c.ChunkIndex).ToList();

            // The stopwatch-derived run end truncates sub-millisecond precision, which
            // can place it up to 1 ms before a chunk's real wall-clock completion
            // sample. Clamp to the latest chunk EndUtc so the run window always
            // contains every chunk's telemetry window (Gantt/JSON consumers rely on it).
            if (orderedChunks.Count > 0)
            {
                var latestChunkEnd = orderedChunks.Max(c => c.Telemetry?.EndUtc ?? DateTime.MinValue);
                if (latestChunkEnd > runEnd)
                {
                    runEnd = latestChunkEnd;
                }
            }

            int succeeded = orderedChunks.Count(c => !string.Equals(c.Telemetry?.Status, "Failed", StringComparison.OrdinalIgnoreCase));
            int failed = orderedChunks.Count - succeeded;

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
                ? Math.Round(lineCount / (elapsedMs / 1000.0), 3)
                : 0.0;

            return new Telemetry.ProcessingMetrics
            {
                StartUtc = runStartUtc,
                EndUtc = runEnd,
                ElapsedMs = elapsedMs,
                ChunkCount = orderedChunks.Count,
                Succeeded = succeeded,
                Failed = failed,
                TotalPromptTokens = totalPrompt,
                TotalCompletionTokens = totalCompletion,
                EffectiveParallelism = effectiveParallelism,
                LinesPerSecond = linesPerSecond,
                Chunks = orderedChunks,
            };
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

            // Defensive guard: ChunkSize <= 0 causes an infinite loop in the
            // chunking for-loop. MaxConcurrentChunks <= 0 deadlocks the semaphore.
            // These should be caught by ProcessingOptionsValidator upstream, but
            // this guard ensures the processor never hangs regardless of entry path.
            if (chunking.ChunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunking),
                    $"ChunkSize must be a positive integer, but was {chunking.ChunkSize}.");
            }

            if (chunking.MaxConcurrentChunks <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunking),
                    $"MaxConcurrentChunks must be a positive integer, but was {chunking.MaxConcurrentChunks}.");
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

            // Capture run start for aggregate metrics. The wall-clock timestamp feeds
            // the Gantt chart and the JSON envelope; the Stopwatch feeds ElapsedMs so a
            // sub-tick run can never report zero elapsed time (DateTime granularity issue).
            var runStart = DateTime.UtcNow;
            var runStopwatch = Stopwatch.StartNew();

            // Prepare a list to capture per-chunk metrics as they complete
            var chunkMetrics = new System.Collections.Concurrent.ConcurrentBag<Telemetry.ChunkMetrics>();

            // 4. Start all tasks concurrently (throttled by the semaphore), capturing the index of each chunk.
            var tasks = chunks.Select(async chunk =>
            {
                var queueStart = DateTime.UtcNow;
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                var queueDelayMs = (long)(DateTime.UtcNow - queueStart).TotalMilliseconds;
                var attemptStartUtc = DateTime.UtcNow;
                int chunkRetryAttempts = 0;
                try
                {
                    // Execute prompt asynchronously with chunk-level retry — use per-call settings if available.
                    // The chunk-level retry wraps the service-level retry (already inside each AiServiceBase).
                    // Chunk retry handles failures that exhaust the service-level retry, giving the chunk
                    // a fresh service-level attempt cycle for each chunk-level retry.
                    AiCallResult aiResult = await TransientRetryPolicy.ExecuteAsync(
                        async cancellationToken =>
                        {
                            return perCallSettings != null
                                ? await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, perCallSettings, cancellationToken).ConfigureAwait(false)
                                : await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, cancellationToken).ConfigureAwait(false);
                        },
                        ex => ex is not OperationCanceledException,
                        processingLimits.ChunkMaxRetries,
                        processingLimits.ChunkBaseDelaySeconds,
                        processingLimits.ChunkMaxDelaySeconds,
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

                    // Record telemetry for the failed chunk (Status = "Failed") so the
                    // aborted run still produces a partial timeline instead of discarding
                    // everything collected before the failure.
                    var failureUtc = DateTime.UtcNow;
                    chunkMetrics.Add(new Telemetry.ChunkMetrics
                    {
                        ChunkIndex = chunk.Index,
                        StartLine = (chunk.Index * chunking.ChunkSize) + 1,
                        EndLine = Math.Min(lines.Length, (chunk.Index + 1) * chunking.ChunkSize),
                        QueueDelayMs = queueDelayMs,
                        RetryAttempts = chunkRetryAttempts,
                        Telemetry = new Telemetry.CallTelemetry
                        {
                            StartUtc = attemptStartUtc,
                            EndUtc = failureUtc,
                            ElapsedMs = (long)(failureUtc - attemptStartUtc).TotalMilliseconds,
                            Attempts = chunkRetryAttempts + 1,
                            Status = "Failed",
                        },
                    });

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
            // we fail fast with a descriptive error specifying the exact lines of the first
            // failed chunk. The collected metrics (successful and failed chunks) travel with
            // the exception so the UI can display a partial timeline of the aborted run.
            var failedResults = results.Where(r => r.Exception != null).ToList();
            if (failedResults.Any())
            {
                var firstFailure = failedResults[0];
                int startLine = (firstFailure.Index * chunking.ChunkSize) + 1;
                int endLine = Math.Min(lines.Length, (firstFailure.Index + 1) * chunking.ChunkSize);

                runStopwatch.Stop();
                var failureMetrics = BuildRunMetrics(chunkMetrics, runStart, runStopwatch.ElapsedMilliseconds, lines.Length);

                throw new ChunkProcessingException(
                    $"Processing failed for lines {startLine}-{endLine} of the file. Details: {firstFailure.Exception!.Message}",
                    firstFailure.Exception,
                    failureMetrics);
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
            runStopwatch.Stop();
            var processingMetrics = BuildRunMetrics(chunkMetrics, runStart, runStopwatch.ElapsedMilliseconds, lines.Length);

            mergedResponse.Metrics = processingMetrics;

            return mergedResponse;
        }
    }
}