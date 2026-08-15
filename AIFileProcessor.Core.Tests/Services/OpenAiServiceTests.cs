namespace AIFileProcessor.Core.Tests.Services
{
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

    public class OpenAiServiceTests
    {
        private readonly Mock<ILogger<OpenAiService>> _loggerMock = new();
        private readonly Mock<IOptions<OpenAiSettings>> _optionsMock = new();
        private readonly HttpClient _httpClient = new();
        private readonly OpenAiSettings _settings = new() { ApiKey = "test-key" };

        public OpenAiServiceTests()
        {
            _optionsMock.Setup(o => o.Value).Returns(_settings);
        }

        // ─── Constructor ──────────────────────────────────────────────────

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OpenAiService(null!, _optionsMock.Object, _httpClient));
        }

        [Fact]
        public void Constructor_NullSettings_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OpenAiService(_loggerMock.Object, null!, _httpClient));
        }

        [Fact]
        public void Constructor_SettingsWithNullValue_ThrowsArgumentNullException()
        {
            var nullOptionsMock = new Mock<IOptions<OpenAiSettings>>();
            nullOptionsMock.Setup(o => o.Value).Returns((OpenAiSettings)null!);

            Assert.Throws<ArgumentNullException>(
                () => new OpenAiService(_loggerMock.Object, nullOptionsMock.Object, _httpClient));
        }

        [Fact]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OpenAiService(_loggerMock.Object, _optionsMock.Object, null!));
        }

        [Fact]
        public void Constructor_ValidArguments_CreatesInstance()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            Assert.NotNull(service);
        }

        // ─── GetSupportedModels ───────────────────────────────────────────

        [Fact]
        public void GetSupportedModels_ReturnsNonEmptyCollection()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            IEnumerable<AiModelInfo> models = service.GetSupportedModels();

            Assert.NotEmpty(models);
        }

        [Fact]
        public void GetSupportedModels_ContainsExpectedModelIds()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            IEnumerable<AiModelInfo> models = service.GetSupportedModels();
            var modelIds = models.Select(m => m.ModelId).ToList();

            Assert.Contains("gpt-5.5", modelIds);
            Assert.Contains("gpt-5.4", modelIds);
        }

        // ─── SupportsModel ────────────────────────────────────────────────

        [Fact]
        public void SupportsModel_NullInput_ReturnsFalse()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel(null!);

            Assert.False(result);
        }

        [Fact]
        public void SupportsModel_EmptyString_ReturnsFalse()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel(string.Empty);

            Assert.False(result);
        }

        [Fact]
        public void SupportsModel_WhitespaceString_ReturnsFalse()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("   ");

            Assert.False(result);
        }

        [Fact]
        public void SupportsModel_ValidModelId_ReturnsTrue()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("gpt-5.5");

            Assert.True(result);
        }

        [Fact]
        public void SupportsModel_ValidModelName_ReturnsTrue()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("GPT-5.5");

            Assert.True(result);
        }

        [Fact]
        public void SupportsModel_ModelIdWithDifferentCasing_ReturnsTrue()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("GPT-5.4-MINI");

            Assert.True(result);
        }

        [Fact]
        public void SupportsModel_ModelNameWithSurroundingSpaces_ReturnsTrue()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("  GPT-5.5  ");

            Assert.True(result);
        }

        [Fact]
        public void SupportsModel_UnknownModel_ReturnsFalse()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("nonexistent-model");

            Assert.False(result);
        }

        [Fact]
        public void SupportsModel_ModelNameCaseInsensitive_ReturnsTrue()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            bool result = service.SupportsModel("gpt-5.4 nano");

            Assert.True(result);
        }

        // ─── ProcessTextAsync (4-param overload) ──────────────────────────

        [Fact]
        public async Task ProcessTextAsync_FourParam_WithEmptyApiKey_ThrowsInvalidOperationException()
        {
            var emptyKeySettings = new OpenAiSettings { ApiKey = string.Empty };
            var emptyOptionsMock = new Mock<IOptions<OpenAiSettings>>();
            emptyOptionsMock.Setup(o => o.Value).Returns(emptyKeySettings);
            var service = new OpenAiService(_loggerMock.Object, emptyOptionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", "data", CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FourParam_WithEmptyModelId_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync(string.Empty, "prompt", "data", CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FourParam_WithEmptyPrompt_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", string.Empty, "data", CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FourParam_WithEmptyData_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", string.Empty, CancellationToken.None));
        }

        // ─── ProcessTextAsync (5-param overload with object settings) ─────

        [Fact]
        public async Task ProcessTextAsync_FiveParam_NullSettings_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", "data", null!, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FiveParam_WrongSettingsType_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", "data", new GeminiSettings(), CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FiveParam_WithEmptyApiKey_ThrowsInvalidOperationException()
        {
            var emptyKeySettings = new OpenAiSettings { ApiKey = string.Empty };
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", "data", emptyKeySettings, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FiveParam_WithEmptyModelId_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync(string.Empty, "prompt", "data", _settings, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FiveParam_WithEmptyPrompt_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", string.Empty, "data", _settings, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessTextAsync_FiveParam_WithEmptyData_ThrowsArgumentException()
        {
            var service = new OpenAiService(_loggerMock.Object, _optionsMock.Object, _httpClient);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ProcessTextAsync("gpt-5.5", "prompt", string.Empty, _settings, CancellationToken.None));
        }
    }
}