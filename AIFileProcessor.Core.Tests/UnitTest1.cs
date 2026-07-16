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
        public async Task SaveAndLoad_GeminiSettings_PersistsPreferencesWithoutApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_gemini_{Guid.NewGuid()}";

            var settings = new GeminiSettings
            {
                ModelName = "gemini-3.5-flash",
                TimeoutSeconds = 45
            };

            try
            {
                // Act
                await provider.SaveGeminiSettingsAsync(userId, settings, TestContext.Current.CancellationToken);

                // Assert: Verify settings file was created
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                // Assert: per-user file must NOT contain an apiKey field
                string fileContent = await File.ReadAllTextAsync(settingsFilePath, TestContext.Current.CancellationToken);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("GeminiSettings", out var geminiSection));
                Assert.False(geminiSection.TryGetProperty("apiKey", out _));
                Assert.False(geminiSection.TryGetProperty("ApiKey", out _));

                // Act: Retrieve settings back
                GeminiSettings loadedSettings = await provider.GetGeminiSettingsAsync(userId, TestContext.Current.CancellationToken);

                // Assert: preferences are loaded; ApiKey is empty (no env var in test)
                Assert.NotNull(loadedSettings);
                Assert.Equal(string.Empty, loadedSettings.ApiKey);
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
        public async Task SaveAndLoad_OpenAiSettings_PersistsPreferencesWithoutApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_openai_{Guid.NewGuid()}";

            var settings = new OpenAiSettings
            {
                ModelName = "gpt-5.6-terra",
                TimeoutSeconds = 120
            };

            try
            {
                // Act
                await provider.SaveOpenAiSettingsAsync(userId, settings, TestContext.Current.CancellationToken);

                // Assert
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                // Assert: per-user file must NOT contain an apiKey field
                string fileContent = await File.ReadAllTextAsync(settingsFilePath, TestContext.Current.CancellationToken);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("OpenAiSettings", out var openaiSection));
                Assert.False(openaiSection.TryGetProperty("apiKey", out _));
                Assert.False(openaiSection.TryGetProperty("ApiKey", out _));

                // Act
                OpenAiSettings loadedSettings = await provider.GetOpenAiSettingsAsync(userId, TestContext.Current.CancellationToken);

                // Assert: preferences are loaded; ApiKey is empty (no env var in test)
                Assert.NotNull(loadedSettings);
                Assert.Equal(string.Empty, loadedSettings.ApiKey);
                Assert.Equal("gpt-5.6-terra", loadedSettings.ModelName);
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
        public async Task SaveAndLoad_DeepSeekSettings_PersistsPreferencesWithoutApiKey()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var provider = new FileSettingsProvider(config);
            string userId = $"test_user_deepseek_{Guid.NewGuid()}";

            var settings = new DeepSeekSettings
            {
                ModelName = "deepseek-v4-flash",
                TimeoutSeconds = 300,
                EnableReasoning = true
            };

            try
            {
                // Act
                await provider.SaveDeepSeekSettingsAsync(userId, settings, TestContext.Current.CancellationToken);

                // Assert
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFilePath = Path.Combine(appData, "AIFileProcessor", userId, "settings.json");
                Assert.True(File.Exists(settingsFilePath));

                // Assert: per-user file must NOT contain an apiKey field
                string fileContent = await File.ReadAllTextAsync(settingsFilePath, TestContext.Current.CancellationToken);
                using var doc = JsonDocument.Parse(fileContent);
                var root = doc.RootElement;
                Assert.True(root.TryGetProperty("DeepSeekSettings", out var deepseekSection));
                Assert.False(deepseekSection.TryGetProperty("apiKey", out _));
                Assert.False(deepseekSection.TryGetProperty("ApiKey", out _));

                // Act
                DeepSeekSettings loadedSettings = await provider.GetDeepSeekSettingsAsync(userId, TestContext.Current.CancellationToken);

                // Assert: preferences are loaded; ApiKey is empty (no env var in test)
                Assert.NotNull(loadedSettings);
                Assert.Equal(string.Empty, loadedSettings.ApiKey);
                Assert.Equal("deepseek-v4-flash", loadedSettings.ModelName);
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