//-----------------------------------------------------------------------
// <copyright file="IDeepSeekSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using AIFileProcessor.Models;

    /// <summary>
    /// Interface for DeepSeek configuration settings.
    /// </summary>
    public interface IDeepSeekSettings : IAiServiceSettings
    {
        /// <summary>
        /// Gets the API Key for DeepSeek. This is used to authenticate with the DeepSeek service and is required for making API calls.
        /// </summary>
        string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the endpoint for the DeepSeek service.
        /// </summary>
        string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the timeout for DeepSeek API calls in seconds.
        /// </summary>
        int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate for each API call.
        /// </summary>
        int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for DeepSeek API calls.
        /// </summary>
        double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the model name for DeepSeek API calls.
        /// </summary>
        string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reasoning responses are enabled for DeepSeek API calls. When enabled, the system will attempt to provide more detailed and context-aware responses by utilizing reasoning capabilities of the DeepSeek model. This may result in more accurate and insightful answers, especially for complex queries that require understanding of context and relationships between data. However, enabling reasoning may also increase response times and resource usage, so it should be used judiciously based on the application's needs.
        /// </summary>
        bool EnableReasoning { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries after the initial attempt for transient failures.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the base delay, in seconds, before the first retry; doubled each subsequent retry.
        /// </summary>
        int BaseDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum delay, in seconds, between retries.
        /// </summary>
        int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for DeepSeek API calls. Null if not configured.
        /// </summary>
        ReasoningLevel? ReasoningLevel { get; set; }
    }
}