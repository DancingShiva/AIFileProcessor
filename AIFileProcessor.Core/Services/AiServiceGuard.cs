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
    }
}
