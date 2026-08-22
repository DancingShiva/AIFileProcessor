//-----------------------------------------------------------------------
// <copyright file="OllamaService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;
    using AIFileProcessor.Telemetry;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using OllamaSharp;
    using OllamaSharp.Models;
    using OllamaSharp.Models.Chat;
    using OllamaModel = OllamaSharp.Models.Model;

    /// <summary>
    /// Service that executes requests against an Ollama server using the OllamaSharp
    /// SDK over the native Ollama REST API. Ollama fronts both locally-run models
    /// and models hosted in Ollama Cloud behind the same API, so the supported-model
    /// catalog is discovered at runtime (local models via <c>/api/tags</c>, plus
    /// configured cloud/known models from the <c>OllamaModels</c> configuration
    /// section) rather than being a fixed list. Generation options are exposed as
    /// strongly-typed settings that map into the Ollama <c>options</c> wire payload
    /// via the SDK's <see cref="RequestOptions"/>.
    /// </summary>
    public class OllamaService : AiServiceBase<OllamaSettings>
    {
        /// <summary>Fallback context window when the server does not report one.</summary>
        private const int DefaultContextLength = 8192;

        private static readonly IReadOnlyList<AiModelInfo> FallbackModels =
        [
            new AiModelInfo("Llama 3.1 (8B)", "llama3.1"),
            new AiModelInfo("Qwen 2.5 (7B)", "qwen2.5"),
        ];

        private readonly ConcurrentDictionary<string, IOllamaApiClient> _clients = new();
        private readonly ConcurrentDictionary<string, int> _contextLengths = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _catalogLock = new(1, 1);
        private readonly List<AiModelInfo> _configuredModels;
        private volatile IReadOnlyList<AiModelInfo>? _catalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="settings">The default Ollama settings (from <c>IOptionsMonitor&lt;T&gt;</c>).</param>
        /// <param name="httpClient">The HTTP client for making requests, managed by IHttpClientFactory.</param>
        /// <param name="modelSettings">The per-model Ollama settings from the <c>OllamaModels</c> configuration section.</param>
        public OllamaService(
            ILogger<OllamaService> logger,
            IOptionsMonitor<OllamaSettings> settings,
            HttpClient httpClient,
            IOptionsMonitor<Dictionary<string, OllamaModelSettings>> modelSettings)
            : base(logger, settings ?? throw new ArgumentNullException(nameof(settings)), httpClient)
        {
            ArgumentNullException.ThrowIfNull(modelSettings);
            _configuredModels = [.. (modelSettings.CurrentValue ?? [])
                .Select(kvp => ToModelInfo(kvp.Key, kvp.Value))];
        }

        /// <inheritdoc/>
        protected override bool RequiresApiKey => false;

        /// <inheritdoc/>
        protected override string ProviderName => "Ollama";

        /// <inheritdoc/>
        protected override IReadOnlyList<AiModelInfo> SupportedModels => ResolveSupportedModels();

        /// <summary>
        /// Gets the context-window size for a model, querying the Ollama server
        /// (<c>/api/show</c>) on first use and caching the result per model.
        /// Falls back to <see cref="DefaultContextLength"/> when the server does
        /// not report a context length.
        /// </summary>
        /// <param name="modelId">The model whose context length to resolve.</param>
        /// <param name="settings">The effective Ollama settings used for the lookup.</param>
        /// <param name="ct">A token to cancel the lookup.</param>
        /// <returns>The model's context length in tokens.</returns>
        public async Task<int> GetContextLengthAsync(string modelId, OllamaSettings settings, CancellationToken ct = default)
        {
            if (settings.ContextWindow.HasValue)
            {
                return settings.ContextWindow.Value;
            }

            if (_contextLengths.TryGetValue(modelId, out int cached))
            {
                return cached;
            }

            try
            {
                var client = GetClient(settings);
                ShowModelResponse? info = await client.ShowModelAsync(new ShowModelRequest { Model = modelId }, ct).ConfigureAwait(false);
                int length = TryExtractContextLength(info?.Info?.ExtraInfo);

                if (length <= 0)
                {
                    length = DefaultContextLength;
                }

                _contextLengths[modelId] = length;
                return length;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogWarning(ex, "Ollama: failed to query context length for model {ModelId}; using default {Default}.", modelId, DefaultContextLength);
                return DefaultContextLength;
            }
        }

        /// <summary>
        /// Refreshes the supported-model catalog from the Ollama server. Local
        /// models come from <c>/api/tags</c> (via
        /// <see cref="IOllamaApiClient.ListLocalModelsAsync(CancellationToken)"/>);
        /// configured models are always included so they remain selectable when
        /// the server is reachable. Refresh is best-effort: on failure the previous
        /// catalog is retained and the failure is logged.
        /// </summary>
        /// <param name="ct">A token to cancel the refresh.</param>
        /// <returns>A task representing the refresh operation.</returns>
        public async Task RefreshModelCatalogAsync(CancellationToken ct = default)
        {
            await _catalogLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var client = GetClient(DefaultSettings);
                Logger.LogInformation("Ollama: refreshing model catalog from endpoint {Endpoint}", DefaultSettings.Endpoint);
                IEnumerable<OllamaModel> localModels = await client.ListLocalModelsAsync(ct).ConfigureAwait(false);
                Logger.LogInformation("Ollama: received {Count} models from server", localModels.Count());

                var models = new List<AiModelInfo>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var local in localModels)
                {
                    if (!string.IsNullOrWhiteSpace(local.Name) && seen.Add(local.Name))
                    {
                        models.Add(new AiModelInfo(local.Name, local.Name));
                    }
                }

                foreach (var configured in _configuredModels)
                {
                    if (seen.Add(configured.ModelId))
                    {
                        models.Add(configured);
                    }
                }

                if (models.Count > 0)
                {
                    _catalog = models;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Ollama: failed to refresh the model catalog from {Endpoint}; retaining the previous catalog. Error: {Message}", DefaultSettings.Endpoint, ex.Message);
            }
            finally
            {
                _catalogLock.Release();
            }
        }

        /// <summary>
        /// Maps the strongly-typed Ollama settings onto the OllamaSharp
        /// <see cref="RequestOptions"/> options bag. Only the strongly-typed
        /// subset is supported; keys omitted (null) inherit the model's Modelfile
        /// default on the server.
        /// </summary>
        /// <param name="settings">The effective Ollama settings.</param>
        /// <returns>The populated request options for the chat call.</returns>
        internal static RequestOptions BuildOptions(OllamaSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            return new RequestOptions
            {
                Temperature = settings.Temperature.HasValue ? (float)settings.Temperature.Value : null,
                TopP = settings.TopP.HasValue ? (float)settings.TopP.Value : null,
                TopK = settings.TopK,
                NumCtx = settings.ContextWindow,
                NumPredict = settings.MaxTokens,
                RepeatPenalty = settings.RepeatPenalty.HasValue ? (float)settings.RepeatPenalty.Value : null,
                Seed = settings.Seed,
                Stop = settings.Stop is { Count: > 0 } ? [.. settings.Stop] : null,
                MinP = settings.MinP.HasValue ? (float)settings.MinP.Value : null,
                TypicalP = settings.TypicalP.HasValue ? (float)settings.TypicalP.Value : null,
                TfsZ = settings.TfsZ.HasValue ? (float)settings.TfsZ.Value : null,
                MiroStat = settings.MiroStat,
                MiroStatTau = settings.MiroStatTau.HasValue ? (float)settings.MiroStatTau.Value : null,
                MiroStatEta = settings.MiroStatEta.HasValue ? (float)settings.MiroStatEta.Value : null,
                PresencePenalty = settings.PresencePenalty.HasValue ? (float)settings.PresencePenalty.Value : null,
                FrequencyPenalty = settings.FrequencyPenalty.HasValue ? (float)settings.FrequencyPenalty.Value : null,
                RepeatLastN = settings.RepeatLastN,
                NumGpu = settings.NumGpu,
                NumBatch = settings.NumBatch,
                NumThread = settings.NumThread,
                NumKeep = settings.NumKeep,
                Numa = settings.Numa,
                UseMlock = settings.UseMlock,
                UseMmap = settings.UseMmap,
                LowVram = settings.LowVram,
            };
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<AiModelInfo> ResolveSupportedModels()
        {
            return _catalog
                ?? (_configuredModels.Count > 0 ? _configuredModels : FallbackModels);
        }

        /// <inheritdoc/>
        protected override bool IsProviderTransient(Exception ex)
        {
            // Non-HTTP SDK failures (OllamaException) are generally not transient
            // (e.g., model-not-found); leave them unretried. The base class handles
            // HttpRequestException, TimeoutException, and TaskCanceledException.
            return false;
        }

        /// <summary>
        /// Internal implementation that executes the Ollama chat call using the
        /// specified settings, caching the underlying SDK client keyed by a
        /// settings fingerprint. Requests non-streamed JSON output constrained to
        /// the expected schema and applies the full set of configured model options.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        /// <param name="settings">The Ollama-specific settings.</param>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>The generated text response and telemetry.</returns>
        protected override async Task<AiCallResult> ProcessTextCoreAsync(
            string modelId,
            string prompt,
            string data,
            OllamaSettings settings,
            CancellationToken ct)
        {
            ValidateRequest(modelId, prompt, data, settings.ApiKey);

            var client = GetClient(settings);
            ThinkValue? think = settings.EnableThinking.HasValue ? new ThinkValue(settings.EnableThinking.Value) : default(ThinkValue?);

            var request = new ChatRequest
            {
                Model = modelId,
                Stream = false,
                Format = "json",
                Options = BuildOptions(settings),
                KeepAlive = settings.KeepAlive,
                Think = think,
                Messages =
                [
                    new Message(ChatRole.System, prompt),
                    new Message(ChatRole.System, PromptInjectionGuard.DataBoundaryInstruction),
                    new Message(ChatRole.User, WrapData(data)),
                ],
            };

            var telemetry = CreateTelemetry();

            try
            {
                var (text, done) = await TransientRetryPolicy.ExecuteAsync(
                    async cancellationToken =>
                    {
                        var text = new StringBuilder();
                        ChatDoneResponseStream? doneFrame = null;

                        await foreach (var frame in client.ChatAsync(request, cancellationToken).ConfigureAwait(false))
                        {
                            if (frame?.Message?.Content != null)
                            {
                                text.Append(frame.Message.Content);
                            }

                            if (frame is ChatDoneResponseStream doneStream)
                            {
                                doneFrame = doneStream;
                            }
                        }

                        return (text.ToString(), doneFrame);
                    },
                    IsTransient,
                    settings.MaxRetries,
                    settings.BaseDelay,
                    settings.MaxDelay,
                    Logger,
                    $"Ollama model {modelId}",
                    ct,
                    onAttempt: attempt => telemetry.Attempts = attempt).ConfigureAwait(false);

                FinalizeTelemetry(telemetry);

                if (done != null)
                {
                    telemetry.PromptTokens = done.PromptEvalCount;
                    telemetry.CompletionTokens = done.EvalCount;
                }

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
                Logger.LogError(ex, "Ollama API request failed for model {ModelId}.", modelId);
                throw;
            }
        }

        /// <summary>
        /// Builds a fingerprint string for client caching, ensuring that
        /// configurations with different endpoints, API keys, or request headers
        /// get separate cached <see cref="IOllamaApiClient"/> instances.
        /// </summary>
        /// <param name="settings">The effective Ollama settings.</param>
        /// <returns>The cache fingerprint string.</returns>
        private static string GetClientFingerprint(OllamaSettings settings)
        {
            string headers = settings.RequestHeaders == null
                ? string.Empty
                : string.Join(
                    ";",
                    settings.RequestHeaders
                        .OrderBy(h => h.Key, StringComparer.Ordinal)
                        .Select(h => $"{h.Key}={h.Value}"));

            return $"{settings.Endpoint}|{settings.ApiKey}|{headers}";
        }

        /// <summary>
        /// Extracts the trained context length from the model-info extra-info map
        /// (keys like <c>llama.context_length</c> or <c>qwen2.context_length</c>).
        /// </summary>
        /// <param name="extraInfo">The model-info extension-data map, or null.</param>
        /// <returns>The context length in tokens, or 0 when not reported.</returns>
        private static int TryExtractContextLength(IDictionary<string, object>? extraInfo)
        {
            if (extraInfo == null)
            {
                return 0;
            }

            foreach (var kvp in extraInfo)
            {
                if (kvp.Key.EndsWith("context_length", StringComparison.OrdinalIgnoreCase)
                    && kvp.Value != null
                    && int.TryParse(Convert.ToString(kvp.Value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out int length)
                    && length > 0)
                {
                    return length;
                }
            }

            return 0;
        }

        /// <summary>
        /// Creates an <see cref="AiModelInfo"/> for a configured model entry, using
        /// the model's thinking capability to decide which reasoning levels it
        /// advertises to the UI.
        /// </summary>
        /// <param name="modelId">The model identifier used as both ID and display name.</param>
        /// <param name="modelSettings">The configured per-model settings.</param>
        /// <returns>The populated model info.</returns>
        private static AiModelInfo ToModelInfo(string modelId, OllamaModelSettings modelSettings)
        {
            List<ReasoningLevel> reasoningLevels = modelSettings.SupportsThinking
                ? [ReasoningLevel.None, ReasoningLevel.Low, ReasoningLevel.Medium, ReasoningLevel.High]
                : [ReasoningLevel.None];

            string displayName = modelSettings.IsCloud ? $"{modelId} (cloud)" : modelId;
            return new AiModelInfo(displayName, modelId, reasoningLevels);
        }

        /// <summary>
        /// Gets (or creates and caches) an <see cref="IOllamaApiClient"/> for the
        /// effective settings. Clients are cached by a fingerprint of the endpoint,
        /// API key, and request headers so distinct configurations get separate
        /// instances. The DI-managed <see cref="HttpClient"/> is reused as the
        /// transport; its BaseAddress is bound to the Ollama endpoint by the DI
        /// registration.
        /// </summary>
        /// <param name="settings">The effective Ollama settings.</param>
        /// <returns>The SDK client for the settings.</returns>
        private IOllamaApiClient GetClient(OllamaSettings settings)
        {
            string fingerprint = GetClientFingerprint(settings);
            return _clients.GetOrAdd(fingerprint, _ =>
            {
                var client = new OllamaApiClient(HttpClient);

                if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                {
                    client.DefaultRequestHeaders["Authorization"] = $"Bearer {settings.ApiKey}";
                }

                if (settings.RequestHeaders != null)
                {
                    foreach (var header in settings.RequestHeaders)
                    {
                        if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
                        {
                            client.DefaultRequestHeaders[header.Key] = header.Value!;
                        }
                    }
                }

                return client;
            });
        }
    }
}
