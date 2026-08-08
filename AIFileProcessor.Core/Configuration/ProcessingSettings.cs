//-----------------------------------------------------------------------
// <copyright file="ProcessingSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AIFileProcessor.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Global, admin-configured processing defaults and hard caps for chunking
    /// and concurrency. Caps always win over user or model settings.
    /// </summary>
    public class ProcessingSettings
    {
        /// <summary>Gets or sets the default number of lines per chunk when no per-model setting exists.</summary>
        public int ChunkSizeDefault { get; set; } = 40;

        /// <summary>Gets or sets the default number of concurrent chunk API calls when no per-model setting exists.</summary>
        public int MaxConcurrentChunksDefault { get; set; } = 3;

        /// <summary>Gets or sets the hard upper limit for chunk size; no per-model or per-user value can exceed this.</summary>
        public int ChunkSizeCap { get; set; } = 40;

        /// <summary>Gets or sets the hard upper limit for concurrent chunks; no per-model or per-user value can exceed this.</summary>
        public int MaxConcurrentChunksCap { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default hard maximum token count when a model is not found in
        /// <see cref="ModelTokenLimitsMap"/>.
        /// </summary>
        public int DefaultTokenLimit { get; set; } = 262144;

        /// <summary>
        /// Gets or sets a dictionary mapping model IDs (e.g., "gemini-3-flash", "gpt-5.6-sol")
        /// to their hard maximum token count. Keys are matched case-insensitively.
        /// </summary>
        public Dictionary<string, int> ModelTokenLimitsMap { get; set; } = new();
    }
}
