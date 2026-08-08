namespace AIFileProcessor.Core.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Response;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ChineseTextProcessorTests
    {
        private readonly Mock<ILogger<ChineseTextProcessor>> _loggerMock = new();
        private readonly Mock<IAiService> _aiServiceMock = new();

        private const string ValidJson = "{\"SP\":\"scratch\",\"Corrections\":[],\"Sentences\":[]}";
        private const string ModelId = "test-model";
        private const string Prompt = "translate";

        public ChineseTextProcessorTests()
        {
            _aiServiceMock.Setup(s => s.SupportsModel(ModelId)).Returns(true);
            _aiServiceMock
                .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AIFileProcessor.Response.AiCallResult { Text = ValidJson });
            _aiServiceMock
                .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AIFileProcessor.Response.AiCallResult { Text = ValidJson });
        }

        private ChineseTextProcessor CreateProcessor() =>
            new(_loggerMock.Object, new[] { _aiServiceMock.Object });

        // --- ProcessChineseTextAsync (4-param overload, providerSettings = null) -----

        [Fact]
        public async Task ProcessChineseTextAsync_WithoutSettings_NullData_ReturnsEmptyResponse()
        {
            var processor = CreateProcessor();

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, null!, 40, 3, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Scratchpad);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithoutSettings_WhitespaceData_ReturnsEmptyResponse()
        {
            var processor = CreateProcessor();

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, "   ", 40, 3, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Scratchpad);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithoutSettings_ValidData_CallsDefaultOverload()
        {
            var processor = CreateProcessor();
            var data = "????";

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, 40, 3, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            _aiServiceMock.Verify(
                s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
            _aiServiceMock.Verify(
                s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithoutSettings_WithExplicitCancellationToken_Works()
        {
            var processor = CreateProcessor();
            var data = "??";

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, 40, 3, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithoutSettings_NoMatchingModel_ThrowsNotSupportedException()
        {
            var processor = CreateProcessor();

            await Assert.ThrowsAsync<NotSupportedException>(
                () => processor.ProcessChineseTextAsync("unknown-model", Prompt, "data", 40, 3, CancellationToken.None));
        }

        // --- ProcessChineseTextAsync (5-param overload, with providerSettings) ------

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_NullProviderSettings_ThrowsArgumentNullException()
        {
            var processor = CreateProcessor();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => processor.ProcessChineseTextAsync(ModelId, Prompt, "data", null!, 40, 3, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_ValidProviderSettings_CallsPerCallOverload()
        {
            var processor = CreateProcessor();
            var data = "????";

            // GetProviderKey uses service.GetType().Name; for a Mock<IAiService> the proxy
            // type name is used as the fallback key, so we use that name as the dictionary key.
            var providerKeyName = _aiServiceMock.Object.GetType().Name;
            var providerSettings = new Dictionary<string, object>
            {
                [providerKeyName] = new { ApiKey = "test" },
            };
            _aiServiceMock
                .Setup(s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AIFileProcessor.Response.AiCallResult { Text = ValidJson });

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, 40, 3, CancellationToken.None);

            Assert.NotNull(result);
            _aiServiceMock.Verify(
                s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_WhitespaceData_ReturnsEmptyResponse()
        {
            var processor = CreateProcessor();
            var providerSettings = new Dictionary<string, object>();

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, "  ", providerSettings, 40, 3, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Scratchpad);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_DefaultCancellationToken_Works()
        {
            var processor = CreateProcessor();
            var providerSettings = new Dictionary<string, object>();
            var data = "??";

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, 40, 3, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_NoMatchingModel_ThrowsNotSupportedException()
        {
            var processor = CreateProcessor();
            var providerSettings = new Dictionary<string, object>();

            await Assert.ThrowsAsync<NotSupportedException>(
                () => processor.ProcessChineseTextAsync("unknown-model", Prompt, "data", providerSettings, 40, 3, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessChineseTextAsync_WithSettings_SettingsWithoutMatchingKey_UsesDefaultOverload()
        {
            var processor = CreateProcessor();
            // Provider key won't match any entry, so perCallSettings stays null and default overload is used
            var providerSettings = new Dictionary<string, object>
            {
                ["UnknownProvider"] = new { ApiKey = "test" },
            };
            var data = "??";

            var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, providerSettings, 40, 3, CancellationToken.None);

            Assert.NotNull(result);
            _aiServiceMock.Verify(
                s => s.ProcessTextAsync(ModelId, Prompt, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}
