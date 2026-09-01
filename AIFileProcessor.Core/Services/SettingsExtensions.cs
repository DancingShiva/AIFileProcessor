//-----------------------------------------------------------------------
// <copyright file="SettingsExtensions.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System.Collections.Generic;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Models;

    /// <summary>
    /// Provides extension methods for overlaying per-user settings overrides
    /// onto default settings instances. Infrastructure settings (Endpoint,
    /// ProxyUrl, RequestHeaders, AdditionalParameters) and ApiKey are never
    /// overlaid: they carry the machine-level trust boundary of the API key
    /// (SEC-010, ADR-0006).
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

            // ApiKey, Endpoint, ProxyUrl, RequestHeaders, and AdditionalParameters are
            // always from machine-level IConfiguration; never overlaid (SEC-010). The
            // endpoint travels with the API key's trust boundary, so a user who does
            // not own the machine key cannot redirect key-authenticated traffic.

            // Only overwrite when the override has a meaningful (non-default) value.
            // Nullable types: overwrite if the override has a value.
            // Non-nullable types: overwrite if the override differs from the default.
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

            if (source.EnableLogging)
            {
                target.EnableLogging = source.EnableLogging;
            }

            if (source.RateLimitPerMinute.HasValue)
            {
                target.RateLimitPerMinute = source.RateLimitPerMinute;
            }

            if (source.UseManagedIdentity)
            {
                target.UseManagedIdentity = source.UseManagedIdentity;
            }

            if (source.CredentialType != null)
            {
                target.CredentialType = source.CredentialType;
            }

            if (source.ReasoningLevel.HasValue)
            {
                target.ReasoningLevel = source.ReasoningLevel;
            }
        }

        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>, effectively overlaying user overrides
        /// on top of defaults. ApiKey and infrastructure settings (Endpoint,
        /// RequestHeaders) are never overlaid — they are always sourced from
        /// machine-level IConfiguration, matching the API key's trust boundary (SEC-010).
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this OpenAiSettings source, OpenAiSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey, Endpoint, and RequestHeaders are always from machine-level
            // IConfiguration; never overlaid (SEC-010). The endpoint travels with the
            // API key's trust boundary, so a user who does not own the machine key
            // cannot redirect key-authenticated traffic.
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

            if (source.Temperature.HasValue)
            {
                target.Temperature = source.Temperature;
            }

            if (source.ReasoningLevel.HasValue)
            {
                target.ReasoningLevel = source.ReasoningLevel;
            }
        }

        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>, effectively overlaying user overrides
        /// on top of defaults. ApiKey and infrastructure settings (Endpoint,
        /// RequestHeaders) are never overlaid — they are always sourced from
        /// machine-level IConfiguration, matching the API key's trust boundary (SEC-010).
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this DeepSeekSettings source, DeepSeekSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey, Endpoint, and RequestHeaders are always from machine-level
            // IConfiguration; never overlaid (SEC-010). The endpoint travels with the
            // API key's trust boundary, so a user who does not own the machine key
            // cannot redirect key-authenticated traffic.
            if (source.TimeoutSeconds != 0)
            {
                target.TimeoutSeconds = source.TimeoutSeconds;
            }

            if (source.MaxTokens.HasValue)
            {
                target.MaxTokens = source.MaxTokens;
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

            if (source.ReasoningLevel.HasValue)
            {
                target.ReasoningLevel = source.ReasoningLevel;
            }
        }

        /// <summary>
        /// Copies non-null/non-default property values from <paramref name="source"/>
        /// onto <paramref name="target"/>, effectively overlaying user overrides
        /// on top of defaults. ApiKey and infrastructure settings (Endpoint,
        /// RequestHeaders) are never overlaid — they are always sourced from
        /// machine-level IConfiguration, matching the API key's trust boundary (SEC-010).
        /// </summary>
        /// <param name="source">The override settings (from per-user file).</param>
        /// <param name="target">The target settings (from defaults) to update in place.</param>
        public static void CopyOnto(this OllamaSettings source, OllamaSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            // ApiKey, Endpoint, and RequestHeaders are always from machine-level
            // IConfiguration; never overlaid (SEC-010). The endpoint travels with the
            // API key's trust boundary, so a user who does not own the machine key
            // cannot redirect key-authenticated traffic.
            if (!string.IsNullOrEmpty(source.ModelName))
            {
                target.ModelName = source.ModelName;
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

            if (source.TopP.HasValue)
            {
                target.TopP = source.TopP;
            }

            if (source.TopK.HasValue)
            {
                target.TopK = source.TopK;
            }

            if (source.ContextWindow.HasValue)
            {
                target.ContextWindow = source.ContextWindow;
            }

            if (source.RepeatPenalty.HasValue)
            {
                target.RepeatPenalty = source.RepeatPenalty;
            }

            if (source.Seed.HasValue)
            {
                target.Seed = source.Seed;
            }

            if (source.Stop != null)
            {
                target.Stop = source.Stop;
            }

            if (source.MinP.HasValue)
            {
                target.MinP = source.MinP;
            }

            if (source.TypicalP.HasValue)
            {
                target.TypicalP = source.TypicalP;
            }

            if (source.TfsZ.HasValue)
            {
                target.TfsZ = source.TfsZ;
            }

            if (source.MiroStat.HasValue)
            {
                target.MiroStat = source.MiroStat;
            }

            if (source.MiroStatTau.HasValue)
            {
                target.MiroStatTau = source.MiroStatTau;
            }

            if (source.MiroStatEta.HasValue)
            {
                target.MiroStatEta = source.MiroStatEta;
            }

            if (source.PresencePenalty.HasValue)
            {
                target.PresencePenalty = source.PresencePenalty;
            }

            if (source.FrequencyPenalty.HasValue)
            {
                target.FrequencyPenalty = source.FrequencyPenalty;
            }

            if (source.RepeatLastN.HasValue)
            {
                target.RepeatLastN = source.RepeatLastN;
            }

            if (source.NumGpu.HasValue)
            {
                target.NumGpu = source.NumGpu;
            }

            if (source.NumBatch.HasValue)
            {
                target.NumBatch = source.NumBatch;
            }

            if (source.NumThread.HasValue)
            {
                target.NumThread = source.NumThread;
            }

            if (source.NumKeep.HasValue)
            {
                target.NumKeep = source.NumKeep;
            }

            if (source.Numa.HasValue)
            {
                target.Numa = source.Numa;
            }

            if (source.UseMlock.HasValue)
            {
                target.UseMlock = source.UseMlock;
            }

            if (source.UseMmap.HasValue)
            {
                target.UseMmap = source.UseMmap;
            }

            if (source.LowVram.HasValue)
            {
                target.LowVram = source.LowVram;
            }

            if (!string.IsNullOrWhiteSpace(source.KeepAlive))
            {
                target.KeepAlive = source.KeepAlive;
            }

            if (source.EnableThinking.HasValue)
            {
                target.EnableThinking = source.EnableThinking;
            }

            if (source.ReasoningLevel.HasValue)
            {
                target.ReasoningLevel = source.ReasoningLevel;
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
        }
    }
}