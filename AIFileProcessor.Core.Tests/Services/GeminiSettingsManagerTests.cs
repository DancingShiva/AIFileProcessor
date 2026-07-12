namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Services;
using Moq;
using Xunit;

public class GeminiSettingsManagerTests
{
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly GeminiSettings _testSettings = new() { ApiKey = "test-key" };

    // ─── Constructor ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullSettingsProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GeminiSettingsManager(null!, _userContextMock.Object));
    }

    [Fact]
    public void Constructor_NullUserContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GeminiSettingsManager(_settingsProviderMock.Object, null!));
    }

    [Fact]
    public void Constructor_ValidArguments_CreatesInstance()
    {
        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);
        Assert.NotNull(manager);
    }

    // ─── ReadSettings ─────────────────────────────────────────────────

    [Fact]
    public void ReadSettings_CallsGetGeminiSettingsAsyncWithUserIdAndReturnsSettings()
    {
        // Arrange
        _userContextMock.SetupGet(u => u.UserId).Returns("user-123");
        _settingsProviderMock
            .Setup(p => p.GetGeminiSettingsAsync("user-123", CancellationToken.None))
            .ReturnsAsync(_testSettings);

        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

        // Act
        var result = manager.ReadSettings();

        // Assert
        Assert.Same(_testSettings, result);
        _settingsProviderMock.Verify(
            p => p.GetGeminiSettingsAsync("user-123", CancellationToken.None), Times.Once);
    }

    [Fact]
    public void ReadSettings_PropagatesExceptionFromProvider()
    {
        // Arrange
        _userContextMock.SetupGet(u => u.UserId).Returns("user-abc");
        _settingsProviderMock
            .Setup(p => p.GetGeminiSettingsAsync("user-abc", CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("provider failure"));

        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => manager.ReadSettings());
    }

    // ─── SaveSettings ─────────────────────────────────────────────────

    [Fact]
    public void SaveSettings_NullSettings_ThrowsArgumentNullException()
    {
        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

        Assert.Throws<ArgumentNullException>(() => manager.SaveSettings(null!));
    }

    [Fact]
    public void SaveSettings_CallsSaveGeminiSettingsAsyncWithUserIdAndSettings()
    {
        // Arrange
        _userContextMock.SetupGet(u => u.UserId).Returns("user-456");
        _settingsProviderMock
            .Setup(p => p.SaveGeminiSettingsAsync("user-456", _testSettings, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

        // Act
        manager.SaveSettings(_testSettings);

        // Assert
        _settingsProviderMock.Verify(
            p => p.SaveGeminiSettingsAsync("user-456", _testSettings, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void SaveSettings_PropagatesExceptionFromProvider()
    {
        // Arrange
        _userContextMock.SetupGet(u => u.UserId).Returns("user-xyz");
        _settingsProviderMock
            .Setup(p => p.SaveGeminiSettingsAsync("user-xyz", _testSettings, CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("save failure"));

        var manager = new GeminiSettingsManager(_settingsProviderMock.Object, _userContextMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => manager.SaveSettings(_testSettings));
    }
}