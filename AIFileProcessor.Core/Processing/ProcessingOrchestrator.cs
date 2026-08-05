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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingOrchestrator"/> class.
        /// </summary>
        /// <param name="chineseTextProcessorFactory">Factory delegate to create transient <see cref="IChineseTextProcessor"/> instances.</param>
        /// <param name="settingsProvider">The settings provider for resolving per-user AI service settings.</param>
        /// <param name="userContext">The current user context for per-user settings resolution.</param>
        /// <param name="logger">The logger instance for diagnostic messages.</param>
        public ProcessingOrchestrator(
            Func<IChineseTextProcessor> chineseTextProcessorFactory,
            ISettingsProvider settingsProvider,
            IUserContext userContext,
            ILogger<ProcessingOrchestrator> logger)
        {
            ArgumentNullException.ThrowIfNull(chineseTextProcessorFactory);
            ArgumentNullException.ThrowIfNull(settingsProvider);
            ArgumentNullException.ThrowIfNull(userContext);
            ArgumentNullException.ThrowIfNull(logger);

            _chineseTextProcessorFactory = chineseTextProcessorFactory;
            _settingsProvider = settingsProvider;
            _userContext = userContext;
            _logger = logger;
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

            // Resolve per-user settings for all providers
            var geminiSettings = await _settingsProvider.GetGeminiSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);
            var openAiSettings = await _settingsProvider.GetOpenAiSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);
            var deepSeekSettings = await _settingsProvider.GetDeepSeekSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);

            // Single source of truth for provider keys (matches ChineseTextProcessor.GetProviderKey).
            var providers = new (string Key, object Settings)[]
            {
                ("Gemini", geminiSettings),
                ("OpenAI", openAiSettings),
                ("DeepSeek", deepSeekSettings),
            };

            // Apply the per-model reasoning level and clamped MaxTokens from processing options
            // onto each provider's settings. ModelReasoningLevels/ModelMaxTokens are the single
            // source of truth — updated by both MainForm and SettingsForm.
            if (!string.IsNullOrWhiteSpace(options.ModelId))
            {
                bool hasReasoningLevel = options.ModelReasoningLevels.TryGetValue(options.ModelId, out var reasoningLevel);
                bool hasMaxTokens = options.ModelMaxTokens.TryGetValue(options.ModelId, out var maxTokens);

                foreach (var (key, settings) in providers)
                {
                    // Clamp to this provider's hard max so out-of-range requests don't fail at the API.
                    // No per-model value => null, letting the API decide.
                    int? clampedMaxTokens = hasMaxTokens
                        ? TokenUtils.ClampRequestedMaxTokens(maxTokens, key, options.ModelId, _logger).ClampedValue
                        : null;

                    switch (settings)
                    {
                        case Configuration.GeminiSettings g:
                            if (hasReasoningLevel)
                            {
                                g.ReasoningLevel = reasoningLevel;
                            }

                            g.MaxTokens = clampedMaxTokens;
                            break;

                        case Configuration.OpenAiSettings o:
                            if (hasReasoningLevel)
                            {
                                o.ReasoningLevel = reasoningLevel;
                            }

                            o.MaxTokens = clampedMaxTokens;
                            break;

                        case Configuration.DeepSeekSettings d:
                            if (hasReasoningLevel)
                            {
                                d.ReasoningLevel = reasoningLevel;
                            }

                            d.MaxTokens = clampedMaxTokens;
                            break;
                    }
                }
            }

            var providerSettings = new Dictionary<string, object>();
            foreach (var (key, settings) in providers)
            {
                providerSettings[key] = settings;
            }

            var chineseTextProcessor = _chineseTextProcessorFactory();

            AiApiResponse results = await chineseTextProcessor.ProcessChineseTextAsync(
                options.ModelId, options.Prompt, options.Data, providerSettings, ct).ConfigureAwait(false);

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
            foreach (string finding in ResponseVerifier.Verify(results))
            {
                results.Corrections ??= [];
                results.Corrections.Add(finding);
            }

            string rawResponse = string.Join(
                System.Environment.NewLine,
                results.ProcessedSentenceRows.Select(r =>
                {
                    string breakdownStr = string.Join("||", r.Breakdown.Select(b =>
                        $"{b.Chinese}|{b.Pinyin}|{b.Gloss}|{b.DisplayPartOfSpeech}|{b.DisplayHskLevel}|{(string.IsNullOrWhiteSpace(b.Note) ? " " : b.Note)}"));
                    return $"{r.Id}\t{r.Chinese}\t{r.Pinyin}\t{breakdownStr}\t{string.Join("||", r.AudioReordering.Select(a => $"{a.VisualString}|{a.StartIndex},{a.EndIndex}"))}\t{r.English}\t{r.Length}";
                }));

            return new ProcessingResult
            {
                RawResponse = rawResponse,
                Scratchpad = results.Scratchpad,
                Corrections = results.Corrections,
                ProcessedSentenceRows = results.ProcessedSentenceRows,
            };
        }
    }
}