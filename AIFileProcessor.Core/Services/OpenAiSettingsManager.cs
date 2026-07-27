//-----------------------------------------------------------------------
// <copyright file="OpenAiSettingsManager.cs" company="Midnight Mission Systems">
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
    /// Manages reading and saving OpenAI settings via <see cref="ISettingsProvider"/>,
    /// delegating to per-user storage rather than the shared appsettings.json.
    /// </summary>
    public class OpenAiSettingsManager
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private readonly Microsoft.Extensions.Logging.ILogger<OpenAiSettingsManager>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiSettingsManager"/> class.
        /// </summary>
        /// <param name="settingsProvider">The settings provider for per-user settings storage.</param>
        /// <param name="userContext">The current user context.</param>
        /// <param name="logger">An optional logger for diagnostic messages.</param>
        public OpenAiSettingsManager(ISettingsProvider settingsProvider, IUserContext userContext, Microsoft.Extensions.Logging.ILogger<OpenAiSettingsManager>? logger = null)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger;
        }

        /// <summary>
        /// Reads the current OpenAI settings for the current user.
        /// </summary>
        /// <returns>The populated <see cref="OpenAiSettings"/>.</returns>
        public OpenAiSettings ReadSettings()
        {
            var settings = _settingsProvider.GetOpenAiSettingsAsync(_userContext.UserId, CancellationToken.None)
                .GetAwaiter().GetResult();

            return settings;
        }

        /// <summary>
        /// Saves the updated OpenAI settings for the current user.
        /// </summary>
        /// <param name="settings">The updated settings to save.</param>
        public void SaveSettings(OpenAiSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _settingsProvider.SaveOpenAiSettingsAsync(_userContext.UserId, settings, CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}