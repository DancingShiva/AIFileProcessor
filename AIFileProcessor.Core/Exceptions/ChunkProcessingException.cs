//-----------------------------------------------------------------------
// <copyright file="ChunkProcessingException.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Exceptions
{
    using System;
    using AIFileProcessor.Telemetry;

    /// <summary>
    /// Thrown by <see cref="Services.ChineseTextProcessor"/> when one or more
    /// chunks fail after exhausting all chunk-level retries. Carries the aggregate
    /// run metrics collected up to the failure so the UI can display a partial
    /// timeline (which chunks succeeded, which failed, and how far the run got)
    /// instead of discarding all telemetry for the aborted run. Inherits from
    /// <see cref="InvalidOperationException"/> for backward compatibility with
    /// existing catch blocks.
    /// </summary>
    public class ChunkProcessingException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkProcessingException"/> class.
        /// </summary>
        /// <param name="message">The error message describing the failed run, including the failed chunk's line range.</param>
        /// <param name="innerException">The exception that caused the first chunk to fail.</param>
        /// <param name="metrics">The aggregate metrics for the run, including per-chunk telemetry for both successful and failed chunks.</param>
        public ChunkProcessingException(string message, Exception innerException, ProcessingMetrics metrics)
            : base(message, innerException)
        {
            Metrics = metrics;
        }

        /// <summary>
        /// Gets the aggregate metrics for the aborted run. <see cref="Telemetry.ProcessingMetrics.Succeeded"/>
        /// and <see cref="Telemetry.ProcessingMetrics.Failed"/> partition the chunks by outcome;
        /// failed chunks carry a telemetry entry whose <see cref="Telemetry.CallTelemetry.Status"/> is "Failed".
        /// </summary>
        public ProcessingMetrics Metrics { get; }
    }
}