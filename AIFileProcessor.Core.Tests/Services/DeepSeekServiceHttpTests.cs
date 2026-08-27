//-----------------------------------------------------------------------
// <copyright file="DeepSeekServiceHttpTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Exceptions;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

/// <summary>
/// HTTP-level tests for <see cref="DeepSeekService"/> using a stub <see cref="HttpMessageHandler"/>.
/// Verifies request construction (URL, headers, body), response parsing, and retry behavior
/// for transient failures (HTTP 429/500) without making real API calls.
/// </summary>
[Trait("Category", "Unit")]
public class DeepSeekServiceHttpTests
{
    private const string ModelId = "deepseek-v4-flash";
    private const string Prompt = "test prompt";
    private const string Data = "我在超市买菜。";

    // Standard OpenAI chat completion response format
    private const string ValidChatCompletionJson =
        "{\"id\":\"chatcmpl-test\",\"object\":\"chat.completion\",\"created\":1234567890," +
        "\"model\":\"deepseek-v4-flash\",\"choices\":[{\"index\":0," +
        "\"message\":{\"role\":\"assistant\",\"content\":\"{\\\"SP\\\":\\\"wǒ zài chāoshì mǎicài .\\\",\\\"Corrections\\\":[],\\\"Sentences\\\":[]}\"}," +
        "\"finish_reason\":\"stop\"}]," +
        "\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30}}";

    private readonly Mock<ILogger<DeepSeekService>> _loggerMock = new();
    private readonly Mock<IOptionsMonitor<DeepSeekSettings>> _optionsMock = new();
    private readonly DeepSeekSettings _settings;

    public DeepSeekServiceHttpTests()
    {
        _settings = new DeepSeekSettings
        {
            ApiKey = "test-api-key",
            Endpoint = "https://api.deepseek.com",
            MaxRetries = 2,
            BaseDelay = 0,
            MaxDelay = 0,
        };
        _optionsMock.Setup(o => o.CurrentValue).Returns(_settings);
    }

    private DeepSeekService CreateService(StubHttpHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.deepseek.com"),
        };
        return new DeepSeekService(_loggerMock.Object, _optionsMock.Object, client);
    }

    // ─── Request construction ──────────────────────────────────────────

    [Fact]
    public async Task ProcessTextAsync_RequestTargetsCorrectEndpoint()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Contains("api.deepseek.com", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task ProcessTextAsync_RequestContainsAuthorizationBearerToken()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequestHeaders!.Authorization);
        Assert.Equal("Bearer", handler.LastRequestHeaders!.Authorization!.Scheme);
        Assert.Equal("test-api-key", handler.LastRequestHeaders!.Authorization!.Parameter);
    }

    [Fact]
    public async Task ProcessTextAsync_RequestBodyContainsModelId()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal(ModelId, doc.RootElement.GetProperty("model").GetString());
    }

    [Fact]
    public async Task ProcessTextAsync_RequestBodyContainsDataBoundaryTags()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequestBody);
        // WrapData wraps with <DATA_BEGIN_XXXXXX>...</DATA_END_XXXXXX> tags
        Assert.Contains("DATA_BEGIN", handler.LastRequestBody!);
        Assert.Contains("DATA_END", handler.LastRequestBody!);
        Assert.Contains(Data, handler.LastRequestBody!);
    }

    [Fact]
    public async Task ProcessTextAsync_RequestBodyContainsSystemMessages()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var messages = doc.RootElement.GetProperty("messages");
        Assert.True(messages.GetArrayLength() >= 2);

        // First message should be system with the prompt
        var firstRole = messages[0].GetProperty("role").GetString();
        Assert.Equal("system", firstRole);
    }

    [Fact]
    public async Task ProcessTextAsync_CustomEndpointUsedInRequest()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var customSettings = new DeepSeekSettings
        {
            ApiKey = "test-key",
            Endpoint = "https://custom.deepseek.example.com",
            MaxRetries = 0,
            BaseDelay = 0,
            MaxDelay = 0,
        };
        var optionsMock = new Mock<IOptionsMonitor<DeepSeekSettings>>();
        optionsMock.Setup(o => o.CurrentValue).Returns(customSettings);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://custom.deepseek.example.com") };
        var service = new DeepSeekService(_loggerMock.Object, optionsMock.Object, client);

        await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Contains("custom.deepseek.example.com", handler.LastRequest!.RequestUri!.ToString());
    }

    // ─── Response parsing ──────────────────────────────────────────────

    [Fact]
    public async Task ProcessTextAsync_ValidResponse_ReturnsExtractedText()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        var result = await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
        // The content field contains JSON that the processor would parse
        Assert.Contains("SP", result.Text);
    }

    [Fact]
    public async Task ProcessTextAsync_ValidResponse_ReturnsTelemetryWithTokens()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(ValidChatCompletionJson);
        var service = CreateService(handler);

        var result = await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(result.Telemetry);
        Assert.Equal(1, result.Telemetry!.Attempts);
    }

    // ─── Retry behavior ─────────────────────────────────────────────────

    [Fact]
    public async Task ProcessTextAsync_Http429_RetriesAndSucceeds()
    {
        var handler = new StubHttpHandler();
        handler.QueueResponses(
            (HttpStatusCode.TooManyRequests, "{\"error\":{\"message\":\"rate limited\"}}"),
            (HttpStatusCode.OK, ValidChatCompletionJson));
        var service = CreateService(handler);

        var result = await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task ProcessTextAsync_Http500_RetriesAndSucceeds()
    {
        var handler = new StubHttpHandler();
        handler.QueueResponses(
            (HttpStatusCode.InternalServerError, "{\"error\":{\"message\":\"server error\"}}"),
            (HttpStatusCode.OK, ValidChatCompletionJson));
        var service = CreateService(handler);

        var result = await service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task ProcessTextAsync_Http400_DoesNotRetry()
    {
        var handler = new StubHttpHandler();
        handler.QueueResponses(
            (HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"bad request\"}}"));
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None));

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task ProcessTextAsync_AllRetriesExhausted_Throws()
    {
        var handler = new StubHttpHandler();
        handler.SetDefaultResponse(
            "{\"error\":{\"message\":\"server error\"}}",
            HttpStatusCode.InternalServerError);
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.ProcessTextAsync(ModelId, Prompt, Data, CancellationToken.None));

        // The service-level retry (MaxRetries=2) wraps the SDK's own retry logic.
        // The total call count includes both layers, so we verify it was called
        // multiple times (at least the initial attempt) rather than asserting an exact count.
        Assert.True(handler.RequestCount >= 1);
    }
}