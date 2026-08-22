//-----------------------------------------------------------------------
// <copyright file="OllamaSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using AIFileProcessor.Interfaces;

    /// <summary>
    /// Concrete implementation of Ollama configuration settings. Covers both
    /// locally-run models and Ollama Cloud models behind a single endpoint,
    /// with an optional API key used only for cloud authentication.
    /// </summary>
    public class OllamaSettings : IOllamaSettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public string ApiKey { get; init; } = string.Empty;

        /// <inheritdoc />
        public string Endpoint { get; set; } = "http://localhost:11434";

        /// <inheritdoc />
        public string ModelName { get; set; } = string.Empty;

        /// <inheritdoc />
        public int TimeoutSeconds { get; set; } = 600;

        /// <inheritdoc />
        public int? MaxTokens { get; set; }

        /// <inheritdoc />
        public double? Temperature { get; set; }

        /// <inheritdoc />
        public double? TopP { get; set; }

        /// <inheritdoc />
        public int? TopK { get; set; }

        /// <inheritdoc />
        public int? ContextWindow { get; set; }

        /// <inheritdoc />
        public double? RepeatPenalty { get; set; }

        /// <inheritdoc />
        public int? Seed { get; set; }

        /// <inheritdoc />
        public IList<string>? Stop { get; set; }

        /// <inheritdoc />
        public double? MinP { get; set; }

        /// <inheritdoc />
        public double? TypicalP { get; set; }

        /// <inheritdoc />
        public double? TfsZ { get; set; }

        /// <inheritdoc />
        public int? MiroStat { get; set; }

        /// <inheritdoc />
        public double? MiroStatTau { get; set; }

        /// <inheritdoc />
        public double? MiroStatEta { get; set; }

        /// <inheritdoc />
        public double? PresencePenalty { get; set; }

        /// <inheritdoc />
        public double? FrequencyPenalty { get; set; }

        /// <inheritdoc />
        public int? RepeatLastN { get; set; }

        /// <inheritdoc />
        public int? NumGpu { get; set; }

        /// <inheritdoc />
        public int? NumBatch { get; set; }

        /// <inheritdoc />
        public int? NumThread { get; set; }

        /// <inheritdoc />
        public int? NumKeep { get; set; }

        /// <inheritdoc />
        public bool? Numa { get; set; }

        /// <inheritdoc />
        public bool? UseMlock { get; set; }

        /// <inheritdoc />
        public bool? UseMmap { get; set; }

        /// <inheritdoc />
        public bool? LowVram { get; set; }

        /// <inheritdoc />
        public string? KeepAlive { get; set; }

        /// <inheritdoc />
        public bool? EnableThinking { get; set; }

        /// <inheritdoc />
        public Models.ReasoningLevel? ReasoningLevel { get; set; }

        /// <inheritdoc />
        public int MaxRetries { get; set; } = 3;

        /// <inheritdoc />
        public int BaseDelay { get; set; } = 1;

        /// <inheritdoc />
        public int MaxDelay { get; set; } = 60;

        /// <inheritdoc />
        public IDictionary<string, string?>? RequestHeaders { get; set; } = new Dictionary<string, string?>();
    }
}
