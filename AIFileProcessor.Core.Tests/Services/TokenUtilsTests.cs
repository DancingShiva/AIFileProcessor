//-----------------------------------------------------------------------
// <copyright file="TokenUtilsTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

[Trait("Category", "Unit")]
public class TokenUtilsTests
{
    // ─── ClampRequestedMaxTokens: requested=null ───────────────────────────────

    [Fact]
    public void ClampRequestedMaxTokens_NullRequested_ReturnsNullAndFalse()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(null, "Gemini", "gemini-3-flash");
        Assert.Null(clampedValue);
        Assert.False(wasClamped);
    }

    // ─── ClampRequestedMaxTokens: requested <= hardMax (no clamp) ──────────────

    [Fact]
    public void ClampRequestedMaxTokens_RequestedEqualsHardMax_ReturnsSameValueAndFalse()
    {
        // Gemini default is 262144
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(262144, "Gemini", "gemini-3-flash");
        Assert.Equal(262144, clampedValue);
        Assert.False(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedLessThanHardMax_ReturnsSameValueAndFalse()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(16000, "Gemini", "gemini-3-flash");
        Assert.Equal(16000, clampedValue);
        Assert.False(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedZero_ReturnsZeroAndFalse()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(0, "Gemini", "gemini-3-flash");
        Assert.Equal(0, clampedValue);
        Assert.False(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedBelowHardMaxForOpenAi_ReturnsSameValueAndFalse()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(4096, "OpenAi", "gpt-5.6-sol");
        Assert.Equal(4096, clampedValue);
        Assert.False(wasClamped);
    }

    // ─── ClampRequestedMaxTokens: requested > hardMax (clamps and sets flag) ───

    [Fact]
    public void ClampRequestedMaxTokens_RequestedExceedsHardMax_ClampsAndReturnsTrue()
    {
        // OpenAi hard max is 262144
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, "OpenAi", "gpt-5.6-sol");
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedExceedsGeminiHardMax_ClampsAndReturnsTrue()
    {
        // Gemini hard max is 262144
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, "Gemini", "gemini-3-flash");
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedExceeds128kHardMax_ClampsAndReturnsTrue()
    {
        // All Gemini models now have 262144 hard max
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, "Gemini", "gemini-3-flash-128k");
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_RequestedExceedsDeepSeekHardMax_ClampsAndReturnsTrue()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, "DeepSeek", "deepseek-v4-pro");
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    // ─── ClampRequestedMaxTokens: null provider / null model edge cases ────────

    [Fact]
    public void ClampRequestedMaxTokens_NullProviderName_UsesConservativeDefault()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, null!, null);
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_NullModelNameForGemini_UsesGeminiDefault()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(16000, "Gemini", null);
        Assert.Equal(16000, clampedValue);
        Assert.False(wasClamped);
    }

    // ─── ClampRequestedMaxTokens: logger verification ──────────────────────────

    [Fact]
    public void ClampRequestedMaxTokens_ClampingOccurs_LogsWarning()
    {
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        TokenUtils.ClampRequestedMaxTokens(524288, "OpenAi", "gpt-5.6-sol", loggerMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ClampRequestedMaxTokens_NoClamping_DoesNotLogWarning()
    {
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        TokenUtils.ClampRequestedMaxTokens(4096, "OpenAi", "gpt-5.6-sol", loggerMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void ClampRequestedMaxTokens_NullRequested_DoesNotLogWarning()
    {
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        TokenUtils.ClampRequestedMaxTokens(null, "Gemini", "gemini-3-flash", loggerMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void ClampRequestedMaxTokens_NullLogger_DoesNotThrow()
    {
        // Should not throw when logger is null
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(524288, "OpenAi", "gpt-5.6-sol", null);
        Assert.Equal(262144, clampedValue);
        Assert.True(wasClamped);
    }

    [Fact]
    public void ClampRequestedMaxTokens_NullLoggerWithNullRequested_DoesNotThrow()
    {
        var (clampedValue, wasClamped) = TokenUtils.ClampRequestedMaxTokens(null, "Gemini", "gemini-3-flash", null);
        Assert.Null(clampedValue);
        Assert.False(wasClamped);
    }
}
