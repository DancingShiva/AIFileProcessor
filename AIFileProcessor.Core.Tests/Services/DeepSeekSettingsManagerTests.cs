namespace AIFileProcessor.Core.Tests.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Services;
    using Moq;
    using Xunit;

    public class DeepSeekSettingsManagerTests
    {
        [Fact]
        public void Constructor_ValidDependencies_StoresInstances()
        {
            // Arrange
            var settingsProvider = new Mock<ISettingsProvider>();
            var userContext = new Mock<IUserContext>();

            // Act
            var manager = new DeepSeekSettingsManager(settingsProvider.Object, userContext.Object);

            // Assert - no exception means the dependencies were accepted
            Assert.NotNull(manager);
        }

        [Fact]
        public void Constructor_NullSettingsProvider_ThrowsArgumentNullException()
        {
            // Arrange
            var userContext = new Mock<IUserContext>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DeepSeekSettingsManager(null!, userContext.Object));
        }

        [Fact]
        public void Constructor_NullUserContext_ThrowsArgumentNullException()
        {
            // Arrange
            var settingsProvider = new Mock<ISettingsProvider>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DeepSeekSettingsManager(settingsProvider.Object, null!));
        }

        [Fact]
        public void ReadSettings_CallsGetDeepSeekSettingsAsyncWithUserIdAndReturnsResult()
        {
            // Arrange
            const string userId = "test-user-123";
            var expectedSettings = new DeepSeekSettings
            {
                ApiKey = "sk-test-key",
                Endpoint = "https://api.deepseek.com",
                TimeoutSeconds = 30,
                MaxTokens = 4096,
                Temperature = 0.7,
                ModelName = "deepseek-v4-flash",
                EnableReasoning = true,
            };

            var settingsProvider = new Mock<ISettingsProvider>();
            settingsProvider
                .Setup(p => p.GetDeepSeekSettingsAsync(userId, CancellationToken.None))
                .ReturnsAsync(expectedSettings);

            var userContext = new Mock<IUserContext>();
            userContext.SetupGet(c => c.UserId).Returns(userId);

            var manager = new DeepSeekSettingsManager(settingsProvider.Object, userContext.Object);

            // Act
            var result = manager.ReadSettings();

            // Assert
            Assert.Same(expectedSettings, result);
            settingsProvider.Verify(p => p.GetDeepSeekSettingsAsync(userId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void SaveSettings_ValidSettings_CallsSaveDeepSeekSettingsAsyncWithUserId()
        {
            // Arrange
            const string userId = "test-user-456";
            var settings = new DeepSeekSettings
            {
                ApiKey = "sk-save-key",
                Endpoint = "https://api.deepseek.com",
                TimeoutSeconds = 60,
            };

            var settingsProvider = new Mock<ISettingsProvider>();
            settingsProvider
                .Setup(p => p.SaveDeepSeekSettingsAsync(userId, settings, CancellationToken.None))
                .Returns(Task.CompletedTask);

            var userContext = new Mock<IUserContext>();
            userContext.SetupGet(c => c.UserId).Returns(userId);

            var manager = new DeepSeekSettingsManager(settingsProvider.Object, userContext.Object);

            // Act
            manager.SaveSettings(settings);

            // Assert
            settingsProvider.Verify(p => p.SaveDeepSeekSettingsAsync(userId, settings, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void SaveSettings_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var settingsProvider = new Mock<ISettingsProvider>();
            var userContext = new Mock<IUserContext>();
            userContext.SetupGet(c => c.UserId).Returns("test-user");

            var manager = new DeepSeekSettingsManager(settingsProvider.Object, userContext.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.SaveSettings(null!));
            settingsProvider.Verify(
                p => p.SaveDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<DeepSeekSettings>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}