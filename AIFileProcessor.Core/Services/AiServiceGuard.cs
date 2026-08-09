//-----------------------------------------------------------------------
// <copyright file="AiServiceGuard.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System;

    /// <summary>
    /// Shared argument validation for the <see cref="Interfaces.IAiService"/> implementations.
    /// The three services had identical <c>IsNullOrWhiteSpace</c> request guards and an identical
    /// "cast <c>object settings</c> to the concrete settings type or throw" pattern; these helpers
    /// remove that duplication. Internal/static, consistent with <see cref="AiModelCatalog"/> and
    /// <see cref="TransientRetryPolicy"/>.
    /// </summary>
    internal static class AiServiceGuard
    {
        /// <summary>
        /// Ensures the request arguments are non-null/non-whitespace.
        /// </summary>
        /// <param name="modelId">The model to use for processing.</param>
        /// <param name="prompt">The user prompt to process.</param>
        /// <param name="data">The string data for the AI model.</param>
        public static void RequireValidRequest(string modelId, string prompt, string data)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("Model ID is required.", nameof(modelId));
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Data is required.", nameof(data));
            }

            PromptInjectionGuard.RequireSafeData(data);
        }

        /// <summary>
        /// Casts the per-call <c>object settings</c> to the concrete <typeparamref name="TSettings"/>
        /// type, throwing a descriptive <see cref="ArgumentException"/> when the type is wrong.
        /// </summary>
        /// <typeparam name="TSettings">The expected concrete settings type.</typeparam>
        /// <param name="settings">The per-call settings object.</param>
        /// <returns>The settings cast to <typeparamref name="TSettings"/>.</returns>
        public static TSettings RequireSettings<TSettings>(object? settings)
            where TSettings : class
        {
            if (settings is not TSettings typed)
            {
                throw new ArgumentException(
                    $"Expected {typeof(TSettings).Name} but received {settings?.GetType().Name ?? "null"}.",
                    nameof(settings));
            }

            return typed;
        }
    }
}
