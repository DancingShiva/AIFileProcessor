//-----------------------------------------------------------------------
// <copyright file="SettingsExtensions.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System.Collections.Generic;
    using AIFileProcessor.Configuration;

    /// <summary>
    /// Provides extension methods for overlaying per-user settings overrides
    /// onto default settings instances.
    /// </summary>
    public static class SettingsExtensions
    {
        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>, effectively overlaying user overrides
        /// on top of defaults. ApiKey is never overlaid — it is always sourced
        /// from machine-level IConfiguration.
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this GeminiSettings source, GeminiSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey is always from machine-level IConfiguration; never overlaid from per-user files.

            // Only overwrite when the override has a meaningful (non-default) value.
            // Nullable types: overwrite if the override has a value.
            // Non-nullable types: overwrite if the override differs from the default.
            if (source.Endpoint != null)
            {
                target.Endpoint = source.Endpoint;
            }

            if (source.ModelName != null)
            {
                target.ModelName = source.ModelName;
            }

            if (source.TimeoutSeconds != 0)
            {
                target.TimeoutSeconds = source.TimeoutSeconds;
            }

            if (source.ProjectId != null)
            {
                target.ProjectId = source.ProjectId;
            }

            if (source.Location != null)
            {
                target.Location = source.Location;
            }

            if (source.MaxTokens.HasValue)
            {
                target.MaxTokens = source.MaxTokens;
            }

            if (source.Temperature.HasValue)
            {
                target.Temperature = source.Temperature;
            }

            if (source.TopP.HasValue)
            {
                target.TopP = source.TopP;
            }

            // StreamingEnabled is a bool — only override if true (explicit opt-in)
            if (source.StreamingEnabled)
            {
                target.StreamingEnabled = source.StreamingEnabled;
            }

            if (source.MaxRetries != 0)
            {
                target.MaxRetries = source.MaxRetries;
            }

            if (source.BaseDelay != 0)
            {
                target.BaseDelay = source.BaseDelay;
            }

            if (source.MaxDelay != 0)
            {
                target.MaxDelay = source.MaxDelay;
            }

            if (source.RequestHeaders != null)
            {
                target.RequestHeaders = source.RequestHeaders;
            }

            if (source.AdditionalParameters != null)
            {
                target.AdditionalParameters = source.AdditionalParameters;
            }

            if (source.EnableLogging)
            {
                target.EnableLogging = source.EnableLogging;
            }

            if (source.RateLimitPerMinute.HasValue)
            {
                target.RateLimitPerMinute = source.RateLimitPerMinute;
            }

            if (source.ProxyUrl != null)
            {
                target.ProxyUrl = source.ProxyUrl;
            }

            if (source.UseManagedIdentity)
            {
                target.UseManagedIdentity = source.UseManagedIdentity;
            }

            if (source.CredentialType != null)
            {
                target.CredentialType = source.CredentialType;
            }
        }

        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>. ApiKey is never overlaid — it is always
        /// sourced from machine-level IConfiguration.
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this OpenAiSettings source, OpenAiSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey is always from machine-level IConfiguration; never overlaid from per-user files.
            if (!string.IsNullOrEmpty(source.Endpoint))
            {
                target.Endpoint = source.Endpoint;
            }

            if (!string.IsNullOrEmpty(source.ModelName))
            {
                target.ModelName = source.ModelName;
            }

            if (source.TimeoutSeconds != 0)
            {
                target.TimeoutSeconds = source.TimeoutSeconds;
            }

            if (source.OrganizationId != null)
            {
                target.OrganizationId = source.OrganizationId;
            }

            if (source.MaxTokens.HasValue)
            {
                target.MaxTokens = source.MaxTokens;
            }

            if (source.Temperature.HasValue)
            {
                target.Temperature = source.Temperature;
            }
        }

        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>. ApiKey is never overlaid — it is always
        /// sourced from machine-level IConfiguration.
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this DeepSeekSettings source, DeepSeekSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey is always from machine-level IConfiguration; never overlaid from per-user files.
            if (!string.IsNullOrEmpty(source.Endpoint))
            {
                target.Endpoint = source.Endpoint;
            }

            if (source.TimeoutSeconds != 0)
            {
                target.TimeoutSeconds = source.TimeoutSeconds;
            }

            if (source.MaxTokens.HasValue)
            {
                target.MaxTokens = source.MaxTokens;
            }

            if (source.Temperature.HasValue)
            {
                target.Temperature = source.Temperature;
            }

            if (source.ModelName != null)
            {
                target.ModelName = source.ModelName;
            }

            if (source.EnableReasoning)
            {
                target.EnableReasoning = source.EnableReasoning;
            }
        }
    }
}