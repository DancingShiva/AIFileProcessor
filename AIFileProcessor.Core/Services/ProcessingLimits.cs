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
        private static int _maxOutputFieldSize = 32_768;
        private static int _retryJitterSeconds = 5;
        private static int _maxJsonDepth = 16;

        /// <summary>
        /// Gets the maximum allowed input size (in characters).
        /// </summary>
        internal static int MaxInputSize => _maxInputSize;

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
        /// Configures the global limits from the provided <see cref="ProcessingSettings"/>.
        /// Called once during application startup from
        /// <see cref="DependencyInjection.ServiceCollectionExtensions.AddAiFileProcessorCore"/>.
        /// </summary>
        /// <param name="settings">The processing settings from configuration.</param>
        internal static void Configure(ProcessingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _maxInputSize = settings.MaxInputSize > 0 ? settings.MaxInputSize : 32_768;
            _maxOutputFieldSize = settings.MaxOutputFieldSize > 0 ? settings.MaxOutputFieldSize : 32_768;
            _retryJitterSeconds = settings.RetryJitterSeconds >= 0 ? settings.RetryJitterSeconds : 5;
            _maxJsonDepth = settings.MaxJsonDepth > 0 ? settings.MaxJsonDepth : 16;
        }
    }
}