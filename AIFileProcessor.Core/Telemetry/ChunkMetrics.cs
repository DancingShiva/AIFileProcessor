//-----------------------------------------------------------------------
// <copyright file="ChunkMetrics.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Telemetry
{
    /// <summary>
    /// Telemetry for one 40-line chunk of the input file, associating the chunk's
    /// 1-based line range with the telemetry of the API call that processed it.
    /// </summary>
    public class ChunkMetrics
    {
        /// <summary>Gets or sets the zero-based chunk index.</summary>
        public int ChunkIndex { get; set; }

        /// <summary>Gets or sets the 1-based line number of the first line in the chunk.</summary>
        public int StartLine { get; set; }

        /// <summary>Gets or sets the 1-based line number of the last line in the chunk (inclusive).</summary>
        public int EndLine { get; set; }

        /// <summary>Gets or sets the time in milliseconds the chunk spent queued on the
        /// concurrency throttle before its API call started. Zero when the semaphore
        /// admitted the chunk immediately.</summary>
        public long QueueDelayMs { get; set; }

        /// <summary>Gets or sets the telemetry of the API call that processed this chunk.</summary>
        public CallTelemetry Telemetry { get; set; } = new CallTelemetry();
    }
}
