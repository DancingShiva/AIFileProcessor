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
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using AIFileProcessor.Telemetry;
    using Google.GenAI.Types;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Service that executes requests against the Google Gemini API using the official Google.GenAI SDK.
    /// </summary>
    public class GeminiService : AiServiceBase<GeminiSettings>
    {
        private static readonly IReadOnlyList<AiModelInfo> SupportedModelsList =
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
            new AiModelInfo(
                "Gemini 3.7 Flash",
                "gemini-3.7-flash",
                [ReasoningLevel.Minimal, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]),
        ];

        private readonly ConcurrentDictionary<string, Google.GenAI.Client> _clients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default Gemini settings (from <c>IOptions&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        public GeminiService(ILogger<GeminiService> logger, IOptions<GeminiSettings> settings, HttpClient httpClient)
            : base(logger, settings?.Value ?? throw new ArgumentNullException(nameof(settings)), httpClient)
        {
        }

        protected override IReadOnlyList<AiModelInfo> SupportedModels => SupportedModelsList;

        protected override string ProviderName => "Gemini";

        protected override bool IsProviderTransient(Exception ex)
        {
            // Google.GenAI.ClientError wraps HTTP errors; check for transient status codes.
            if (ex is Google.GenAI.ClientError ce)
            {
                int code = ce.StatusCode;
                return code == (int)HttpStatusCode.TooManyRequests || code >= (int)HttpStatusCode.InternalServerError;
            }

            return false;
        }

        /// <summary>
        /// Internal implementation that executes the Gemini API call using the
        /// specified settings, caching clients keyed by a settings fingerprint.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The Gemini-specific settings.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        protected override async Task<AiCallResult> ProcessTextCoreAsync(string modelId, string prompt, string data, GeminiSettings settings, CancellationToken ct)
        {
            ValidateRequest(modelId, prompt, data, settings.ApiKey);

            string fingerprint = GetClientFingerprint(settings);
            var client = _clients.GetOrAdd(fingerprint, _ =>
            {
                var options = new ClientOptions
                {
                    HttpClientFactory = () => HttpClient
                };

                return new Google.GenAI.Client(
                    apiKey: settings.ApiKey,
                    project: settings.ProjectId,
                    location: settings.Location,
                    clientOptions: options);
            });

            var schema = BuildResponseSchema();

            // Append the data-boundary instruction to the system instruction so the
            // model understands the data boundary before receiving the user message.
            var systemText = $"""
                {prompt}

                {PromptInjectionGuard.DataBoundaryInstruction}
                """;

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemText } }
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

            // Wrap user-supplied data in cryptographically random per-request delimiter
            // tags to defend against prompt injection (tag unpredictability + leak detection).
            string wrappedData = WrapData(data);

            string targetModel = modelId;

            // Initialize telemetry for this call
            var telemetry = CreateTelemetry();

            if (settings.StreamingEnabled)
            {
                try
                {
                    var responseStream = client.Models.GenerateContentStreamAsync(
                        model: targetModel,
                        contents: wrappedData,
                        config: config,
                        cancellationToken: ct);

                    using var writer = new StringWriter();
                    bool sawFirst = false;
                    Google.GenAI.Types.GenerateContentResponseUsageMetadata? streamUsage = null;
                    await foreach (var chunk in responseStream.WithCancellation(ct).ConfigureAwait(false))
                    {
                        ct.ThrowIfCancellationRequested();

                        if (!sawFirst)
                        {
                            sawFirst = true;
                            telemetry.TimeToFirstByteMs = (long)(DateTime.UtcNow - telemetry.StartUtc).TotalMilliseconds;
                        }

                        // Usage metadata is typically only present on the final streamed chunk.
                        streamUsage ??= chunk.UsageMetadata;

                        if (chunk.Text != null)
                        {
                            await writer.WriteAsync(chunk.Text);

                            if (settings.EnableLogging)
                            {
                                Logger.LogInformation("{Chunk}", chunk.Text);
                            }
                        }
                    }

                    FinalizeTelemetry(telemetry);

                    // Map token counts from the usage metadata carried by the final streamed chunk.
                    try
                    {
                        if (streamUsage != null)
                        {
                            telemetry.PromptTokens = streamUsage.PromptTokenCount;
                            telemetry.CompletionTokens = streamUsage.CandidatesTokenCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Gemini streaming: failed to extract token counts from usage metadata.");
                    }

                    // Return AiCallResult carrying the concatenated text and telemetry
                    return new AiCallResult { Text = writer.ToString(), Telemetry = telemetry };
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Gemini streaming API request failed for model {TargetModel}.", targetModel);
                    throw;
                }
            }
            else
            {
                try
                {
                    var response = await TransientRetryPolicy.ExecuteAsync(
                        async cancellationToken =>
                        {
                            return await client.Models.GenerateContentAsync(
                                model: targetModel,
                                contents: wrappedData,
                                config: config,
                                cancellationToken: cancellationToken).ConfigureAwait(false);
                        },
                        IsTransient,
                        settings.MaxRetries,
                        settings.BaseDelay,
                        settings.MaxDelay,
                        Logger,
                        $"Gemini model {targetModel}",
                        ct,
                        onAttempt: attempt => telemetry.Attempts = attempt).ConfigureAwait(false);

                    if (settings.EnableLogging)
                    {
                        Logger.LogInformation("[Gemini Log] Model: {TargetModel}, Response Status: Success", targetModel);
                    }

                    // finalize telemetry for non-streaming response
                    FinalizeTelemetry(telemetry);

                    // Try reflection-based mapping for token counts
                    try
                    {
                        if (TelemetryUtils.TryExtractTokens(response, out var p, out var c))
                        {
                            telemetry.PromptTokens = p;
                            telemetry.CompletionTokens = c;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Gemini: failed to extract token counts from completion response.");
                    }

                    return new AiCallResult { Text = response.Text ?? string.Empty, Telemetry = telemetry };
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Gemini API request failed for model {TargetModel}.", targetModel);
                    throw;
                }
            }
        }

        /// <summary>
        /// Builds the JSON response schema used to constrain Gemini output to the
        /// expected sentence-processing structure.
        /// </summary>
        /// <returns>The response schema for the Gemini API.</returns>
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
        /// Builds a fingerprint string for client caching, ensuring that users
        /// with different API keys, endpoints, projects, or locations get separate
        /// cached <see cref="Google.GenAI.Client"/> instances.
        /// </summary>
        private static string GetClientFingerprint(GeminiSettings settings)
        {
            return $"{settings.ApiKey}:{settings.Endpoint}:{settings.ProjectId}:{settings.Location}";
        }
    }
}