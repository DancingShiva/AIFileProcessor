namespace AIFileProcessor.Services
{
    using System;

    /// <summary>
    /// Provides known token hard-limits for providers / models and sensible fallbacks.
    /// </summary>
    public static class ModelTokenLimits
    {
        /// <summary>
        /// Returns a provider/model specific hard maximum number of tokens to accept.
        /// If the provider or model is unknown, returns a conservative default.
        /// </summary>
        public static int GetHardMax(string providerName, string? modelName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                return 8192; // conservative fallback
            }

            providerName = providerName.Trim().ToLowerInvariant();

            return providerName switch
            {
                "gemini" => GetGeminiHardMax(modelName),
                "openai" => 8192,
                "deepseek" => 8192,
                _ => 8192,
            };
        }

        private static int GetGeminiHardMax(string? modelName)
        {
            // Heuristics: modern Gemini variants often support very large contexts but
            // we choose a safe default unless a specific model is known to support more.
            if (!string.IsNullOrEmpty(modelName))
            {
                modelName = modelName.ToLowerInvariant();
                if (modelName.Contains("pro") || modelName.Contains("-32k") || modelName.Contains("32k"))
                {
                    return 32768;
                }
                if (modelName.Contains("-128k") || modelName.Contains("128k"))
                {
                    return 131072; // allow larger when explicitly requested
                }
            }

            return 32768; // sensible default for Gemini
        }
    }
}
