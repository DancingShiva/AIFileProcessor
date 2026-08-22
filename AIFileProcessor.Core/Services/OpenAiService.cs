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
    public class OpenAiService : AiServiceBase<OpenAiSettings>
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModelsList =
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

        private readonly ConcurrentDictionary<string, ChatClient> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default OpenAI settings (from <c>IOptionsMonitor&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public OpenAiService(ILogger<OpenAiService> logger, IOptionsMonitor<OpenAiSettings> settings, HttpClient httpClient)
            : base(logger, settings ?? throw new ArgumentNullException(nameof(settings)), httpClient)
        {
        }

        protected override IReadOnlyList<AiModelInfo> SupportedModels => SupportedModelsList;

        protected override string ProviderName => "OpenAI";

        protected override bool IsProviderTransient(Exception ex)
        {
            // The OpenAI SDK wraps HTTP errors in ClientResultException; retry on 429/5xx.
            if (ex is ClientResultException cre)
            {
                return cre.Status == (int)System.Net.HttpStatusCode.TooManyRequests
                    || cre.Status >= (int)System.Net.HttpStatusCode.InternalServerError;
            }

            return false;
        }

        /// <summary>
        /// Internal implementation that executes the OpenAI API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The OpenAI-specific settings.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        protected override async Task<AiCallResult> ProcessTextCoreAsync(
            string modelId,
            string prompt,
            string data,
            OpenAiSettings settings,
            CancellationToken ct)
        {
            ValidateRequest(modelId, prompt, data, settings.ApiKey);

            string fingerprint = GetClientFingerprint(modelId, settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var clientOptions = new OpenAIClientOptions
                {
                    Transport = new HttpClientPipelineTransport(HttpClient),
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

            // Enforce JSON output to constrain the model to the expected schema.
            chatOptions.ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat();

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

            // Wrap user-supplied data in cryptographically random per-request delimiter
            // tags to defend against prompt injection (tag unpredictability + leak detection).
            string wrappedData = WrapData(data);

            try
            {
                // Initialize telemetry for this call
                var telemetry = CreateTelemetry();

                var completion = await TransientRetryPolicy.ExecuteAsync(
                    async cancellationToken =>
                    {
                        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(
                            new List<ChatMessage>
                            {
                                new SystemChatMessage(prompt),
                                new SystemChatMessage(PromptInjectionGuard.DataBoundaryInstruction),
                                new UserChatMessage(wrappedData),
                            },
                            chatOptions,
                            cancellationToken).ConfigureAwait(false);

                        return result.Value;
                    },
                    IsTransient,
                    settings.MaxRetries,
                    settings.BaseDelay,
                    settings.MaxDelay,
                    Logger,
                    $"OpenAI model {modelId}",
                    ct,
                    onAttempt: attempt => telemetry.Attempts = attempt).ConfigureAwait(false);

                // finalize telemetry and return
                FinalizeTelemetry(telemetry);

                // Try to map token counts; guard against SDK shape differences and possible nulls.
                if (completion != null)
                {
                    try
                    {
                        if (TelemetryUtils.TryExtractTokens(completion, out var p, out var c))
                        {
                            telemetry.PromptTokens = p;
                            telemetry.CompletionTokens = c;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "OpenAI: failed to extract token counts from completion response.");
                    }
                }

                string text = (completion?.Content != null && completion.Content.Count > 0)
                    ? (completion.Content[0].Text ?? string.Empty)
                    : string.Empty;
                return new AiCallResult
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
                Logger.LogError(ex, "OpenAI API request failed for model {ModelId}.", modelId);
                throw;
            }
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
    }
}