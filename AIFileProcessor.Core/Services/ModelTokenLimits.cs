//-----------------------------------------------------------------------
// <copyright file="ModelTokenLimits.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    /// <summary>
    /// Provides known token hard-limits for providers / models and sensible fallbacks.
    /// </summary>
    public static class ModelTokenLimits
    {
        /// <summary>
        /// Returns a provider/model specific hard maximum number of tokens to accept.
        /// If the provider or model is unknown, returns a conservative default.
        /// </summary>
        /// <param name="providerName">The name of the AI provider (e.g. "Gemini", "OpenAi").</param>
        /// <param name="modelName">The optional model name used for provider-specific limits.</param>
        /// <returns>The hard maximum token count for the given provider/model, or a conservative default.</returns>
        public static int GetHardMax(string providerName, string? modelName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                return 262144; // conservative fallback
            }

            providerName = providerName.Trim().ToLowerInvariant();

            return providerName switch
            {
                "gemini" => GetGeminiHardMax(modelName),
                "openai" => 262144,
                "deepseek" => 262144,
                _ => 262144,
            };
        }

        private static int GetGeminiHardMax(string? modelName)
        {
            return 262144; // 256k for all Gemini models
        }
    }
}
