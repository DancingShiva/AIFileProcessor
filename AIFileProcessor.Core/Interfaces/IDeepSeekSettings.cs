//-----------------------------------------------------------------------
// <copyright file="IDeepSeekSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    /// <summary>
    /// Interface for DeepSeek configuration settings.
    /// </summary>
    public interface IDeepSeekSettings
    {
        /// <summary>
        /// Gets the API Key for DeepSeek. This is used to authenticate with the DeepSeek service and is required for making API calls.
        /// </summary>
        public string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the endpoint for the DeepSeek service.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the timeout for DeepSeek API calls in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate for each API call.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for DeepSeek API calls.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the model name for DeepSeek API calls.
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reasoning responses are enabled for DeepSeek API calls. When enabled, the system will attempt to provide more detailed and context-aware responses by utilizing reasoning capabilities of the DeepSeek model. This may result in more accurate and insightful answers, especially for complex queries that require understanding of context and relationships between data. However, enabling reasoning may also increase response times and resource usage, so it should be used judiciously based on the application's needs.
        /// </summary>
        public bool EnableReasoning { get; set; }
    }
}