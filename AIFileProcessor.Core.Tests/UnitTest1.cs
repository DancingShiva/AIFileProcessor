namespace AIFileProcessor.Core.Tests
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class FileSettingsProviderTests
    {
        [Fact]
        public async Task SaveAndLoad_GeminiSettings_EncryptsAndDecryptsApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_gemini_{Guid.NewGuid()}";
            string plainKey = "sk-gemini-test-key-998877";

            var settings = new GeminiSettings
            {
                ApiKey = plainKey,
                ModelName = "gemini-3.5-flash",
                TimeoutSeconds = 45
            };

            try
            {
                // Act
                await provider.SaveGeminiSettingsAsync(userId, settings);

                // Assert: Verify settings file was created
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                // Assert: Verify that the stored key is encrypted (not plain text)
                string fileContent = await File.ReadAllTextAsync(settingsFilePath);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("GeminiSettings", out var geminiSection));
                Assert.True(geminiSection.TryGetProperty("apiKey", out var apiKeyProp));

                string storedValue = apiKeyProp.GetString() ?? string.Empty;
                Assert.NotEmpty(storedValue);
                Assert.NotEqual(plainKey, storedValue);

                // Act: Retrieve settings back
                GeminiSettings loadedSettings = await provider.GetGeminiSettingsAsync(userId);

                // Assert: Verify settings are correctly loaded and decrypted
                Assert.NotNull(loadedSettings);
                Assert.Equal(plainKey, loadedSettings.ApiKey);
                Assert.Equal("gemini-3.5-flash", loadedSettings.ModelName);
                Assert.Equal(45, loadedSettings.TimeoutSeconds);
            }
            finally
            {
                // Cleanup
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string userDirectory = Path.Combine(appData, "AIFileProcessor", userId);
                if (Directory.Exists(userDirectory))
                {
                    Directory.Delete(userDirectory, true);
                }
            }
        }

        [Fact]
        public async Task SaveAndLoad_OpenAiSettings_EncryptsAndDecryptsApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_openai_{Guid.NewGuid()}";
            string plainKey = "sk-openai-test-key-554433";

            var settings = new OpenAiSettings
            {
                ApiKey = plainKey,
                ModelName = "gpt-4o",
                TimeoutSeconds = 120
            };

            try
            {
                // Act
                await provider.SaveOpenAiSettingsAsync(userId, settings);

                // Assert
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                string fileContent = await File.ReadAllTextAsync(settingsFilePath);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("OpenAiSettings", out var openaiSection));
                Assert.True(openaiSection.TryGetProperty("apiKey", out var apiKeyProp));

                string storedValue = apiKeyProp.GetString() ?? string.Empty;
                Assert.NotEmpty(storedValue);
                Assert.NotEqual(plainKey, storedValue);

                // Act
                OpenAiSettings loadedSettings = await provider.GetOpenAiSettingsAsync(userId);

                // Assert
                Assert.NotNull(loadedSettings);
                Assert.Equal(plainKey, loadedSettings.ApiKey);
                Assert.Equal("gpt-4o", loadedSettings.ModelName);
                Assert.Equal(120, loadedSettings.TimeoutSeconds);
            }
            finally
            {
                // Cleanup
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string userDirectory = Path.Combine(appData, "AIFileProcessor", userId);
                if (Directory.Exists(userDirectory))
                {
                    Directory.Delete(userDirectory, true);
                }
            }
        }

        [Fact]
        public async Task SaveAndLoad_DeepSeekSettings_EncryptsAndDecryptsApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_deepseek_{Guid.NewGuid()}";
            string plainKey = "sk-deepseek-test-key-112233";

            var settings = new DeepSeekSettings
            {
                ApiKey = plainKey,
                ModelName = "deepseek-chat",
                TimeoutSeconds = 300,
                EnableReasoning = true
            };

            try
            {
                // Act
                await provider.SaveDeepSeekSettingsAsync(userId, settings);

                // Assert
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                string fileContent = await File.ReadAllTextAsync(settingsFilePath);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("DeepSeekSettings", out var deepseekSection));
                Assert.True(deepseekSection.TryGetProperty("apiKey", out var apiKeyProp));

                string storedValue = apiKeyProp.GetString() ?? string.Empty;
                Assert.NotEmpty(storedValue);
                Assert.NotEqual(plainKey, storedValue);

                // Act
                DeepSeekSettings loadedSettings = await provider.GetDeepSeekSettingsAsync(userId);

                // Assert
                Assert.NotNull(loadedSettings);
                Assert.Equal(plainKey, loadedSettings.ApiKey);
                Assert.Equal("deepseek-chat", loadedSettings.ModelName);
                Assert.Equal(300, loadedSettings.TimeoutSeconds);
                Assert.True(loadedSettings.EnableReasoning);
            }
            finally
            {
                // Cleanup
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string userDirectory = Path.Combine(appData, "AIFileProcessor", userId);
                if (Directory.Exists(userDirectory))
                {
                    Directory.Delete(userDirectory, true);
                }
            }
        }
    }
}