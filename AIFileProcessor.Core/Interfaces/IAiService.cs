//-----------------------------------------------------------------------
// <copyright file="IAiService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;

    /// <summary>
    /// Defines the contract for an AI service that processes text generation prompts.
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Gets the list of models supported by this AI service.
        /// </summary>
        /// <returns>A collection of <see cref="AiModelInfo"/> instances describing each supported model.</returns>
        IEnumerable<AiModelInfo> GetSupportedModels();

        /// <summary>
        /// Determines if the AI service supports the specified model name.
        /// </summary>
        /// <param name="modelNameOrId">The name or Id of the AI model to check for support.</param>
        /// <returns>True if the model is supported; otherwise, false.</returns>
        bool SupportsModel(string modelNameOrId);

        /// <summary>
        /// Gets the list of reasoning levels supported by the specified model.
        /// </summary>
        /// <param name="modelId">The model identifier to look up.</param>
        /// <returns>A read-only list of supported reasoning levels, or an empty list if the model is unknown.</returns>
        IReadOnlyList<ReasoningLevel> GetSupportedReasoningLevels(string modelId);

        /// <summary>
        /// Generates text content based on the provided prompt and configuration settings.
        /// Uses the settings injected via the constructor (from <c>IOptions&lt;T&gt;</c>).
        /// </summary>
        /// <param name="modelId">The name of the AI model to use for text generation.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the generated text response and its call telemetry.</returns>
        Task<AiCallResult> ProcessTextAsync(string modelId, string prompt, string data, CancellationToken ct = default);

        /// <summary>
        /// Generates text content using the provided per-call settings, allowing
        /// per-user configuration without relying on the constructor-injected snapshot.
        /// </summary>
        /// <param name="modelId">The name of the AI model to use for text generation.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The provider-specific settings instance (e.g., <see cref="GeminiSettings"/>, <see cref="OpenAiSettings"/>, <see cref="DeepSeekSettings"/>).</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the generated text response and its call telemetry.</returns>
        Task<AiCallResult> ProcessTextAsync(string modelId, string prompt, string data, object settings, CancellationToken ct = default);
    }
}