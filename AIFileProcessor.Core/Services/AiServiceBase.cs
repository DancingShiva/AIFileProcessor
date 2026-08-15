//-----------------------------------------------------------------------
// <copyright file="AiServiceBase.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using AIFileProcessor.Telemetry;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Abstract base class for AI service implementations. Extracts the structural
    /// shell shared by <see cref="DeepSeekService"/>, <see cref="GeminiService"/>,
    /// and <see cref="OpenAiService"/>: constructor pattern, model catalog lookups,
    /// the two public <see cref="IAiService.ProcessTextAsync(string, string, string, System.Threading.CancellationToken)"/> overloads, transient
    /// exception detection, request validation, and telemetry helpers.
    /// </summary>
    /// <typeparam name="TSettings">The concrete settings type for this provider.</typeparam>
    public abstract class AiServiceBase<TSettings> : IAiService
        where TSettings : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AiServiceBase{TSettings}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="defaultSettings">The default (constructor-injected) settings.</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        protected AiServiceBase(ILogger logger, TSettings defaultSettings, HttpClient httpClient)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            DefaultSettings = defaultSettings ?? throw new ArgumentNullException(nameof(defaultSettings));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the default (constructor-injected) settings.
        /// </summary>
        protected TSettings DefaultSettings { get; }

        /// <summary>
        /// Gets the HTTP client for making requests.
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the list of models supported by this AI service.
        /// </summary>
        protected abstract IReadOnlyList<AiModelInfo> SupportedModels { get; }

        /// <summary>
        /// Gets the provider name used in log messages and error strings
        /// (e.g., "DeepSeek", "Gemini", "OpenAI").
        /// </summary>
        protected abstract string ProviderName { get; }

        /// <inheritdoc/>
        public IEnumerable<AiModelInfo> GetSupportedModels() => SupportedModels;

        /// <inheritdoc/>
        public bool SupportsModel(string modelNameOrId)
        {
            return AiModelCatalog.SupportsModel(SupportedModels, modelNameOrId);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ReasoningLevel> GetSupportedReasoningLevels(string modelId)
        {
            return AiModelCatalog.GetReasoningLevels(SupportedModels, modelId);
        }

        /// <inheritdoc/>
        public Task<AiCallResult> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            CancellationToken ct = default)
        {
            return ProcessTextCoreAsync(modelId, prompt, data, DefaultSettings, ct);
        }

        /// <inheritdoc/>
        public Task<AiCallResult> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            IAiServiceSettings settings,
            CancellationToken ct = default)
        {
            if (settings is not TSettings typedSettings)
            {
                throw new ArgumentException(
                    $"Expected {typeof(TSettings).Name} but received {settings?.GetType().Name ?? "null"}.",
                    nameof(settings));
            }

            return ProcessTextCoreAsync(modelId, prompt, data, typedSettings, ct);
        }

        /// <summary>
        /// Wraps user-supplied data in cryptographically random per-request delimiter
        /// tags to defend against prompt injection.
        /// </summary>
        /// <param name="data">The raw user-supplied data to wrap.</param>
        /// <returns>The wrapped data string.</returns>
        protected static string WrapData(string data)
        {
            return PromptInjectionGuard.WrapData(data).WrappedData;
        }

        /// <summary>
        /// Creates a <see cref="CallTelemetry"/> instance initialized with the
        /// current UTC timestamp and <c>Attempts = 1</c>.
        /// </summary>
        /// <returns>A new telemetry instance ready for an API call.</returns>
        protected static CallTelemetry CreateTelemetry()
        {
            return new CallTelemetry { StartUtc = DateTime.UtcNow, Attempts = 1 };
        }

        /// <summary>
        /// Finalizes a <see cref="CallTelemetry"/> instance by setting
        /// <see cref="CallTelemetry.EndUtc"/> and computing <see cref="CallTelemetry.ElapsedMs"/>.
        /// </summary>
        /// <param name="telemetry">The telemetry instance to finalize.</param>
        protected static void FinalizeTelemetry(CallTelemetry telemetry)
        {
            telemetry.EndUtc = DateTime.UtcNow;
            telemetry.ElapsedMs = (long)(telemetry.EndUtc - telemetry.StartUtc).TotalMilliseconds;
        }

        /// <summary>
        /// Internal implementation that executes the provider API call using the
        /// specified settings. Each derived class implements the SDK-specific
        /// logic for building clients, constructing messages, and extracting responses.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The provider-specific settings.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        protected abstract Task<AiCallResult> ProcessTextCoreAsync(
            string modelId,
            string prompt,
            string data,
            TSettings settings,
            CancellationToken ct);

        /// <summary>
        /// Determines whether an exception represents a transient failure worth
        /// retrying. Checks the common cases (HTTP 429/5xx, timeout, cancellation)
        /// and then delegates to <see cref="IsProviderTransient"/> for SDK-specific
        /// exception types.
        /// </summary>
        /// <param name="ex">The exception to evaluate.</param>
        /// <returns>True if the exception is transient; otherwise, false.</returns>
        protected virtual bool IsTransient(Exception ex)
        {
            if (ex is HttpRequestException hre)
            {
                return hre.StatusCode is null or HttpStatusCode.TooManyRequests
                    or >= HttpStatusCode.InternalServerError;
            }

            if (ex is TimeoutException or TaskCanceledException)
            {
                return true;
            }

            return IsProviderTransient(ex);
        }

        /// <summary>
        /// Determines whether an SDK-specific exception represents a transient
        /// failure. Override in derived classes to handle provider-specific
        /// exception types (e.g., <c>ClientResultException</c>, <c>ClientError</c>).
        /// </summary>
        /// <param name="ex">The exception to evaluate (never <see cref="HttpRequestException"/>,
        /// <see cref="TimeoutException"/>, or <see cref="TaskCanceledException"/> —
        /// those are handled by <see cref="IsTransient"/>).</param>
        /// <returns>True if the exception is transient; otherwise, false.</returns>
        protected abstract bool IsProviderTransient(Exception ex);

        /// <summary>
        /// Validates the request arguments and API key. Throws if any required
        /// value is missing or if the data contains a prompt-injection pattern.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="apiKey">The API key from the provider settings.</param>
        protected void ValidateRequest(string modelId, string prompt, string data, string apiKey)
        {
            AiServiceGuard.RequireValidRequest(modelId, prompt, data);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException($"{ProviderName} API key is not configured.");
            }
        }
    }
}