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
        public static (int? clampedValue, bool wasClamped) ClampRequestedMaxTokens(int? requested, string providerName, string? modelName, ILogger? logger = null)
        {
            if (!requested.HasValue)
            {
                return (null, false);
            }

            int hardMax = ModelTokenLimits.GetHardMax(providerName ?? string.Empty, modelName);

            if (requested.Value > hardMax)
            {
                logger?.LogWarning("Requested MaxTokens {Requested} for provider {Provider} (model={Model}) exceeds hard max {HardMax}; clamping to {HardMax}.", requested.Value, providerName, modelName, hardMax);
                return (hardMax, true);
            }

            return (requested, false);
        }
    }
}
