//-----------------------------------------------------------------------
// <copyright file="OllamaModelSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AIFileProcessor.Configuration
{
    /// <summary>
    /// Per-model Ollama settings: inherits all provider-level <see cref="OllamaSettings"/>
    /// properties and adds chunking, concurrency, and location/thinking flags for
    /// this specific model.
    /// </summary>
    public class OllamaModelSettings : OllamaSettings, IModelProcessingSettings
    {
        /// <summary>Gets or sets the number of lines per API call chunk for this model.</summary>
        public int? ChunkSize { get; set; }

        /// <summary>Gets or sets the maximum number of concurrent API calls for this model.</summary>
        public int? MaxConcurrentChunks { get; set; }

        /// <summary>Gets or sets the maximum reasoning level supported by this model.</summary>
        public Models.ReasoningLevel? MaxReasoningLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model runs on Ollama Cloud
        /// rather than on the server at <see cref="OllamaSettings.Endpoint"/>.
        /// Cloud models require <see cref="OllamaSettings.ApiKey"/> to be configured.
        /// </summary>
        public bool IsCloud { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model supports the Ollama
        /// thinking capability (<c>think</c> parameter). Used to surface the
        /// reasoning toggle in the UI for thinking-capable models.
        /// </summary>
        public bool SupportsThinking { get; set; }
    }
}
