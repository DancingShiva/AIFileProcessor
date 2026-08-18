//-----------------------------------------------------------------------
// <copyright file="ProcessingLimits.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using AIFileProcessor.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Provides hot-reloadable processing limits configured from the
    /// "Processing" section of appsettings.json via <see cref="IOptionsMonitor{T}"/>.
    /// Values are read from <see cref="IOptionsMonitor{T}.CurrentValue"/> on every
    /// access, so changes to appsettings.json take effect without a restart.
    /// </summary>
    public class ProcessingLimits
    {
        private readonly IOptionsMonitor<ProcessingSettings> _settingsMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingLimits"/> class.
        /// </summary>
        /// <param name="settingsMonitor">The options monitor for processing settings.</param>
        public ProcessingLimits(IOptionsMonitor<ProcessingSettings> settingsMonitor)
        {
            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        }

        /// <summary>
        /// Gets the maximum allowed input size (in characters).
        /// </summary>
        public int MaxInputSize => Settings.MaxInputSize > 0 ? Settings.MaxInputSize : 32_768;

        /// <summary>
        /// Gets the maximum allowed input line count.
        /// </summary>
        public int MaxInputLines => Settings.MaxInputLines > 0 ? Settings.MaxInputLines : 400;

        /// <summary>
        /// Gets the maximum allowed output field size (in characters).
        /// </summary>
        public int MaxOutputFieldSize => Settings.MaxOutputFieldSize > 0 ? Settings.MaxOutputFieldSize : 32_768;

        /// <summary>
        /// Gets the maximum retry jitter (in seconds).
        /// </summary>
        public int RetryJitterSeconds => Settings.RetryJitterSeconds >= 0 ? Settings.RetryJitterSeconds : 5;

        /// <summary>
        /// Gets the maximum JSON depth for all JsonSerializerOptions instances.
        /// </summary>
        public int MaxJsonDepth => Settings.MaxJsonDepth > 0 ? Settings.MaxJsonDepth : 16;

        /// <summary>
        /// Gets the maximum number of chunk-level retries after the initial attempt.
        /// </summary>
        public int ChunkMaxRetries => Settings.ChunkMaxRetries >= 0 ? Settings.ChunkMaxRetries : 3;

        /// <summary>
        /// Gets the base delay (seconds) for chunk-level retry backoff.
        /// </summary>
        public int ChunkBaseDelaySeconds => Settings.ChunkBaseDelaySeconds > 0 ? Settings.ChunkBaseDelaySeconds : 5;

        /// <summary>
        /// Gets the maximum delay (seconds) between chunk-level retries.
        /// </summary>
        public int ChunkMaxDelaySeconds => Settings.ChunkMaxDelaySeconds > 0 ? Settings.ChunkMaxDelaySeconds : 30;

        /// <summary>
        /// Gets the current processing settings from the options monitor.
        /// </summary>
        private ProcessingSettings Settings => _settingsMonitor.CurrentValue;
    }
}