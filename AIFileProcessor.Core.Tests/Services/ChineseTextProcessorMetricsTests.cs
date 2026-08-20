namespace AIFileProcessor.Core.Tests.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Configuration;
using AIFileProcessor.Response;
using AIFileProcessor.Telemetry;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
[Trait("Category", "Integration")]
public class ChineseTextProcessorMetricsTests
{
    private const string ValidJson = "{\"SP\":\"scratch\",\"Corrections\":[],\"Sentences\":[]}";
    [Fact]
    public async Task ProcessChineseTextAsync_AttachesProcessingMetrics_WithCorrectAggregation()
    {
        // Arrange
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var modelId = "test-model";
        var prompt = "p";
        // Create three chunks of 40 lines each => 120 lines total
        var data = string.Join(Environment.NewLine, System.Linq.Enumerable.Range(1, 120));
        aiService.Setup(s => s.SupportsModel(modelId)).Returns(true);
        var callTelemetry = new CallTelemetry
        {
            StartUtc = DateTime.UtcNow.AddSeconds(-1),
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 1000,
            PromptTokens = 10,
            CompletionTokens = 20,
            Attempts = 1,
        };
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = ValidJson, Telemetry = callTelemetry });
        var processor = new ChineseTextProcessor(logger.Object, new[] { aiService.Object }, new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));
        // Act
        var result = await processor.ProcessChineseTextAsync(modelId, prompt, data, new ChunkingOptions(), CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Metrics);
        Assert.Equal(3, result.Metrics!.ChunkCount);
        Assert.Equal(3, result.Metrics.Succeeded);
        Assert.Equal(0, result.Metrics.Failed);
        Assert.Equal(30, result.Metrics.TotalPromptTokens);
        Assert.Equal(60, result.Metrics.TotalCompletionTokens);
        // EffectiveParallelism = sum(chunkElapsed) / elapsed(ms); each chunk elapsed = 1000ms
        // Since elapsed is small (run time), EffectiveParallelism should be > 0 and <= chunks count when run time >= sum, but assert it's computed.
        Assert.True(result.Metrics.EffectiveParallelism >= 0.0);
        // LinesPerSecond = lines / seconds; we can't assert exact time, but it should be > 0
        Assert.True(result.Metrics.LinesPerSecond > 0);
        // Chunks list should be ordered by index
        Assert.Equal(3, result.Metrics.Chunks.Count);
        Assert.Collection(result.Metrics.Chunks,
            c => Assert.Equal(0, c.ChunkIndex),
            c => Assert.Equal(1, c.ChunkIndex),
            c => Assert.Equal(2, c.ChunkIndex));
    }
    [Fact]
    public async Task ProcessChineseTextAsync_AttachesProcessingMetrics_PropagatesThroughOrchestrator()
    {
        // Arrange: create processor that returns an AiApiResponse with Metrics populated
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var modelId = "test-model";
        var prompt = "p";
        var data = "一行数据"; // single line -> 1 chunk
        aiService.Setup(s => s.SupportsModel(modelId)).Returns(true);
        var callTelemetry = new CallTelemetry
        {
            StartUtc = DateTime.UtcNow.AddMilliseconds(-100),
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 100,
            PromptTokens = null,
            CompletionTokens = null,
            Attempts = 1,
        };
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = ValidJson, Telemetry = callTelemetry });
        var aiResponseWithMetrics = new AiApiResponse { Scratchpad = "scratch" };
        // Attach simple metrics as would be produced by ChineseTextProcessor
        aiResponseWithMetrics.Metrics = new ProcessingMetrics
        {
            StartUtc = DateTime.UtcNow.AddMilliseconds(-200),
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 200,
            ChunkCount = 1,
            Succeeded = 1,
            Failed = 0,
            TotalPromptTokens = null,
            TotalCompletionTokens = null,
            EffectiveParallelism = 0.5,
            LinesPerSecond = 5.0,
            Chunks = new List<ChunkMetrics> { new ChunkMetrics { ChunkIndex = 0, Telemetry = callTelemetry } },
        };
        var chineseProcessorMock = new Mock<IChineseTextProcessor>();
        chineseProcessorMock.Setup(p => p.ProcessChineseTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, IAiServiceSettings>>(), It.IsAny<ChunkingOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponseWithMetrics);
        // Create orchestrator via existing factory mocking used in ProcessingOrchestratorTests
        var settingsMock = new Mock<ISettingsProvider>();
        settingsMock.Setup(s => s.GetGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AIFileProcessor.Configuration.GeminiSettings());
        settingsMock.Setup(s => s.GetOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AIFileProcessor.Configuration.OpenAiSettings());
        settingsMock.Setup(s => s.GetDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AIFileProcessor.Configuration.DeepSeekSettings());
        settingsMock.Setup(s => s.GetGeminiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AIFileProcessor.Configuration.GeminiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        var userCtxMock = new Mock<IUserContext>();
        userCtxMock.Setup(u => u.UserId).Returns("test-user");
        var aiServices = new Mock<IAiService>();
        aiServices.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);
        Func<IChineseTextProcessor> factory = () => chineseProcessorMock.Object;
        var orchestrator = new AIFileProcessor.Processing.ProcessingOrchestrator(factory, settingsMock.Object, userCtxMock.Object, new Mock<ILogger<AIFileProcessor.Processing.ProcessingOrchestrator>>().Object, new[] { aiServices.Object }, new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));
        var options = new AIFileProcessor.Models.ProcessingOptions
        {
            ModelId = modelId,
            Prompt = prompt,
            Data = data,
            IdPrefix = "S",
            IdStart = 1,
            IdPad = 3,
            LengthPad = 3,
        };
        // Act
        var processingResult = await orchestrator.ProcessAsync(options, CancellationToken.None);
        // Assert
        Assert.NotNull(processingResult);
        Assert.NotNull(processingResult.Metrics);
        Assert.Equal(1, processingResult.Metrics!.ChunkCount);
        // EffectiveParallelism should match the mocked value we attached
        Assert.Equal(0.5, processingResult.Metrics.EffectiveParallelism);
        Assert.Equal(5.0, processingResult.Metrics.LinesPerSecond);
    }
}

