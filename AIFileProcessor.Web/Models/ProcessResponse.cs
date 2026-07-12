//-----------------------------------------------------------------------
// <copyright file="ProcessResponse.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Models
{
    using System.Collections.Generic;
    using AIFileProcessor.Response;

    /// <summary>
    /// Response DTO for the text processing endpoint.
    /// </summary>
    public class ProcessResponse
    {
        /// <summary>
        /// Gets or sets the raw tab-delimited response text.
        /// </summary>
        public string RawResponse { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scratchpad text from the AI response.
        /// </summary>
        public string Scratchpad { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of corrections applied by the AI.
        /// </summary>
        public List<string> Corrections { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of processed sentence rows.
        /// </summary>
        public List<ProcessedSentenceRow> ProcessedSentenceRows { get; set; } = [];
    }
}