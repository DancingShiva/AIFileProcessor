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
    using AIFileProcessor.Models;
    using AIFileProcessor.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Web-specific implementation of <see cref="ISettingsProvider"/> that
    /// resolves per-user settings from the HTTP request. API keys are sourced
    /// from server-side <see cref="IConfiguration"/> (environment variables)
    /// and are never received from the client. Non-secret preferences are
    /// supplied via the request body.
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

        /// <inheritdoc/>
        public Task<ProcessingOptions> GetProcessingOptionsAsync(string userId, CancellationToken ct = default)
        {
            return Task.FromResult(new ProcessingOptions());
        }

        /// <inheritdoc/>
        public Task SaveProcessingOptionsAsync(string userId, ProcessingOptions options, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<GeminiModelSettings> GetGeminiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            var modelSettings = new GeminiModelSettings();
            var caps = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            _configuration.GetSection("GeminiSettings").Bind(modelSettings);
            string modelSection = $"GeminiModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize ?? caps.ChunkSizeDefault, caps.ChunkSizeCap));
            modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks ?? caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap));

            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Items.TryGetValue("GeminiPreferences", out var prefsObj) && prefsObj is GeminiSettings prefs)
            {
                prefs.CopyOnto(modelSettings);
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public async Task<OpenAiModelSettings> GetOpenAiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            var modelSettings = new OpenAiModelSettings();
            var caps = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            _configuration.GetSection("OpenAiSettings").Bind(modelSettings);
            string modelSection = $"OpenAiModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize ?? caps.ChunkSizeDefault, caps.ChunkSizeCap));
            modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks ?? caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap));

            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Items.TryGetValue("OpenAiPreferences", out var prefsObj) && prefsObj is OpenAiSettings prefs)
            {
                prefs.CopyOnto(modelSettings);
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public async Task<DeepSeekModelSettings> GetDeepSeekModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            var modelSettings = new DeepSeekModelSettings();
            var caps = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            _configuration.GetSection("DeepSeekSettings").Bind(modelSettings);
            string modelSection = $"DeepSeekModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize ?? caps.ChunkSizeDefault, caps.ChunkSizeCap));
            modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks ?? caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap));

            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Items.TryGetValue("DeepSeekPreferences", out var prefsObj) && prefsObj is DeepSeekSettings prefs)
            {
                prefs.CopyOnto(modelSettings);
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public Task SaveGeminiModelSettingsAsync(string modelId, string userId, GeminiModelSettings settings, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOpenAiModelSettingsAsync(string modelId, string userId, OpenAiModelSettings settings, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveDeepSeekModelSettingsAsync(string modelId, string userId, DeepSeekModelSettings settings, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<ProcessingSettings> GetProcessingSettingsAsync(CancellationToken ct = default)
        {
            var settings = new ProcessingSettings();
            _configuration.GetSection("Processing").Bind(settings);
            return Task.FromResult(settings);
        }

        /// <summary>
        /// Overrides settings with non-secret preferences from the current HTTP request.
        /// API keys are never read from the request — they come from server-side
        /// <see cref="IConfiguration"/> (environment variables).
        /// </summary>
        private void ApplyRequestOverrides(GeminiSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            // Non-secret preferences from request items (set by controller from request body).
            // CopyOnto skips ApiKey, so the server-side env-var-provided key is preserved.
            if (ctx.Items.TryGetValue("GeminiPreferences", out var prefsObj) && prefsObj is GeminiSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }

        /// <summary>
        /// Overrides settings with non-secret preferences from the current HTTP request.
        /// </summary>
        private void ApplyRequestOverrides(OpenAiSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            if (ctx.Items.TryGetValue("OpenAiPreferences", out var prefsObj) && prefsObj is OpenAiSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }

        /// <summary>
        /// Overrides settings with non-secret preferences from the current HTTP request.
        /// </summary>
        private void ApplyRequestOverrides(DeepSeekSettings settings)
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }

            if (ctx.Items.TryGetValue("DeepSeekPreferences", out var prefsObj) && prefsObj is DeepSeekSettings prefs)
            {
                prefs.CopyOnto(settings);
            }
        }
    }
}