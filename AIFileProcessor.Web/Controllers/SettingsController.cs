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
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private readonly ILogger<SettingsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="settingsProvider">The settings provider for resolving settings.</param>
        /// <param name="userContext">The current user context.</param>
        /// <param name="logger">The logger instance.</param>
        public SettingsController(
            ISettingsProvider settingsProvider,
            IUserContext userContext,
            ILogger<SettingsController> logger)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

            // ApiKey is [JsonIgnore] on the settings models, so it is automatically
            // excluded from the JSON response — no manual stripping needed.
            return Ok(new ProviderSettingsResponse
            {
                GeminiSettings = gemini,
                OpenAiSettings = openAi,
                DeepSeekSettings = deepSeek,
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