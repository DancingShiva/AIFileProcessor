//-----------------------------------------------------------------------
// <copyright file="IOpenAiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    /// <summary>
    /// Interface for OpenAI configuration settings.
    /// </summary>
    public interface IOpenAiSettings
    {
        /// <summary>
        /// Gets the API Key for Gemini. This is used to authenticate with the Gemini service and is required for making API calls.
        /// </summary>
        public string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the endpoint for the OpenAI service.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the name of the model to use for OpenAI API calls.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the timeout for OpenAI API calls in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the organization ID to use for OpenAI API calls. This is used to specify the organization context for the API calls, which can be important for billing and access control purposes when using OpenAI services. If your OpenAI account is associated with multiple organizations, you can set this property to ensure that API calls are made under the correct organizational context.
        /// </summary>
        public string? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate for each OpenAI API call.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for OpenAI API calls. This controls the randomness of the generated text.
        /// </summary>
        public double? Temperature { get; set; }
    }
}