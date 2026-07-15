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
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using OpenAI;
    using OpenAI.Chat;

    /// <summary>
    /// Service that executes requests against the DeepSeek API using the OpenAI-compatible SDK interface.
    /// </summary>
    public class DeepSeekService : IAiService
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModels =
        [
            new AiModelInfo("DeepSeek-V4-Flash", "deepseek-v4-flash"),
            new AiModelInfo("DeepSeek-V4-Pro", "deepseek-v4-pro"),
        ];

        private readonly ILogger _logger;
        private readonly DeepSeekSettings _defaultSettings;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, ChatClient> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepSeekService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default DeepSeek settings (from <c>IOptions&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public DeepSeekService(ILogger<DeepSeekService> logger, IOptions<DeepSeekSettings> settings, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IEnumerable<AiModelInfo> GetSupportedModels() => SupportedModels;

        public bool SupportsModel(string modelNameOrId)
        {
            if (string.IsNullOrWhiteSpace(modelNameOrId))
            {
                return false;
            }

            string normalizedModel = modelNameOrId.Trim().ToLowerInvariant();
            foreach (var model in SupportedModels)
            {
                if (model.ModelId.Equals(normalizedModel, StringComparison.OrdinalIgnoreCase)
                    || model.ModelName.Trim().ToLowerInvariant() == normalizedModel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates content using the default (constructor-injected) DeepSeek settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response.</returns>
        public Task<string> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            CancellationToken ct = default)
        {
            return ProcessTextAsync(modelId, prompt, data, _defaultSettings, ct);
        }

        /// <summary>
        /// Generates content using the provided per-call DeepSeek settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The per-call settings (must be a <see cref="DeepSeekSettings"/>).</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response.</returns>
        public Task<string> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            object settings,
            CancellationToken ct = default)
        {
            if (settings is not DeepSeekSettings deepSeekSettings)
            {
                throw new ArgumentException($"Expected DeepSeekSettings but received {settings?.GetType().Name ?? "null"}.", nameof(settings));
            }

            return ProcessTextAsync(modelId, prompt, data, deepSeekSettings, ct);
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

        /// <summary>
        /// Internal implementation that executes the DeepSeek API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        private async Task<string> ProcessTextAsync(
            string modelId,
            string prompt,
            string data,
            DeepSeekSettings settings,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("Model ID is required.", nameof(modelId));
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Data is required.", nameof(data));
            }

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException("DeepSeek API key is not configured.");
            }

            string fingerprint = GetClientFingerprint(modelId, settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var clientOptions = new OpenAIClientOptions
                {
                    Transport = new HttpClientPipelineTransport(_httpClient),
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

            if (settings.MaxTokens.HasValue)
            {
                chatOptions.MaxOutputTokenCount = settings.MaxTokens.Value;
            }

            if (settings.Temperature.HasValue)
            {
                chatOptions.Temperature = (float)settings.Temperature.Value;
            }

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

                    messages =
                    [
                        reasoningInstruction,
                        new SystemChatMessage(prompt),
                        new UserChatMessage(data)
                    ];
                }
                else
                {
                    messages =
                    [
                        new SystemChatMessage(prompt),
                        new UserChatMessage(data)
                    ];
                }

                ClientResult<ChatCompletion> result = await client.CompleteChatAsync(
                    messages,
                    chatOptions,
                    ct);

                ChatCompletion completion = result.Value;

                return completion.Content.Count > 0
                    ? completion.Content[0].Text ?? string.Empty
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeepSeek API request failed for model {ModelId}.", modelId);
                throw;
            }
        }
    }
}