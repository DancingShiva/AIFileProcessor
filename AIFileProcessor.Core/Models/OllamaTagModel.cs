//-----------------------------------------------------------------------
// <copyright file="OllamaTagModel.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a single model entry in the Ollama <c>/api/tags</c> response.
    /// </summary>
    public class OllamaTagModel
    {
        /// <summary>Gets or sets the model name (e.g., "llama3.1", "kimi-k3:cloud").</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the model identifier (same as Name in most cases).</summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>Gets or sets the remote model name (present only for cloud models).</summary>
        [JsonPropertyName("remote_model")]
        public string? RemoteModel { get; set; }

        /// <summary>Gets or sets the remote host (present only for cloud models).</summary>
        [JsonPropertyName("remote_host")]
        public string? RemoteHost { get; set; }

        /// <summary>Gets or sets the model details including context length.</summary>
        [JsonPropertyName("details")]
        public OllamaTagModelDetails Details { get; set; } = new();

        /// <summary>Gets or sets the model capabilities (e.g., "completion", "tools", "thinking", "vision").</summary>
        [JsonPropertyName("capabilities")]
        public string[] Capabilities { get; set; } = [];
    }
}