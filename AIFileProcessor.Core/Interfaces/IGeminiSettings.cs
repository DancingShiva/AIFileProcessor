//-----------------------------------------------------------------------
// <copyright file="IGeminiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Models;

    /// <summary>
    /// Configuration settings specific to the Google Gemini API.
    /// </summary>
    public interface IGeminiSettings : IAiServiceSettings
    {
        /// <summary>
        /// Gets the API key or credential used to authenticate with the Gemini service.
        /// </summary>
        string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the Base URL or endpoint for the Gemini service. Leave empty to use the provider default.
        /// </summary>
        string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the Model identifier to request from the Gemini service (e.g., "gemini-1.5-pro").
        /// </summary>
        string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the Default timeout in seconds for requests.
        /// </summary>
        int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the Google Cloud Project ID associated with the account (if applicable).
        /// </summary>
        string? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the Region / location for API requests (e.g., "us-central1").
        /// </summary>
        string? Location { get; set; }

        /// <summary>
        /// Gets or sets the Maximum tokens to request from the model. Null if not applicable.
        /// </summary>
        int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the Sampling temperature for the model (typically 0.0 - 1.0). Null if not applicable.
        /// </summary>
        double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the Nucleus sampling probability (top-p). Null if not applicable.
        /// </summary>
        double? TopP { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to request streaming responses from the model.
        /// </summary>
        bool StreamingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of retry attempts for transient failures.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the base delay in seconds for the first retry attempt.
        /// Subsequent retries use exponential backoff (baseDelay * 2^attempt), capped at MaxDelay.
        /// </summary>
        int BaseDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum delay in seconds between retry attempts.
        /// </summary>
        int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary request headers to include when calling the provider.
        /// </summary>
        IDictionary<string, string?>? RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the Provider-specific additional parameters for extensibility.
        /// </summary>
        IDictionary<string, string?>? AdditionalParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable request/response logging. Do not enable in production if it logs secrets.
        /// </summary>
        bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets the Client-side rate limit (requests per minute) to help respect provider quotas. Null disables client throttling.
        /// </summary>
        int? RateLimitPerMinute { get; set; }

        /// <summary>
        /// Gets or sets the optional proxy URL to use for outgoing requests.
        /// </summary>
        string? ProxyUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use managed identity / non-key credential flow instead of an API key.
        /// </summary>
        bool UseManagedIdentity { get; set; }

        /// <summary>
        /// Gets or sets the optional credential type or hint for advanced auth flows (e.g., "OAuth", "ServiceAccount").
        /// </summary>
        string? CredentialType { get; set; }

        /// <summary>
        /// Gets or sets the reasoning/thinking level for the Gemini model. Null if not configured.
        /// </summary>
        ReasoningLevel? ReasoningLevel { get; set; }
    }
}