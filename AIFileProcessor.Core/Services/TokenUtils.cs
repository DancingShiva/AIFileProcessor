//-----------------------------------------------------------------------
// <copyright file="TokenUtils.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Helper methods for clamping requested token values to provider/model hard limits
    /// and logging when clamping occurs.
    /// </summary>
    public static class TokenUtils
    {
        /// <summary>
        /// Clamps a requested token count to the provider/model hard maximum.
        /// Returns the clamped value (or null) and a flag indicating whether a clamp occurred.
        /// </summary>
        /// <param name="requested">The requested maximum token count, or null if unspecified.</param>
        /// <param name="providerName">The name of the AI provider (e.g. "Gemini", "OpenAi").</param>
        /// <param name="modelName">The optional model name used for provider-specific limits.</param>
        /// <param name="logger">An optional logger used to warn when clamping occurs.</param>
        /// <returns>A tuple containing the clamped value (or null) and a flag indicating whether clamping occurred.</returns>
        public static (int? ClampedValue, bool WasClamped) ClampRequestedMaxTokens(int? requested, string providerName, string? modelName, ILogger? logger = null)
        {
            if (!requested.HasValue)
            {
                return (null, false);
            }

            int hardMax = ModelTokenLimits.GetHardMax(providerName ?? string.Empty, modelName);

            if (requested.Value > hardMax)
            {
                logger?.LogWarning("Requested MaxTokens {Requested} for provider {Provider} (model={Model}) exceeds hard max {HardMax}; clamping to {ClampedTo}.", requested.Value, providerName, modelName, hardMax, hardMax);
                return (hardMax, true);
            }

            return (requested, false);
        }
    }
}
