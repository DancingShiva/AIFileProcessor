//-----------------------------------------------------------------------
// <copyright file="OpenAiModelSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AIFileProcessor.Configuration
{
    /// <summary>
    /// Per-model OpenAI settings: inherits all provider-level <see cref="OpenAiSettings"/>
    /// properties and adds chunking and concurrency overrides for this specific model.
    /// </summary>
    public class OpenAiModelSettings : OpenAiSettings, IModelProcessingSettings
    {
        /// <summary>Gets or sets the number of lines per API call chunk for this model.</summary>
        public int? ChunkSize { get; set; }

        /// <summary>Gets or sets the maximum number of concurrent API calls for this model.</summary>
        public int? MaxConcurrentChunks { get; set; }

        /// <summary>Gets or sets the maximum reasoning level supported by this model.</summary>
        public Models.ReasoningLevel? MaxReasoningLevel { get; set; }
    }
}
