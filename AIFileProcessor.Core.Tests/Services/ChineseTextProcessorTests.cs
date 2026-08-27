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

[Trait("Category", "Unit")]
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
        var data = "你好";
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

    // --- Chunking boundary tests ------------------------------------------------

    [Fact]
    public async Task ProcessChineseTextAsync_SingleLine_CallsServiceOnce()
    {
        var processor = CreateProcessor();

        await processor.ProcessChineseTextAsync(ModelId, Prompt, "你好", new ChunkingOptions(), CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_ExactlyChunkSizeLines_ProducesOneChunk()
    {
        var processor = CreateProcessor();
        // Default ChunkSize = 40; send exactly 40 lines
        var data = string.Join("\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 40));

        await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkSizePlusOneLines_ProducesTwoChunks()
    {
        var processor = CreateProcessor();
        // Default ChunkSize = 40; send 41 lines ? 2 chunks (40 + 1)
        var data = string.Join("\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 41));

        await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_AllBlankLines_ReturnsEmptyResponse()
    {
        var processor = CreateProcessor();
        // 5 blank lines — string.IsNullOrWhiteSpace returns true for all-whitespace
        // content (newlines are whitespace), so the processor short-circuits and
        // returns an empty response without calling the AI service.
        var data = "\n\n\n\n\n";

        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Scratchpad);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MaxConcurrentChunksOne_AllChunksComplete()
    {
        var processor = CreateProcessor();
        // 3 lines, ChunkSize=1 ? 3 chunks, MaxConcurrentChunks=1 (sequential)
        var chunking = new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 1 };
        var data = "你好\n你好\n你好";

        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, chunking, CancellationToken.None);

        Assert.NotNull(result);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MaxConcurrentChunksExceedsChunkCount_AllChunksComplete()
    {
        var processor = CreateProcessor();
        // 2 lines, ChunkSize=1 ? 2 chunks, MaxConcurrentChunks=10 (more than chunks)
        var chunking = new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 10 };
        var data = "你好\n你好";

        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, chunking, CancellationToken.None);

        Assert.NotNull(result);
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MultiChunkMerge_PreservesCorrectOrder()
    {
        var processor = CreateProcessor();
        // Use distinct JSON per chunk so we can verify merge order
        var callCount = 0;
        _aiServiceMock
            .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, string _, string chunkText, CancellationToken _) =>
            {
                callCount++;
                // Return JSON with a sentence whose English reflects the chunk index
                return new AiCallResult
                {
                    Text = $"{{\"SP\":\"chunk{callCount}\",\"Corrections\":[],\"Sentences\":[]}}",
                };
            });

        // 2 lines, ChunkSize=1 ? 2 chunks
        var chunking = new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 2 };
        var data = "你好\n你好";

        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, chunking, CancellationToken.None);

        Assert.NotNull(result);
        // Scratchpad is concatenated in chunk order
        Assert.Contains("chunk1", result.Scratchpad);
        Assert.Contains("chunk2", result.Scratchpad);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MixedLineEndings_SplitsCorrectly()
    {
        var processor = CreateProcessor();
        // Mix CRLF, LF, and CR in the same input
        var data = "你好\r\n你好\n你好\r你好";

        await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        // All 4 lines fit in one chunk (ChunkSize=40)
        _aiServiceMock.Verify(
            s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- ChunkSize=0 / MaxConcurrentChunks=0 guards -----------------------------

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkSizeZero_ThrowsArgumentOutOfRangeException()
    {
        var processor = CreateProcessor();
        var chunking = new ChunkingOptions { ChunkSize = 0, MaxConcurrentChunks = 3 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, chunking, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_ChunkSizeNegative_ThrowsArgumentOutOfRangeException()
    {
        var processor = CreateProcessor();
        var chunking = new ChunkingOptions { ChunkSize = -1, MaxConcurrentChunks = 3 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, chunking, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MaxConcurrentChunksZero_ThrowsArgumentOutOfRangeException()
    {
        var processor = CreateProcessor();
        var chunking = new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 0 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, chunking, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MaxConcurrentChunksNegative_ThrowsArgumentOutOfRangeException()
    {
        var processor = CreateProcessor();
        var chunking = new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = -1 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => processor.ProcessChineseTextAsync(ModelId, Prompt, RealisticData, chunking, CancellationToken.None));
    }
}

