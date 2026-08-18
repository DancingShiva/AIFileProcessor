namespace AIFileProcessor.Core.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Models;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
[Trait("Category", "Integration")]
public class DeepSeekServiceTests
{
    private readonly Mock<ILogger<DeepSeekService>> _loggerMock = new();
    private readonly Mock<IOptionsMonitor<DeepSeekSettings>> _optionsMock = new();
    private readonly HttpClient _httpClient = new();
    private readonly DeepSeekSettings _settings = new() { ApiKey = "test-key", ModelName = "deepseek-v4-flash" };
    public DeepSeekServiceTests()
    {
        _optionsMock.Setup(o => o.CurrentValue).Returns(_settings);
    }
    // ─── Constructor ──────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DeepSeekService(null!, _optionsMock.Object, _httpClient));
    }
    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DeepSeekService(_loggerMock.Object, null!, _httpClient));
    }
    [Fact]
    public void Constructor_SettingsWithNullValue_ThrowsArgumentNullException()
    {
        var nullOptionsMock = new Mock<IOptionsMonitor<DeepSeekSettings>>();
        nullOptionsMock.Setup(o => o.CurrentValue).Returns((DeepSeekSettings)null!);
        Assert.Throws<ArgumentNullException>(
            () => new DeepSeekService(_loggerMock.Object, nullOptionsMock.Object, _httpClient));
    }
    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DeepSeekService(_loggerMock.Object, _optionsMock.Object, null!));
    }
    [Fact]
    public void Constructor_ValidArguments_CreatesInstance()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        Assert.NotNull(service);
    }
    // ─── GetSupportedModels ───────────────────────────────────────────
    [Fact]
    public void GetSupportedModels_ReturnsNonEmptyCollection()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        Assert.NotEmpty(models);
    }
    [Fact]
    public void GetSupportedModels_ContainsExpectedModelIds()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        var modelIds = models.Select(m => m.ModelId).ToList();
        // Deprecated model IDs were removed; verify only current IDs are present
        Assert.Contains("deepseek-v4-flash", modelIds);
        Assert.Contains("deepseek-v4-pro", modelIds);
    }
    [Fact]
    public void GetSupportedModels_ContainsExpectedModelNames()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        IEnumerable<AiModelInfo> models = service.GetSupportedModels();
        var modelNames = models.Select(m => m.ModelName).ToList();
        // Deprecated human-friendly names were removed; verify only current names are present
        Assert.Contains("DeepSeek-V4-Flash", modelNames);
        Assert.Contains("DeepSeek-V4-Pro", modelNames);
    }
    // ─── SupportsModel ────────────────────────────────────────────────
    [Fact]
    public void SupportsModel_NullInput_ReturnsFalse()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel(null!);
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_EmptyString_ReturnsFalse()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel(string.Empty);
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_WhitespaceString_ReturnsFalse()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("   ");
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_ValidModelId_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("deepseek-v4-flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ValidModelName_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("DeepSeek-V4-Flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ModelIdWithDifferentCasing_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("DEEPSEEK-V4-FLASH");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_ModelNameWithSurroundingSpaces_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("  DeepSeek-V4-Flash  ");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_UnknownModel_ReturnsFalse()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("nonexistent-model");
        Assert.False(result);
    }
    [Fact]
    public void SupportsModel_ModelNameCaseInsensitive_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("deepseek-v4-pro");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_V4FlashModelId_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("deepseek-v4-flash");
        Assert.True(result);
    }
    [Fact]
    public void SupportsModel_V4ProModelName_ReturnsTrue()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        bool result = service.SupportsModel("DeepSeek-V4-Pro");
        Assert.True(result);
    }
    // ─── ProcessTextAsync (4-param overload) ──────────────────────────
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyApiKey_ThrowsInvalidOperationException()
    {
        var emptyKeySettings = new DeepSeekSettings { ApiKey = string.Empty };
        var emptyOptionsMock = new Mock<IOptionsMonitor<DeepSeekSettings>>();
        emptyOptionsMock.Setup(o => o.CurrentValue).Returns(emptyKeySettings);
        var service = new DeepSeekService(_loggerMock.Object, emptyOptionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyModelId_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync(string.Empty, "prompt", "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyPrompt_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", string.Empty, "data", CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FourParam_WithEmptyData_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", string.Empty, CancellationToken.None));
    }
    // ─── ProcessTextAsync (5-param overload with object settings) ─────
    [Fact]
    public async Task ProcessTextAsync_FiveParam_NullSettings_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", "data", null!, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WrongSettingsType_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", "data", new GeminiSettings(), CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyApiKey_ThrowsInvalidOperationException()
    {
        var emptyKeySettings = new DeepSeekSettings { ApiKey = string.Empty };
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", "data", emptyKeySettings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyModelId_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync(string.Empty, "prompt", "data", _settings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyPrompt_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", string.Empty, "data", _settings, CancellationToken.None));
    }
    [Fact]
    public async Task ProcessTextAsync_FiveParam_WithEmptyData_ThrowsArgumentException()
    {
        var service = new DeepSeekService(_loggerMock.Object, _optionsMock.Object, _httpClient);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("deepseek-v4-flash", "prompt", string.Empty, _settings, CancellationToken.None));
    }
}

