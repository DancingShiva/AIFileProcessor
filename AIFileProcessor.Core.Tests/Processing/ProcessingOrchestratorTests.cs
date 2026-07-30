namespace AIFileProcessor.Core.Tests.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Response;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using Times = Moq.Times;

    public class ProcessingOrchestratorTests
    {
        private static ProcessingOptions CreateValidOptions() => new()
        {
            ModelId = "test-model",
            Prompt = "test prompt",
            Data = "test data",
            IdPrefix = "S",
            IdStart = 1,
            IdPad = 3,
            LengthPad = 3,
        };

        private static AiApiResponse CreateApiResponse() => new()
        {
            Scratchpad = "scratch",
            Corrections = new List<string> { "fix1" },
            ProcessedSentenceRows = new List<ProcessedSentenceRow>
            {
                new()
                {
                    Chinese = "你好",
                    Pinyin = "ni hao",
                    English = "hello",
                    Breakdown = new List<BreakdownEntry>
                    {
                        new()
                        {
                            Chinese = "你",
                            Pinyin = "ni",
                            Gloss = "you",
                            PartOfSpeech = "pron",
                            HskLevel = "1",
                            Note = "",
                        },
                    },
                    AudioReordering = new List<AudioReorder>
                    {
                        new() { VisualString = "你好", StartIndex = 0, EndIndex = 1 },
                    },
                },
            },
        };

        private static (Mock<ISettingsProvider> settings, Mock<IUserContext> userCtx, Mock<IChineseTextProcessor> processor, Func<IChineseTextProcessor> factory) CreateMocks(AiApiResponse? response = null)
        {
            response ??= CreateApiResponse();

            var settingsMock = new Mock<ISettingsProvider>();
            settingsMock.Setup(s => s.GetGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeminiSettings());
            settingsMock.Setup(s => s.GetOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenAiSettings());
            settingsMock.Setup(s => s.GetDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeepSeekSettings());

            var userCtxMock = new Mock<IUserContext>();
            userCtxMock.Setup(u => u.UserId).Returns("test-user");

            var processorMock = new Mock<IChineseTextProcessor>();
            processorMock.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            Func<IChineseTextProcessor> factory = () => processorMock.Object;

            return (settingsMock, userCtxMock, processorMock, factory);
        }

        private static ProcessingOrchestrator CreateOrchestrator(
            Mock<ISettingsProvider> settings,
            Mock<IUserContext> userCtx,
            Func<IChineseTextProcessor> factory)
        {
            return new ProcessingOrchestrator(factory, settings.Object, userCtx.Object, new Mock<ILogger<ProcessingOrchestrator>>().Object);
        }

        // ---- Constructor tests ----

        [Fact]
        public void Constructor_NullFactory_ThrowsArgumentNullException()
        {
            var (settings, userCtx, _, _) = CreateMocks();

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessingOrchestrator(null!, settings.Object, userCtx.Object, new Mock<ILogger<ProcessingOrchestrator>>().Object));
        }

        [Fact]
        public void Constructor_NullSettingsProvider_ThrowsArgumentNullException()
        {
            var (_, userCtx, _, factory) = CreateMocks();

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessingOrchestrator(factory, null!, userCtx.Object, new Mock<ILogger<ProcessingOrchestrator>>().Object));
        }

        [Fact]
        public void Constructor_NullUserContext_ThrowsArgumentNullException()
        {
            var (settings, _, _, factory) = CreateMocks();

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessingOrchestrator(factory, settings.Object, null!, new Mock<ILogger<ProcessingOrchestrator>>().Object));
        }

        [Fact]
        public void Constructor_AllArgumentsValid_CreatesInstance()
        {
            var (settings, userCtx, _, factory) = CreateMocks();

            var orchestrator = CreateOrchestrator(settings, userCtx, factory);

            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            var (settings, userCtx, _, factory) = CreateMocks();

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessingOrchestrator(factory, settings.Object, userCtx.Object, null!));
        }

        // ---- ProcessAsync tests ----

        [Fact]
        public async Task ProcessAsync_NullOptions_ThrowsArgumentNullException()
        {
            var (settings, userCtx, _, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                orchestrator.ProcessAsync(null!, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ProcessAsync_ValidOptions_ReturnsProcessingResultWithRawResponse()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.NotEmpty(result.RawResponse);
            Assert.Equal("scratch", result.Scratchpad);
            Assert.Single(result.Corrections);
            Assert.Equal("fix1", result.Corrections[0]);
            Assert.Single(result.ProcessedSentenceRows);
        }

        [Fact]
        public async Task ProcessAsync_InvokesSettingsProviderForAllProviders()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            settings.Verify(s => s.GetGeminiSettingsAsync("test-user", It.IsAny<CancellationToken>()), Times.Once);
            settings.Verify(s => s.GetOpenAiSettingsAsync("test-user", It.IsAny<CancellationToken>()), Times.Once);
            settings.Verify(s => s.GetDeepSeekSettingsAsync("test-user", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_InvokesFactoryAndProcessChineseText()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            processor.Verify(p => p.ProcessChineseTextAsync(
                "test-model", "test prompt", "test data",
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_PassesProviderSettingsDictionaryToProcessor()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            IDictionary<string, object>? capturedSettings = null;

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, IDictionary<string, object>, CancellationToken>(
                    (_, _, _, dict, _) => capturedSettings = dict)
                .ReturnsAsync(CreateApiResponse());

            await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.NotNull(capturedSettings);
            Assert.True(capturedSettings!.ContainsKey("Gemini"));
            Assert.True(capturedSettings.ContainsKey("OpenAI"));
            Assert.True(capturedSettings.ContainsKey("DeepSeek"));
            Assert.IsType<GeminiSettings>(capturedSettings["Gemini"]);
            Assert.IsType<OpenAiSettings>(capturedSettings["OpenAI"]);
            Assert.IsType<DeepSeekSettings>(capturedSettings["DeepSeek"]);
        }

        [Fact]
        public async Task ProcessAsync_InitializesIdsWithPrefixAndPadding()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.IdPrefix = " S ";
            options.IdStart = 5;
            options.IdPad = 4;

            var response = new AiApiResponse
            {
                ProcessedSentenceRows = new List<ProcessedSentenceRow>
                {
                    new() { Chinese = "?", Pinyin = "yi", English = "one", Breakdown = new List<BreakdownEntry>(), AudioReordering = new List<AudioReorder>() },
                    new() { Chinese = "?", Pinyin = "�r", English = "two", Breakdown = new List<BreakdownEntry>(), AudioReordering = new List<AudioReorder>() },
                },
            };

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.Equal("S0005", result.ProcessedSentenceRows[0].Id);
            Assert.Equal("S0006", result.ProcessedSentenceRows[1].Id);
        }

        [Fact]
        public async Task ProcessAsync_ComputesLengthForRows()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.LengthPad = 3;

            var response = CreateApiResponse();
            response.ProcessedSentenceRows[0].Chinese = "你好世界";

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.Equal("004Chars", result.ProcessedSentenceRows[0].Length);
        }

        [Fact]
        public async Task ProcessAsync_RawResponseContainsTabDelimitedFields()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.IdPrefix = "T";
            options.IdStart = 1;
            options.IdPad = 3;

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            string[] lines = result.RawResponse.Split(Environment.NewLine);
            Assert.Single(lines);
            string[] fields = lines[0].Split('\t');
            Assert.Equal("T001", fields[0]);
            Assert.Equal("你好", fields[1]);
            Assert.Equal("ni hao", fields[2]);
            Assert.Equal("hello", fields[5]);
        }

        [Fact]
        public async Task ProcessAsync_RawResponseBreakdownContainsNoteAsSpaceWhenEmpty()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            var response = CreateApiResponse();
            response.ProcessedSentenceRows[0].Breakdown[0].Note = "";

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            string[] lines = result.RawResponse.Split(Environment.NewLine);
            string[] fields = lines[0].Split('\t');
            string breakdownStr = fields[3];
            string[] breakdownEntries = breakdownStr.Split("||");
            string[] parts = breakdownEntries[0].Split('|');
            Assert.Equal(" ", parts[5]);
        }

        [Fact]
        public async Task ProcessAsync_RawResponseBreakdownContainsNoteWhenNonEmpty()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            var response = CreateApiResponse();
            response.ProcessedSentenceRows[0].Breakdown[0].Note = "special note";

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            string[] lines = result.RawResponse.Split(Environment.NewLine);
            string[] fields = lines[0].Split('\t');
            string breakdownStr = fields[3];
            string[] breakdownEntries = breakdownStr.Split("||");
            string[] parts = breakdownEntries[0].Split('|');
            Assert.Equal("special note", parts[5]);
        }

        [Fact]
        public async Task ProcessAsync_RawResponseContainsAudioReorderingData()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            string[] lines = result.RawResponse.Split(Environment.NewLine);
            string[] fields = lines[0].Split('\t');
            string audioReorderStr = fields[4];
            Assert.Contains("你好|0,1", audioReorderStr);
        }

        [Fact]
        public async Task ProcessAsync_MultipleRows_AllHaveIdsInitialized()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.IdPrefix = "R";
            options.IdStart = 10;
            options.IdPad = 3;

            var response = new AiApiResponse
            {
                ProcessedSentenceRows = new List<ProcessedSentenceRow>
                {
                    new() { Chinese = "?", Pinyin = "yi", English = "one", Breakdown = new List<BreakdownEntry>(), AudioReordering = new List<AudioReorder>() },
                    new() { Chinese = "?", Pinyin = "�r", English = "two", Breakdown = new List<BreakdownEntry>(), AudioReordering = new List<AudioReorder>() },
                },
            };

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.Equal("R010", result.ProcessedSentenceRows[0].Id);
            Assert.Equal("R011", result.ProcessedSentenceRows[1].Id);
            Assert.Equal(2, result.ProcessedSentenceRows.Count);
        }

        [Fact]
        public async Task ProcessAsync_EmptySentenceRows_ProducesEmptyRawResponse()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();

            var response = new AiApiResponse
            {
                ProcessedSentenceRows = new List<ProcessedSentenceRow>(),
            };

            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ProcessingResult result = await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.Equal(string.Empty, result.RawResponse);
            Assert.Empty(result.ProcessedSentenceRows);
        }

        [Fact]
        public async Task ProcessAsync_PassesCancellationTokenToSettingsAndProcessor()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            await orchestrator.ProcessAsync(options, token);

            settings.Verify(s => s.GetGeminiSettingsAsync("test-user", token), Times.Once);
            processor.Verify(p => p.ProcessChineseTextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IDictionary<string, object>>(), token), Times.Once);
        }

        // ---- MaxTokens clamping tests ----

        [Fact]
        public async Task ProcessAsync_MaxTokensExceedsHardMax_IsClampedForAllProviders()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.ModelMaxTokens["test-model"] = 524288; // exceeds 262144 hard max

            IDictionary<string, object>? captured = null;
            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, IDictionary<string, object>, CancellationToken>(
                    (_, _, _, dict, _) => captured = dict)
                .ReturnsAsync(CreateApiResponse());

            await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.NotNull(captured);
            Assert.Equal(262144, ((GeminiSettings)captured!["Gemini"]).MaxTokens);
            Assert.Equal(262144, ((OpenAiSettings)captured["OpenAI"]).MaxTokens);
            Assert.Equal(262144, ((DeepSeekSettings)captured["DeepSeek"]).MaxTokens);
        }

        [Fact]
        public async Task ProcessAsync_MaxTokensWithinHardMax_IsPassedThroughUnchanged()
        {
            var (settings, userCtx, processor, factory) = CreateMocks();
            var orchestrator = CreateOrchestrator(settings, userCtx, factory);
            var options = CreateValidOptions();
            options.ModelMaxTokens["test-model"] = 4096;

            IDictionary<string, object>? captured = null;
            processor.Setup(p => p.ProcessChineseTextAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, IDictionary<string, object>, CancellationToken>(
                    (_, _, _, dict, _) => captured = dict)
                .ReturnsAsync(CreateApiResponse());

            await orchestrator.ProcessAsync(options, TestContext.Current.CancellationToken);

            Assert.NotNull(captured);
            Assert.Equal(4096, ((GeminiSettings)captured!["Gemini"]).MaxTokens);
            Assert.Equal(4096, ((OpenAiSettings)captured["OpenAI"]).MaxTokens);
            Assert.Equal(4096, ((DeepSeekSettings)captured["DeepSeek"]).MaxTokens);
        }
    }
}