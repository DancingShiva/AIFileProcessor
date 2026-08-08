//-----------------------------------------------------------------------
// <copyright file="ProcessingMetrics.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Telemetry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Aggregated performance metrics for an entire file-processing run: the overall
    /// wall-clock time plus the per-chunk breakdown. Because chunks run concurrently
    /// (throttled), per-chunk start/end times overlap and must not be summed to
    /// reconstruct the total; <see cref="ElapsedMs"/> is measured at the run boundary.
    /// </summary>
    public class ProcessingMetrics
    {
        /// <summary>Gets or sets the UTC timestamp when the run started.</summary>
        public DateTime StartUtc { get; set; }

        /// <summary>Gets or sets the UTC timestamp when the run finished.</summary>
        public DateTime EndUtc { get; set; }

        /// <summary>Gets or sets the overall wall-clock duration of the run in milliseconds.</summary>
        public long ElapsedMs { get; set; }

        /// <summary>Gets or sets the total number of chunks processed.</summary>
        public int ChunkCount { get; set; }

        /// <summary>Gets or sets the number of chunks that succeeded.</summary>
        public int Succeeded { get; set; }

        /// <summary>Gets or sets the number of chunks that failed.</summary>
        public int Failed { get; set; }

        /// <summary>Gets or sets the summed prompt tokens across chunks (null if no provider reported usage).</summary>
        public int? TotalPromptTokens { get; set; }

        /// <summary>Gets or sets the summed completion tokens across chunks (null if no provider reported usage).</summary>
        public int? TotalCompletionTokens { get; set; }

        /// <summary>Gets or sets the ratio of summed per-chunk time to elapsed time (a measure of effective concurrency).</summary>
        public double EffectiveParallelism { get; set; }

        /// <summary>Gets or sets the overall throughput in source lines per second.</summary>
        public double LinesPerSecond { get; set; }

        /// <summary>Gets or sets the per-chunk metrics, ordered by <see cref="ChunkMetrics.ChunkIndex"/>.</summary>
        public List<ChunkMetrics> Chunks { get; set; } = new List<ChunkMetrics>();
    }
}
