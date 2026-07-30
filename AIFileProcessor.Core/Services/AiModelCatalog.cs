//-----------------------------------------------------------------------
// <copyright file="AiModelCatalog.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;
    using System.Collections.Generic;
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

            string normalized = modelNameOrId.Trim().ToLowerInvariant();
            foreach (var model in models)
            {
                if (model.ModelId.Trim().ToLowerInvariant() == normalized
                    || model.ModelName.Trim().ToLowerInvariant() == normalized)
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
    }
}
