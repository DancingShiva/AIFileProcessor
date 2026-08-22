namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

[Trait("Category", "Unit")]
public class OllamaServiceTests
{
    private const string Prompt = "system prompt";
    private const string Data = "word";
    private const string ValidJsonResponse = "{\"result\": \"ok\"}";

    private static readonly string[] ExpectedStopSequences = ["</s>"];

    private readonly Mock<ILogger<OllamaService>> _loggerMock = new();
    private readonly Mock<IOptionsMonitor<OllamaSettings>> _optionsMock = new();
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly OllamaSettings _settings = new() { Endpoint = "http://localhost:11434" };

    public OllamaServiceTests()
    {
        _optionsMock.Setup(o => o.CurrentValue).Returns(_settings);
        _userContextMock.Setup(u => u.UserId).Returns("test-user");
        _settingsProviderMock
            .Setup(s => s.GetOllamaModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OllamaModelSettings());
    }

    private OllamaService CreateService(HttpMessageHandler handler)
    {
        return new OllamaService(
            _loggerMock.Object,
            _optionsMock.Object,
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") },
            _settingsProviderMock.Object,
            _userContextMock.Object);
    }

    // ─── Constructor ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new OllamaService(null!, _optionsMock.Object, new HttpClient(), _settingsProviderMock.Object, _userContextMock.Object));
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new OllamaService(_loggerMock.Object, null!, new HttpClient(), _settingsProviderMock.Object, _userContextMock.Object));
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new OllamaService(_loggerMock.Object, _optionsMock.Object, null!, _settingsProviderMock.Object, _userContextMock.Object));
    }

    [Fact]
    public void GetSupportedModels_NoCatalog_ReturnsEmpty()
    {
        var service = CreateService(new StubOkHandler());
        var ids = service.GetSupportedModels().Select(m => m.ModelId).ToList();

        Assert.Empty(ids);
    }

    [Fact]
    public async Task RefreshModelCatalogAsync_PopulatesFromLocalServer()
    {
        var tagsResponse = new
        {
            models = new[]
            {
                new { name = "qwen3:8b", model = "qwen3:8b", modified_at = DateTime.UtcNow, size = 1L, digest = "d" },
            },
        };
        var handler = new StubOkHandler();
        handler.SetResponse("/api/tags", JsonSerializer.Serialize(tagsResponse));

        var service = CreateService(handler);
        await service.RefreshModelCatalogAsync(TestContext.Current.CancellationToken);

        var ids = service.GetSupportedModels().Select(m => m.ModelId).ToList();
        Assert.Contains("qwen3:8b", ids);
    }

    [Fact]
    public async Task SupportsModel_AfterRefresh_ReturnsTrueForLocalModel()
    {
        var tagsResponse = new { models = new[] { new { name = "my-local:latest", model = "my-local:latest", modified_at = DateTime.UtcNow, size = 1L, digest = "d" } } };
        var handler = new StubOkHandler();
        handler.SetResponse("/api/tags", JsonSerializer.Serialize(tagsResponse));

        var service = CreateService(handler);
        await service.RefreshModelCatalogAsync(TestContext.Current.CancellationToken);

        Assert.True(service.SupportsModel("my-local:latest"));
    }

    // ─── Api key optionality ──────────────────────────────────────────

    [Fact]
    public async Task ProcessTextAsync_EmptyApiKey_DoesNotThrowApiKeyNotConfigured()
    {
        var noKeySettings = new OllamaSettings { ApiKey = string.Empty, Endpoint = "http://localhost:11434" };
        var emptyKeyOptions = new Mock<IOptionsMonitor<OllamaSettings>>();
        emptyKeyOptions.Setup(o => o.CurrentValue).Returns(noKeySettings);

        var handler = new StubOkHandler();
        handler.SetResponse("/api/chat", MakeChatResponse(ValidJsonResponse), appendNewline: true);

        var service = new OllamaService(
            _loggerMock.Object,
            emptyKeyOptions.Object,
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") },
            _settingsProviderMock.Object,
            _userContextMock.Object);

        var result = await service.ProcessTextAsync("test-model", Prompt, Data, noKeySettings, TestContext.Current.CancellationToken);

        Assert.Equal(ValidJsonResponse, result.Text);
    }

    [Fact]
    public async Task ProcessTextAsync_SendsOptionsAndFormatFromSettings()
    {
        var handler = new StubOkHandler();
        handler.SetResponse("/api/chat", MakeChatResponse(ValidJsonResponse), appendNewline: true);

        var testSettings = new OllamaSettings
        {
            Endpoint = "http://localhost:11434",
            Temperature = 0.5,
            TopP = 0.9,
            TopK = 7,
            ContextWindow = 4096,
            MaxTokens = 512,
            RepeatPenalty = 1.1,
            Seed = 42,
            MinP = 0.05,
        };

        var service = CreateService(handler);
        await service.ProcessTextAsync("test-model", Prompt, Data, testSettings, TestContext.Current.CancellationToken);

        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;

        Assert.Equal("test-model", root.GetProperty("model").GetString());
        Assert.Equal("json", root.GetProperty("format").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());

        var options = root.GetProperty("options");
        Assert.Equal(0.5f, options.GetProperty("temperature").GetSingle());
        Assert.Equal(0.9f, options.GetProperty("top_p").GetSingle());
        Assert.Equal(7, options.GetProperty("top_k").GetInt32());
        Assert.Equal(4096, options.GetProperty("num_ctx").GetInt32());
        Assert.Equal(512, options.GetProperty("num_predict").GetInt32());
        Assert.Equal(1.1f, options.GetProperty("repeat_penalty").GetSingle());
        Assert.Equal(42, options.GetProperty("seed").GetInt32());
    }

    [Fact]
    public async Task ProcessTextAsync_NullOptionValues_AreOmitted()
    {
        var handler = new StubOkHandler();
        handler.SetResponse("/api/chat", MakeChatResponse(ValidJsonResponse), appendNewline: true);

        var service = CreateService(handler);
        await service.ProcessTextAsync("test-model", Prompt, Data, new OllamaSettings { Endpoint = "http://localhost:11434" }, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        JsonElement options = doc.RootElement.GetProperty("options");

        Assert.False(options.TryGetProperty("temperature", out _));
        Assert.False(options.TryGetProperty("top_k", out _));
        Assert.False(options.TryGetProperty("num_ctx", out _));
        Assert.False(options.TryGetProperty("num_predict", out _));
    }

    [Fact]
    public async Task ProcessTextAsync_WithApiKey_SendsBearerHeader()
    {
        var handler = new StubOkHandler();
        handler.SetResponse("/api/chat", MakeChatResponse(ValidJsonResponse), appendNewline: true);

        var cloudSettings = new OllamaSettings { ApiKey = "cloud-key", Endpoint = "http://localhost:11434" };
        var service = CreateService(handler);

        await service.ProcessTextAsync("cloud-model", Prompt, Data, cloudSettings, CancellationToken.None);

        Assert.NotNull(handler.LastRequestHeaders.Authorization);
        Assert.Equal("Bearer", handler.LastRequestHeaders.Authorization!.Scheme);
        Assert.Equal("cloud-key", handler.LastRequestHeaders.Authorization.Parameter);
    }

    [Fact]
    public async Task ProcessTextAsync_EmptyModelId_ThrowsArgumentException()
    {
        var service = CreateService(new StubOkHandler());
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync(string.Empty, Prompt, Data, _settings, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ProcessTextAsync_WrongSettingsType_ThrowsArgumentException()
    {
        var service = CreateService(new StubOkHandler());
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessTextAsync("test-model", Prompt, Data, new GeminiSettings(), TestContext.Current.CancellationToken));
    }

    // ─── Options mapping ─────────────────────────────────────────────

    [Fact]
    public void BuildOptions_MapsAllTypedProperties()
    {
        var settings = new OllamaSettings
        {
            Temperature = 0.7,
            TopP = 0.95,
            TopK = 40,
            ContextWindow = 8192,
            MaxTokens = 2048,
            RepeatPenalty = 1.1,
            Seed = 123,
            Stop = ["</s>"],
            MinP = 0.05,
            TypicalP = 0.7,
            TfsZ = 1.0,
            MiroStat = 2,
            MiroStatTau = 5.0,
            MiroStatEta = 0.1,
            PresencePenalty = 0.5,
            FrequencyPenalty = 0.3,
            RepeatLastN = 64,
            NumGpu = 20,
            NumBatch = 512,
            NumThread = 8,
            NumKeep = 4,
            Numa = true,
            UseMlock = true,
            UseMmap = false,
            LowVram = true,
        };

        var options = OllamaService.BuildOptions(settings);

        Assert.Equal(0.7f, options.Temperature);
        Assert.Equal(0.95f, options.TopP);
        Assert.Equal(40, options.TopK);
        Assert.Equal(8192, options.NumCtx);
        Assert.Equal(2048, options.NumPredict);
        Assert.Equal(1.1f, options.RepeatPenalty);
        Assert.Equal(123, options.Seed);
        Assert.Equal(ExpectedStopSequences, options.Stop);
        Assert.Equal(0.05f, options.MinP);
        Assert.Equal(0.7f, options.TypicalP);
        Assert.Equal(1.0f, options.TfsZ);
        Assert.Equal(2, options.MiroStat);
        Assert.Equal(5.0f, options.MiroStatTau);
        Assert.Equal(0.1f, options.MiroStatEta);
        Assert.Equal(0.5f, options.PresencePenalty);
        Assert.Equal(0.3f, options.FrequencyPenalty);
        Assert.Equal(64, options.RepeatLastN);
        Assert.Equal(20, options.NumGpu);
        Assert.Equal(512, options.NumBatch);
        Assert.Equal(8, options.NumThread);
        Assert.Equal(4, options.NumKeep);
        Assert.True(options.Numa);
        Assert.True(options.UseMlock);
        Assert.False(options.UseMmap);
        Assert.True(options.LowVram);
    }

    // ─── Helpers ───────────────────────────────────────────────────────

    private static string MakeChatResponse(string content)
    {
        var response = new
        {
            model = "test-model",
            created_at = DateTime.UtcNow.ToString("o"),
            message = new { role = "assistant", content },
            done_reason = "stop",
            done = true,
            total_duration = 1L,
            load_duration = 1L,
            prompt_eval_count = 10,
            prompt_eval_duration = 1L,
            eval_count = 5,
            eval_duration = 1L,
        };

        return JsonSerializer.Serialize(response);
    }

    private sealed class StubOkHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);

        public string? LastRequestBody { get; private set; }

        public System.Net.Http.Headers.HttpRequestHeaders LastRequestHeaders { get; private set; } = new HttpClient().DefaultRequestHeaders;

        public void SetResponse(string path, string body, bool appendNewline = false)
        {
            _responses[path] = appendNewline ? body + "\n" : body;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            LastRequestHeaders = request.Headers;

            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var body = _responses.TryGetValue(path, out var configured)
                ? configured
                : "{}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }
    }
}
