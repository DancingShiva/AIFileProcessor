//-----------------------------------------------------------------------
// <copyright file="GeminiPreferencesDto.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Models
{
    /// <summary>
    /// Narrow DTO for per-request Gemini preferences sent by web clients.
    /// Deliberately excludes infrastructure settings (Endpoint, ProxyUrl,
    /// RequestHeaders, AdditionalParameters): the server-held API key travels
    /// with the server-configured endpoint, so clients must never redirect it
    /// (SEC-010, WEB-005). Only safe model/preference knobs are accepted.
    /// </summary>
    public class GeminiPreferencesDto
    {
        /// <summary>Gets or sets the model name preference.</summary>
        public string? ModelName { get; set; }

        /// <summary>Gets or sets the request timeout in seconds.</summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>Gets or sets the maximum output tokens.</summary>
        public int? MaxTokens { get; set; }

        /// <summary>Gets or sets the sampling temperature.</summary>
        public double? Temperature { get; set; }

        /// <summary>Gets or sets the top-p sampling value.</summary>
        public double? TopP { get; set; }

        /// <summary>Gets or sets the Google Cloud project ID.</summary>
        public string? ProjectId { get; set; }

        /// <summary>Gets or sets the Vertex AI location.</summary>
        public string? Location { get; set; }

        /// <summary>Gets or sets a value indicating whether streaming is enabled.</summary>
        public bool StreamingEnabled { get; set; }

        /// <summary>Gets or sets the reasoning level.</summary>
        public AIFileProcessor.Models.ReasoningLevel? ReasoningLevel { get; set; }
    }
}