//-----------------------------------------------------------------------
// <copyright file="GeminiService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using Google.GenAI.Types;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Service that executes requests against the Google Gemini API using the official Google.GenAI SDK.
    /// </summary>
    public class GeminiService : IAiService
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModels =
        [
            new AiModelInfo(
                "Gemini 3 Flash",
                "gemini-3-flash",
                [ReasoningLevel.Minimal, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]),
            new AiModelInfo(
                "Gemini 3.1 Pro",
                "gemini-3.1-pro-preview",
                [ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]),
            new AiModelInfo(
                "Gemini 3.5 Flash",
                "gemini-3.5-flash",
                [ReasoningLevel.Minimal, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]),
            new AiModelInfo(
                "Gemini 3.6 Flash",
                "gemini-3.6-flash",
                [ReasoningLevel.Minimal, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]),
        ];

        private readonly GeminiSettings _defaultSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, Google.GenAI.Client> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default Gemini settings (from <c>IOptions&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public GeminiService(ILogger<GeminiService> logger, IOptions<GeminiSettings> settings, HttpClient httpClient)
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
        /// Generates content using the default (constructor-injected) Gemini settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response.</returns>
        public Task<string> ProcessTextAsync(string modelId, string prompt, string data, CancellationToken ct = default)
        {
            return ProcessTextAsync(modelId, prompt, data, _defaultSettings, ct);
        }

        /// <summary>
        /// Generates content using the provided per-call Gemini settings.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The per-call settings (must be a <see cref="GeminiSettings"/>).</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response.</returns>
        public Task<string> ProcessTextAsync(string modelId, string prompt, string data, object settings, CancellationToken ct = default)
        {
            GeminiSettings geminiSettings = AiServiceGuard.RequireSettings<GeminiSettings>(settings);
            return ProcessTextAsync(modelId, prompt, data, geminiSettings, ct);
        }

        /// <summary>
        /// Builds a fingerprint string for client caching, ensuring that users
        /// with different API keys, endpoints, projects, or locations get separate
        /// cached <see cref="Google.GenAI.Client"/> instances.
        /// </summary>
        private static string GetClientFingerprint(GeminiSettings settings)
        {
            return $"{settings.ApiKey}:{settings.Endpoint}:{settings.ProjectId}:{settings.Location}";
        }

        private static bool IsTransient(Exception ex)
        {
            if (ex is HttpRequestException hre)
            {
                return hre.StatusCode is null or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError;
            }

            // Google.GenAI.ClientError wraps HTTP errors; check for transient status codes.
            if (ex is Google.GenAI.ClientError ce)
            {
                int code = ce.StatusCode;
                return code == (int)HttpStatusCode.TooManyRequests || code >= (int)HttpStatusCode.InternalServerError;
            }

            // Timeout exceptions are transient.
            if (ex is TimeoutException or TaskCanceledException)
            {
                return true;
            }

            return false;
        }

        private static Schema BuildResponseSchema()
        {
            Schema sentenceSchema = new()
            {
                Type = Google.GenAI.Types.Type.Object,
                Properties = new()
                    {
                        { "C", new() { Type = Google.GenAI.Types.Type.String } },
                        { "P", new() { Type = Google.GenAI.Types.Type.String } },
                        { "B", new() { Type = Google.GenAI.Types.Type.String } },
                        { "R", new() { Type = Google.GenAI.Types.Type.String } },
                        { "E", new() { Type = Google.GenAI.Types.Type.String } }
                    },
                Required = new System.Collections.Generic.List<string> { "C", "P", "B", "R", "E" }
            };

            return new()
            {
                Type = Google.GenAI.Types.Type.Object,
                Properties = new()
                {
                    { "SP", new() { Type = Google.GenAI.Types.Type.String } },
                    {
                        "Corrections", new()
                        {
                            Type = Google.GenAI.Types.Type.Array,
                            Items = new() { Type = Google.GenAI.Types.Type.String }
                        }
                    },
                    {
                        "Sentences", new()
                        {
                            Type = Google.GenAI.Types.Type.Array,
                            Items = sentenceSchema
                        }
                    }
                },
                Required = new System.Collections.Generic.List<string> { "SP", "Sentences" }
            };
        }

        /// <summary>
        /// Internal implementation that executes the Gemini API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        private async Task<string> ProcessTextAsync(string modelId, string prompt, string data, GeminiSettings settings, CancellationToken ct)
        {
            AiServiceGuard.RequireValidRequest(modelId, prompt, data);

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured.");
            }

            string fingerprint = GetClientFingerprint(settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var options = new ClientOptions
                {
                    HttpClientFactory = () => _httpClient
                };

                return new Google.GenAI.Client(
                    apiKey: settings.ApiKey,
                    project: settings.ProjectId,
                    location: settings.Location,
                    clientOptions: options);
            });

            var schema = BuildResponseSchema();

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = prompt } }
                },
                ResponseMimeType = "application/json",
                ResponseSchema = schema,
                Temperature = (float?)settings.Temperature,
                TopP = (float?)settings.TopP,
                MaxOutputTokens = settings.MaxTokens,
            };

            if (settings.ReasoningLevel.HasValue)
            {
                config.ThinkingConfig = new ThinkingConfig
                {
                    ThinkingLevel = settings.ReasoningLevel.Value switch
                    {
                        Models.ReasoningLevel.Minimal => Google.GenAI.Types.ThinkingLevel.Minimal,
                        Models.ReasoningLevel.Low => Google.GenAI.Types.ThinkingLevel.Low,
                        Models.ReasoningLevel.Medium => Google.GenAI.Types.ThinkingLevel.Medium,
                        Models.ReasoningLevel.High => Google.GenAI.Types.ThinkingLevel.High,
                        _ => Google.GenAI.Types.ThinkingLevel.ThinkingLevelUnspecified,
                    },
                };
            }

            string targetModel = modelId;

            if (settings.StreamingEnabled)
            {
                try
                {
                    var responseStream = client.Models.GenerateContentStreamAsync(
                        model: targetModel,
                        contents: data,
                        config: config,
                        cancellationToken: ct);

                    using var writer = new StringWriter();
                    await foreach (var chunk in responseStream.WithCancellation(ct).ConfigureAwait(false))
                    {
                        ct.ThrowIfCancellationRequested();

                        if (chunk.Text != null)
                        {
                            await writer.WriteAsync(chunk.Text);

                            if (settings.EnableLogging)
                            {
                                _logger.LogInformation("{Chunk}", chunk.Text);
                            }
                        }
                    }

                    return writer.ToString();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gemini streaming API request failed for model {TargetModel}.", targetModel);
                    throw;
                }
            }
            else
            {
                try
                {
                    return await TransientRetryPolicy.ExecuteAsync(
                        async cancellationToken =>
                        {
                            var response = await client.Models.GenerateContentAsync(
                                model: targetModel,
                                contents: data,
                                config: config,
                                cancellationToken: cancellationToken).ConfigureAwait(false);

                            if (settings.EnableLogging)
                            {
                                _logger.LogInformation("[Gemini Log] Model: {TargetModel}, Response Status: Success", targetModel);
                            }

                            return response.Text ?? string.Empty;
                        },
                        IsTransient,
                        settings.MaxRetries,
                        settings.BaseDelay,
                        settings.MaxDelay,
                        _logger,
                        $"Gemini model {targetModel}",
                        ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gemini API request failed for model {TargetModel}.", targetModel);
                    throw;
                }
            }
        }
    }
}