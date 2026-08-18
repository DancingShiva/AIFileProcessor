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

        /// <summary>
        /// Gets or sets the maximum allowed size (in characters) of the input data
        /// sent for processing. Input exceeding this is rejected before any API
        /// calls are made. Default is 32,768.
        /// </summary>
        public int MaxInputSize { get; set; } = 32_768;

        /// <summary>
        /// Gets or sets the maximum allowed number of lines in the input data
        /// sent for processing. Input exceeding this is rejected before any API
        /// calls are made. Default is 400.
        /// </summary>
        public int MaxInputLines { get; set; } = 400;

        /// <summary>
        /// Gets or sets the maximum allowed size (in characters) of any single
        /// output field returned by the AI model. Fields exceeding this are
        /// flagged as violations by the ResponseVerifier. Default is 32,768.
        /// </summary>
        public int MaxOutputFieldSize { get; set; } = 32_768;

        /// <summary>
        /// Gets or sets the maximum jitter (in seconds) added to each retry delay
        /// to prevent thundering-herd retry storms. The actual jitter is a random
        /// value between 0 and this setting. Default is 5 seconds.
        /// </summary>
        public int RetryJitterSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum JSON serialization/deserialization depth for
        /// all JsonSerializerOptions instances in the application. Default is 16.
        /// </summary>
        public int MaxJsonDepth { get; set; } = 16;

        /// <summary>
        /// Gets or sets the maximum number of chunk-level retries after the initial
        /// attempt for a failed chunk. This is a second retry layer on top of the
        /// per-provider service-level retry. Default is 3.
        /// </summary>
        public int ChunkMaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base delay, in seconds, before the first chunk-level retry;
        /// doubled each subsequent retry. Default is 5.
        /// </summary>
        public int ChunkBaseDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum delay, in seconds, between chunk-level retries.
        /// Default is 30.
        /// </summary>
        public int ChunkMaxDelaySeconds { get; set; } = 30;
    }
}
