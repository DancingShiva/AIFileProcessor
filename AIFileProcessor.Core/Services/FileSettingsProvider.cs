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

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSettingsProvider"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration providing default settings.</param>
        public FileSettingsProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        public Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var settings = new GeminiSettings();
            _configuration.GetSection("GeminiSettings").Bind(settings);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides != null && userOverrides["GeminiSettings"] is JsonObject geminiOverride)
            {
                string json = geminiOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<GeminiSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                    // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                    overrideSettings.CopyOnto(settings);
                }
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
            if (userOverrides != null && userOverrides["OpenAiSettings"] is JsonObject openAiOverride)
            {
                string json = openAiOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OpenAiSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                    // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                    overrideSettings.CopyOnto(settings);
                }
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
            if (userOverrides != null && userOverrides["DeepSeekSettings"] is JsonObject deepSeekOverride)
            {
                string json = deepSeekOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<DeepSeekSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    // ApiKey is always from IConfiguration (machine-level); never from per-user file.
                    // CopyOnto skips ApiKey, so the env-var-provided key is preserved.
                    overrideSettings.CopyOnto(settings);
                }
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
        public Task<ProcessingOptions> GetProcessingOptionsAsync(string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides != null && userOverrides["ProcessingOptions"] is JsonObject optionsNode)
            {
                string json = optionsNode.ToJsonString();
                var options = JsonSerializer.Deserialize<ProcessingOptions>(json, ReadOptions);
                if (options != null)
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

        /// <summary>
        /// Returns the directory path for the specified user's settings.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The full path to the user's settings directory.</returns>
        private static string GetUserSettingsDirectory(string userId)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, AppFolderName, userId);
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
        /// Reads the per-user override file if it exists, returning the root
        /// JSON object or <c>null</c> if the file is not present.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The parsed root <see cref="JsonObject"/>, or <c>null</c>.</returns>
        private static JsonObject? ReadUserOverrides(string userId)
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
        /// Saves a settings section to the per-user override file. The
        /// <c>ApiKey</c> field is always stripped before saving — per-user
        /// files contain only non-secret preferences. API keys are sourced
        /// from machine-level <see cref="IConfiguration"/> at read time.
        /// </summary>
        /// <typeparam name="T">The settings type to serialize.</typeparam>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sectionName">The JSON section name (e.g., "GeminiSettings").</param>
        /// <param name="settings">The settings instance to save.</param>
        private static void SaveSection<T>(string userId, string sectionName, T settings)
            where T : class
        {
            string filePath = GetUserSettingsFilePath(userId);
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonObject? rootNode = null;

            if (File.Exists(filePath))
            {
                string existingContent = File.ReadAllText(filePath);
                rootNode = JsonNode.Parse(existingContent) as JsonObject;
            }

            rootNode ??= new JsonObject();

            string sectionJson = JsonSerializer.Serialize(settings, WriteOptions);
            var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;

            // ApiKey is [JsonIgnore] on the settings models, so it is never
            // present in the serialized JSON — no manual stripping needed.
            rootNode[sectionName] = sectionNode;

            File.WriteAllText(filePath, rootNode.ToJsonString(WriteOptions));
        }

        /// <inheritdoc/>
        public Task<GeminiModelSettings> GetGeminiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new GeminiModelSettings();
            var processing = GetProcessingSettingsAsync(ct).Result;

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

                    JsonObject? legacyRoot = ReadUserOverrides(userId);
                    if (legacyRoot != null && legacyRoot["GeminiSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<GeminiSettings>(legacyJson, ReadOptions);
                        if (legacySettings != null)
                        {
                            legacySettings.CopyOnto(migrated);
                        }
                    }

                    return new GeminiModelSettings
                    {
                        Endpoint = migrated.Endpoint,
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
                        RequestHeaders = migrated.RequestHeaders,
                        AdditionalParameters = migrated.AdditionalParameters,
                        EnableLogging = migrated.EnableLogging,
                        RateLimitPerMinute = migrated.RateLimitPerMinute,
                        ProxyUrl = migrated.ProxyUrl,
                        UseManagedIdentity = migrated.UseManagedIdentity,
                        CredentialType = migrated.CredentialType,
                        ReasoningLevel = migrated.ReasoningLevel,
                        ChunkSize = Math.Max(1, Math.Min(caps.ChunkSizeDefault, caps.ChunkSizeCap)),
                        MaxConcurrentChunks = Math.Max(1, Math.Min(caps.MaxConcurrentChunksDefault, caps.MaxConcurrentChunksCap)),
                    };
                });

            // Apply per-user overrides (same clamping)
            JsonObject? userOverrides = ReadUserOverrides(userId);
            if (userOverrides != null && userOverrides["GeminiModels"] is JsonObject geminiModels
                && geminiModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<GeminiModelSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return Task.FromResult(modelSettings);
        }

        /// <inheritdoc/>
        public Task<OpenAiModelSettings> GetOpenAiModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new OpenAiModelSettings();
            var processing = GetProcessingSettingsAsync(ct).Result;

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

                    JsonObject? legacyRoot = ReadUserOverrides(userId);
                    if (legacyRoot != null && legacyRoot["OpenAiSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<OpenAiSettings>(legacyJson, ReadOptions);
                        if (legacySettings != null)
                        {
                            legacySettings.CopyOnto(migrated);
                        }
                    }

                    return new OpenAiModelSettings
                    {
                        Endpoint = migrated.Endpoint,
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
            if (userOverrides != null && userOverrides["OpenAiModels"] is JsonObject openAiModels
                && openAiModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<OpenAiModelSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return Task.FromResult(modelSettings);
        }

        /// <inheritdoc/>
        public Task<DeepSeekModelSettings> GetDeepSeekModelSettingsAsync(string modelId, string userId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(modelId);

            var modelSettings = new DeepSeekModelSettings();
            var processing = GetProcessingSettingsAsync(ct).Result;

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

                    JsonObject? legacyRoot = ReadUserOverrides(userId);
                    if (legacyRoot != null && legacyRoot["DeepSeekSettings"] is JsonObject legacyOverride)
                    {
                        string legacyJson = legacyOverride.ToJsonString();
                        var legacySettings = JsonSerializer.Deserialize<DeepSeekSettings>(legacyJson, ReadOptions);
                        if (legacySettings != null)
                        {
                            legacySettings.CopyOnto(migrated);
                        }
                    }

                    return new DeepSeekModelSettings
                    {
                        Endpoint = migrated.Endpoint,
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
            if (userOverrides != null && userOverrides["DeepSeekModels"] is JsonObject deepSeekModels
                && deepSeekModels[modelId] is JsonObject userModelOverride)
            {
                string json = userModelOverride.ToJsonString();
                var overrideSettings = JsonSerializer.Deserialize<DeepSeekModelSettings>(json, ReadOptions);
                if (overrideSettings != null)
                {
                    overrideSettings.CopyOnto(modelSettings);
                    overrideSettings.ChunkSize = modelSettings.ChunkSize;
                    overrideSettings.MaxConcurrentChunks = modelSettings.MaxConcurrentChunks;
                }
            }

            return Task.FromResult(modelSettings);
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
        public Task<ProcessingSettings> GetProcessingSettingsAsync(CancellationToken ct = default)
        {
            var settings = new ProcessingSettings();
            _configuration.GetSection("Processing").Bind(settings);
            return Task.FromResult(settings);
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
            JsonObject? root = ReadUserOverrides(userId);
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

                string sectionJson = JsonSerializer.Serialize(modelSettings, WriteOptions);
                var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;
                modelsSection[modelId] = sectionNode;

                string filePath = GetUserSettingsFilePath(userId);
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, root.ToJsonString(WriteOptions));
            }
        }

        private void SaveModelSection<T>(string userId, string modelsSectionName, string modelId, T settings)
            where T : class
        {
            string filePath = GetUserSettingsFilePath(userId);
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonObject? rootNode = null;

            if (File.Exists(filePath))
            {
                string existingContent = File.ReadAllText(filePath);
                rootNode = JsonNode.Parse(existingContent) as JsonObject;
            }

            rootNode ??= new JsonObject();

            // Ensure the models section exists
            rootNode[modelsSectionName] ??= new JsonObject();
            if (rootNode[modelsSectionName] is JsonObject modelsSection)
            {
                string sectionJson = JsonSerializer.Serialize(settings, WriteOptions);
                var sectionNode = JsonNode.Parse(sectionJson) as JsonObject;
                modelsSection[modelId] = sectionNode;
            }

            File.WriteAllText(filePath, rootNode.ToJsonString(WriteOptions));
        }
    }
}