//-----------------------------------------------------------------------
// <copyright file="GeminiSettingsManager.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Threading;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;

    /// <summary>
    /// Manages reading and saving Gemini settings via <see cref="ISettingsProvider"/>,
    /// delegating to per-user storage rather than the shared appsettings.json.
    /// </summary>
    public class GeminiSettingsManager
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private readonly Microsoft.Extensions.Logging.ILogger<GeminiSettingsManager>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiSettingsManager"/> class.
        /// </summary>
        /// <param name="settingsProvider">The settings provider for per-user settings storage.</param>
        /// <param name="userContext">The current user context.</param>
        public GeminiSettingsManager(ISettingsProvider settingsProvider, IUserContext userContext, Microsoft.Extensions.Logging.ILogger<GeminiSettingsManager>? logger = null)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger;
        }

        /// <summary>
        /// Reads the current Gemini settings for the current user.
        /// </summary>
        /// <returns>The populated <see cref="GeminiSettings"/>.</returns>
        public GeminiSettings ReadSettings()
        {
            var settings = _settingsProvider.GetGeminiSettingsAsync(_userContext.UserId, CancellationToken.None)
                .GetAwaiter().GetResult();

            var (clamped, wasClamped) = TokenUtils.ClampRequestedMaxTokens(settings.MaxTokens, "Gemini", settings.ModelName, _logger);
            if (wasClamped)
            {
                settings.MaxTokens = clamped;
            }

            return settings;
        }

        /// <summary>
        /// Saves the updated Gemini settings for the current user.
        /// </summary>
        /// <param name="settings">The updated settings to save.</param>
        public void SaveSettings(GeminiSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            // Clamp MaxTokens before persisting
            var (clamped, wasClamped) = TokenUtils.ClampRequestedMaxTokens(settings.MaxTokens, "Gemini", settings.ModelName, _logger);
            if (wasClamped)
            {
                settings.MaxTokens = clamped;
            }

            _settingsProvider.SaveGeminiSettingsAsync(_userContext.UserId, settings, CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}