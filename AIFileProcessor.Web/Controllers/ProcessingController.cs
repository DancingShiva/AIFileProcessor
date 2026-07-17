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
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Web.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// API controller for processing Chinese text through the AI pipeline.
    /// Users supply their own API keys via request headers; non-secret
    /// preferences are sent in the request body.
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

            if (string.IsNullOrWhiteSpace(request.ModelId))
            {
                return BadRequest("ModelId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Data))
            {
                return BadRequest("Data is required.");
            }

            // Stash non-secret preferences in HttpContext.Items so WebSettingsProvider can overlay them
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx != null)
            {
                if (request.GeminiPreferences != null)
                {
                    ctx.Items["GeminiPreferences"] = request.GeminiPreferences;
                }

                if (request.OpenAiPreferences != null)
                {
                    ctx.Items["OpenAiPreferences"] = request.OpenAiPreferences;
                }

                if (request.DeepSeekPreferences != null)
                {
                    ctx.Items["DeepSeekPreferences"] = request.DeepSeekPreferences;
                }
            }

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

            // Apply reasoning level from the request to the per-model dictionary
            if (request.ReasoningLevel.HasValue)
            {
                options.ModelReasoningLevels[request.ModelId] = request.ReasoningLevel.Value;
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
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing was cancelled.");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, "Processing cancelled.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("API key is not configured", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("API key missing for model {ModelId}.", request.ModelId);
                return BadRequest(ex.Message);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Unsupported model {ModelId}: {Message}.", request.ModelId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed for model {ModelId}.", request.ModelId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Processing failed: {ex.Message}");
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