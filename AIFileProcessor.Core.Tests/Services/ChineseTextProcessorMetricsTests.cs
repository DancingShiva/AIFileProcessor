//-----------------------------------------------------------------------
// <copyright file="ChineseTextProcessorMetricsTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Exceptions;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using AIFileProcessor.Services;
using AIFileProcessor.Telemetry;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

[Trait("Category", "Unit")]
public class ChineseTextProcessorMetricsTests
{
    private const string ValidJson = "{\"SP\":\"scratch\",\"Corrections\":[],\"Sentences\":[]}";
    private const string ModelId = "test-model";
    private const string Prompt = "p";

    [Fact]
    public async Task ProcessChineseTextAsync_AttachesProcessingMetrics_WithCorrectAggregation()
    {
        // Arrange
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        // Create three chunks of 40 lines each => 120 lines total
        var data = string.Join(Environment.NewLine, Enumerable.Range(1, 120));

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);

        // Telemetry is created inside the mocked call (not pre-built) so its
        // wall-clock window lands inside the run window, and a real 50 ms delay
        // guarantees a nonzero elapsed measurement on every platform clock.
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(50, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 50,
                        PromptTokens = 10,
                        CompletionTokens = 20,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));
        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Metrics);

        var metrics = result.Metrics!;
        Assert.Equal(3, metrics.ChunkCount);
        Assert.Equal(3, metrics.Succeeded);
        Assert.Equal(0, metrics.Failed);
        Assert.Equal(30, metrics.TotalPromptTokens);
        Assert.Equal(60, metrics.TotalCompletionTokens);

        // Wall-clock sanity: coherent run window containing every chunk's call window.
        Assert.True(metrics.StartUtc <= metrics.EndUtc);
        Assert.All(metrics.Chunks, c =>
        {
            Assert.True(c.Telemetry.StartUtc >= metrics.StartUtc);
            Assert.True(c.Telemetry.EndUtc <= metrics.EndUtc);
        });

        // Exact recomputation pins the derived formulas:
        // EffectiveParallelism = sum(chunk ElapsedMs) / run ElapsedMs; LinesPerSecond = lines / seconds.
        long chunkElapsedSum = metrics.Chunks.Sum(c => c.Telemetry.ElapsedMs);
        Assert.Equal(Math.Round(chunkElapsedSum / (double)metrics.ElapsedMs, 3), metrics.EffectiveParallelism);
        Assert.Equal(Math.Round(120 / (metrics.ElapsedMs / 1000.0), 3), metrics.LinesPerSecond);
        Assert.True(metrics.LinesPerSecond > 0);

        // Chunks list should be ordered by index.
        Assert.Equal(3, metrics.Chunks.Count);
        Assert.Collection(metrics.Chunks,
            c => Assert.Equal(0, c.ChunkIndex),
            c => Assert.Equal(1, c.ChunkIndex),
            c => Assert.Equal(2, c.ChunkIndex));
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MixedTokenTelemetry_SumsOnlyReportedValues()
    {
        // Arrange: 2 lines => 2 chunks (ChunkSize=1). Chunk 0 reports both token
        // counts; chunk 1 reports neither. The aggregate must stay non-null and
        // treat unreported chunks as contributing zero — not flip the whole total
        // to null (which would render "-" in the metrics table).
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好\n再见";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string chunkText, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(10, ct);
                bool firstChunk = chunkText == "你好";
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 10,
                        PromptTokens = firstChunk ? 7 : null,
                        CompletionTokens = firstChunk ? 13 : null,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions { ChunkSize = 1 }, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        Assert.Equal(2, result.Metrics!.ChunkCount);
        Assert.Equal(7, result.Metrics.TotalPromptTokens);
        Assert.Equal(13, result.Metrics.TotalCompletionTokens);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_AllChunksWithoutTokenTelemetry_AggregatesNullTotals()
    {
        // Arrange: no chunk reports usage — totals must be null so the UI
        // shows "-" instead of a misleading 0.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好\n你好";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(10, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 10,
                        PromptTokens = null,
                        CompletionTokens = null,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions { ChunkSize = 1 }, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        Assert.Equal(2, result.Metrics!.ChunkCount);
        Assert.Null(result.Metrics.TotalPromptTokens);
        Assert.Null(result.Metrics.TotalCompletionTokens);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_NullCallTelemetry_UsesFallbackWithoutThrowing()
    {
        // Arrange: the mocked call returns no telemetry at all. The processor must
        // substitute fallback telemetry (CallTelemetry with Attempts = 1) so the
        // aggregation — and later the WinForms per-chunk table — cannot NRE.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = ValidJson, Telemetry = null! });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        var metrics = result.Metrics!;
        Assert.Single(metrics.Chunks);
        Assert.NotNull(metrics.Chunks[0].Telemetry);
        Assert.Equal(1, metrics.Chunks[0].Telemetry.Attempts);
        Assert.Null(metrics.TotalPromptTokens);
        Assert.Null(metrics.TotalCompletionTokens);
        Assert.Equal(0, metrics.EffectiveParallelism);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_WhitespaceData_ReturnsResponseWithoutMetrics()
    {
        // Arrange: whitespace input short-circuits before any chunking or API call,
        // so no run ever started — Metrics must remain null, not an empty aggregate.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, "   \n\t  ", new ChunkingOptions(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Metrics);
        aiService.Verify(
            s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_PartialFinalChunk_ReportsCappedEndLine()
    {
        // Arrange: 50 lines with ChunkSize=40 => chunks [1-40] and [41-50]. The
        // final chunk's EndLine must be capped at the real line count via Math.Min,
        // not padded to a full 40-line window. MainForm's LinesSuccessful/LinesFailed
        // accounting and the Gantt rows are derived from these ranges.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = string.Join(Environment.NewLine, Enumerable.Range(1, 50).Select(i => $"行{i}"));

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(10, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 10,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        var metrics = result.Metrics!;
        Assert.Equal(2, metrics.ChunkCount);
        Assert.Equal(2, metrics.Chunks.Count);
        Assert.Collection(metrics.Chunks,
            c =>
            {
                Assert.Equal(0, c.ChunkIndex);
                Assert.Equal(1, c.StartLine);
                Assert.Equal(40, c.EndLine);
            },
            c =>
            {
                Assert.Equal(1, c.ChunkIndex);
                Assert.Equal(41, c.StartLine);
                Assert.Equal(50, c.EndLine);
            });
    }

    [Fact]
    public async Task ProcessChineseTextAsync_SerializedChunks_ProducesSequentialLineRanges()
    {
        // Arrange: 120 lines / ChunkSize=40 => three full chunks with contiguous
        // 1-based ranges [1-40], [41-80], [81-120]. Contiguity is what lets the
        // Gantt renderer attribute each row to the right source lines.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = string.Join(Environment.NewLine, Enumerable.Range(1, 120).Select(i => $"行{i}"));

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(10, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 10,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(ModelId, Prompt, data, new ChunkingOptions(), CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        var chunks = result.Metrics!.Chunks;
        Assert.Equal(3, chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            Assert.Equal(i, chunks[i].ChunkIndex);
            Assert.Equal((i * 40) + 1, chunks[i].StartLine);
            Assert.Equal((i + 1) * 40, chunks[i].EndLine);
        }
    }

    [Fact]
    public async Task ProcessChineseTextAsync_MaxConcurrentChunksOne_RecordsQueueDelayForThrottledChunk()
    {
        // Arrange: 2 chunks with MaxConcurrentChunks=1 forces the second chunk to
        // queue behind the first. With a 50 ms gated call, the queued chunk's
        // QueueDelayMs must be nonzero — this feeds the "Queued" segment of the
        // Gantt chart.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好\n再见";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(50, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 50,
                        Attempts = 1,
                    },
                };
            });

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        // Act
        var result = await processor.ProcessChineseTextAsync(
            ModelId, Prompt, data,
            new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 1 },
            CancellationToken.None);

        // Assert
        Assert.NotNull(result.Metrics);
        var metrics = result.Metrics!;
        Assert.Equal(2, metrics.ChunkCount);
        Assert.All(metrics.Chunks, c => Assert.True(c.QueueDelayMs >= 0));
        Assert.Contains(metrics.Chunks, c => c.QueueDelayMs > 0);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_AllChunksFail_ThrowsWithFailureMetricsAttached()
    {
        // Arrange: both chunks exhaust retries. The thrown ChunkProcessingException
        // must carry the aggregate metrics so the UI can display a partial timeline
        // of the aborted run instead of discarding all telemetry.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好\n再见";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Persistent failure"));

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings { ChunkMaxRetries = 1, ChunkBaseDelaySeconds = 1, ChunkMaxDelaySeconds = 1, RetryJitterSeconds = 0 })));

        // Act
        var ex = await Assert.ThrowsAsync<ChunkProcessingException>(
            () => processor.ProcessChineseTextAsync(
                ModelId, Prompt, data,
                new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 2 },
                CancellationToken.None));

        // Assert: the metrics travel with the exception.
        Assert.NotNull(ex.Metrics);
        var metrics = ex.Metrics;
        Assert.Equal(2, metrics.ChunkCount);
        Assert.Equal(0, metrics.Succeeded);
        Assert.Equal(2, metrics.Failed);

        // Every chunk carries failed telemetry with a coherent attempt window.
        Assert.All(metrics.Chunks, c =>
        {
            Assert.Equal("Failed", c.Telemetry.Status);
            Assert.True(c.Telemetry.StartUtc <= c.Telemetry.EndUtc);
            Assert.Equal(2, c.Telemetry.Attempts); // 1 initial + 1 chunk-level retry (ChunkMaxRetries = 1)
        });

        // Chunks are ordered by index even though they failed concurrently.
        Assert.Collection(metrics.Chunks,
            c => Assert.Equal(0, c.ChunkIndex),
            c => Assert.Equal(1, c.ChunkIndex));

        // Wall-clock sanity: the run window contains the failure telemetry.
        Assert.True(metrics.StartUtc <= metrics.EndUtc);
        Assert.All(metrics.Chunks, c =>
        {
            Assert.True(c.Telemetry.StartUtc >= metrics.StartUtc);
            Assert.True(c.Telemetry.EndUtc <= metrics.EndUtc);
        });
    }

    [Fact]
    public async Task ProcessChineseTextAsync_PartialFailure_PartitionsSucceededAndFailed()
    {
        // Arrange: chunk 0 succeeds with token telemetry; chunk 1 fails permanently.
        // The exception metrics must partition 1 succeeded / 1 failed, keep the
        // success telemetry intact, and exclude failed-chunk nulls from the totals.
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "你好\n再见";

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        aiService.Setup(s => s.ProcessTextAsync(ModelId, Prompt, "你好", It.IsAny<CancellationToken>()))
            .Returns(async (string _, string _, string _, CancellationToken ct) =>
            {
                var callStartUtc = DateTime.UtcNow;
                await Task.Delay(10, ct);
                return new AiCallResult
                {
                    Text = ValidJson,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = callStartUtc,
                        EndUtc = DateTime.UtcNow,
                        ElapsedMs = 10,
                        PromptTokens = 7,
                        CompletionTokens = 13,
                        Attempts = 1,
                    },
                };
            });
        aiService.Setup(s => s.ProcessTextAsync(ModelId, Prompt, "再见", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Persistent failure"));

        var processor = new ChineseTextProcessor(
            logger.Object,
            new[] { aiService.Object },
            new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings { ChunkMaxRetries = 1, ChunkBaseDelaySeconds = 1, ChunkMaxDelaySeconds = 1, RetryJitterSeconds = 0 })));

        // Act
        var ex = await Assert.ThrowsAsync<ChunkProcessingException>(
            () => processor.ProcessChineseTextAsync(
                ModelId, Prompt, data,
                new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 2 },
                CancellationToken.None));

        // Assert
        var metrics = ex.Metrics;
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics!.ChunkCount);
        Assert.Equal(1, metrics.Succeeded);
        Assert.Equal(1, metrics.Failed);
        Assert.Equal(7, metrics.TotalPromptTokens);
        Assert.Equal(13, metrics.TotalCompletionTokens);

        var failedChunk = Assert.Single(metrics.Chunks, c => c.Telemetry.Status == "Failed");
        Assert.Equal(1, failedChunk.ChunkIndex);
        Assert.Equal(2, failedChunk.StartLine);
        Assert.Equal(2, failedChunk.EndLine);

        var succeededChunk = Assert.Single(metrics.Chunks, c => c.Telemetry.Status != "Failed");
        Assert.Equal(0, succeededChunk.ChunkIndex);
    }

    [Fact]
    public void ProcessingMetrics_JsonRoundTrip_PreservesEveryField()
    {
        // The WinForms JSON tab envelopes ProcessingMetrics with RunInfo metadata and
        // serializes it with default options; saved files are consumed downstream.
        // This pins the wire contract: every field must survive a round trip.
        var telemetry = new CallTelemetry
        {
            StartUtc = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2024, 1, 1, 12, 0, 1, DateTimeKind.Utc),
            ElapsedMs = 1000,
            TimeToFirstByteMs = 120,
            PromptTokens = 10,
            CompletionTokens = 20,
            Attempts = 2,
            Status = "Success",
        };
        var metrics = new ProcessingMetrics
        {
            StartUtc = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2024, 1, 1, 12, 0, 2, DateTimeKind.Utc),
            ElapsedMs = 2000,
            ChunkCount = 2,
            Succeeded = 2,
            Failed = 0,
            TotalPromptTokens = 10,
            TotalCompletionTokens = 20,
            EffectiveParallelism = 0.5,
            LinesPerSecond = 30.0,
            Chunks =
            [
                new ChunkMetrics
                {
                    ChunkIndex = 0,
                    StartLine = 1,
                    EndLine = 40,
                    QueueDelayMs = 15,
                    RetryAttempts = 1,
                    Telemetry = telemetry,
                },
                new ChunkMetrics
                {
                    ChunkIndex = 1,
                    StartLine = 41,
                    EndLine = 80,
                    QueueDelayMs = 0,
                    RetryAttempts = 0,
                    Telemetry = new CallTelemetry
                    {
                        StartUtc = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                        EndUtc = new DateTime(2024, 1, 1, 12, 0, 1, DateTimeKind.Utc),
                        ElapsedMs = 900,
                        TimeToFirstByteMs = null,
                        PromptTokens = null,
                        CompletionTokens = null,
                        Attempts = 1,
                        Status = "Success",
                    },
                },
            ],
        };

        string json = System.Text.Json.JsonSerializer.Serialize(metrics);
        var restored = System.Text.Json.JsonSerializer.Deserialize<ProcessingMetrics>(json);

        Assert.NotNull(restored);
        Assert.Equal(metrics.StartUtc, restored!.StartUtc);
        Assert.Equal(metrics.EndUtc, restored.EndUtc);
        Assert.Equal(metrics.ElapsedMs, restored.ElapsedMs);
        Assert.Equal(metrics.ChunkCount, restored.ChunkCount);
        Assert.Equal(metrics.Succeeded, restored.Succeeded);
        Assert.Equal(metrics.Failed, restored.Failed);
        Assert.Equal(metrics.TotalPromptTokens, restored.TotalPromptTokens);
        Assert.Equal(metrics.TotalCompletionTokens, restored.TotalCompletionTokens);
        Assert.Equal(metrics.EffectiveParallelism, restored.EffectiveParallelism);
        Assert.Equal(metrics.LinesPerSecond, restored.LinesPerSecond);
        Assert.Equal(2, restored.Chunks.Count);

        var first = restored.Chunks[0];
        Assert.Equal(0, first.ChunkIndex);
        Assert.Equal(1, first.StartLine);
        Assert.Equal(40, first.EndLine);
        Assert.Equal(15, first.QueueDelayMs);
        Assert.Equal(1, first.RetryAttempts);
        Assert.Equal(1000, first.Telemetry.ElapsedMs);
        Assert.Equal(120, first.Telemetry.TimeToFirstByteMs);
        Assert.Equal(10, first.Telemetry.PromptTokens);
        Assert.Equal(20, first.Telemetry.CompletionTokens);
        Assert.Equal(2, first.Telemetry.Attempts);
        Assert.Equal("Success", first.Telemetry.Status);

        // Nullable fields serialize as explicit nulls, not omitted defaults.
        var second = restored.Chunks[1];
        Assert.Null(second.Telemetry.TimeToFirstByteMs);
        Assert.Null(second.Telemetry.PromptTokens);
        Assert.Null(second.Telemetry.CompletionTokens);
        Assert.Contains("\"TimeToFirstByteMs\":null", json);
        Assert.Contains("\"PromptTokens\":null", json);
    }

    [Fact]
    public async Task ProcessChineseTextAsync_AttachesProcessingMetrics_PropagatesThroughOrchestrator()
    {
        // Arrange: create processor that returns an AiApiResponse with Metrics populated
        var logger = new Mock<ILogger<ChineseTextProcessor>>();
        var aiService = new Mock<IAiService>();
        var data = "一行数据"; // single line -> 1 chunk

        aiService.Setup(s => s.SupportsModel(ModelId)).Returns(true);
        var callTelemetry = new CallTelemetry
        {
            StartUtc = DateTime.UtcNow.AddMilliseconds(-100),
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 100,
            PromptTokens = null,
            CompletionTokens = null,
            Attempts = 1,
        };
        aiService.Setup(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCallResult { Text = ValidJson, Telemetry = callTelemetry });
        var aiResponseWithMetrics = new AiApiResponse { Scratchpad = "scratch" };

        // Attach simple metrics as would be produced by ChineseTextProcessor
        aiResponseWithMetrics.Metrics = new ProcessingMetrics
        {
            StartUtc = DateTime.UtcNow.AddMilliseconds(-200),
            EndUtc = DateTime.UtcNow,
            ElapsedMs = 200,
            ChunkCount = 1,
            Succeeded = 1,
            Failed = 0,
            TotalPromptTokens = null,
            TotalCompletionTokens = null,
            EffectiveParallelism = 0.5,
            LinesPerSecond = 5.0,
            Chunks = new List<ChunkMetrics> { new ChunkMetrics { ChunkIndex = 0, Telemetry = callTelemetry } },
        };
        var chineseProcessorMock = new Mock<IChineseTextProcessor>();
        chineseProcessorMock.Setup(p => p.ProcessChineseTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, IAiServiceSettings>>(), It.IsAny<ChunkingOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponseWithMetrics);

        // Create orchestrator via existing factory mocking used in ProcessingOrchestratorTests
        var settingsMock = new Mock<ISettingsProvider>();
        settingsMock.Setup(s => s.GetGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GeminiSettings());
        settingsMock.Setup(s => s.GetOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OpenAiSettings());
        settingsMock.Setup(s => s.GetDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DeepSeekSettings());
        settingsMock.Setup(s => s.GetGeminiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GeminiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });

        var userCtxMock = new Mock<IUserContext>();
        userCtxMock.Setup(u => u.UserId).Returns("test-user");

        var aiServices = new Mock<IAiService>();
        aiServices.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);

        Func<IChineseTextProcessor> factory = () => chineseProcessorMock.Object;
        var orchestrator = new ProcessingOrchestrator(factory, settingsMock.Object, userCtxMock.Object, new Mock<ILogger<ProcessingOrchestrator>>().Object, new[] { aiServices.Object }, new ProcessingLimits(new TestOptionsMonitor<ProcessingSettings>(new ProcessingSettings())));

        var options = new ProcessingOptions
        {
            ModelId = ModelId,
            Prompt = Prompt,
            Data = data,
            IdPrefix = "S",
            IdStart = 1,
            IdPad = 3,
            LengthPad = 3,
        };

        // Act
        var processingResult = await orchestrator.ProcessAsync(options, CancellationToken.None);

        // Assert
        Assert.NotNull(processingResult);
        Assert.NotNull(processingResult.Metrics);
        Assert.Equal(1, processingResult.Metrics!.ChunkCount);

        // EffectiveParallelism should match the mocked value we attached
        Assert.Equal(0.5, processingResult.Metrics.EffectiveParallelism);
        Assert.Equal(5.0, processingResult.Metrics.LinesPerSecond);
    }
}

