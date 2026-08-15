//-----------------------------------------------------------------------
// <copyright file="IModelProcessingSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;

    /// <summary>
    /// Defines the common per-model processing settings that the orchestrator
    /// applies uniformly across all AI providers: chunking, concurrency,
    /// reasoning level, and max tokens. Extends <see cref="IAiServiceSettings"/>
    /// so that per-model settings instances are also valid
    /// <see cref="IAiServiceSettings"/> for the AI service call chain.
    /// Implemented by <see cref="GeminiModelSettings"/>,
    /// <see cref="OpenAiModelSettings"/>, and <see cref="DeepSeekModelSettings"/>.
    /// </summary>
    public interface IModelProcessingSettings : IAiServiceSettings
    {
        /// <summary>Gets or sets the number of lines per API call chunk for this model.</summary>
        int? ChunkSize { get; set; }

        /// <summary>Gets or sets the maximum number of concurrent API calls for this model.</summary>
        int? MaxConcurrentChunks { get; set; }

        /// <summary>Gets or sets the reasoning level for this model.</summary>
        ReasoningLevel? ReasoningLevel { get; set; }

        /// <summary>Gets or sets the maximum output tokens for this model.</summary>
        int? MaxTokens { get; set; }

        /// <summary>Gets or sets the maximum reasoning level supported by this model.</summary>
        ReasoningLevel? MaxReasoningLevel { get; set; }
    }
}