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

        // Configure ProcessingLimits to use minimal delays so retry tests run instantly.
        _processingLimits = new ProcessingLimits(
            new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
            {
                ChunkMaxRetries = 3,
                ChunkBaseDelaySeconds = 1,
                ChunkMaxDelaySeconds = 1,
                RetryJitterSeconds = 0,
            }));
    }

    private ProcessingLimits _processingLimits = null!;

    private ChineseTextProcessor CreateProcessor() =>
        new(_loggerMock.Object, new[] { _aiServiceMock.Object }, _processingLimits);
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

    // --- Chunk-level retry tests ---------------------------------------------

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkFailsOnFirstAttempt_SucceedsOnRetry()
    {
        var callCount = 0;
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, string _, string _, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Transient failure");
                }
                return new AiCallResult { Text = ValidJson };
            });

        var processor = CreateProcessor();
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, new ChunkingOptions(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Scratchpad);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkFailsAfterAllRetries_ThrowsInvalidOperationException()
    {
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Persistent failure"));

        var processor = CreateProcessor();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, new ChunkingOptions(), CancellationToken.None));

        Assert.Contains("Processing failed for lines", ex.Message);
        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkRetryRecordsAttemptsInTelemetry()
    {
        var callCount = 0;
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, string _, string _, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Transient failure");
                }
                return new AiCallResult { Text = ValidJson };
            });

        var processor = CreateProcessor();
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, new ChunkingOptions(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Metrics);
        Assert.Single(result.Metrics.Chunks);
        Assert.Equal(1, result.Metrics.Chunks[0].RetryAttempts);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_CancellationDuringChunkRetry_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, string _, string _, CancellationToken ct) =>
            {
                cts.Cancel();
                throw new OperationCanceledException(ct);
            });

        var processor = CreateProcessor();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, new ChunkingOptions(), cts.Token));

        // Verify only the initial attempt was made (no retries for cancellation)
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

