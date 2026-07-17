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
        public int TimeoutSeconds { get; set; } = 30;

        /// <inheritdoc />
        public string? OrganizationId { get; set; }

        /// <inheritdoc />
        public int? MaxTokens { get; set; }

        /// <inheritdoc />
        public double? Temperature { get; set; }

        /// <inheritdoc />
        public ReasoningLevel? ReasoningLevel { get; set; }
    }
}