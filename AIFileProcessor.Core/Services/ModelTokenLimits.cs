//-----------------------------------------------------------------------
// <copyright file="ModelTokenLimits.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides known token hard-limits for providers / models and sensible fallbacks.
    /// Limits are configurable via <see cref="Configure"/> at startup from
    /// <c>ProcessingSettings.ModelTokenLimitsMap</c>; when not configured, a
    /// conservative default of 262144 is returned for all models.
    /// </summary>
    public static class ModelTokenLimits
    {
        private static readonly Dictionary<string, int> ConfiguredLimits = new(StringComparer.OrdinalIgnoreCase);
        private static int configuredDefault = 262144;
        private static bool isConfigured;

        /// <summary>
        /// Configures the per-model token limits from the application configuration.
        /// Called once at startup (e.g., from <c>ServiceCollectionExtensions</c>).
        /// </summary>
        /// <param name="modelTokenLimitsMap">A dictionary mapping model IDs to their hard max token count.</param>
        /// <param name="defaultTokenLimit">The fallback when a model is not in the dictionary.</param>
        public static void Configure(Dictionary<string, int> modelTokenLimitsMap, int defaultTokenLimit)
        {
            ConfiguredLimits.Clear();
            configuredDefault = defaultTokenLimit > 0 ? defaultTokenLimit : 262144;
            isConfigured = true;

            if (modelTokenLimitsMap != null)
            {
                foreach (var kvp in modelTokenLimitsMap)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key) && kvp.Value > 0)
                    {
                        ConfiguredLimits[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a provider/model specific hard maximum number of tokens to accept.
        /// If the model has a configured limit, returns it. Otherwise returns the
        /// configured default (or 262144 if not configured).
        /// </summary>
        /// <param name="providerName">The name of the AI provider (e.g. "Gemini", "OpenAi").</param>
        /// <param name="modelName">The optional model name used for provider-specific limits.</param>
        /// <returns>The hard maximum token count for the given provider/model, or a conservative default.</returns>
        public static int GetHardMax(string providerName, string? modelName)
        {
            // If configured, look up by model ID first, then by provider name, then default.
            if (isConfigured)
            {
                if (!string.IsNullOrWhiteSpace(modelName)
                    && ConfiguredLimits.TryGetValue(modelName, out var modelLimit))
                {
                    return modelLimit;
                }

                if (!string.IsNullOrWhiteSpace(providerName)
                    && ConfiguredLimits.TryGetValue(providerName, out var providerLimit))
                {
                    return providerLimit;
                }

                return configuredDefault;
            }

            // Not configured: conservative fallback for backward compatibility.
            return 262144;
        }
    }
}