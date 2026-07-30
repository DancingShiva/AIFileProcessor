namespace AIFileProcessor.Core.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class TransientRetryPolicyTests
    {
        private readonly Mock<ILogger> _loggerMock = new();

        private static bool AlwaysTransient(Exception ex) => ex is InvalidOperationException;

        private static bool NeverTransient(Exception ex) => false;

        // ─── Success paths ──────────────────────────────────────────────

        [Fact]
        public async Task ExecuteAsync_SucceedsFirstTry_ReturnsResult_NoRetry()
        {
            int calls = 0;
            var result = await TransientRetryPolicy.ExecuteAsync(
                ct => { calls++; return Task.FromResult(42); },
                AlwaysTransient,
                maxRetries: 3, baseDelaySeconds: 1, maxDelaySeconds: 5,
                _loggerMock.Object, "op", CancellationToken.None);

            Assert.Equal(42, result);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ExecuteAsync_TransientThenSucceeds_Retries()
        {
            int calls = 0;
            var result = await TransientRetryPolicy.ExecuteAsync(
                ct =>
                {
                    calls++;
                    return calls < 3
                        ? Task.FromException<int>(new InvalidOperationException("transient"))
                        : Task.FromResult(7);
                },
                AlwaysTransient,
                maxRetries: 5, baseDelaySeconds: 1, maxDelaySeconds: 2,
                _loggerMock.Object, "op", CancellationToken.None);

            Assert.Equal(7, result);
            Assert.Equal(3, calls);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        // ─── Failure paths ──────────────────────────────────────────────

        [Fact]
        public async Task ExecuteAsync_NonTransient_ThrowsImmediately_NoRetry()
        {
            int calls = 0;
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                TransientRetryPolicy.ExecuteAsync<int>(
                    ct => { calls++; return Task.FromException<int>(new InvalidOperationException("boom")); },
                    NeverTransient,
                    maxRetries: 3, baseDelaySeconds: 1, maxDelaySeconds: 2,
                    _loggerMock.Object, "op", CancellationToken.None));

            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ExecuteAsync_TransientBeyondMaxRetries_Throws()
        {
            int calls = 0;
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                TransientRetryPolicy.ExecuteAsync<int>(
                    ct => { calls++; return Task.FromException<int>(new InvalidOperationException("always")); },
                    AlwaysTransient,
                    maxRetries: 2, baseDelaySeconds: 1, maxDelaySeconds: 2,
                    _loggerMock.Object, "op", CancellationToken.None));

            // 1 initial attempt + 2 retries.
            Assert.Equal(3, calls);
        }

        [Fact]
        public async Task ExecuteAsync_Cancellation_Propagates_WithoutRetry()
        {
            int calls = 0;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                TransientRetryPolicy.ExecuteAsync<int>(
                    ct => { calls++; return Task.FromCanceled<int>(new CancellationToken(canceled: true)); },
                    AlwaysTransient,
                    maxRetries: 3, baseDelaySeconds: 1, maxDelaySeconds: 2,
                    _loggerMock.Object, "op", CancellationToken.None));

            Assert.Equal(1, calls);
        }

        // ─── Argument validation ────────────────────────────────────────

        [Fact]
        public async Task ExecuteAsync_NullAction_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                TransientRetryPolicy.ExecuteAsync<int>(
                    null!, AlwaysTransient, 1, 1, 2, _loggerMock.Object, "op", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_NullLogger_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                TransientRetryPolicy.ExecuteAsync<int>(
                    ct => Task.FromResult(1), AlwaysTransient, 1, 1, 2, null!, "op", CancellationToken.None));
        }
    }
}
