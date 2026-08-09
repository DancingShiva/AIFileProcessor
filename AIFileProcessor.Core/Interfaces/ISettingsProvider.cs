//-----------------------------------------------------------------------
// <copyright file="ISettingsProvider.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Models;

    /// <summary>
    /// Resolves per-user AI service settings on demand, supporting multiple
    /// deployment modes (single-user desktop, Terminal Server, web).
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the Gemini settings for the specified user, merging defaults
        /// with any per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to resolve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved <see cref="GeminiSettings"/> for the user.</returns>
        Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Gets the OpenAI settings for the specified user, merging defaults
        /// with any per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to resolve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved <see cref="OpenAiSettings"/> for the user.</returns>
        Task<OpenAiSettings> GetOpenAiSettingsAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Gets the DeepSeek settings for the specified user, merging defaults
        /// with any per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to resolve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved <see cref="DeepSeekSettings"/> for the user.</returns>
        Task<DeepSeekSettings> GetDeepSeekSettingsAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified Gemini settings as per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to save.</param>
        /// <param name="settings">The settings to persist.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveGeminiSettingsAsync(string userId, GeminiSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified OpenAI settings as per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to save.</param>
        /// <param name="settings">The settings to persist.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveOpenAiSettingsAsync(string userId, OpenAiSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified DeepSeek settings as per-user overrides.
        /// </summary>
        /// <param name="userId">The user identifier whose settings to save.</param>
        /// <param name="settings">The settings to persist.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveDeepSeekSettingsAsync(string userId, DeepSeekSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Gets the last-used processing options for the specified user.
        /// </summary>
        /// <param name="userId">The user identifier whose processing options to retrieve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The persisted <see cref="ProcessingOptions"/>, or a default instance if none exists.</returns>
        Task<ProcessingOptions> GetProcessingOptionsAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified processing options as per-user preferences.
        /// </summary>
        /// <param name="userId">The user identifier whose processing options to save.</param>
        /// <param name="options">The processing options to persist.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveProcessingOptionsAsync(string userId, ProcessingOptions options, CancellationToken ct = default);

        /// <summary>
        /// Gets the resolved per-model Gemini settings for the specified user and model,
        /// merging defaults → per-model default → per-user override, capped by global limits.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to resolve.</param>
        /// <param name="userId">The user identifier whose settings to retrieve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved per-model Gemini settings.</returns>
        Task<GeminiModelSettings> GetGeminiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default);

        /// <summary>
        /// Gets the resolved per-model OpenAI settings for the specified user and model,
        /// merging defaults → per-model default → per-user override, capped by global limits.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to resolve.</param>
        /// <param name="userId">The user identifier whose settings to retrieve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved per-model OpenAI settings.</returns>
        Task<OpenAiModelSettings> GetOpenAiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default);

        /// <summary>
        /// Gets the resolved per-model DeepSeek settings for the specified user and model,
        /// merging defaults → per-model default → per-user override, capped by global limits.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to resolve.</param>
        /// <param name="userId">The user identifier whose settings to retrieve.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The resolved per-model DeepSeek settings.</returns>
        Task<DeepSeekModelSettings> GetDeepSeekModelSettingsAsync(string modelId, string userId, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified per-model Gemini settings as per-user overrides.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to save.</param>
        /// <param name="userId">The user identifier whose settings to persist.</param>
        /// <param name="settings">The per-model Gemini settings to save.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveGeminiModelSettingsAsync(string modelId, string userId, GeminiModelSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified per-model OpenAI settings as per-user overrides.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to save.</param>
        /// <param name="userId">The user identifier whose settings to persist.</param>
        /// <param name="settings">The per-model OpenAI settings to save.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveOpenAiModelSettingsAsync(string modelId, string userId, OpenAiModelSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Persists the specified per-model DeepSeek settings as per-user overrides.
        /// </summary>
        /// <param name="modelId">The model identifier whose settings to save.</param>
        /// <param name="userId">The user identifier whose settings to persist.</param>
        /// <param name="settings">The per-model DeepSeek settings to save.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveDeepSeekModelSettingsAsync(string modelId, string userId, DeepSeekModelSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Gets the global processing settings (defaults and caps) for the current machine.
        /// </summary>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>The global processing settings containing defaults and caps.</returns>
        Task<ProcessingSettings> GetProcessingSettingsAsync(CancellationToken ct = default);
    }
}