namespace AIFileProcessor.Core.Tests.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Response;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
[Trait("Category", "Integration")]
public class ChineseTextProcessorTests
{
    private readonly Mock<ILogger<ChineseTextProcessor>> _loggerMock = new();
    private readonly Mock<IAiService> _aiServiceMock = new();
    private const string ValidJson = "{\"SP\":\"wǒ zài chāoshì mǎicài .\",\"Corrections\":[],\"Sentences\":[]}";
    private const string ModelId = "test-model";
    private const string Prompt = "translate";
    private const string RealisticData = "我在超市买菜。";
    public ChineseTextProcessorTests()
    {
        _aiServiceMock.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIFileProcessor.Response.AiCallResult { Text = ValidJson });
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<IAiServiceSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIFileProcessor.Response.AiCallResult { Text = ValidJson });
    }
    private ChineseTextProcessor CreateProcessor() =>
        new(_loggerMock.Object, new[] { _aiServiceMock.Object });
    // --- ProcessChineseTextAsync (4-param overload, providerSettings = null) -----
    [Fact]
    public async Task ProcessChineseTextAsync_WithoutSettings_NullData_ReturnsEmptyResponse()
    {
        var processor = CreateProcessor();
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, null!, new ChunkingOptions(), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Scratchpad);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithoutSettings_WhitespaceData_ReturnsEmptyResponse()
    {
        var processor = CreateProcessor();
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, "   ", new ChunkingOptions(), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Scratchpad);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithoutSettings_ValidData_CallsDefaultOverload()
    {
        var processor = CreateProcessor();
        var data = RealisticData;
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<IAiServiceSettings>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithoutSettings_WithExplicitCancellationToken_Works()
    {
        var processor = CreateProcessor();
        var data = RealisticData;
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), TestContext.Current.CancellationToken);
        Assert.NotNull(result);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithoutSettings_NoMatchingModel_ThrowsNotSupportedException()
    {
        var processor = CreateProcessor();
        await Assert.ThrowsAsync<NotSupportedException>(
            () => processor.ProcessChineseTextAsync("unknown-model", Prompt, "data", new ChunkingOptions(), CancellationToken.None));
    }
    // --- ProcessChineseTextAsync (5-param overload, with providerSettings) ------
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_NullProviderSettings_ThrowsArgumentNullException()
    {
        var processor = CreateProcessor();
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, "data", null!, new ChunkingOptions(), CancellationToken.None));
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_UnmatchedProviderKey_FallsBackToDefaultOverload()
    {
        var processor = CreateProcessor();
        var data = RealisticData;
        // GetProviderKey returns null for Moq proxies, so the per-call settings
        // path is skipped and the default (constructor-injected) overload is used.
        var providerSettings = new Dictionary<string, IAiServiceSettings>
        {
            ["Gemini"] = new GeminiSettings { ApiKey = "test" },
        };
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, new ChunkingOptions(), CancellationToken.None);
        Assert.NotNull(result);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_WhitespaceData_ReturnsEmptyResponse()
    {
        var processor = CreateProcessor();
        var providerSettings = new Dictionary<string, IAiServiceSettings>();
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, "  ", providerSettings, new ChunkingOptions(), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Scratchpad);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_DefaultCancellationToken_Works()
    {
        var processor = CreateProcessor();
        var providerSettings = new Dictionary<string, IAiServiceSettings>();
        var data = "??";
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, new ChunkingOptions(), TestContext.Current.CancellationToken);
        Assert.NotNull(result);
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_NoMatchingModel_ThrowsNotSupportedException()
    {
        var processor = CreateProcessor();
        var providerSettings = new Dictionary<string, IAiServiceSettings>();
        await Assert.ThrowsAsync<NotSupportedException>(
            () => processor.ProcessChineseTextAsync("unknown-model", Prompt, "data", providerSettings, new ChunkingOptions(), CancellationToken.None));
    }
    [Fact]
    public async Task ProcessChineseTextAsync_WithSettings_SettingsWithoutMatchingKey_UsesDefaultOverload()
    {
        var processor = CreateProcessor();
        // Provider key won't match any entry, so perCallSettings stays null and default overload is used
        var providerSettings = new Dictionary<string, IAiServiceSettings>
        {
            ["UnknownProvider"] = new GeminiSettings { ApiKey = "test" },
        };
        var data = RealisticData;
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, new ChunkingOptions(), CancellationToken.None);
        Assert.NotNull(result);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}

