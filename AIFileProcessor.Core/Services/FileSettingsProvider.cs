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
    }
}