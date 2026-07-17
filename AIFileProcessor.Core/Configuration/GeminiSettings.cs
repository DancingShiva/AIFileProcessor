//-----------------------------------------------------------------------
// <copyright file="GeminiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;

    /// <summary>
    /// Concrete implementation of <see cref="IGeminiSettings"/> used for binding from configuration.
    /// </summary>
    public class GeminiSettings : IGeminiSettings
    {
        [JsonIgnore]
        public string ApiKey { get; init; } = string.Empty;

        public string? Endpoint { get; set; } = null;

        public string? ModelName { get; set; } = null;

        public int TimeoutSeconds { get; set; } = 30;

        public string? ProjectId { get; set; } = null;

        public string? Location { get; set; } = null;

        public int? MaxTokens { get; set; } = null;

        public double? Temperature { get; set; } = null;

        public double? TopP { get; set; } = null;

        public bool StreamingEnabled { get; set; } = false;

        public int MaxRetries { get; set; } = 3;

        public int BaseDelay { get; set; } = 1;

        public int MaxDelay { get; set; } = 60;

        public IDictionary<string, string?>? RequestHeaders { get; set; } = new Dictionary<string, string?>();

        public IDictionary<string, string?>? AdditionalParameters { get; set; } = new Dictionary<string, string?>();

        public bool EnableLogging { get; set; } = false;

        public int? RateLimitPerMinute { get; set; } = null;

        public string? ProxyUrl { get; set; } = null;

        public bool UseManagedIdentity { get; set; } = false;

        public string? CredentialType { get; set; } = null;

        public ReasoningLevel? ReasoningLevel { get; set; } = null;
    }
}