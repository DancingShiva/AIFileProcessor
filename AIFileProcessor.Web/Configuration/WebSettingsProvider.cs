//-----------------------------------------------------------------------
// <copyright file="WebSettingsProvider.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Configuration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Web-specific implementation of <see cref="ISettingsProvider"/> that
    /// reconstructs per-user settings from the HTTP request. API keys are
    /// supplied per-request via headers (never stored server-side), and
    /// non-secret preferences are supplied via the request body.
    /// Defaults are read from <see cref="IConfiguration"/> (appsettings.json).
    /// </summary>
    public class WebSettingsProvider : ISettingsProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSettingsProvider"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration providing default settings.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for obtaining per-request data.</param>
        public WebSettingsProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct = default)
        {
            var settings = new GeminiSettings();
            _configuration.GetSection("GeminiSettings").Bind(settings);

            ApplyRequestOverrides(settings);
            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task<OpenAiSettings> GetOpenAiSettingsAsync(string userId, CancellationToken ct = default)
        {
            var settings = new OpenAiSettings();
            _configuration.GetSection("OpenAiSettings").Bind(settings);

            ApplyRequestOverrides(settings);
            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task<DeepSeekSettings> GetDeepSeekSettingsAsync(string userId, CancellationToken ct = default)
        {
            var settings = new DeepSeekSettings();
            _configuration.GetSection("DeepSeekSettings").Bind(settings);

            ApplyRequestOverrides(settings);
            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task SaveGeminiSettingsAsync(string userId, GeminiSettings settings, CancellationToken ct = default)
        {
            // Web mode: settings are client-side (localStorage), not persisted server-side.
            // This is a no-op. The client is responsible for storing non-secret preferences.
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOpenAiSettingsAsync(string userId, OpenAiSettings settings, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveDeepSeekSettingsAsync(string userId, DeepSeekSettings settings, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        private static string? GetApiKeyFromHeader(HttpContext ctx, string headerName)
        {
            if (ctx.Request.Headers.TryGetValue(headerName, out var value))
            {
                string key = value.ToString();
                return string.IsNullOrWhiteSpace(key) ? null : key;
            }

            return null;
        }

        /// <summary>
        /// Overrides settings with values from the current HTTP request:
        /// API keys from headers, preferences from the request items collection.
        /// </summary>
        private void ApplyRequestOverrides(GeminiSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            // API key from header
            string? apiKey = GetApiKeyFromHeader(ctx, "X-Gemini-Api-Key");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings.ApiKey = apiKey;
            }

            // Non-secret preferences from request items (set by controller from request body)
            if (ctx.Items.TryGetValue("GeminiPreferences", out var prefsObj) && prefsObj is GeminiSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }

        /// <summary>
        /// Overrides settings with values from the current HTTP request.
        /// </summary>
        private void ApplyRequestOverrides(OpenAiSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            string? apiKey = GetApiKeyFromHeader(ctx, "X-OpenAi-Api-Key");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings.ApiKey = apiKey;
            }

            if (ctx.Items.TryGetValue("OpenAiPreferences", out var prefsObj) && prefsObj is OpenAiSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }

        /// <summary>
        /// Overrides settings with values from the current HTTP request.
        /// </summary>
        private void ApplyRequestOverrides(DeepSeekSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            string? apiKey = GetApiKeyFromHeader(ctx, "X-DeepSeek-Api-Key");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings.ApiKey = apiKey;
            }

            if (ctx.Items.TryGetValue("DeepSeekPreferences", out var prefsObj) && prefsObj is DeepSeekSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }
    }
}