//-----------------------------------------------------------------------
// <copyright file="ProviderSettingsResponse.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Models
{
    using AIFileProcessor.Configuration;

    /// <summary>
    /// Response DTO for the settings endpoint. Returns the current default
    /// settings (with API keys stripped) for all three AI providers.
    /// </summary>
    public class ProviderSettingsResponse
    {
        /// <summary>
        /// Gets or sets the Gemini settings (API key excluded).
        /// </summary>
        public GeminiSettings GeminiSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the OpenAI settings (API key excluded).
        /// </summary>
        public OpenAiSettings OpenAiSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the DeepSeek settings (API key excluded).
        /// </summary>
        public DeepSeekSettings DeepSeekSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the Ollama settings (API key excluded).
        /// </summary>
        public OllamaSettings OllamaSettings { get; set; } = new();
    }
}