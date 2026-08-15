//-----------------------------------------------------------------------
// <copyright file="ChunkingOptions.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    /// <summary>
    /// Bundles the two chunking parameters that always travel together
    /// through the processing pipeline.
    /// </summary>
    public sealed class ChunkingOptions
    {
        /// <summary>
        /// Gets or sets the number of lines per API call chunk.
        /// </summary>
        public int ChunkSize { get; set; } = 40;

        /// <summary>
        /// Gets or sets the maximum number of concurrent API calls (semaphore size).
        /// </summary>
        public int MaxConcurrentChunks { get; set; } = 3;
    }
}