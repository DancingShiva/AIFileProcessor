//-----------------------------------------------------------------------
// <copyright file="GanttRendererTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.WinForms.Tests;

using System;
using System.Collections.Generic;
using System.Drawing;
using AIFileProcessor.Telemetry;
using Xunit;

[Trait("Category", "WinForms")]
public class GanttRendererTests
{
    private static ProcessingMetrics CreateMetrics(int chunkCount)
    {
        var chunks = new List<ChunkMetrics>();
        for (int i = 0; i < chunkCount; i++)
        {
            chunks.Add(new ChunkMetrics
            {
                ChunkIndex = i,
                StartLine = (i * 40) + 1,
                EndLine = (i + 1) * 40,
                QueueDelayMs = 10,
                Telemetry = new CallTelemetry
                {
                    StartUtc = DateTime.UtcNow,
                    EndUtc = DateTime.UtcNow.AddSeconds(1),
                    ElapsedMs = 1000,
                    Attempts = 1,
                },
            });
        }

        return new ProcessingMetrics
        {
            Chunks = chunks,
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddSeconds(10),
            ElapsedMs = 10000,
            ChunkCount = chunkCount,
            Succeeded = chunkCount,
            Failed = 0,
        };
    }

    [Fact]
    public void GetRequiredSize_NullMetrics_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ChunkTimelineGanttRenderer.GetRequiredSize(null!));
    }

    [Fact]
    public void GetRequiredSize_SingleChunk_ReturnsValidSize()
    {
        var metrics = CreateMetrics(1);

        var size = ChunkTimelineGanttRenderer.GetRequiredSize(metrics);

        Assert.True(size.Width >= 200);
        Assert.True(size.Height > 0);
    }

    [Fact]
    public void GetRequiredSize_MultipleChunks_HeightGrowsPerChunk()
    {
        var one = CreateMetrics(1);
        var five = CreateMetrics(5);

        var sizeOne = ChunkTimelineGanttRenderer.GetRequiredSize(one);
        var sizeFive = ChunkTimelineGanttRenderer.GetRequiredSize(five);

        Assert.True(sizeFive.Height > sizeOne.Height);
    }

    [Fact]
    public void Draw_NullGraphics_ThrowsArgumentNullException()
    {
        var metrics = CreateMetrics(1);

        Assert.Throws<ArgumentNullException>(() =>
            ChunkTimelineGanttRenderer.Draw(null!, metrics, new Rectangle(0, 0, 800, 400)));
    }

    [Fact]
    public void Draw_NullMetrics_ThrowsArgumentNullException()
    {
        using var bmp = new Bitmap(800, 400);
        using var g = Graphics.FromImage(bmp);

        Assert.Throws<ArgumentNullException>(() =>
            ChunkTimelineGanttRenderer.Draw(g, null!, new Rectangle(0, 0, 800, 400)));
    }

    [Fact]
    public void Draw_EmptyChunks_CompletesWithoutError()
    {
        var metrics = new ProcessingMetrics
        {
            Chunks = new List<ChunkMetrics>(),
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 0,
            ChunkCount = 0,
            Succeeded = 0,
            Failed = 0,
        };
        using var bmp = new Bitmap(800, 400);
        using var g = Graphics.FromImage(bmp);

        ChunkTimelineGanttRenderer.Draw(g, metrics, new Rectangle(0, 0, 800, 400));

        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void Draw_SingleChunk_DrawsNonWhitePixels()
    {
        var metrics = CreateMetrics(1);
        using var bmp = new Bitmap(800, 400);
        using var g = Graphics.FromImage(bmp);

        ChunkTimelineGanttRenderer.Draw(g, metrics, new Rectangle(0, 0, 800, 400));

        // Verify that the bitmap is not entirely white (something was drawn)
        bool foundNonWhite = false;
        for (int x = 0; x < bmp.Width && !foundNonWhite; x += 10)
        {
            for (int y = 0; y < bmp.Height && !foundNonWhite; y += 10)
            {
                var pixel = bmp.GetPixel(x, y);
                if (pixel.R < 250 || pixel.G < 250 || pixel.B < 250)
                {
                    foundNonWhite = true;
                }
            }
        }

        Assert.True(foundNonWhite);
    }

    [Fact]
    public void Draw_WithTitleSuffix_DoesNotThrow()
    {
        var metrics = CreateMetrics(2);
        using var bmp = new Bitmap(800, 400);
        using var g = Graphics.FromImage(bmp);

        ChunkTimelineGanttRenderer.Draw(g, metrics, new Rectangle(0, 0, 800, 400), "Test Model");

        Assert.True(true);
    }
}