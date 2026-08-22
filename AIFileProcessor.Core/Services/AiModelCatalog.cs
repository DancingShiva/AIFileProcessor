//-----------------------------------------------------------------------
// <copyright file="AiModelCatalog.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;

    /// <summary>
    /// Shared, stateless lookup logic over a provider's supported-model list.
    /// Each AI service keeps its own static <c>SupportedModels</c> list and
    /// delegates the (identical) lookup behavior here, rather than duplicating
    /// it. Pure and internal — consistent with <see cref="TokenUtils"/> and
    /// <see cref="ModelTokenLimits"/>.
    /// </summary>
    internal static class AiModelCatalog
    {
        /// <summary>
        /// The set of recognized provider key strings. When a new provider is
        /// added, add its key here and to the <see cref="GetProviderKey"/> switch.
        /// </summary>
        internal static readonly HashSet<string> KnownProviderKeys =
            ["Gemini", "OpenAI", "DeepSeek", "Ollama"];

        /// <summary>
        /// Determines whether any model in <paramref name="models"/> matches the
        /// given name or ID. The query and both <see cref="AiModelInfo.ModelId"/>
        /// and <see cref="AiModelInfo.ModelName"/> are normalized via
        /// <c>Trim().ToLowerInvariant()</c> before comparison.
        /// </summary>
        /// <param name="models">The provider's supported-model list.</param>
        /// <param name="modelNameOrId">The model name or ID to look up.</param>
        /// <returns>True if a matching model is found; otherwise, false.</returns>
        public static bool SupportsModel(IReadOnlyList<AiModelInfo> models, string? modelNameOrId)
        {
            ArgumentNullException.ThrowIfNull(models);

            if (string.IsNullOrWhiteSpace(modelNameOrId))
            {
                return false;
            }

            string normalized = modelNameOrId.Trim();
            foreach (var model in models)
            {
                if (string.Equals(model.ModelId.Trim(), normalized, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(model.ModelName.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the supported reasoning levels for the specified model ID.
        /// Model IDs are compared case-insensitively (<see cref="StringComparison.OrdinalIgnoreCase"/>).
        /// </summary>
        /// <param name="models">The provider's supported-model list.</param>
        /// <param name="modelId">The model identifier to look up.</param>
        /// <returns>A read-only list of supported reasoning levels, or an empty list if unknown.</returns>
        public static IReadOnlyList<ReasoningLevel> GetReasoningLevels(IReadOnlyList<AiModelInfo> models, string? modelId)
        {
            ArgumentNullException.ThrowIfNull(models);

            if (string.IsNullOrWhiteSpace(modelId))
            {
                return [];
            }

            foreach (var model in models)
            {
                if (model.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase))
                {
                    return model.SupportedReasoningLevels;
                }
            }

            return [];
        }

        /// <summary>
        /// Maps an <see cref="IAiService"/> instance to its provider key
        /// (e.g., "Gemini", "OpenAI", "DeepSeek") for looking up per-user
        /// settings in the provider settings dictionary. Returns <c>null</c>
        /// for unknown service types (e.g., test mocks).
        /// </summary>
        /// <param name="service">The AI service to identify.</param>
        /// <returns>The provider key string, or <c>null</c> if unrecognized.</returns>
        public static string? GetProviderKey(IAiService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            return service.GetType().Name switch
            {
                "GeminiService" => "Gemini",
                "OpenAiService" => "OpenAI",
                "DeepSeekService" => "DeepSeek",
                "OllamaService" => "Ollama",
                _ => null,
            };
        }
    }
}
