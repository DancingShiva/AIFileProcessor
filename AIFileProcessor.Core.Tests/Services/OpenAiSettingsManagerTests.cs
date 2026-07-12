//-----------------------------------------------------------------------
// <copyright file="OpenAiSettingsManagerTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="OpenAiSettingsManager"/>.
    /// </summary>
    public class OpenAiSettingsManagerTests
    {
        private readonly Mock<ISettingsProvider> _settingsProviderMock;
        private readonly Mock<IUserContext> _userContextMock;

        public OpenAiSettingsManagerTests()
        {
            _settingsProviderMock = new Mock<ISettingsProvider>(MockBehavior.Strict);
            _userContextMock = new Mock<IUserContext>(MockBehavior.Strict);
            _userContextMock.SetupGet(u => u.UserId).Returns("test-user");
        }

        // --- Constructor Tests ---

        [Fact]
        public void Constructor_NullSettingsProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                "settingsProvider",
                () => new OpenAiSettingsManager(null!, _userContextMock.Object));
        }

        [Fact]
        public void Constructor_NullUserContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                "userContext",
                () => new OpenAiSettingsManager(_settingsProviderMock.Object, null!));
        }

        [Fact]
        public void Constructor_ValidDependencies_CreatesInstance()
        {
            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            Assert.NotNull(manager);
        }

        // --- ReadSettings Tests ---

        [Fact]
        public void ReadSettings_CallsGetOpenAiSettingsAsyncWithUserId_ReturnsSettings()
        {
            OpenAiSettings expectedSettings = new OpenAiSettings();
            _settingsProviderMock
                .Setup(s => s.GetOpenAiSettingsAsync("test-user", CancellationToken.None))
                .ReturnsAsync(expectedSettings);

            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            OpenAiSettings result = manager.ReadSettings();

            Assert.Same(expectedSettings, result);
            _settingsProviderMock.Verify(
                s => s.GetOpenAiSettingsAsync("test-user", CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public void ReadSettings_ProviderThrows_ExceptionPropagates()
        {
            _settingsProviderMock
                .Setup(s => s.GetOpenAiSettingsAsync("test-user", CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("provider error"));

            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            Assert.Throws<InvalidOperationException>(() => manager.ReadSettings());
        }

        // --- SaveSettings Tests ---

        [Fact]
        public void SaveSettings_NullSettings_ThrowsArgumentNullException()
        {
            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            Assert.Throws<ArgumentNullException>("settings", () => manager.SaveSettings(null!));
        }

        [Fact]
        public void SaveSettings_ValidSettings_CallsSaveOpenAiSettingsAsyncWithUserId()
        {
            OpenAiSettings settings = new OpenAiSettings();
            _settingsProviderMock
                .Setup(s => s.SaveOpenAiSettingsAsync("test-user", settings, CancellationToken.None))
                .Returns(Task.CompletedTask);

            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            manager.SaveSettings(settings);

            _settingsProviderMock.Verify(
                s => s.SaveOpenAiSettingsAsync("test-user", settings, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public void SaveSettings_ProviderThrows_ExceptionPropagates()
        {
            OpenAiSettings settings = new OpenAiSettings();
            _settingsProviderMock
                .Setup(s => s.SaveOpenAiSettingsAsync("test-user", settings, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("save error"));

            OpenAiSettingsManager manager = new OpenAiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

            Assert.Throws<InvalidOperationException>(() => manager.SaveSettings(settings));
        }
    }
}