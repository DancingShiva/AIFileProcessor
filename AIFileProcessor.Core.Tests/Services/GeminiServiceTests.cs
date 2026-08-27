namespace AIFileProcessor.Core.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Models;
using AIFileProcessor.Exceptions;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
[Trait("Category", "Unit")]
public class GeminiServiceTests
{
    private readonly Mock<ILogger<GeminiService>> _loggerMock = new();
    private readonly Mock<IOptionsMonitor<GeminiSettings>> _optionsMock = new();
    private readonly HttpClient _httpClient = new();
    private readonly GeminiSettings _settings = new() { ApiKey = "test-key" };
    public GeminiServiceTests()
    {
        _optionsMock.Setup(o => o.CurrentValue).Returns(_settings);
    }
    // ─── Constructor ──────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GeminiService(null!, _optionsMock.Object, _httpClient));
    }
    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GeminiService(_loggerMock.Object, null!, _httpClient));
    }
    [Fact]
    public void Constructor_SettingsWithNullValue_ThrowsArgumentNullException()
    {
        var nullOptionsMock = new Mock<IOptionsMonitor<GeminiSettings>>();
        nullOptionsMock.Setup(o => o.CurrentValue).Returns((GeminiSettings)null!);
        Assert.Throws<ArgumentNullException>(
            () => new GeminiService(_loggerMock.Object, nullOptionsMock.Object, _httpClient));
    }
    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GeminiService(_loggerMock.Object, _optionsMock.Object, null!));
    }
    [Fact]
    public void Constructor_ValidArguments_CreatesInstance()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        Assert.NotNull(service);
    }
    // ─── GetSupportedModels ───────────────────────────────────────────
    [Fact]
    public void GetSupportedModels_ReturnsNonEmptyCollection()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        Assert.NotEmpty(models);
    }
    [Fact]
    public void GetSupportedModels_ContainsExpectedModelIds()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        var modelIds = models.Select(m => m.ModelId).ToList();
        Assert.Contains("gemini-3-flash", modelIds);
        Assert.Contains("gemini-3.1-pro-preview", modelIds);
        Assert.Contains("gemini-3.5-flash", modelIds);
        Assert.Contains("gemini-3.6-flash", modelIds);
    }
    [Fact]
    public void GetSupportedModels_ContainsExpectedModelNames()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        var modelNames = models.Select(m => m.ModelName).ToList();
        Assert.Contains("Gemini 3 Flash", modelNames);
        Assert.Contains("Gemini 3.1 Pro", modelNames);
        Assert.Contains("Gemini 3.5 Flash", modelNames);
        Assert.Contains("Gemini 3.6 Flash", modelNames);
    }
    // ─── SupportsModel ────────────────────────────────────────────────
    [Fact]
    public void SupportsModel_NullInput_ReturnsFalse()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel(null!);
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_EmptyString_ReturnsFalse()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel(string.Empty);
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_WhitespaceString_ReturnsFalse()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("   ");
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_ValidModelId_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("gemini-3-flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ValidModelName_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("Gemini 3 Flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ModelIdWithDifferentCasing_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("GEMINI-3-FLASH");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ModelNameWithSurroundingSpaces_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("  Gemini 3 Flash  ");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_UnknownModel_ReturnsFalse()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("nonexistent-model");
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_ModelNameCaseInsensitive_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("gemini 3 flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ProPreviewModelName_ReturnsTrue()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("Gemini 3.1 Pro");
        Assert.True(result);
    }
    // ─── ProcessTextAsync (4-param overload with CancellationToken) ───
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyApiKey_ThrowsApiKeyNotConfiguredException()
    {
        var emptyKeySettings = new GeminiSettings { ApiKey = string.Empty };
        var emptyOptionsMock = new Mock<IOptionsMonitor<GeminiSettings>>();
        emptyOptionsMock.Setup(o => o.CurrentValue).Returns(emptyKeySettings);
        var service = new GeminiService(_loggerMock.Object, emptyOptionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ApiKeyNotConfiguredException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyModelId_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync(string.Empty, "prompt", "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyPrompt_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", string.Empty, "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyData_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", string.Empty, CancellationToken.None));
    }
    // ─── ProcessTextAsync (5-param overload with object settings) ─────
    [Fact]
    public async Task ProcessTextAsync_FiveParam_NullSettings_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", "data", null!, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WrongSettingsType_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", "data", new OpenAiSettings(), CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyApiKey_ThrowsApiKeyNotConfiguredException()
    {
        var emptyKeySettings = new GeminiSettings { ApiKey = string.Empty };
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ApiKeyNotConfiguredException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", "data", emptyKeySettings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyModelId_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync(string.Empty, "prompt", "data", _settings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyPrompt_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", string.Empty, "data", _settings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyData_ThrowsArgumentException()
    {
        var service = new GeminiService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("gemini-3-flash", "prompt", string.Empty, _settings, CancellationToken.None));
    }
}

