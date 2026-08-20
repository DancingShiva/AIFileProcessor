//-----------------------------------------------------------------------
// <copyright file="ApiKeyNotConfiguredException.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Exceptions
{
    /// <summary>
    /// Thrown when an AI provider API key is not configured. The call never
    /// reaches the provider — the request is rejected before any network
    /// activity. Inherits from <see cref="System.InvalidOperationException"/>
    /// for backward compatibility with existing catch blocks.
    /// </summary>
    public class ApiKeyNotConfiguredException : System.InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyNotConfiguredException"/> class.
        /// </summary>
        /// <param name="providerName">The name of the provider whose API key is missing.</param>
        public ApiKeyNotConfiguredException(string providerName)
            : base($"{providerName} API key is not configured.")
        {
            ProviderName = providerName;
        }

        /// <summary>
        /// Gets the name of the provider whose API key is missing
        /// (e.g., "DeepSeek", "Gemini", "OpenAI").
        /// </summary>
        public string ProviderName { get; }
    }
}