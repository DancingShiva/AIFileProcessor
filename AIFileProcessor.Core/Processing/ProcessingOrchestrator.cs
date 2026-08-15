//-----------------------------------------------------------------------
// <copyright file="ProcessingOrchestrator.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Extensions;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Orchestrates the Chinese text processing pipeline: calling the AI service,
    /// applying ID prefixes/padding, computing character counts, and assembling
    /// the raw tab-delimited response text. Extracted from MainForm to enable
    /// sharing between UI frontends.
    /// </summary>
    public class ProcessingOrchestrator
    {
        private readonly Func<IChineseTextProcessor> _chineseTextProcessorFactory;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private readonly ILogger<ProcessingOrchestrator> _logger;
        private readonly IEnumerable<IAiService> _aiServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingOrchestrator"/> class.
        /// </summary>
        /// <param name="chineseTextProcessorFactory">Factory delegate to create transient <see cref="IChineseTextProcessor"/> instances.</param>
        /// <param name="settingsProvider">The settings provider for resolving per-user AI service settings.</param>
        /// <param name="userContext">The current user context for per-user settings resolution.</param>
        /// <param name="logger">The logger instance for diagnostic messages.</param>
        /// <param name="aiServices">All registered AI services, used to determine which provider supports the requested model.</param>
        public ProcessingOrchestrator(
            Func<IChineseTextProcessor> chineseTextProcessorFactory,
            ISettingsProvider settingsProvider,
            IUserContext userContext,
            ILogger<ProcessingOrchestrator> logger,
            IEnumerable<IAiService> aiServices)
        {
            ArgumentNullException.ThrowIfNull(chineseTextProcessorFactory);
            ArgumentNullException.ThrowIfNull(settingsProvider);
            ArgumentNullException.ThrowIfNull(userContext);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(aiServices);

            _chineseTextProcessorFactory = chineseTextProcessorFactory;
            _settingsProvider = settingsProvider;
            _userContext = userContext;
            _logger = logger;
            _aiServices = aiServices;
        }

        /// <summary>
        /// Executes stage 1 of the processing pipeline: calls the AI service and prepares
        /// the raw tab-delimited response text along with scratchpad and corrections.
        /// Resolves per-user settings via <see cref="ISettingsProvider"/> and passes them
        /// through to the AI services.
        /// </summary>
        /// <param name="options">The processing options containing model ID, prompt, data, and formatting settings.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A <see cref="ProcessingResult"/> containing the raw response, scratchpad, corrections, and parsed rows.</returns>
        public async Task<ProcessingResult> ProcessAsync(ProcessingOptions options, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                throw new ArgumentException("ModelId is required.", nameof(options));
            }

            // Determine which provider supports the requested model
            string? detectedProviderKey = null;
            foreach (var service in _aiServices)
            {
                if (service.SupportsModel(options.ModelId))
                {
                    detectedProviderKey = AiModelCatalog.GetProviderKey(service);
                    if (detectedProviderKey != null)
                    {
                        break;
                    }

                    // Unknown type (e.g., test mock) — try all providers below.
                    break;
                }
            }

            bool hasReasoningLevel = options.ModelReasoningLevels.TryGetValue(options.ModelId, out var reasoningLevel);
            bool hasMaxTokens = options.ModelMaxTokens.TryGetValue(options.ModelId, out var maxTokens);

            string providerKey = string.Empty;
            IModelProcessingSettings modelProcessingSettings = null!;

            if (detectedProviderKey != null)
            {
                providerKey = detectedProviderKey;
                modelProcessingSettings = providerKey switch
                {
                    "Gemini" => await _settingsProvider.GetGeminiModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false),
                    "OpenAI" => await _settingsProvider.GetOpenAiModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false),
                    "DeepSeek" => await _settingsProvider.GetDeepSeekModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false),
                    _ => throw new NotSupportedException($"No registered IAiService supports the model '{options.ModelId}'."),
                };
            }
            else
            {
                // Unknown provider type (e.g., test mock with proxy type name). Try providers in order.
                // The first _settingsProvider method that doesn't throw provides the resolved settings.
                Exception? lastException = null;

                try
                {
                    providerKey = "Gemini";
                    modelProcessingSettings = await _settingsProvider.GetGeminiModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Fallback: Gemini settings resolution failed for model {ModelId}.", options.ModelId);
                }

                if (lastException != null)
                {
                    try
                    {
                        providerKey = "OpenAI";
                        modelProcessingSettings = await _settingsProvider.GetOpenAiModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false);
                        lastException = null;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        _logger.LogWarning(ex, "Fallback: OpenAI settings resolution failed for model {ModelId}.", options.ModelId);
                    }
                }

                if (lastException != null)
                {
                    try
                    {
                        providerKey = "DeepSeek";
                        modelProcessingSettings = await _settingsProvider.GetDeepSeekModelSettingsAsync(options.ModelId, _userContext.UserId, ct).ConfigureAwait(false);
                        lastException = null;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        _logger.LogWarning(ex, "Fallback: DeepSeek settings resolution failed for model {ModelId}.", options.ModelId);
                    }
                }

                if (lastException != null)
                {
                    throw new InvalidOperationException(
                        $"All providers failed to resolve settings for model '{options.ModelId}'. See inner exception for the last failure.",
                        lastException);
                }
            }

            // Shared provider-agnostic settings resolution: chunking, concurrency,
            // reasoning level, and token clamping are identical across all providers.
            var chunking = new ChunkingOptions
            {
                ChunkSize = modelProcessingSettings.ChunkSize ?? 40,
                MaxConcurrentChunks = modelProcessingSettings.MaxConcurrentChunks ?? 3,
            };
            if (hasReasoningLevel)
            {
                modelProcessingSettings.ReasoningLevel = reasoningLevel;
            }

            modelProcessingSettings.MaxTokens = hasMaxTokens
                ? TokenUtils.ClampRequestedMaxTokens(maxTokens, providerKey, options.ModelId, _logger).ClampedValue
                : null;

            IAiServiceSettings providerSettings = modelProcessingSettings;

            var providerSettingsMap = new Dictionary<string, IAiServiceSettings>
            {
                [providerKey] = providerSettings,
            };

            options.Chunking = chunking;

            var chineseTextProcessor = _chineseTextProcessorFactory();

            AiApiResponse results = await chineseTextProcessor.ProcessChineseTextAsync(
                options.ModelId, options.Prompt, options.Data, providerSettingsMap, chunking, ct).ConfigureAwait(false);

            for (int i = 0; i < results.ProcessedSentenceRows.Count; i++)
            {
                results.ProcessedSentenceRows[i].InitializeId(
                    options.IdPrefix,
                    options.IdStart + i,
                    options.IdPad);
                results.ProcessedSentenceRows[i].Length =
                    StringAnalyticsExtensions.GetCleanedCharacterCount(
                        results.ProcessedSentenceRows[i].Chinese,
                        options.LengthPad);
            }

            // Always-on deterministic verification (no API cost). Findings are
            // appended to Corrections alongside the model-reported corrections.
            IReadOnlyList<string> violations = ResponseVerifier.Verify(results);
            foreach (string finding in violations)
            {
                results.Corrections ??= [];
                results.Corrections.Add(finding);
            }

            // If any violation is an INJECTION finding, abort the entire
            // processing run immediately — all returned data is untrusted. Sanitize
            // the response first so Corrections contain the full diagnostic details,
            // then throw so that no ProcessingResult is ever returned to callers.
            try
            {
                ResponseVerifier.AbortOnInjectionViolations(violations);
            }
            catch (InvalidOperationException ex)
            {
                // Sanitize so the Corrections log carries the full details before
                // the run is torn down.
                ResponseVerifier.SanitizeResponse(results);
                _logger.LogError(
                    ex,
                    "Processing aborted for model {ModelId}: {InjectionCount} injection finding(s) among {TotalViolations} violation(s).",
                    options.ModelId,
                    violations.Count(static v => v.Contains("INJECTION", StringComparison.OrdinalIgnoreCase)),
                    violations.Count);

                // Forensic log: record the sanitized injection findings so operators
                // can review what triggered the abort without handling raw content.
                foreach (string finding in violations.Where(
                    static v => v.Contains("INJECTION", StringComparison.OrdinalIgnoreCase)).Take(3))
                {
                    _logger.LogWarning("Injection finding (sanitized): {Finding}", finding);
                }

                throw;
            }

            string rawResponse = ResponseFormatter.FormatRawRows(results.ProcessedSentenceRows);

            return new ProcessingResult
            {
                RawResponse = rawResponse,
                Scratchpad = results.Scratchpad,
                Corrections = results.Corrections,
                ProcessedSentenceRows = results.ProcessedSentenceRows,
                Metrics = results.Metrics,
            };
        }
    }
}