//-----------------------------------------------------------------------
// <copyright file="FileSettingsProvider.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// File-based implementation of <see cref="ISettingsProvider"/> that reads
    /// default settings from <see cref="IConfiguration"/> (appsettings.json) and
    /// merges per-user overrides from
    /// <c>%APPDATA%\AIFileProcessor\{userId}\settings.json</c>.
    /// API keys are always sourced from machine-level <see cref="IConfiguration"/>
    /// (environment variables or appsettings.json) and are never stored in or
    /// read from per-user files. Per-user files contain only non-secret preferences.
    /// Suitable for WinForms single-user and Terminal Server multi-user deployments.
    /// </summary>
    public class FileSettingsProvider : ISettingsProvider
    {
        private const string AppFolderName = "AIFileProcessor";
        private const string SettingsFileName = "settings.json";

        private readonly JsonSerializerOptions _readOptions;
        private readonly JsonSerializerOptions _writeOptions;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSettingsProvider"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration providing default settings.</param>
        /// <param name="processingLimits">The hot-reloadable processing limits.</param>
        public FileSettingsProvider(IConfiguration configuration, ProcessingLimits processingLimits)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ArgumentNullException.ThrowIfNull(processingLimits);

            _readOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                MaxDepth = processingLimits.MaxJsonDepth,
            };

            _writeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                MaxDepth = processingLimits.MaxJsonDepth,
            };
        }

        /// <inheritdoc/>
        public Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var settings = new GeminiSettings();
            _configuration.GetSection("GeminiSettings").Bind(settings);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["GeminiSettings"] is JsonObject geminiOverride)
            {
                string json = geminiOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<GeminiSettings>(json, _readOptions);

                // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                overrideSettings?.CopyOnto(settings);
            }

            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task<OpenAiSettings> GetOpenAiSettingsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var settings = new OpenAiSettings();
            _configuration.GetSection("OpenAiSettings").Bind(settings);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["OpenAiSettings"] is JsonObject openAiOverride)
            {
                string json = openAiOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OpenAiSettings>(json, _readOptions);

                // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                overrideSettings?.CopyOnto(settings);
            }

            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task<DeepSeekSettings> GetDeepSeekSettingsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var settings = new DeepSeekSettings();
            _configuration.GetSection("DeepSeekSettings").Bind(settings);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["DeepSeekSettings"] is JsonObject deepSeekOverride)
            {
                string json = deepSeekOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<DeepSeekSettings>(json, _readOptions);

                // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                overrideSettings?.CopyOnto(settings);
            }

            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task<OllamaSettings> GetOllamaSettingsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var settings = new OllamaSettings();
            _configuration.GetSection("OllamaSettings").Bind(settings);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["OllamaSettings"] is JsonObject ollamaOverride)
            {
                string json = ollamaOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OllamaSettings>(json, _readOptions);

                // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                overrideSettings?.CopyOnto(settings);
            }

            return Task.FromResult(settings);
        }

        /// <inheritdoc/>
        public Task SaveGeminiSettingsAsync(string userId, GeminiSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveSection(userId, "GeminiSettings", settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOpenAiSettingsAsync(string userId, OpenAiSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveSection(userId, "OpenAiSettings", settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveDeepSeekSettingsAsync(string userId, DeepSeekSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveSection(userId, "DeepSeekSettings", settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOllamaSettingsAsync(string userId, OllamaSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveSection(userId, "OllamaSettings", settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<ProcessingOptions> GetProcessingOptionsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["ProcessingOptions"] is JsonObject optionsNode)
            {
                string json = optionsNode.ToJsonString();
                var options = JsonSerializer.Deserialize<ProcessingOptions>(json, _readOptions);
                if (options is not null)
                {
                    return Task.FromResult(options);
                }
            }

            return Task.FromResult(new ProcessingOptions());
        }

        /// <inheritdoc/>
        public Task SaveProcessingOptionsAsync(string userId, ProcessingOptions options, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(options);

            SaveSection(userId, "ProcessingOptions", options);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<GeminiModelSettings> GetGeminiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new GeminiModelSettings();
            var processing = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            // Start with provider-level defaults
            _configuration.GetSection("GeminiSettings").Bind(modelSettings);

            // Overlay per-model defaults from appsettings.json
            string modelSection = $"GeminiModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            // Clamp to global caps
            var caps = processing;
            if (modelSettings.ChunkSize.HasValue)
            {
                modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize.Value, caps.ChunkSizeCap));
            }

            modelSettings.ChunkSize ??= caps.ChunkSizeDefault;

            if (modelSettings.MaxConcurrentChunks.HasValue)
            {
                modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks.Value, caps.MaxConcurrentChunksCap));
            }

            modelSettings.MaxConcurrentChunks ??= caps.MaxConcurrentChunksDefault;

            // Migrate legacy "GeminiSettings" user override into "GeminiModels:{modelId}" if needed.
            MigrateLegacyProviderToModelSettings(
                userId,
                "GeminiSettings",
                "GeminiModels",
                modelId,
                () =>
                {
                    var migrated = new GeminiSettings();
                    _configuration.GetSection("GeminiSettings").Bind(migrated);

                    JsonObject? legacyRoot = ReadUserOverridesInternal(userId);
                    if (legacyRoot is not null && legacyRoot["GeminiSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<GeminiSettings>(legacyJson, _readOptions);
                        legacySettings?.CopyOnto(migrated);
                    }

                    return new GeminiModelSettings
                    {
                        // Endpoint/ProxyUrl/RequestHeaders/AdditionalParameters are machine-level
                        // only (SEC-010): never seeded into per-user files.
                        ModelName = migrated.ModelName ?? modelId,
                        TimeoutSeconds = migrated.TimeoutSeconds,
                        ProjectId = migrated.ProjectId,
                        Location = migrated.Location,
                        MaxTokens = migrated.MaxTokens,
                        Temperature = migrated.Temperature,
                        TopP = migrated.TopP,
                        StreamingEnabled = migrated.StreamingEnabled,
                        MaxRetries = migrated.MaxRetries,
                        BaseDelay = migrated.BaseDelay,
                        MaxDelay = migrated.MaxDelay,
                        EnableLogging = migrated.EnableLogging,
                        RateLimitPerMinute = migrated.RateLimitPerMinute,
                        UseManagedIdentity = migrated.UseManagedIdentity,
                        CredentialType = migrated.CredentialType,
                        ReasoningLevel = migrated.ReasoningLevel,
                        ChunkSize = Math.Max(1, Math.Min(caps.ChunkSizeDefault, caps.ChunkSizeCap)),
                        MaxConcurrentChunks = Math.Max(1, Math.Min(caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap)),
                    };
                });

            // Apply per-user overrides (same clamping)
            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["GeminiModels"] is JsonObject geminiModels
                && geminiModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<GeminiModelSettings>(json, _readOptions);
                if (overrideSettings is not null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public async Task<OpenAiModelSettings> GetOpenAiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new OpenAiModelSettings();
            var processing = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            // Start with provider-level defaults
            _configuration.GetSection("OpenAiSettings").Bind(modelSettings);

            // Overlay per-model defaults from appsettings.json
            string modelSection = $"OpenAiModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            // Clamp to global caps
            var caps = processing;
            if (modelSettings.ChunkSize.HasValue)
            {
                modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize.Value, caps.ChunkSizeCap));
            }

            modelSettings.ChunkSize ??= caps.ChunkSizeDefault;

            if (modelSettings.MaxConcurrentChunks.HasValue)
            {
                modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks.Value, caps.MaxConcurrentChunksCap));
            }

            modelSettings.MaxConcurrentChunks ??= caps.MaxConcurrentChunksDefault;

            // Migrate legacy "OpenAiSettings" user override into "OpenAiModels:{modelId}" if needed.
            MigrateLegacyProviderToModelSettings(
                userId,
                "OpenAiSettings",
                "OpenAiModels",
                modelId,
                () =>
                {
                    var migrated = new OpenAiSettings();
                    _configuration.GetSection("OpenAiSettings").Bind(migrated);

                    JsonObject? legacyRoot = ReadUserOverridesInternal(userId);
                    if (legacyRoot is not null && legacyRoot["OpenAiSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<OpenAiSettings>(legacyJson, _readOptions);
                        legacySettings?.CopyOnto(migrated);
                    }

                    return new OpenAiModelSettings
                    {
                        // Endpoint/RequestHeaders are machine-level only (SEC-010):
                        // never seeded into per-user files.
                        ModelName = migrated.ModelName ?? modelId,
                        TimeoutSeconds = migrated.TimeoutSeconds,
                        OrganizationId = migrated.OrganizationId,
                        MaxTokens = migrated.MaxTokens,
                        MaxRetries = migrated.MaxRetries,
                        BaseDelay = migrated.BaseDelay,
                        MaxDelay = migrated.MaxDelay,
                        Temperature = migrated.Temperature,
                        ReasoningLevel = migrated.ReasoningLevel,
                        ChunkSize = Math.Max(1, Math.Min(caps.ChunkSizeDefault, caps.ChunkSizeCap)),
                        MaxConcurrentChunks = Math.Max(1, Math.Min(caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap)),
                    };
                });

            // Apply per-user overrides (same clamping)
            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["OpenAiModels"] is JsonObject openAiModels
                && openAiModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OpenAiModelSettings>(json, _readOptions);
                if (overrideSettings is not null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public async Task<DeepSeekModelSettings> GetDeepSeekModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new DeepSeekModelSettings();
            var processing = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            // Start with provider-level defaults
            _configuration.GetSection("DeepSeekSettings").Bind(modelSettings);

            // Overlay per-model defaults from appsettings.json
            string modelSection = $"DeepSeekModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            // Clamp to global caps
            var caps = processing;
            if (modelSettings.ChunkSize.HasValue)
            {
                modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize.Value, caps.ChunkSizeCap));
            }

            modelSettings.ChunkSize ??= caps.ChunkSizeDefault;

            if (modelSettings.MaxConcurrentChunks.HasValue)
            {
                modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks.Value, caps.MaxConcurrentChunksCap));
            }

            modelSettings.MaxConcurrentChunks ??= caps.MaxConcurrentChunksDefault;

            // Migrate legacy "DeepSeekSettings" user override into "DeepSeekModels:{modelId}" if needed.
            MigrateLegacyProviderToModelSettings(
                userId,
                "DeepSeekSettings",
                "DeepSeekModels",
                modelId,
                () =>
                {
                    var migrated = new DeepSeekSettings();
                    _configuration.GetSection("DeepSeekSettings").Bind(migrated);

                    JsonObject? legacyRoot = ReadUserOverridesInternal(userId);
                    if (legacyRoot is not null && legacyRoot["DeepSeekSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<DeepSeekSettings>(legacyJson, _readOptions);
                        legacySettings?.CopyOnto(migrated);
                    }

                    return new DeepSeekModelSettings
                    {
                        // Endpoint/RequestHeaders are machine-level only (SEC-010):
                        // never seeded into per-user files.
                        ModelName = migrated.ModelName ?? modelId,
                        TimeoutSeconds = migrated.TimeoutSeconds,
                        MaxTokens = migrated.MaxTokens,
                        MaxRetries = migrated.MaxRetries,
                        BaseDelay = migrated.BaseDelay,
                        MaxDelay = migrated.MaxDelay,
                        Temperature = migrated.Temperature,
                        EnableReasoning = migrated.EnableReasoning,
                        ReasoningLevel = migrated.ReasoningLevel,
                        ChunkSize = Math.Max(1, Math.Min(caps.ChunkSizeDefault, caps.ChunkSizeCap)),
                        MaxConcurrentChunks = Math.Max(1, Math.Min(caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap)),
                    };
                });

            // Apply per-user overrides (same clamping)
            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["DeepSeekModels"] is JsonObject deepSeekModels
                && deepSeekModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<DeepSeekModelSettings>(json, _readOptions);
                if (overrideSettings is not null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public async Task<OllamaModelSettings> GetOllamaModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new OllamaModelSettings();
            var processing = await GetProcessingSettingsAsync(ct).ConfigureAwait(false);

            // Start with provider-level defaults
            _configuration.GetSection("OllamaSettings").Bind(modelSettings);

            // Overlay per-model defaults from appsettings.json
            string modelSection = $"OllamaModels:{modelId}";
            if (_configuration.GetSection(modelSection).Exists())
            {
                _configuration.GetSection(modelSection).Bind(modelSettings);
            }

            // Clamp to global caps
            var caps = processing;
            if (modelSettings.ChunkSize.HasValue)
            {
                modelSettings.ChunkSize = Math.Max(1, Math.Min(modelSettings.ChunkSize.Value, caps.ChunkSizeCap));
            }

            modelSettings.ChunkSize ??= caps.ChunkSizeDefault;

            if (modelSettings.MaxConcurrentChunks.HasValue)
            {
                modelSettings.MaxConcurrentChunks = Math.Max(1, Math.Min(modelSettings.MaxConcurrentChunks.Value, caps.MaxConcurrentChunksCap));
            }

            modelSettings.MaxConcurrentChunks ??= caps.MaxConcurrentChunksDefault;

            // Apply per-user overrides (same clamping)
            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides is not null && userOverrides["OllamaModels"] is JsonObject ollamaModels
                && ollamaModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OllamaModelSettings>(json, _readOptions);
                if (overrideSettings is not null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return modelSettings;
        }

        /// <inheritdoc/>
        public Task SaveGeminiModelSettingsAsync(string modelId, string userId, GeminiModelSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveModelSection(userId, "GeminiModels", modelId, settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOpenAiModelSettingsAsync(string modelId, string userId, OpenAiModelSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveModelSection(userId, "OpenAiModels", modelId, settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveDeepSeekModelSettingsAsync(string modelId, string userId, DeepSeekModelSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveModelSection(userId, "DeepSeekModels", modelId, settings);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveOllamaModelSettingsAsync(string modelId, string userId, OllamaModelSettings settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);
            ArgumentNullException.ThrowIfNull(settings);

            SaveModelSection(userId, "OllamaModels", modelId, settings);
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
        /// Returns the directory path for the specified user's settings.
        /// The userId is sanitized to prevent path traversal: path separators,
        /// parent-directory sequences, and invalid filename characters are stripped.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The full path to the user's settings directory.</returns>
        private static string GetUserSettingsDirectory(string userId)
        {
            string safeUserId = SanitizeUserId(userId);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, AppFolderName, safeUserId);
        }

        /// <summary>
        /// Returns the full path to the specified user's settings file.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The full path to the user's settings JSON file.</returns>
        private static string GetUserSettingsFilePath(string userId)
        {
            return Path.Combine(GetUserSettingsDirectory(userId), SettingsFileName);
        }

        /// <summary>
        /// Sanitizes a user identifier for safe use as a directory name. Strips
        /// path separators, parent-directory sequences, and invalid filename
        /// characters to prevent path traversal attacks.
        /// </summary>
        /// <param name="userId">The raw user identifier.</param>
        /// <returns>A sanitized string safe for use as a directory name.</returns>
        private static string SanitizeUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return "unknown";
            }

            // Strip path separators and parent-directory sequences.
            string sanitized = userId;
            sanitized = sanitized.Replace("..", string.Empty, StringComparison.Ordinal);
            sanitized = sanitized.Replace('/', '_');
            sanitized = sanitized.Replace('\\', '_');

            // Strip any remaining invalid filename characters.
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }

            // Trim and truncate to a reasonable length.
            sanitized = sanitized.Trim();
            if (sanitized.Length > 128)
            {
                sanitized = sanitized[..128];
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
        }

        /// <summary>
        /// Reads the per-user override file if it exists, returning the root
        /// JSON object or <c>null</c> if the file is not present. Acquires a
        /// cross-process lock to prevent reading a partially written file.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The parsed root <see cref="JsonObject"/>, or <c>null</c>.</returns>
        private static JsonObject? ReadUserOverrides(string userId)
        {
            return WithUserFileLock(userId, () => ReadUserOverridesInternal(userId));
        }

        /// <summary>
        /// Unlocked read of the per-user override file. Callers already
        /// holding the user file lock (via <see cref="WithUserFileLock"/>)
        /// should call this to avoid re-entrant deadlock.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The parsed root <see cref="JsonObject"/>, or <c>null</c>.</returns>
        private static JsonObject? ReadUserOverridesInternal(string userId)
        {
            string filePath = GetUserSettingsFilePath(userId);
            if (!File.Exists(filePath))
            {
                return null;
            }

            string jsonContent = File.ReadAllText(filePath);
            return JsonNode.Parse(jsonContent) as JsonObject;
        }

        /// <summary>
        /// Removes machine-level infrastructure properties (<c>Endpoint</c>, <c>ProxyUrl</c>,
        /// <c>RequestHeaders</c>, <c>AdditionalParameters</c>) from a serialized settings node
        /// before it is written to a per-user file. Per-user files must contain only
        /// non-secret user preferences (SET-007, SEC-010): these values are machine-level
        /// and may include gateway credentials, so they must never be persisted where
        /// users can read or write them. Both the camelCase keys produced by the
        /// <see cref="JsonNamingPolicy.CamelCase"/> write options and the raw PascalCase
        /// property names are removed.
        /// </summary>
        /// <param name="sectionNode">The serialized settings node to strip.</param>
        private static void StripInfraProperties(JsonObject? sectionNode)
        {
            if (sectionNode is null)
            {
                return;
            }

            sectionNode.Remove("endpoint");
            sectionNode.Remove("proxyUrl");
            sectionNode.Remove("requestHeaders");
            sectionNode.Remove("additionalParameters");

            sectionNode.Remove("Endpoint");
            sectionNode.Remove("ProxyUrl");
            sectionNode.Remove("RequestHeaders");
            sectionNode.Remove("AdditionalParameters");
        }

        /// <summary>
        /// Executes an operation while holding a cross-process mutex that
        /// serializes access to the per-user settings file. On Terminal Server,
        /// two RDP sessions for the same user share the same settings file;
        /// this mutex prevents concurrent read-modify-write races that could
        /// corrupt the file or silently lose changes.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="operation">The operation to execute under the lock.</param>
        private static T WithUserFileLock<T>(string userId, Func<T> operation)
        {
            string mutexName = $"Global\\AIFileProcessor_{SanitizeUserId(userId)}_Settings";
            using var mutex = new Mutex(false, mutexName);
            try
            {
                mutex.WaitOne();
                return operation();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Executes an action while holding the cross-process user file lock.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="action">The action to execute under the lock.</param>
        private static void WithUserFileLock(string userId, Action action)
        {
            string mutexName = $"Global\\AIFileProcessor_{SanitizeUserId(userId)}_Settings";
            using var mutex = new Mutex(false, mutexName);
            try
            {
                mutex.WaitOne();
                action();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Saves a settings section to the per-user override file. The
        /// <c>ApiKey</c> field is always stripped before saving — per-user
        /// files contain only non-secret preferences. API keys are sourced
        /// from machine-level <see cref="IConfiguration"/> at read time.
        /// Infrastructure settings (<c>Endpoint</c>, <c>ProxyUrl</c>, <c>RequestHeaders</c>,
        /// <c>AdditionalParameters</c>) are likewise stripped: they are machine-level
        /// values (SEC-010, ADR-0006) and may contain gateway credentials that must
        /// not persist to user-writable files.
        /// </summary>
        /// <typeparam name="T">The settings type to serialize.</typeparam>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sectionName">The JSON section name (e.g., "GeminiSettings").</param>
        /// <param name="settings">The settings instance to save.</param>
        private void SaveSection<T>(string userId, string sectionName, T settings)
            where T : class
        {
            WithUserFileLock(userId, () =>
            {
                string filePath = GetUserSettingsFilePath(userId);
                string? directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                JsonObject? rootNode = ReadUserOverridesInternal(userId) ?? [];

                string sectionJson = JsonSerializer.Serialize(settings, _writeOptions);
                var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;
                StripInfraProperties(sectionNode);

                // ApiKey is [JsonIgnore] on the settings models, so it is never
                // present in the serialized JSON — no manual stripping needed.
                rootNode[sectionName] = sectionNode;

                File.WriteAllText(filePath, rootNode.ToJsonString(_writeOptions));
            });
        }

        /// <summary>
        /// Migrates a legacy provider-level settings section in the per-user file into
        /// the new per-model section for the requested model, if the user has an old
        /// <paramref name="legacySectionName"/> entry and no existing per-model override
        /// for <paramref name="modelId"/>. This preserves prior user customizations by
        /// seeding the per-model override from the user's previous provider-level settings.
        /// Migration is idempotent: nothing happens when the model already has an override
        /// or when the legacy section is absent.
        /// </summary>
        /// <typeparam name="TModel">The concrete model settings type (inherits provider type).</typeparam>
        /// <param name="userId">The user identifier.</param>
        /// <param name="legacySectionName">The legacy provider section name (e.g., "GeminiSettings").</param>
        /// <param name="modelsSectionName">The per-model section name (e.g., "GeminiModels").</param>
        /// <param name="modelId">The model identifier to seed.</param>
        /// <param name="seed">A factory that produces the model settings from the legacy section data and caps.</param>
        private void MigrateLegacyProviderToModelSettings<TModel>(
            string userId,
            string legacySectionName,
            string modelsSectionName,
            string modelId,
            Func<TModel> seed)
            where TModel : class
        {
            WithUserFileLock(userId, () =>
            {
                JsonObject? root = ReadUserOverridesInternal(userId);
                if (root == null)
                {
                    return; // no user file => nothing to migrate
                }

                // Skip if this model already has a per-model override.
                if (root[modelsSectionName] is JsonObject existingModels
                    && existingModels.ContainsKey(modelId))
                {
                    return;
                }

                // Skip if there's no legacy provider section to migrate.
                if (root[legacySectionName] is not JsonObject legacySection)
                {
                    return;
                }

                // Seed the model override from the legacy data (preserving user's prior
                // provider-level preferences) and write it under modelsSectionName[modelId].
                root[modelsSectionName] ??= new JsonObject();
                if (root[modelsSectionName] is JsonObject modelsSection)
                {
                    TModel modelSettings;
                    try
                    {
                        modelSettings = seed();
                    }
                    catch
                    {
                        return; // malformed legacy data: skip migration rather than corrupt the file
                    }

                    string sectionJson = JsonSerializer.Serialize(modelSettings, _writeOptions);
                    var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;
                    modelsSection[modelId] = sectionNode;

                    string filePath = GetUserSettingsFilePath(userId);
                    string? directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(filePath, root.ToJsonString(_writeOptions));
                }
            });
        }

        private void SaveModelSection<T>(string userId, string modelsSectionName, string modelId, T settings)
            where T : class
        {
            WithUserFileLock(userId, () =>
            {
                string filePath = GetUserSettingsFilePath(userId);
                string? directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                JsonObject? rootNode = ReadUserOverridesInternal(userId) ?? [];

                // Ensure the models section exists
                rootNode[modelsSectionName] ??= new JsonObject();
                if (rootNode[modelsSectionName] is JsonObject modelsSection)
                {
                    string sectionJson = JsonSerializer.Serialize(settings, _writeOptions);
                    var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;
                    StripInfraProperties(sectionNode);
                    modelsSection[modelId] = sectionNode;
                }

                File.WriteAllText(filePath, rootNode.ToJsonString(_writeOptions));
            });
        }
    }
}