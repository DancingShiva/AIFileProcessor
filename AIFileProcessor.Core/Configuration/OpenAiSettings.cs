//-----------------------------------------------------------------------
// <copyright file="OpenAiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System.Text.Json.Serialization;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;

    /// <summary>
    /// Concrete implementation of OpenAI configuration settings.
    /// </summary>
    public class OpenAiSettings : IOpenAiSettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public string ApiKey { get; init; } = string.Empty;

        /// <inheritdoc />
        public string Endpoint { get; set; } = string.Empty;

        /// <inheritdoc />
        public string ModelName { get; set; } = string.Empty;

        /// <inheritdoc />
        public int TimeoutSeconds { get; set; } = 300;

        /// <inheritdoc />
        public string? OrganizationId { get; set; }

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
        public ReasoningLevel? ReasoningLevel { get; set; }
    }
}