//-----------------------------------------------------------------------
// <copyright file="OpenAiService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using AIFileProcessor.Telemetry;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using OpenAI;
    using OpenAI.Chat;

    /// <summary>
    /// Service that executes requests against the OpenAI API using the official OpenAI SDK.
    /// </summary>
    public class OpenAiService : IAiService
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModels =
        [
            new AiModelInfo(
                "GPT-5.6 Sol",
                "gpt-5.6-sol",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh, ReasoningLevel.Max]),
            new AiModelInfo(
                "GPT-5.6 Terra",
                "gpt-5.6-terra",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh, ReasoningLevel.Max]),
            new AiModelInfo(
                "GPT-5.6 Luna",
                "gpt-5.6-luna",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh, ReasoningLevel.Max]),
            new AiModelInfo(
                "GPT-5.5",
                "gpt-5.5",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
            new AiModelInfo(
                "GPT-5.5 Pro",
                "gpt-5.5-pro",
                [ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
            new AiModelInfo(
                "GPT-5.4",
                "gpt-5.4",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
            new AiModelInfo(
                "GPT-5.4 Pro",
                "gpt-5.4-pro",
                [ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
            new AiModelInfo(
                "GPT-5.4 Mini",
                "gpt-5.4-mini",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
            new AiModelInfo(
                "GPT-5.4 Nano",
                "gpt-5.4-nano",
                [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High, ReasoningLevel.XHigh]),
        ];

        private readonly ILogger _logger;
        private readonly OpenAiSettings _defaultSettings;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, ChatClient> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default OpenAI settings (from <c>IOptions&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public OpenAiService(ILogger<OpenAiService> logger, IOptions<OpenAiSettings> settings, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IEnumerable<AiModelInfo> GetSupportedModels() => SupportedModels;

        public bool SupportsModel(string modelNameOrId)
        {
            return AiModelCatalog.SupportsModel(SupportedModels, modelNameOrId);
        }

        public IReadOnlyList<ReasoningLevel> GetSupportedReasoningLevels(string modelId)
        {
            return AiModelCatalog.GetReasoningLevels(SupportedModels, modelId);
        }

        /// <summary>
        /// Generates content using the default (constructor-injected) OpenAI settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        public Task<AiCallResult> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            CancellationToken ct = default)
        {
            return ProcessTextAsync(modelId, prompt, data, _defaultSettings, ct);
        }

        /// <summary>
        /// Generates content using the provided per-call OpenAI settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The per-call settings (must be an <see cref="OpenAiSettings"/>).</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        public Task<AiCallResult> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            object settings,
            CancellationToken ct = default)
        {
            OpenAiSettings openAiSettings = AiServiceGuard.RequireSettings<OpenAiSettings>(settings);
            return ProcessTextAsync(modelId, prompt, data, openAiSettings, ct);
        }

        /// <summary>
        /// Builds a fingerprint string for client caching, ensuring that users
        /// with different API keys, endpoints, or models get separate cached
        /// <see cref="ChatClient"/> instances.
        /// </summary>
        private static string GetClientFingerprint(string modelId, OpenAiSettings settings)
        {
            return $"{settings.ApiKey}:{settings.Endpoint}:{modelId}";
        }

        /// <summary>
        /// Determines whether an exception from the OpenAI SDK represents a transient
        /// failure worth retrying (rate-limit, server error, network, or timeout).
        /// </summary>
        private static bool IsTransient(Exception ex)
        {
            if (ex is HttpRequestException hre)
            {
                return hre.StatusCode is null or System.Net.HttpStatusCode.TooManyRequests or >= System.Net.HttpStatusCode.InternalServerError;
            }

            // The OpenAI SDK wraps HTTP errors in ClientResultException; retry on 429/5xx.
            if (ex is ClientResultException cre)
            {
                return cre.Status == (int)System.Net.HttpStatusCode.TooManyRequests
                    || cre.Status >= (int)System.Net.HttpStatusCode.InternalServerError;
            }

            if (ex is TimeoutException or TaskCanceledException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Internal implementation that executes the OpenAI API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        private async Task<AIFileProcessor.Response.AiCallResult> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            OpenAiSettings settings,
            CancellationToken ct)
        {
            AiServiceGuard.RequireValidRequest(modelId, prompt, data);

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured.");
            }

            string fingerprint = GetClientFingerprint(modelId, settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var clientOptions = new OpenAIClientOptions
                {
                    Transport = new HttpClientPipelineTransport(_httpClient),
                };

                if (!string.IsNullOrWhiteSpace(settings.Endpoint))
                {
                    clientOptions.Endpoint = new Uri(settings.Endpoint);
                }

                return new ChatClient(
                    model: modelId,
                    credential: new ApiKeyCredential(settings.ApiKey),
                    options: clientOptions);
            });

            var chatOptions = new ChatCompletionOptions();

            if (settings.MaxTokens.HasValue)
            {
                chatOptions.MaxOutputTokenCount = settings.MaxTokens.Value;
            }

            if (settings.Temperature.HasValue)
            {
                chatOptions.Temperature = (float)settings.Temperature.Value;
            }

            if (settings.ReasoningLevel.HasValue)
            {
#pragma warning disable OPENAI001
                chatOptions.ReasoningEffortLevel = settings.ReasoningLevel.Value switch
                {
                    Models.ReasoningLevel.None => new ChatReasoningEffortLevel("none"),
                    Models.ReasoningLevel.Low => ChatReasoningEffortLevel.Low,
                    Models.ReasoningLevel.Medium => ChatReasoningEffortLevel.Medium,
                    Models.ReasoningLevel.High => ChatReasoningEffortLevel.High,
                    Models.ReasoningLevel.XHigh => new ChatReasoningEffortLevel("xhigh"),
                    Models.ReasoningLevel.Max => new ChatReasoningEffortLevel("max"),
                    _ => null,
                };
#pragma warning restore OPENAI001
            }

            // Wrap user-supplied data in delimiter tags to defend against prompt injection.
            string wrappedData = $"<DATA_BEGIN>\n{data}\n<DATA_END>";

            // Initialize telemetry for this call
            var telemetry = new AIFileProcessor.Telemetry.CallTelemetry { StartUtc = DateTime.UtcNow, Attempts = 1 };

            try
            {
                var completion = await TransientRetryPolicy.ExecuteAsync(
                    async cancellationToken =>
                    {
                        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(
                            new List<ChatMessage> { new SystemChatMessage(prompt), new UserChatMessage(wrappedData) },
                            chatOptions,
                            cancellationToken).ConfigureAwait(false);

                        return result.Value;
                    },
                    IsTransient,
                    settings.MaxRetries,
                    settings.BaseDelay,
                    settings.MaxDelay,
                    _logger,
                    $"OpenAI model {modelId}",
                    ct,
                    onAttempt: attempt => telemetry.Attempts = attempt).ConfigureAwait(false);

                // finalize telemetry and return
                telemetry.EndUtc = DateTime.UtcNow;
                telemetry.ElapsedMs = (long)(telemetry.EndUtc - telemetry.StartUtc).TotalMilliseconds;

                // Try to map token counts; guard against SDK shape differences and possible nulls.
                if (completion != null)
                {
                    try
                    {
                        if (Telemetry.TelemetryUtils.TryExtractTokens(completion, out var p, out var c))
                        {
                            telemetry.PromptTokens = p;
                            telemetry.CompletionTokens = c;
                        }
                    }
                    catch
                    {
                        // best-effort mapping only
                    }
                }

                string text = (completion?.Content != null && completion.Content.Count > 0)
                    ? (completion.Content[0].Text ?? string.Empty)
                    : string.Empty;
                return new AIFileProcessor.Response.AiCallResult
                {
                    Text = text,
                    Telemetry = telemetry,
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API request failed for model {ModelId}.", modelId);
                throw;
            }
        }
    }
}