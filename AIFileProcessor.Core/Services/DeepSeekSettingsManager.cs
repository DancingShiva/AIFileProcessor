//-----------------------------------------------------------------------
// <copyright file="DeepSeekSettingsManager.cs" company="Midnight Mission Systems">
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
    /// Manages reading and saving DeepSeek settings via <see cref="ISettingsProvider"/>,
    /// delegating to per-user storage rather than the shared appsettings.json.
    /// </summary>
    public class DeepSeekSettingsManager
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepSeekSettingsManager"/> class.
        /// </summary>
        /// <param name="settingsProvider">The settings provider for per-user settings storage.</param>
        /// <param name="userContext">The current user context.</param>
        public DeepSeekSettingsManager(ISettingsProvider settingsProvider, IUserContext userContext)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Reads the current DeepSeek settings for the current user.
        /// </summary>
        /// <returns>The populated <see cref="DeepSeekSettings"/>.</returns>
        public DeepSeekSettings ReadSettings()
        {
            return _settingsProvider.GetDeepSeekSettingsAsync(_userContext.UserId, CancellationToken.None)
                .GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the updated DeepSeek settings for the current user.
        /// </summary>
        /// <param name="settings">The updated settings to save.</param>
        public void SaveSettings(DeepSeekSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settingsProvider.SaveDeepSeekSettingsAsync(_userContext.UserId, settings, CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}