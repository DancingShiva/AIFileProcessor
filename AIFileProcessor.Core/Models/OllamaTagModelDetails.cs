//-----------------------------------------------------------------------
// <copyright file="OllamaTagModelDetails.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the details block of a model in the Ollama <c>/api/tags</c> response.
    /// </summary>
    public class OllamaTagModelDetails
    {
        /// <summary>Gets or sets the model family (e.g., "llama", "gemma", "qwen").</summary>
        [JsonPropertyName("family")]
        public string? Family { get; set; }

        /// <summary>Gets or sets the context window length in tokens.</summary>
        [JsonPropertyName("context_length")]
        public int? ContextLength { get; set; }
    }
}