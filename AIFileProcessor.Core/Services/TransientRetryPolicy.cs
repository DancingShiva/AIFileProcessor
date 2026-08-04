//-----------------------------------------------------------------------
// <copyright file="TransientRetryPolicy.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Shared retry-with-exponential-backoff for transient failures, shared by all
    /// AI service implementations. Stateless and parameter-driven (no DI) — each
    /// caller supplies its own <see langword="isTransient"/> predicate because the
    /// exception types differ per SDK. Consistent with <see cref="TokenUtils"/> and
    /// <see cref="ModelTokenLimits"/>.
    /// </summary>
    internal static class TransientRetryPolicy
    {
        /// <summary>
        /// Executes <paramref name="action"/>, retrying transient failures with
        /// exponential backoff (<c>baseDelaySeconds * 2^attempt</c>, capped at
        /// <paramref name="maxDelaySeconds"/>). Cancellation is never retried.
        /// </summary>
        /// <typeparam name="T">The result type of the action.</typeparam>
        /// <param name="action">The operation to execute.</param>
        /// <param name="isTransient">Predicate deciding whether a failure is transient and worth retrying.</param>
        /// <param name="maxRetries">Maximum number of retries after the initial attempt.</param>
        /// <param name="baseDelaySeconds">Base delay (seconds); doubled each retry.</param>
        /// <param name="maxDelaySeconds">Maximum delay (seconds) between retries.</param>
        /// <param name="logger">Logger for retry warnings.</param>
        /// <param name="operationName">Operation/model name included in log messages.</param>
        /// <param name="ct">A token to cancel the operation and any retry delay.</param>
        /// <returns>The result of a successful execution.</returns>
        public static async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> action,
            Func<Exception, bool> isTransient,
            int maxRetries,
            int baseDelaySeconds,
            int maxDelaySeconds,
            ILogger logger,
            string operationName,
            CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(isTransient);
            ArgumentNullException.ThrowIfNull(logger);

            int retries = Math.Max(0, maxRetries);
            int baseDelay = Math.Max(1, baseDelaySeconds);
            int maxDelay = Math.Max(baseDelay, maxDelaySeconds);

            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return await action(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < retries && isTransient(ex))
                {
                    int delaySeconds = Math.Min(baseDelay * (int)Math.Pow(2, attempt), maxDelay);
                    logger.LogWarning(
                        ex,
                        "Transient failure on attempt {Attempt}/{MaxRetries} for {OperationName}. Retrying in {DelaySeconds}s.",
                        attempt + 1,
                        retries,
                        operationName,
                        delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
                }
            }
        }
    }
}
