//-----------------------------------------------------------------------
// <copyright file="ProcessingResult.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System.Collections.Generic;
    using AIFileProcessor.Response;
    using AIFileProcessor.Telemetry;

    /// <summary>
    /// Contains the results of the processing pipeline stage 1.
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Gets or sets the raw tab-delimited response text suitable for display or file saving.
        /// </summary>
        public string RawResponse { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scratchpad text from the AI response (debugging/diagnostic info).
        /// </summary>
        public string Scratchpad { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of corrections applied by the AI during processing.
        /// </summary>
        public List<string> Corrections { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of processed sentence rows from the AI response.
        /// </summary>
        public List<ProcessedSentenceRow> ProcessedSentenceRows { get; set; } = new List<ProcessedSentenceRow>();

        /// <summary>
        /// Gets or sets the aggregated processing metrics for the entire run.
        /// </summary>
        public ProcessingMetrics? Metrics { get; set; }
    }
}