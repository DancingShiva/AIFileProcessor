namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Models;
using AIFileProcessor.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

[Trait("Category", "Integration")]
public class FileSettingsProviderTests
{
    private const string AppFolderName = "AIFileProcessor";
    private const string SettingsFileName = "settings.json";

    private static Mock<IConfiguration> CreateConfigMock()
    {
        var configMock = new Mock<IConfiguration>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s.Path).Returns(string.Empty);
        sectionMock.Setup(s => s.Key).Returns(string.Empty);
        sectionMock.Setup(s => s.Value).Returns((string?)null);
        sectionMock.Setup(s => s.GetSection(It.IsAny<string>())).Returns((IConfigurationSection)null!);
        sectionMock.Setup(s => s.GetChildren()).Returns(Array.Empty<IConfigurationSection>());
        configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(sectionMock.Object);
        return configMock;
    }

    private static string GetTestUserSettingsDirectory(string userId)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, AppFolderName, userId);
    }

    private static string GetTestUserSettingsFilePath(string userId)
    {
        return Path.Combine(GetTestUserSettingsDirectory(userId), SettingsFileName);
    }

    private static void WriteUserOverrideFile(string userId, string json)
    {
        string dir = GetTestUserSettingsDirectory(userId);
        Directory.CreateDirectory(dir);
        File.WriteAllText(GetTestUserSettingsFilePath(userId), json);
    }

    private static void CleanupUser(string userId)
    {
        string dir = GetTestUserSettingsDirectory(userId);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // --- Constructor --------------------------------------------------

    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FileSettingsProvider(null!));
    }

    [Fact]
    public void Constructor_ValidConfiguration_CreatesInstance()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        Assert.NotNull(provider);
    }

    // --- GetGeminiSettingsAsync ---------------------------------------

    [Fact]
    public async Task GetGeminiSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetGeminiSettingsAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetGeminiSettingsAsync_NoOverrideFile_ReturnsDefaultSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            GeminiSettings result = await provider.GetGeminiSettingsAsync(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.ApiKey);
            Assert.Null(result.Endpoint);
            Assert.Null(result.ModelName);
            Assert.Equal(300, result.TimeoutSeconds);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetGeminiSettingsAsync_WithOverrides_MergesNonSecretProperties()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "endpoint": "https://override.example.com",
                    "modelName": "override-model",
                    "timeoutSeconds": 60,
                    "temperature": 0.7
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            GeminiSettings result = await provider.GetGeminiSettingsAsync(userId, CancellationToken.None);

            Assert.Equal("https://override.example.com", result.Endpoint);
            Assert.Equal("override-model", result.ModelName);
            Assert.Equal(60, result.TimeoutSeconds);
            Assert.Equal(0.7, result.Temperature);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetGeminiSettingsAsync_WithOverrides_PreservesApiKeyFromConfig()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "apiKey": "should-not-appear",
                    "endpoint": "https://override.example.com"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            GeminiSettings result = await provider.GetGeminiSettingsAsync(userId, CancellationToken.None);

            // ApiKey is [JsonIgnore] on the model, so it won't be deserialized from the override file.
            // It stays as whatever was bound from IConfiguration (empty string with the mock).
            Assert.NotEqual("should-not-appear", result.ApiKey);
            Assert.Equal(string.Empty, result.ApiKey);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetGeminiSettingsAsync_WithEmptyOverrideSection_ReturnsDefaults()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            // The file exists but doesn't contain a GeminiSettings section.
            string json = """
            {
                "OpenAiSettings": {
                    "modelName": "other-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            GeminiSettings result = await provider.GetGeminiSettingsAsync(userId, CancellationToken.None);

            // No GeminiSettings override, so defaults should be returned.
            Assert.Null(result.Endpoint);
            Assert.Null(result.ModelName);
            Assert.Equal(300, result.TimeoutSeconds);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetGeminiSettingsAsync_OverridesAllProperties_AllOverwritten()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "endpoint": "https://override.example.com",
                    "modelName": "override-model",
                    "timeoutSeconds": 60,
                    "projectId": "override-project",
                    "location": "override-location",
                    "temperature": 0.7,
                    "topP": 0.9,
                    "streamingEnabled": true,
                    "maxRetries": 5,
                    "baseDelay": 2,
                    "maxDelay": 120,
                    "enableLogging": true,
                    "rateLimitPerMinute": 100,
                    "proxyUrl": "https://proxy.example.com",
                    "useManagedIdentity": true,
                    "credentialType": "Default"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            GeminiSettings result = await provider.GetGeminiSettingsAsync(userId, CancellationToken.None);

            Assert.Equal("https://override.example.com", result.Endpoint);
            Assert.Equal("override-model", result.ModelName);
            Assert.Equal(60, result.TimeoutSeconds);
            Assert.Equal("override-project", result.ProjectId);
            Assert.Equal("override-location", result.Location);
            Assert.Equal(0.7, result.Temperature);
            Assert.Equal(0.9, result.TopP);
            Assert.True(result.StreamingEnabled);
            Assert.Equal(5, result.MaxRetries);
            Assert.Equal(2, result.BaseDelay);
            Assert.Equal(120, result.MaxDelay);
            Assert.True(result.EnableLogging);
            Assert.Equal(100, result.RateLimitPerMinute);
            Assert.Equal("https://proxy.example.com", result.ProxyUrl);
            Assert.True(result.UseManagedIdentity);
            Assert.Equal("Default", result.CredentialType);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- GetOpenAiSettingsAsync ---------------------------------------

    [Fact]
    public async Task GetOpenAiSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetOpenAiSettingsAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetOpenAiSettingsAsync_NoOverrideFile_ReturnsDefaultSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            OpenAiSettings result = await provider.GetOpenAiSettingsAsync(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.ApiKey);
            Assert.Equal(string.Empty, result.Endpoint);
            Assert.Equal(string.Empty, result.ModelName);
            Assert.Equal(300, result.TimeoutSeconds);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetOpenAiSettingsAsync_WithOverrides_MergesNonSecretProperties()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "OpenAiSettings": {
                    "endpoint": "https://override.example.com",
                    "modelName": "override-model",
                    "timeoutSeconds": 60,
                    "organizationId": "override-org",
                    "temperature": 0.7
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            OpenAiSettings result = await provider.GetOpenAiSettingsAsync(userId, CancellationToken.None);

            Assert.Equal("https://override.example.com", result.Endpoint);
            Assert.Equal("override-model", result.ModelName);
            Assert.Equal(60, result.TimeoutSeconds);
            Assert.Equal("override-org", result.OrganizationId);
            Assert.Equal(0.7, result.Temperature);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetOpenAiSettingsAsync_WithOverrides_PreservesApiKeyFromConfig()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "OpenAiSettings": {
                    "apiKey": "should-not-appear",
                    "endpoint": "https://override.example.com"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            OpenAiSettings result = await provider.GetOpenAiSettingsAsync(userId, CancellationToken.None);

            Assert.NotEqual("should-not-appear", result.ApiKey);
            Assert.Equal(string.Empty, result.ApiKey);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetOpenAiSettingsAsync_WithEmptyOverrideSection_ReturnsDefaults()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "modelName": "other-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            OpenAiSettings result = await provider.GetOpenAiSettingsAsync(userId, CancellationToken.None);

            Assert.Equal(string.Empty, result.Endpoint);
            Assert.Equal(string.Empty, result.ModelName);
            Assert.Equal(300, result.TimeoutSeconds);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- GetDeepSeekSettingsAsync -------------------------------------

    [Fact]
    public async Task GetDeepSeekSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetDeepSeekSettingsAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetDeepSeekSettingsAsync_NoOverrideFile_ReturnsDefaultSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            DeepSeekSettings result = await provider.GetDeepSeekSettingsAsync(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.ApiKey);
            Assert.Equal(string.Empty, result.Endpoint);
            Assert.Equal(300, result.TimeoutSeconds);
            Assert.Null(result.ModelName);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetDeepSeekSettingsAsync_WithOverrides_MergesNonSecretProperties()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "DeepSeekSettings": {
                    "endpoint": "https://override.example.com",
                    "timeoutSeconds": 60,
                    "temperature": 0.7,
                    "modelName": "override-model",
                    "enableReasoning": true
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            DeepSeekSettings result = await provider.GetDeepSeekSettingsAsync(userId, CancellationToken.None);

            Assert.Equal("https://override.example.com", result.Endpoint);
            Assert.Equal(60, result.TimeoutSeconds);
            Assert.Equal(0.7, result.Temperature);
            Assert.Equal("override-model", result.ModelName);
            Assert.True(result.EnableReasoning);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetDeepSeekSettingsAsync_WithOverrides_PreservesApiKeyFromConfig()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "DeepSeekSettings": {
                    "apiKey": "should-not-appear",
                    "endpoint": "https://override.example.com"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            DeepSeekSettings result = await provider.GetDeepSeekSettingsAsync(userId, CancellationToken.None);

            Assert.NotEqual("should-not-appear", result.ApiKey);
            Assert.Equal(string.Empty, result.ApiKey);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetDeepSeekSettingsAsync_WithEmptyOverrideSection_ReturnsDefaults()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "modelName": "other-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            DeepSeekSettings result = await provider.GetDeepSeekSettingsAsync(userId, CancellationToken.None);

            Assert.Equal(string.Empty, result.Endpoint);
            Assert.Equal(300, result.TimeoutSeconds);
            Assert.Null(result.ModelName);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- SaveGeminiSettingsAsync --------------------------------------

    [Fact]
    public async Task SaveGeminiSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        var settings = new GeminiSettings();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.SaveGeminiSettingsAsync(null!, settings, CancellationToken.None));
    }

    [Fact]
    public async Task SaveGeminiSettingsAsync_NullSettings_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => provider.SaveGeminiSettingsAsync(userId, null!, CancellationToken.None));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveGeminiSettingsAsync_ValidArguments_WritesFileWithSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new GeminiSettings
            {
                Endpoint = "https://test.example.com",
                ModelName = "test-model",
                TimeoutSeconds = 45,
                Temperature = 0.5,
            };

            await provider.SaveGeminiSettingsAsync(userId, settings, CancellationToken.None);

            string filePath = GetTestUserSettingsFilePath(userId);
            Assert.True(File.Exists(filePath));

            string fileContent = File.ReadAllText(filePath);
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);
            Assert.True(root!["GeminiSettings"] is JsonObject);

            var geminiNode = root["GeminiSettings"]!.AsObject();
            Assert.Equal("https://test.example.com", geminiNode["endpoint"]?.GetValue<string>());
            Assert.Equal("test-model", geminiNode["modelName"]?.GetValue<string>());
            Assert.Equal(45, geminiNode["timeoutSeconds"]?.GetValue<int>());
            Assert.Equal(0.5, geminiNode["temperature"]?.GetValue<double>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveGeminiSettingsAsync_ApiKeyNotWrittenToFile()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new GeminiSettings
            {
                ApiKey = "secret-key-should-not-be-saved",
                Endpoint = "https://test.example.com",
            };

            await provider.SaveGeminiSettingsAsync(userId, settings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            Assert.DoesNotContain("secret-key-should-not-be-saved", fileContent);
            Assert.DoesNotContain("apiKey", fileContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveGeminiSettingsAsync_ExistingFile_PreservesOtherSections()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            // Pre-write a file with an OpenAiSettings section.
            string existingJson = """
            {
                "OpenAiSettings": {
                    "modelName": "existing-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, existingJson);

            var geminiSettings = new GeminiSettings { Endpoint = "https://gemini.example.com" };
            await provider.SaveGeminiSettingsAsync(userId, geminiSettings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);

            // GeminiSettings should be written.
            Assert.True(root!["GeminiSettings"] is JsonObject);
            Assert.Equal("https://gemini.example.com", root["GeminiSettings"]!["endpoint"]?.GetValue<string>());

            // OpenAiSettings should be preserved.
            Assert.True(root["OpenAiSettings"] is JsonObject);
            Assert.Equal("existing-model", root["OpenAiSettings"]!["modelName"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- SaveOpenAiSettingsAsync -------------------------------------

    [Fact]
    public async Task SaveOpenAiSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        var settings = new OpenAiSettings();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.SaveOpenAiSettingsAsync(null!, settings, CancellationToken.None));
    }

    [Fact]
    public async Task SaveOpenAiSettingsAsync_NullSettings_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => provider.SaveOpenAiSettingsAsync(userId, null!, CancellationToken.None));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveOpenAiSettingsAsync_ValidArguments_WritesFileWithSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new OpenAiSettings
            {
                Endpoint = "https://openai.example.com",
                ModelName = "gpt-test",
                TimeoutSeconds = 45,
                OrganizationId = "test-org",
            };

            await provider.SaveOpenAiSettingsAsync(userId, settings, CancellationToken.None);

            string filePath = GetTestUserSettingsFilePath(userId);
            Assert.True(File.Exists(filePath));

            string fileContent = File.ReadAllText(filePath);
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);
            Assert.True(root!["OpenAiSettings"] is JsonObject);

            var openAiNode = root["OpenAiSettings"]!.AsObject();
            Assert.Equal("https://openai.example.com", openAiNode["endpoint"]?.GetValue<string>());
            Assert.Equal("gpt-test", openAiNode["modelName"]?.GetValue<string>());
            Assert.Equal(45, openAiNode["timeoutSeconds"]?.GetValue<int>());
            Assert.Equal("test-org", openAiNode["organizationId"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveOpenAiSettingsAsync_ApiKeyNotWrittenToFile()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new OpenAiSettings
            {
                ApiKey = "secret-openai-key",
                Endpoint = "https://openai.example.com",
            };

            await provider.SaveOpenAiSettingsAsync(userId, settings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            Assert.DoesNotContain("secret-openai-key", fileContent);
            Assert.DoesNotContain("apiKey", fileContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveOpenAiSettingsAsync_ExistingFile_PreservesOtherSections()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string existingJson = """
            {
                "GeminiSettings": {
                    "modelName": "existing-gemini-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, existingJson);

            var openAiSettings = new OpenAiSettings { Endpoint = "https://openai.example.com" };
            await provider.SaveOpenAiSettingsAsync(userId, openAiSettings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);

            Assert.True(root!["OpenAiSettings"] is JsonObject);
            Assert.Equal("https://openai.example.com", root["OpenAiSettings"]!["endpoint"]?.GetValue<string>());

            Assert.True(root["GeminiSettings"] is JsonObject);
            Assert.Equal("existing-gemini-model", root["GeminiSettings"]!["modelName"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- SaveDeepSeekSettingsAsync -----------------------------------

    [Fact]
    public async Task SaveDeepSeekSettingsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        var settings = new DeepSeekSettings();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.SaveDeepSeekSettingsAsync(null!, settings, CancellationToken.None));
    }

    [Fact]
    public async Task SaveDeepSeekSettingsAsync_NullSettings_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => provider.SaveDeepSeekSettingsAsync(userId, null!, CancellationToken.None));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveDeepSeekSettingsAsync_ValidArguments_WritesFileWithSettings()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new DeepSeekSettings
            {
                Endpoint = "https://deepseek.example.com",
                ModelName = "deepseek-test",
                TimeoutSeconds = 60,
                EnableReasoning = true,
            };

            await provider.SaveDeepSeekSettingsAsync(userId, settings, CancellationToken.None);

            string filePath = GetTestUserSettingsFilePath(userId);
            Assert.True(File.Exists(filePath));

            string fileContent = File.ReadAllText(filePath);
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);
            Assert.True(root!["DeepSeekSettings"] is JsonObject);

            var deepSeekNode = root["DeepSeekSettings"]!.AsObject();
            Assert.Equal("https://deepseek.example.com", deepSeekNode["endpoint"]?.GetValue<string>());
            Assert.Equal("deepseek-test", deepSeekNode["modelName"]?.GetValue<string>());
            Assert.Equal(60, deepSeekNode["timeoutSeconds"]?.GetValue<int>());
            Assert.True(deepSeekNode["enableReasoning"]?.GetValue<bool>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveDeepSeekSettingsAsync_ApiKeyNotWrittenToFile()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings = new DeepSeekSettings
            {
                ApiKey = "secret-deepseek-key",
                Endpoint = "https://deepseek.example.com",
            };

            await provider.SaveDeepSeekSettingsAsync(userId, settings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            Assert.DoesNotContain("secret-deepseek-key", fileContent);
            Assert.DoesNotContain("apiKey", fileContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveDeepSeekSettingsAsync_ExistingFile_PreservesOtherSections()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string existingJson = """
            {
                "OpenAiSettings": {
                    "modelName": "existing-openai-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, existingJson);

            var deepSeekSettings = new DeepSeekSettings { Endpoint = "https://deepseek.example.com" };
            await provider.SaveDeepSeekSettingsAsync(userId, deepSeekSettings, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);

            Assert.True(root!["DeepSeekSettings"] is JsonObject);
            Assert.Equal("https://deepseek.example.com", root["DeepSeekSettings"]!["endpoint"]?.GetValue<string>());

            Assert.True(root["OpenAiSettings"] is JsonObject);
            Assert.Equal("existing-openai-model", root["OpenAiSettings"]!["modelName"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- GetProcessingOptionsAsync -----------------------------------

    [Fact]
    public async Task GetProcessingOptionsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetProcessingOptionsAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetProcessingOptionsAsync_NoOverrideFile_ReturnsDefaultOptions()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            ProcessingOptions result = await provider.GetProcessingOptionsAsync(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Prompt);
            Assert.Equal(string.Empty, result.PromptPath);
            Assert.Equal(string.Empty, result.DataPath);
            Assert.Equal(string.Empty, result.ModelId);
            Assert.Equal(string.Empty, result.IdPrefix);
            Assert.Equal(1, result.IdStart);
            Assert.Equal(3, result.IdPad);
            Assert.Equal(3, result.LengthPad);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetProcessingOptionsAsync_WithOverrides_ReturnsOverriddenOptions()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "ProcessingOptions": {
                    "promptPath": "C:\\prompts\\test.txt",
                    "dataPath": "C:\\data\\test.txt",
                    "modelId": "gpt-test",
                    "idPrefix": "T",
                    "idStart": 5,
                    "idPad": 4,
                    "lengthPad": 2
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            ProcessingOptions result = await provider.GetProcessingOptionsAsync(userId, CancellationToken.None);

            // Prompt is [JsonIgnore] so it is not deserialized from the override file
            Assert.Equal(string.Empty, result.Prompt);
            // PromptPath and DataPath are persisted and should be deserialized
            Assert.Equal("C:\\prompts\\test.txt", result.PromptPath);
            Assert.Equal("C:\\data\\test.txt", result.DataPath);
            Assert.Equal("gpt-test", result.ModelId);
            Assert.Equal("T", result.IdPrefix);
            Assert.Equal(5, result.IdStart);
            Assert.Equal(4, result.IdPad);
            Assert.Equal(2, result.LengthPad);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task GetProcessingOptionsAsync_WithEmptyOverrideSection_ReturnsDefaults()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string json = """
            {
                "GeminiSettings": {
                    "modelName": "other-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, json);

            ProcessingOptions result = await provider.GetProcessingOptionsAsync(userId, CancellationToken.None);

            Assert.Equal(string.Empty, result.Prompt);
            Assert.Equal(string.Empty, result.ModelId);
            Assert.Equal(1, result.IdStart);
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- SaveProcessingOptionsAsync ----------------------------------

    [Fact]
    public async Task SaveProcessingOptionsAsync_NullUserId_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        var options = new ProcessingOptions();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.SaveProcessingOptionsAsync(null!, options, CancellationToken.None));
    }

    [Fact]
    public async Task SaveProcessingOptionsAsync_NullOptions_ThrowsArgumentNullException()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => provider.SaveProcessingOptionsAsync(userId, null!, CancellationToken.None));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveProcessingOptionsAsync_ValidArguments_WritesFileWithOptions()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var options = new ProcessingOptions
            {
                Prompt = "should-not-persist",
                PromptPath = "C:\\prompts\\test.txt",
                DataPath = "C:\\data\\test.txt",
                ModelId = "gpt-test",
                IdPrefix = "T",
                IdStart = 10,
                IdPad = 5,
                LengthPad = 2,
            };

            await provider.SaveProcessingOptionsAsync(userId, options, CancellationToken.None);

            string filePath = GetTestUserSettingsFilePath(userId);
            Assert.True(File.Exists(filePath));

            string fileContent = File.ReadAllText(filePath);
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);
            Assert.True(root!["ProcessingOptions"] is JsonObject);

            var optionsNode = root["ProcessingOptions"]!.AsObject();
            Assert.Equal("gpt-test", optionsNode["modelId"]?.GetValue<string>());
            Assert.Equal("T", optionsNode["idPrefix"]?.GetValue<string>());
            Assert.Equal(10, optionsNode["idStart"]?.GetValue<int>());
            Assert.Equal(5, optionsNode["idPad"]?.GetValue<int>());
            Assert.Equal(2, optionsNode["lengthPad"]?.GetValue<int>());
            // Prompt should not be persisted ([JsonIgnore])
            Assert.Null(optionsNode["prompt"]);
            // PromptPath and DataPath should be persisted
            Assert.Equal("C:\\prompts\\test.txt", optionsNode["promptPath"]?.GetValue<string>());
            Assert.Equal("C:\\data\\test.txt", optionsNode["dataPath"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveProcessingOptionsAsync_ExistingFile_PreservesOtherSections()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            string existingJson = """
            {
                "GeminiSettings": {
                    "modelName": "existing-model"
                }
            }
            """;
            WriteUserOverrideFile(userId, existingJson);

            var options = new ProcessingOptions { ModelId = "gpt-test" };
            await provider.SaveProcessingOptionsAsync(userId, options, CancellationToken.None);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;
            Assert.NotNull(root);

            Assert.True(root!["ProcessingOptions"] is JsonObject);
            Assert.Equal("gpt-test", root["ProcessingOptions"]!["modelId"]?.GetValue<string>());

            Assert.True(root["GeminiSettings"] is JsonObject);
            Assert.Equal("existing-model", root["GeminiSettings"]!["modelName"]?.GetValue<string>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    // --- Concurrency tests --------------------------------------------

    [Fact]
    public async Task SaveGeminiSettings_ConcurrentWrites_DifferentSections_BothSettingsPreserved()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var geminiSettings = new GeminiSettings { Temperature = 0.5, MaxTokens = 1000 };
            var openAiSettings = new OpenAiSettings { Temperature = 0.7, MaxTokens = 2000 };

            var t1 = Task.Run(() => provider.SaveGeminiSettingsAsync(userId, geminiSettings, CancellationToken.None));
            var t2 = Task.Run(() => provider.SaveOpenAiSettingsAsync(userId, openAiSettings, CancellationToken.None));
            await Task.WhenAll(t1, t2);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;

            Assert.NotNull(root);
            Assert.True(root!["GeminiSettings"] is JsonObject);
            Assert.True(root["OpenAiSettings"] is JsonObject);
            Assert.Equal(0.5, root["GeminiSettings"]!["temperature"]?.GetValue<double>());
            Assert.Equal(0.7, root["OpenAiSettings"]!["temperature"]?.GetValue<double>());
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveGeminiSettings_ConcurrentWrites_SameSection_FileNotCorrupted()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var settings1 = new GeminiSettings { Temperature = 0.1, MaxTokens = 100 };
            var settings2 = new GeminiSettings { Temperature = 0.9, MaxTokens = 900 };

            var t1 = Task.Run(() => provider.SaveGeminiSettingsAsync(userId, settings1, CancellationToken.None));
            var t2 = Task.Run(() => provider.SaveGeminiSettingsAsync(userId, settings2, CancellationToken.None));
            await Task.WhenAll(t1, t2);

            // The file should be valid JSON (not corrupted) with one of the two values.
            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;

            Assert.NotNull(root);
            Assert.True(root!["GeminiSettings"] is JsonObject);
            double temp = root["GeminiSettings"]!["temperature"]?.GetValue<double>() ?? -1;
            Assert.True(temp == 0.1 || temp == 0.9, $"Expected 0.1 or 0.9, got {temp}");
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task ReadUserOverrides_DuringWrite_ReturnsValidJson()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            // Pre-populate the file so reads have something to find.
            var initialSettings = new GeminiSettings { Temperature = 0.3, MaxTokens = 500 };
            await provider.SaveGeminiSettingsAsync(userId, initialSettings, CancellationToken.None);

            var readResults = new List<JsonObject?>();
            var writeTask = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    provider.SaveGeminiSettingsAsync(userId, new GeminiSettings { Temperature = 0.3 + (i * 0.01), MaxTokens = 500 + i }, CancellationToken.None).Wait();
                }
            });

            for (int i = 0; i < 20; i++)
            {
                var gemini = provider.GetGeminiSettingsAsync(userId, CancellationToken.None).Result;
                readResults.Add(JsonNode.Parse(JsonSerializer.Serialize(gemini)) as JsonObject);
            }

            await writeTask;

            // Every read should have produced a valid GeminiSettings object (no exceptions thrown).
            Assert.Equal(20, readResults.Count);
            Assert.All(readResults, r => Assert.NotNull(r));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task SaveModelSection_ConcurrentWrites_DifferentModels_BothPreserved()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            var geminiModel = new GeminiModelSettings { Temperature = 0.4, MaxTokens = 800 };
            var openAiModel = new OpenAiModelSettings { Temperature = 0.6, MaxTokens = 1200 };

            var t1 = Task.Run(() => provider.SaveGeminiModelSettingsAsync("gemini-3-flash", userId, geminiModel, CancellationToken.None));
            var t2 = Task.Run(() => provider.SaveOpenAiModelSettingsAsync("gpt-5.5", userId, openAiModel, CancellationToken.None));
            await Task.WhenAll(t1, t2);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;

            Assert.NotNull(root);
            Assert.True(root!["GeminiModels"] is JsonObject);
            Assert.True(root["GeminiModels"]!.AsObject().ContainsKey("gemini-3-flash"));
            Assert.True(root["OpenAiModels"] is JsonObject);
            Assert.True(root["OpenAiModels"]!.AsObject().ContainsKey("gpt-5.5"));
        }
        finally
        {
            CleanupUser(userId);
        }
    }

    [Fact]
    public async Task MigrateLegacyProviderToModelSettings_ConcurrentMigration_MigrationHappensOnce()
    {
        var configMock = CreateConfigMock();
        var provider = new FileSettingsProvider(configMock.Object);
        string userId = $"test-{Guid.NewGuid():N}";

        try
        {
            // Set up a legacy provider-level override file (pre-migration format).
            WriteUserOverrideFile(userId, """{"GeminiSettings":{"temperature":0.5,"maxTokens":1000}}""");

            // Two threads trigger migration for the same model concurrently.
            var t1 = Task.Run(() => provider.GetGeminiModelSettingsAsync("gemini-3-flash", userId, CancellationToken.None));
            var t2 = Task.Run(() => provider.GetGeminiModelSettingsAsync("gemini-3-flash", userId, CancellationToken.None));
            var results = await Task.WhenAll(t1, t2);

            // Both calls should succeed, and the file should be valid JSON.
            Assert.NotNull(results[0]);
            Assert.NotNull(results[1]);

            string fileContent = File.ReadAllText(GetTestUserSettingsFilePath(userId));
            var root = JsonNode.Parse(fileContent) as JsonObject;

            // File should not be corrupted - valid JSON parse.
            Assert.NotNull(root);
            // Migration should have happened - GeminiModels section should exist.
            Assert.True(root!["GeminiModels"] is JsonObject);
            Assert.True(root["GeminiModels"]!.AsObject().ContainsKey("gemini-3-flash"));
        }
        finally
        {
            CleanupUser(userId);
        }
    }
}
