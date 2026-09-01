//-----------------------------------------------------------------------
// <copyright file="SettingsController.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Web.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// API controller for retrieving and saving per-user AI provider settings.
    /// API keys are sourced from server-side environment variables and are never
    /// received from or sent to the client. Non-secret preferences are stored
    /// client-side in localStorage and sent with each processing request.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController(
        ISettingsProvider settingsProvider,
        IUserContext userContext,
        ILogger<SettingsController> logger) : ControllerBase
    {
        private readonly ISettingsProvider _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        private readonly IUserContext _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        private readonly ILogger<SettingsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Gets the current settings for all three AI providers. API keys are
        /// stripped from the response for security.
        /// </summary>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>A <see cref="ProviderSettingsResponse"/> with current settings (keys stripped via [JsonIgnore]).</returns>
        [HttpGet]
        public async Task<ActionResult<ProviderSettingsResponse>> GetSettings(CancellationToken ct)
        {
            var gemini = await _settingsProvider.GetGeminiSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);
            var openAi = await _settingsProvider.GetOpenAiSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);
            var deepSeek = await _settingsProvider.GetDeepSeekSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);
            var ollama = await _settingsProvider.GetOllamaSettingsAsync(_userContext.UserId, ct).ConfigureAwait(false);

            // Map Core settings onto narrow DTOs: ApiKey is [JsonIgnore] on the settings
            // models, and infrastructure settings (Endpoint/ProxyUrl/RequestHeaders/
            // AdditionalParameters) are deliberately omitted — machine-level infrastructure
            // may embed gateway credentials and must not be echoed to clients (SEC-010).
            return Ok(new ProviderSettingsResponse
            {
                GeminiSettings = new GeminiPreferencesDto
                {
                    ModelName = gemini.ModelName,
                    TimeoutSeconds = gemini.TimeoutSeconds,
                    MaxTokens = gemini.MaxTokens,
                    Temperature = gemini.Temperature,
                    TopP = gemini.TopP,
                    ProjectId = gemini.ProjectId,
                    Location = gemini.Location,
                    StreamingEnabled = gemini.StreamingEnabled,
                    ReasoningLevel = gemini.ReasoningLevel,
                },
                OpenAiSettings = new OpenAiPreferencesDto
                {
                    ModelName = openAi.ModelName,
                    TimeoutSeconds = openAi.TimeoutSeconds,
                    MaxTokens = openAi.MaxTokens,
                    Temperature = openAi.Temperature,
                    OrganizationId = openAi.OrganizationId,
                    ReasoningLevel = openAi.ReasoningLevel,
                },
                DeepSeekSettings = new DeepSeekPreferencesDto
                {
                    ModelName = deepSeek.ModelName,
                    TimeoutSeconds = deepSeek.TimeoutSeconds,
                    MaxTokens = deepSeek.MaxTokens,
                    Temperature = deepSeek.Temperature,
                    EnableReasoning = deepSeek.EnableReasoning,
                    ReasoningLevel = deepSeek.ReasoningLevel,
                },
                OllamaSettings = new OllamaPreferencesDto
                {
                    ModelName = ollama.ModelName,
                    TimeoutSeconds = ollama.TimeoutSeconds,
                    MaxTokens = ollama.MaxTokens,
                    Temperature = ollama.Temperature,
                    TopP = ollama.TopP,
                    TopK = ollama.TopK,
                    ContextWindow = ollama.ContextWindow,
                    EnableThinking = ollama.EnableThinking,
                    ReasoningLevel = ollama.ReasoningLevel,
                },
            });
        }

        /// <summary>
        /// Saves settings for all three AI providers. In web mode, this is a
        /// no-op on the server — preferences should be persisted client-side.
        /// API keys are configured via server-side environment variables.
        /// </summary>
        /// <param name="request">The settings to save.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpPost]
        public IActionResult SaveSettings([FromBody] ProviderSettingsResponse request, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(request);

            // In web mode, settings persistence is client-side (localStorage).
            // This endpoint exists for API symmetry with the desktop SettingsForm.
            // The client should store non-secret preferences in localStorage and
            // send them with each processing request. API keys are configured via
            // server-side environment variables, not by the client.
            _logger.LogInformation("Settings save requested for user {UserId} (no-op in web mode).", _userContext.UserId);

            return Ok(new { message = "In web mode, store preferences client-side in localStorage. API keys are configured via server-side environment variables." });
        }
    }
}