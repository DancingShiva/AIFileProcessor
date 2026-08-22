//-----------------------------------------------------------------------
// <copyright file="DeepSeekService.cs" company="Midnight Mission Systems">
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
    /// Service that executes requests against the DeepSeek API using the OpenAI-compatible SDK interface.
    /// </summary>
    public class DeepSeekService : AiServiceBase<DeepSeekSettings>
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModelsList =
        [
            new AiModelInfo(
                "DeepSeek-V4-Flash",
                "deepseek-v4-flash",
                [ReasoningLevel.High, ReasoningLevel.Max]),
            new AiModelInfo(
                "DeepSeek-V4-Pro",
                "deepseek-v4-pro",
                [ReasoningLevel.High, ReasoningLevel.Max]),
        ];

        private readonly ConcurrentDictionary<string, ChatClient> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepSeekService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default DeepSeek settings (from <c>IOptionsMonitor&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public DeepSeekService(ILogger<DeepSeekService> logger, IOptionsMonitor<DeepSeekSettings> settings, HttpClient httpClient)
            : base(logger, settings ?? throw new ArgumentNullException(nameof(settings)), httpClient)
        {
        }

        protected override IReadOnlyList<AiModelInfo> SupportedModels => SupportedModelsList;

        protected override string ProviderName => "DeepSeek";

        protected override bool IsProviderTransient(Exception ex)
        {
            // The OpenAI-compatible SDK wraps HTTP errors in ClientResultException; retry on 429/5xx.
            if (ex is ClientResultException cre)
            {
                return cre.Status == (int)System.Net.HttpStatusCode.TooManyRequests
                    || cre.Status >= (int)System.Net.HttpStatusCode.InternalServerError;
            }

            return false;
        }

        /// <summary>
        /// Internal implementation that executes the DeepSeek API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The DeepSeek-specific settings.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        protected override async Task<AiCallResult> ProcessTextCoreAsync(
            string modelId,
            string prompt,
            string data,
            DeepSeekSettings settings,
            CancellationToken ct)
        {
            ValidateRequest(modelId, prompt, data, settings.ApiKey);

            string fingerprint = GetClientFingerprint(modelId, settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var clientOptions = new OpenAIClientOptions
                {
                    Transport = new HttpClientPipelineTransport(HttpClient),
                    Endpoint = new Uri(
                        string.IsNullOrWhiteSpace(settings.Endpoint)
                            ? "https://api.deepseek.com"
                            : settings.Endpoint)
                };

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
                // DeepSeek supports "high" and "max" for reasoning_effort.
                // "low" and "medium" are mapped to "high" for compatibility;
                // "xhigh" is mapped to "max". "none" disables thinking.
                chatOptions.ReasoningEffortLevel = settings.ReasoningLevel.Value switch
                {
                    Models.ReasoningLevel.None => null, // disabled via not setting effort
                    Models.ReasoningLevel.Low => ChatReasoningEffortLevel.High,
                    Models.ReasoningLevel.Minimal => ChatReasoningEffortLevel.High,
                    Models.ReasoningLevel.Medium => ChatReasoningEffortLevel.High,
                    Models.ReasoningLevel.High => ChatReasoningEffortLevel.High,
                    Models.ReasoningLevel.XHigh => new ChatReasoningEffortLevel("max"),
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
                // Build message array using modern, type-safe collection literals
                IEnumerable<ChatMessage> messages;

                if (settings.EnableReasoning)
                {
                    // A gentle, non-sensitive instruction that encourages step-by-step reasoning.
                    var reasoningInstruction = new SystemChatMessage(
                        "When answering, please show your chain-of-thought reasoning step-by-step, then provide a concise final answer. " +
                        "If the model supports explicit reasoning configuration, that will be used instead.");

                    messages = new List<ChatMessage>
                    {
                        reasoningInstruction,
                        new SystemChatMessage(prompt),
                        new SystemChatMessage(PromptInjectionGuard.DataBoundaryInstruction),
                        new UserChatMessage(wrappedData),
                    };
                }
                else
                {
                    messages = new List<ChatMessage>
                    {
                        new SystemChatMessage(prompt),
                        new SystemChatMessage(PromptInjectionGuard.DataBoundaryInstruction),
                        new UserChatMessage(wrappedData),
                    };
                }

                // Initialize telemetry before the API call so the retry hook can update Attempts.
                var telemetry = CreateTelemetry();

                var completion = await TransientRetryPolicy.ExecuteAsync(
                    async cancellationToken =>
                    {
                        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(
                            messages,
                            chatOptions,
                            cancellationToken).ConfigureAwait(false);
                        return result.Value;
                    },
                    IsTransient,
                    settings.MaxRetries,
                    settings.BaseDelay,
                    settings.MaxDelay,
                    Logger,
                    $"DeepSeek model {modelId}",
                    ct,
                    onAttempt: attempt => telemetry.Attempts = attempt).ConfigureAwait(false);

                // finalize telemetry and return
                FinalizeTelemetry(telemetry);

                // ChatCompletion.Usage properties may vary by SDK version; attempt best-effort mapping using reflection helper.
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
                        Logger.LogWarning(ex, "DeepSeek: failed to extract token counts from completion response.");
                    }
                }

                string text = (completion?.Content != null && completion.Content.Count > 0)
                    ? (completion.Content[0].Text ?? string.Empty)
                    : string.Empty;
                return new AiCallResult { Text = text, Telemetry = telemetry };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DeepSeek API request failed for model {ModelId}.", modelId);
                throw;
            }
        }

        /// <summary>
        /// Builds a fingerprint string for client caching, ensuring that users
        /// with different API keys, endpoints, or models get separate cached
        /// <see cref="ChatClient"/> instances.
        /// </summary>
        private static string GetClientFingerprint(string modelId, DeepSeekSettings settings)
        {
            return $"{settings.ApiKey}:{settings.Endpoint}:{modelId}";
        }
    }
}