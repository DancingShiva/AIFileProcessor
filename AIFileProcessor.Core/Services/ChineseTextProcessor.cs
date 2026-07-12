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
    using AIFileProcessor.Response;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes Chinese text blocks by chunking and executing them in parallel.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="aiServices">The AI services to execute prompts against.</param>
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
    public class ChineseTextProcessor(ILogger<ChineseTextProcessor> logger, IEnumerable<IAiService> aiServices) : IChineseTextProcessor
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
    {
        private const int MaxConcurrentChunks = 3;

        // Define the separators as a static readonly field
        private static readonly string[] _lineSeparators = ["\r\n", "\r", "\n"];

        private static readonly JsonSerializerOptions _defaultSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Splits Chinese text into 40-line chunks, processes them in parallel,
        /// tracks their original index to maintain order, and merges them.
        /// </summary>
        /// <param name="modelId">The ID of the AI model to use for processing.</param>
        /// <param name="prompt">The prompt to execute for each chunk of text.</param>
        /// <param name="data">The block of Chinese text to process.</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>A merged AiApiResponse containing the combined results from all chunks.</returns>
        public async Task<AiApiResponse> ProcessChineseTextAsync(string modelId, string prompt, string data, CancellationToken ct = default)
        {
            return await ProcessInternalAsync(modelId, prompt, data, providerSettings: null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<AiApiResponse> ProcessChineseTextAsync(
            string modelId,
            string prompt,
            string data,
            IDictionary<string, object> providerSettings,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(providerSettings);
            return await ProcessInternalAsync(modelId, prompt, data, providerSettings, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Maps an <see cref="IAiService"/> instance to its provider key
        /// for looking up per-user settings in the provider settings dictionary.
        /// </summary>
        private static string GetProviderKey(IAiService service)
        {
            return service.GetType().Name switch
            {
                "GeminiService" => "Gemini",
                "OpenAiService" => "OpenAI",
                "DeepSeekService" => "DeepSeek",
                _ => service.GetType().Name,
            };
        }

        /// <summary>
        /// Internal implementation that handles chunking, parallel execution,
        /// and merging. When <paramref name="providerSettings"/> is non-null,
        /// the per-call settings overload of <see cref="IAiService.ProcessTextAsync(string, string, string, object, CancellationToken)"/>
        /// is used; otherwise the default (constructor-injected) overload is used.
        /// </summary>
        private async Task<AiApiResponse> ProcessInternalAsync(
            string modelId,
            string prompt,
            string data,
            IDictionary<string, object>? providerSettings,
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
                    providerKey = GetProviderKey(service);
                    break;
                }
            }

            if (aiService == null)
            {
                throw new NotSupportedException($"No registered IAiService supports the model '{modelId}'.");
            }

            // Resolve per-call settings for this provider, if available
            object? perCallSettings = null;
            if (providerSettings != null && providerKey != null && providerSettings.TryGetValue(providerKey, out var settingsObj))
            {
                perCallSettings = settingsObj;
            }

            // 1. Split text into individual lines
            string[] lines = data.Split(_lineSeparators, StringSplitOptions.None);

            // 2. Break down into chunks of 40 lines and associate each with its original index
            var chunks = new List<(int Index, string Text)>();
            const int ChunkSize = 40;

            for (int i = 0; i < lines.Length; i += ChunkSize)
            {
                var chunkLines = lines.Skip(i).Take(ChunkSize);
                string chunkText = string.Join(Environment.NewLine, chunkLines);
                chunks.Add((Index: i / ChunkSize, Text: chunkText));
            }

            // 3. Configure JsonSerializerOptions (Mapping "SP" to "Scratchpad" and "Sentences" to "ProcessedSentenceRows")
            var serializerOptions = _defaultSerializerOptions;

            // Use a semaphore to limit concurrency, preventing rate-limit violations
            // and connection pool exhaustion when processing many chunks.
            using var semaphore = new SemaphoreSlim(MaxConcurrentChunks, MaxConcurrentChunks);

            // 4. Start all tasks concurrently (throttled by the semaphore), capturing the index of each chunk.
            var tasks = chunks.Select(async chunk =>
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    // Execute prompt asynchronously — use per-call settings if available
                    string rawJsonResult = perCallSettings != null
                        ? await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, perCallSettings, ct)
                        : await aiService.ProcessTextAsync(modelId, prompt, chunk.Text, ct);

                    // Parse directly into a AiApiResponse using the specified constructor
                    var response = new AiApiResponse(rawJsonResult, serializerOptions);

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
                int startLine = (firstFailure.Index * ChunkSize) + 1;
                int endLine = Math.Min(lines.Length, (firstFailure.Index + 1) * ChunkSize);

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

            return mergedResponse;
        }
    }
}