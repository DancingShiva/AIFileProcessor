//-----------------------------------------------------------------------
// <copyright file="OllamaTagsResponse.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Lightweight DTO for deserializing the Ollama <c>/api/tags</c> response.
    /// The OllamaSharp SDK's <c>Model</c> type does not expose capabilities,
    /// context_length, or remote_model/remote_host fields, so this DTO captures
    /// the additional fields needed for dynamic model intelligence.
    /// </summary>
    public class OllamaTagsResponse
    {
        /// <summary>Gets or sets the list of models returned by the server.</summary>
        [JsonPropertyName("models")]
        public OllamaTagModel[] Models { get; set; } = [];
    }
}