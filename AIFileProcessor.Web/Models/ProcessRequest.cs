//-----------------------------------------------------------------------
// <copyright file="ProcessRequest.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Models
{
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Models;

    /// <summary>
    /// Request DTO for the text processing endpoint. Contains the processing
    /// options and per-user non-secret preferences for each AI provider.
    /// API keys are supplied via request headers, not this body.
    /// </summary>
    public class ProcessRequest
    {
        /// <summary>
        /// Gets or sets the model ID to use for processing (e.g., "gemini-3.5-flash").
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the prompt text to send to the AI.
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data text to process.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID prefix for generated sentence IDs.
        /// </summary>
        public string IdPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the starting number for generated IDs.
        /// </summary>
        public int IdStart { get; set; } = 1;

        /// <summary>
        /// Gets or sets the padding length for the numeric part of generated IDs.
        /// </summary>
        public int IdPad { get; set; } = 3;

        /// <summary>
        /// Gets or sets the padding length for the length indicator of each sentence.
        /// </summary>
        public int LengthPad { get; set; } = 3;

        /// <summary>
        /// Gets or sets non-secret Gemini preferences to override defaults.
        /// </summary>
        public GeminiSettings? GeminiPreferences { get; set; }

        /// <summary>
        /// Gets or sets non-secret OpenAI preferences to override defaults.
        /// </summary>
        public OpenAiSettings? OpenAiPreferences { get; set; }

        /// <summary>
        /// Gets or sets non-secret DeepSeek preferences to override defaults.
        /// </summary>
        public DeepSeekSettings? DeepSeekPreferences { get; set; }

        /// <summary>
        /// Gets or sets the reasoning level to use for this processing request.
        /// When set, this overrides the provider-level default.
        /// </summary>
        public ReasoningLevel? ReasoningLevel { get; set; }
    }
}