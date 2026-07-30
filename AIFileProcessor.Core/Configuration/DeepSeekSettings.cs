//-----------------------------------------------------------------------
// <copyright file="DeepSeekSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
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
        public int TimeoutSeconds { get; set; } = 30;

        /// <inheritdoc />
        public int? MaxTokens { get; set; }

        /// <inheritdoc />
        public int MaxRetries { get; set; } = 3;

        /// <inheritdoc />
        public int BaseDelay { get; set; } = 1;

        /// <inheritdoc />
        public int MaxDelay { get; set; } = 60;

        /// <inheritdoc />
        public double? Temperature { get; set; }

        /// <inheritdoc />
        public string? ModelName { get; set; } = null;

        /// <inheritdoc />
        public bool EnableReasoning { get; set; }

        /// <inheritdoc />
        public ReasoningLevel? ReasoningLevel { get; set; }
    }
}