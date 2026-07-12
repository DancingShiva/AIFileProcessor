//-----------------------------------------------------------------------
// <copyright file="OpenAiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using AIFileProcessor.Interfaces;

    /// <summary>
    /// Concrete implementation of OpenAI configuration settings.
    /// </summary>
    public class OpenAiSettings : IOpenAiSettings
    {
        /// <inheritdoc />
        public string ApiKey { get; set; } = string.Empty;

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
    }
}