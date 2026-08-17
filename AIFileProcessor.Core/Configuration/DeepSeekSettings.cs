//-----------------------------------------------------------------------
// <copyright file="DeepSeekSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;

    /// <summary>
    /// Concrete implementation of DeepSeek configuration settings.
    /// </summary>
    public class DeepSeekSettings : IDeepSeekSettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public string ApiKey { get; init; } = string.Empty;

        /// <inheritdoc />
        public string Endpoint { get; set; } = string.Empty;

        /// <inheritdoc />
        public int TimeoutSeconds { get; set; } = 300;

        /// <inheritdoc />
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries after the initial attempt for transient failures.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base delay, in seconds, before the first retry; doubled each subsequent retry.
        /// </summary>
        public int BaseDelay { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum delay, in seconds, between retries.
        /// </summary>
        public int MaxDelay { get; set; } = 60;

        /// <inheritdoc />
        public double? Temperature { get; set; }

        /// <inheritdoc />
        public string? ModelName { get; set; } = null;

        /// <inheritdoc />
        public bool EnableReasoning { get; set; }

        /// <inheritdoc />
        public ReasoningLevel? ReasoningLevel { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary request headers to include when calling the provider.
        /// </summary>
        public IDictionary<string, string?>? RequestHeaders { get; set; } = new Dictionary<string, string?>();
    }
}