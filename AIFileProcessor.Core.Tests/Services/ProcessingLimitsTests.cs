//-----------------------------------------------------------------------
// <copyright file="ProcessingLimitsTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using AIFileProcessor.Configuration;
using AIFileProcessor.Services;
using Xunit;

[Trait("Category", "Unit")]
public class ProcessingLimitsTests
{
    [Fact]
    public void CurrentValue_ReturnsSettingsFromMonitor()
    {
        var settings = new ProcessingSettings
        {
            MaxInputSize = 10_000,
            MaxInputLines = 200,
            MaxOutputFieldSize = 5_000,
            RetryJitterSeconds = 3,
            MaxJsonDepth = 8,
            ChunkMaxRetries = 5,
            ChunkBaseDelaySeconds = 2,
            ChunkMaxDelaySeconds = 20,
        };
        var monitor = new TestOptionsMonitor<ProcessingSettings>(settings);
        var limits = new ProcessingLimits(monitor);

        Assert.Equal(10_000, limits.MaxInputSize);
        Assert.Equal(200, limits.MaxInputLines);
        Assert.Equal(5_000, limits.MaxOutputFieldSize);
        Assert.Equal(3, limits.RetryJitterSeconds);
        Assert.Equal(8, limits.MaxJsonDepth);
        Assert.Equal(5, limits.ChunkMaxRetries);
        Assert.Equal(2, limits.ChunkBaseDelaySeconds);
        Assert.Equal(20, limits.ChunkMaxDelaySeconds);
    }

    [Fact]
    public void CurrentValue_ReflectsUpdatedSettings()
    {
        var monitor = new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
        {
            MaxInputSize = 10_000,
            MaxInputLines = 200,
        });
        var limits = new ProcessingLimits(monitor);

        Assert.Equal(10_000, limits.MaxInputSize);
        Assert.Equal(200, limits.MaxInputLines);

        // Simulate a config change
        monitor.Update(new ProcessingSettings
        {
            MaxInputSize = 5_000,
            MaxInputLines = 100,
        });

        Assert.Equal(5_000, limits.MaxInputSize);
        Assert.Equal(100, limits.MaxInputLines);
    }

    [Fact]
    public void Constructor_AppliesFallbackDefaults_WhenSettingsAreZero()
    {
        var monitor = new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings
        {
            MaxInputSize = 0,
            MaxInputLines = 0,
            MaxOutputFieldSize = 0,
            RetryJitterSeconds = -1,
            MaxJsonDepth = 0,
            ChunkMaxRetries = -1,
            ChunkBaseDelaySeconds = 0,
            ChunkMaxDelaySeconds = 0,
        });
        var limits = new ProcessingLimits(monitor);

        Assert.Equal(32_768, limits.MaxInputSize);
        Assert.Equal(400, limits.MaxInputLines);
        Assert.Equal(32_768, limits.MaxOutputFieldSize);
        Assert.Equal(5, limits.RetryJitterSeconds);
        Assert.Equal(16, limits.MaxJsonDepth);
        Assert.Equal(3, limits.ChunkMaxRetries);
        Assert.Equal(5, limits.ChunkBaseDelaySeconds);
        Assert.Equal(30, limits.ChunkMaxDelaySeconds);
    }
}