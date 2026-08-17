//-----------------------------------------------------------------------
// <copyright file="IOpenAiSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Collections.Generic;
    using AIFileProcessor.Models;

    /// <summary>
    /// Interface for OpenAI configuration settings.
    /// </summary>
    public interface IOpenAiSettings : IAiServiceSettings
    {
        /// <summary>
        /// Gets the API Key for OpenAI. This is used to authenticate with the OpenAI service and is required for making API calls.
        /// </summary>
        string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the endpoint for the OpenAI service.
        /// </summary>
        string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the name of the model to use for OpenAI API calls.
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the timeout for OpenAI API calls in seconds.
        /// </summary>
        int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the organization ID to use for OpenAI API calls. This is used to specify the organization context for the API calls, which can be important for billing and access control purposes when using OpenAI services. If your OpenAI account is associated with multiple organizations, you can set this property to ensure that API calls are made under the correct organizational context.
        /// </summary>
        string? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate for each OpenAI API call.
        /// </summary>
        int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for OpenAI API calls. This controls the randomness of the generated text.
        /// </summary>
        double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries after the initial attempt for transient failures.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the base delay, in seconds, before the first retry; doubled each subsequent retry.
        /// </summary>
        int BaseDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum delay, in seconds, between retries.
        /// </summary>
        int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for OpenAI API calls. Null if not configured.
        /// </summary>
        ReasoningLevel? ReasoningLevel { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary request headers to include when calling the provider.
        /// </summary>
        IDictionary<string, string?>? RequestHeaders { get; set; }
    }
}