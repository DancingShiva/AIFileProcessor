//-----------------------------------------------------------------------
// <copyright file="ProcessingLimits.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using AIFileProcessor.Configuration;

    /// <summary>
    /// Static holder for global processing limits configured from the
    /// "Processing" section of appsettings.json. Set once during startup
    /// via <see cref="Configure(ProcessingSettings)"/>. Consistent with
    /// <see cref="ModelTokenLimits"/> and <see cref="AiModelCatalog"/>.
    /// </summary>
    internal static class ProcessingLimits
    {
        private static int _maxInputSize = 32_768;
        private static int _maxInputLines = 400;
        private static int _maxOutputFieldSize = 32_768;
        private static int _retryJitterSeconds = 5;
        private static int _maxJsonDepth = 16;
        private static int _chunkMaxRetries = 3;
        private static int _chunkBaseDelaySeconds = 5;
        private static int _chunkMaxDelaySeconds = 30;

        /// <summary>
        /// Gets the maximum allowed input size (in characters).
        /// </summary>
        internal static int MaxInputSize => _maxInputSize;

        /// <summary>
        /// Gets the maximum allowed input line count.
        /// </summary>
        internal static int MaxInputLines => _maxInputLines;

        /// <summary>
        /// Gets the maximum allowed output field size (in characters).
        /// </summary>
        internal static int MaxOutputFieldSize => _maxOutputFieldSize;

        /// <summary>
        /// Gets the maximum retry jitter (in seconds).
        /// </summary>
        internal static int RetryJitterSeconds => _retryJitterSeconds;

        /// <summary>
        /// Gets the maximum JSON depth for all JsonSerializerOptions instances.
        /// </summary>
        internal static int MaxJsonDepth => _maxJsonDepth;

        /// <summary>
        /// Gets the maximum number of chunk-level retries after the initial attempt.
        /// </summary>
        internal static int ChunkMaxRetries => _chunkMaxRetries;

        /// <summary>
        /// Gets the base delay (seconds) for chunk-level retry backoff.
        /// </summary>
        internal static int ChunkBaseDelaySeconds => _chunkBaseDelaySeconds;

        /// <summary>
        /// Gets the maximum delay (seconds) between chunk-level retries.
        /// </summary>
        internal static int ChunkMaxDelaySeconds => _chunkMaxDelaySeconds;

        /// <summary>
        /// Configures the global limits from the provided <see cref="ProcessingSettings"/>.
        /// Called once during application startup from
        /// <see cref="DependencyInjection.ServiceCollectionExtensions.AddAiFileProcessorCore"/>.
        /// </summary>
        /// <param name="settings">The processing settings from configuration.</param>
        internal static void Configure(ProcessingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _maxInputSize = settings.MaxInputSize > 0 ? settings.MaxInputSize : 32_768;
            _maxInputLines = settings.MaxInputLines > 0 ? settings.MaxInputLines : 400;
            _maxOutputFieldSize = settings.MaxOutputFieldSize > 0 ? settings.MaxOutputFieldSize : 32_768;
            _retryJitterSeconds = settings.RetryJitterSeconds >= 0 ? settings.RetryJitterSeconds : 5;
            _maxJsonDepth = settings.MaxJsonDepth > 0 ? settings.MaxJsonDepth : 16;
            _chunkMaxRetries = settings.ChunkMaxRetries >= 0 ? settings.ChunkMaxRetries : 3;
            _chunkBaseDelaySeconds = settings.ChunkBaseDelaySeconds > 0 ? settings.ChunkBaseDelaySeconds : 5;
            _chunkMaxDelaySeconds = settings.ChunkMaxDelaySeconds > 0 ? settings.ChunkMaxDelaySeconds : 30;
        }
    }
}