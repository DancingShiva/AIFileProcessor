//-----------------------------------------------------------------------
// <copyright file="ProcessingController.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Exceptions;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Web.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// API controller for processing Chinese text through the AI pipeline.
    /// Non-secret user preferences are sent in the request body; API keys are resolved server-side.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingController : ControllerBase
    {
        private readonly ProcessingOrchestrator _orchestrator;
        private readonly ILogger<ProcessingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingController"/> class.
        /// </summary>
        /// <param name="orchestrator">The processing orchestrator.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for stashing per-request preferences.</param>
        public ProcessingController(
            ProcessingOrchestrator orchestrator,
            ILogger<ProcessingController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Processes Chinese text through the AI pipeline using the specified model
        /// and the user's per-request settings.
        /// </summary>
        /// <param name="request">The processing request containing model ID, prompt, data, and non-secret preferences.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A <see cref="ProcessResponse"/> with the processing results.</returns>
        [HttpPost("process")]
        public async Task<ActionResult<ProcessResponse>> Process([FromBody] ProcessRequest request, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(request);

            var options = new ProcessingOptions
            {
                ModelId = request.ModelId,
                Prompt = request.Prompt,
                Data = request.Data,
                IdPrefix = request.IdPrefix,
                IdStart = request.IdStart,
                IdPad = request.IdPad,
                LengthPad = request.LengthPad,
            };

            // Validate once with the shared validator (consistent with the desktop UI). An empty
            // IdPrefix is valid for the API (IDs are still generated), so substitute a placeholder
            // to avoid a spurious error while keeping the other field checks.
            var validationOptions = new ProcessingOptions
            {
                ModelId = options.ModelId,
                Prompt = options.Prompt,
                Data = options.Data,
                IdPrefix = string.IsNullOrWhiteSpace(options.IdPrefix) ? "x" : options.IdPrefix,
                IdStart = options.IdStart,
                IdPad = options.IdPad,
                LengthPad = options.LengthPad,
            };

            var errors = ProcessingOptionsValidator.Validate(validationOptions, options.ModelId);
            if (errors.Count > 0)
            {
                return BadRequest(new { errors });
            }

            // Stash non-secret preferences in HttpContext.Items so WebSettingsProvider can overlay them.
            // The request carries narrow DTOs; they are mapped onto fresh Core settings objects
            // populated only from the DTO's safe fields, so client bodies cannot inject
            // infrastructure settings (Endpoint/ProxyUrl/RequestHeaders/AdditionalParameters)
            // that would redirect the server-held API key (SEC-010, WEB-005).
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx != null)
            {
                if (request.GeminiPreferences is { } geminiDto)
                {
                    ctx.Items["GeminiPreferences"] = new AIFileProcessor.Configuration.GeminiSettings
                    {
                        ModelName = geminiDto.ModelName,
                        TimeoutSeconds = geminiDto.TimeoutSeconds,
                        MaxTokens = geminiDto.MaxTokens,
                        Temperature = geminiDto.Temperature,
                        TopP = geminiDto.TopP,
                        ProjectId = geminiDto.ProjectId,
                        Location = geminiDto.Location,
                        StreamingEnabled = geminiDto.StreamingEnabled,
                        ReasoningLevel = geminiDto.ReasoningLevel,
                    };
                }

                if (request.OpenAiPreferences is { } openAiDto)
                {
                    ctx.Items["OpenAiPreferences"] = new AIFileProcessor.Configuration.OpenAiSettings
                    {
                        ModelName = openAiDto.ModelName ?? string.Empty,
                        TimeoutSeconds = openAiDto.TimeoutSeconds,
                        MaxTokens = openAiDto.MaxTokens,
                        Temperature = openAiDto.Temperature,
                        OrganizationId = openAiDto.OrganizationId,
                        ReasoningLevel = openAiDto.ReasoningLevel,
                    };
                }

                if (request.DeepSeekPreferences is { } deepSeekDto)
                {
                    ctx.Items["DeepSeekPreferences"] = new AIFileProcessor.Configuration.DeepSeekSettings
                    {
                        ModelName = deepSeekDto.ModelName,
                        TimeoutSeconds = deepSeekDto.TimeoutSeconds,
                        MaxTokens = deepSeekDto.MaxTokens,
                        Temperature = deepSeekDto.Temperature,
                        EnableReasoning = deepSeekDto.EnableReasoning,
                        ReasoningLevel = deepSeekDto.ReasoningLevel,
                    };
                }
            }

            // Apply reasoning level from the request to the per-model dictionary
            if (request.ReasoningLevel.HasValue)
            {
                options.ModelReasoningLevels[request.ModelId] = request.ReasoningLevel.Value;
            }

            // Apply per-model MaxTokens from the request preferences if available
            if (request.GeminiPreferences?.MaxTokens.HasValue == true)
            {
                options.ModelMaxTokens[request.ModelId] = request.GeminiPreferences.MaxTokens.Value;
            }
            else if (request.OpenAiPreferences?.MaxTokens.HasValue == true)
            {
                options.ModelMaxTokens[request.ModelId] = request.OpenAiPreferences.MaxTokens.Value;
            }
            else if (request.DeepSeekPreferences?.MaxTokens.HasValue == true)
            {
                options.ModelMaxTokens[request.ModelId] = request.DeepSeekPreferences.MaxTokens.Value;
            }

            try
            {
                _logger.LogInformation("Processing request for model {ModelId}.", request.ModelId);

                ProcessingResult result = await _orchestrator.ProcessAsync(options, ct).ConfigureAwait(false);

                return Ok(new ProcessResponse
                {
                    RawResponse = result.RawResponse,
                    Scratchpad = result.Scratchpad,
                    Corrections = result.Corrections,
                    ProcessedSentenceRows = result.ProcessedSentenceRows,
                    Metrics = result.Metrics,
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing was cancelled.");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, "Processing cancelled.");
            }
            catch (ApiKeyNotConfiguredException ex)
            {
                _logger.LogWarning("API key missing for model {ModelId}.", request.ModelId);
                return BadRequest(ex.Message);
            }
            catch (PromptInjectionDetectedException ex)
            {
                _logger.LogWarning("Prompt injection detected in input for model {ModelId}.", request.ModelId);
                return BadRequest(ex.Message);
            }
            catch (InjectionAftermathException ex)
            {
                string errorId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogWarning(ex, "Processing aborted for model {ModelId} due to prompt-injection detection. Error ID: {ErrorId}", request.ModelId, errorId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { errorId, message = $"Processing aborted due to a security check. Reference ID: {errorId}" });
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Unsupported model {ModelId}: {Message}.", request.ModelId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                string errorId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogError(ex, "Processing failed for model {ModelId}. Error ID: {ErrorId}", request.ModelId, errorId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { errorId, message = $"An internal error occurred. Reference ID: {errorId}" });
            }
        }

        /// <summary>
        /// Gets the list of all supported AI models across all providers.
        /// </summary>
        /// <param name="aiServices">The collection of registered AI services.</param>
        /// <returns>A collection of model names and IDs.</returns>
        [HttpGet("models")]
        public ActionResult GetSupportedModels([FromServices] System.Collections.Generic.IEnumerable<IAiService> aiServices)
        {
            var models = new System.Collections.Generic.List<object>();
            foreach (var service in aiServices)
            {
                foreach (var model in service.GetSupportedModels())
                {
                    models.Add(new { name = model.ModelName, id = model.ModelId });
                }
            }

            return Ok(models);
        }
    }
}