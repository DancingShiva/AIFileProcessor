//-----------------------------------------------------------------------
// <copyright file="CancellationPropagationTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Processing;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Cancellation propagation tests through the full pipeline:
/// Orchestrator → ChineseTextProcessor → IAiService.
/// Verifies that a CancellationToken flows through every layer and that
/// a pre-cancelled or mid-processing cancellation throws OperationCanceledException.
/// </summary>
[Trait("Category", "Unit")]
public class CancellationPropagationTests
{
    private static ProcessingOptions CreateValidOptions() => new()
    {
        ModelId = "test-model",
        Prompt = "test prompt",
        Data = "我在超市买菜。",
        IdPrefix = "S",
        IdStart = 1,
        IdPad = 3,
        LengthPad = 3,
    };

    // ─── Helper: create mocks for Orchestrator-level tests ─────────────

    private static (Mock<ISettingsProvider> settings, Mock<IUserContext> userCtx, Mock<IChineseTextProcessor> processor, Func<IChineseTextProcessor> factory, Mock<IAiService> aiService) CreateMocks(AiApiResponse? response = null)
    {
        response ??= TestDataHelper.CreateSupermarketResponse();
        var settingsMock = new Mock<ISettingsProvider>();
        settingsMock.Setup(s => s.GetGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeminiSettings());
        settingsMock.Setup(s => s.GetOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenAiSettings());
        settingsMock.Setup(s => s.GetDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeepSeekSettings());
        settingsMock.Setup(s => s.GetGeminiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeminiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        settingsMock.Setup(s => s.GetOpenAiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenAiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        settingsMock.Setup(s => s.GetDeepSeekModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeepSeekModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        settingsMock.Setup(s => s.GetProcessingSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingSettings());
        var userCtxMock = new Mock<IUserContext>();
        userCtxMock.Setup(u => u.UserId).Returns("test-user");
        var processorMock = new Mock<IChineseTextProcessor>();
        processorMock.Setup(p => p.ProcessChineseTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IDictionary<string, IAiServiceSettings>>(), It.IsAny<ChunkingOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);
        Func<IChineseTextProcessor> factory = () => processorMock.Object;
        return (settingsMock, userCtxMock, processorMock, factory, aiServiceMock);
    }

    private static ProcessingOrchestrator CreateOrchestrator(
        Mock<ISettingsProvider> settings,
        Mock<IUserContext> userCtx,
        Func<IChineseTextProcessor> factory,
        Mock<IAiService>? aiService = null)
    {
        var aiServices = aiService != null ? new[] { aiService.Object } : Array.Empty<IAiService>();
        return new ProcessingOrchestrator(
            factory,
            settings.Object,
            userCtx.Object,
            new Mock<ILogger<ProcessingOrchestrator>>().Object,
            aiServices,
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));
    }

    // ─── Orchestrator: ct forwarded to IChineseTextProcessor ───────────

    [Fact]
    public async Task ProcessAsync_ForwardsCancellationToken_ToChineseTextProcessor()
    {
        var (settings, userCtx, processor, factory, aiService) = CreateMocks();
        var orchestrator = CreateOrchestrator(settings, userCtx, factory, aiService);
        var options = CreateValidOptions();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await orchestrator.ProcessAsync(options, token);

        processor.Verify(p => p.ProcessChineseTextAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, IAiServiceSettings>>(),
            It.IsAny<ChunkingOptions>(),
            token), Times.Once);
    }

    // ─── Orchestrator: ct forwarded to ISettingsProvider ───────────────

    [Fact]
    public async Task ProcessAsync_ForwardsCancellationToken_ToSettingsProvider()
    {
        var (settings, userCtx, _, factory, aiService) = CreateMocks();
        var orchestrator = CreateOrchestrator(settings, userCtx, factory, aiService);
        var options = CreateValidOptions();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await orchestrator.ProcessAsync(options, token);

        // The orchestrator detects provider via aiService.SupportsModel, then calls
        // GetGeminiModelSettingsAsync (since the mock aiService returns true for any model,
        // GetProviderKey returns null for the Moq proxy, triggering the Gemini fallback path).
        settings.Verify(
            s => s.GetGeminiModelSettingsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                token),
            Times.AtLeastOnce);
    }

    // ─── Orchestrator: pre-cancelled token throws OperationCanceledException ─

    [Fact]
    public async Task ProcessAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        var (settings, userCtx, processor, factory, aiService) = CreateMocks();

        // Make the processor throw OperationCanceledException when called with a cancelled token
        processor.Setup(p => p.ProcessChineseTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IDictionary<string, IAiServiceSettings>>(), It.IsAny<ChunkingOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var orchestrator = CreateOrchestrator(settings, userCtx, factory, aiService);
        var options = CreateValidOptions();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => orchestrator.ProcessAsync(options, cts.Token));
    }

    // ─── ChineseTextProcessor: ct forwarded to IAiService ──────────────

    [Fact]
    public async Task ProcessChineseTextAsync_ForwardsCancellationToken_ToAiService()
    {
        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);
        var validJson = "{\"SP\":\"\",\"Corrections\":[],\"Sentences\":[]}";
        aiServiceMock
            .Setup(s => s.ProcessTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = validJson });
        aiServiceMock
            .Setup(s => s.ProcessTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IAiServiceSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = validJson });

        var processingLimits = new ProcessingLimits(
            new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
            {
                ChunkMaxRetries = 0,
                ChunkBaseDelaySeconds = 1,
                ChunkMaxDelaySeconds = 1,
                RetryJitterSeconds = 0,
            }));

        var processor = new ChineseTextProcessor(
            new Mock<ILogger<ChineseTextProcessor>>().Object,
            new[] { aiServiceMock.Object },
            processingLimits);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await processor.ProcessChineseTextAsync(
            "test-model", "prompt", "我在超市买菜。",
            new Dictionary<string, IAiServiceSettings>(),
            new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 1 },
            token);

        // GetProviderKey returns null for Moq proxies, so the 4-param overload
        // (without IAiServiceSettings) is the one called. Verify the token was forwarded.
        aiServiceMock.Verify(s => s.ProcessTextAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            token), Times.Once);
    }

    // ─── ChineseTextProcessor: pre-cancelled token throws ──────────────

    [Fact]
    public async Task ProcessChineseTextAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);

        var processingLimits = new ProcessingLimits(
            new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
            {
                ChunkMaxRetries = 0,
                ChunkBaseDelaySeconds = 1,
                ChunkMaxDelaySeconds = 1,
                RetryJitterSeconds = 0,
            }));

        var processor = new ChineseTextProcessor(
            new Mock<ILogger<ChineseTextProcessor>>().Object,
            new[] { aiServiceMock.Object },
            processingLimits);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessChineseTextAsync(
                "test-model", "prompt", "我在超市买菜。",
                new Dictionary<string, IAiServiceSettings>(),
                new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 1 },
                cts.Token));
    }

    // ─── ChineseTextProcessor: cancellation during chunk processing propagates ─

    [Fact]
    public async Task ProcessChineseTextAsync_AiServiceThrowsCancellation_PropagatesWithoutRetry()
    {
        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);

        // The AI service throws OperationCanceledException — the chunk loop's
        // catch (OperationCanceledException) { throw; } must re-throw it
        // without swallowing it or retrying.
        aiServiceMock
            .Setup(s => s.ProcessTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        aiServiceMock
            .Setup(s => s.ProcessTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IAiServiceSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var processingLimits = new ProcessingLimits(
            new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
            {
                ChunkMaxRetries = 3,
                ChunkBaseDelaySeconds = 1,
                ChunkMaxDelaySeconds = 1,
                RetryJitterSeconds = 0,
            }));

        var processor = new ChineseTextProcessor(
            new Mock<ILogger<ChineseTextProcessor>>().Object,
            new[] { aiServiceMock.Object },
            processingLimits);

        // Use a non-cancelled token — the cancellation comes from the AI service,
        // not from the caller. The processor must propagate it.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessChineseTextAsync(
                "test-model", "prompt", "我在超市买菜。",
                new Dictionary<string, IAiServiceSettings>(),
                new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 1 },
                CancellationToken.None));

        // Verify the service was called exactly once — no retries on cancellation.
        // GetProviderKey returns null for Moq proxies, so the 4-param overload
        // (without IAiServiceSettings) is the one called.
        aiServiceMock.Verify(s => s.ProcessTextAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        aiServiceMock.Verify(s => s.ProcessTextAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IAiServiceSettings>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}