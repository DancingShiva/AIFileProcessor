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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingOrchestrator"/> class.
        /// </summary>
        /// <param name="chineseTextProcessorFactory">Factory delegate to create transient <see cref="IChineseTextProcessor"/> instances.</param>
        /// <param name="settingsProvider">The settings provider for resolving per-user AI service settings.</param>
        /// <param name="userContext">The current user context for per-user settings resolution.</param>
        public ProcessingOrchestrator(
            Func<IChineseTextProcessor> chineseTextProcessorFactory,
            ISettingsProvider settingsProvider,
            IUserContext userContext)
        {
            ArgumentNullException.ThrowIfNull(chineseTextProcessorFactory);
            ArgumentNullException.ThrowIfNull(settingsProvider);
            ArgumentNullException.ThrowIfNull(userContext);

            _chineseTextProcessorFactory = chineseTextProcessorFactory;
            _settingsProvider = settingsProvider;
            _userContext = userContext;
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

            var providerSettings = new Dictionary<string, object>
            {
                ["Gemini"] = geminiSettings,
                ["OpenAI"] = openAiSettings,
                ["DeepSeek"] = deepSeekSettings,
            };

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

            string rawResponse = string.Join(
                System.Environment.NewLine,
                results.ProcessedSentenceRows.Select(r =>
                {
                    string breakdownStr = string.Join("||", r.Breakdown.Select(b =>
                        $"{b.Chinese}|{b.Pinyin}|{b.Gloss}|{b.PartOfSpeech}|{b.HskLevel}|{(string.IsNullOrWhiteSpace(b.Note) ? " " : b.Note)}"));
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