//-----------------------------------------------------------------------
// <copyright file="ProviderSettingsResponse.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Models
{
    /// <summary>
    /// Response DTO for the settings endpoint. Returns current default
    /// settings per provider with API keys excluded ([JsonIgnore] → SEC-005)
    /// and infrastructure settings (Endpoint, ProxyUrl, RequestHeaders,
    /// AdditionalParameters) omitted: machine-level infrastructure may embed
    /// gateway credentials and must not be echoed to clients (SEC-010, WEB-003).
    /// </summary>
    public class ProviderSettingsResponse
    {
        /// <summary>
        /// Gets or sets the Gemini preferences (API key and infrastructure excluded).
        /// </summary>
        public GeminiPreferencesDto GeminiSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the OpenAI preferences (API key and infrastructure excluded).
        /// </summary>
        public OpenAiPreferencesDto OpenAiSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the DeepSeek preferences (API key and infrastructure excluded).
        /// </summary>
        public DeepSeekPreferencesDto DeepSeekSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the Ollama preferences (API key and infrastructure excluded).
        /// </summary>
        public OllamaPreferencesDto OllamaSettings { get; set; } = new();
    }
}