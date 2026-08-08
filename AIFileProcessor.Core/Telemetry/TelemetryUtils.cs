//-----------------------------------------------------------------------
// <copyright file="TelemetryUtils.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Telemetry
{
    using System;
    using System.Reflection;

    internal static class TelemetryUtils
    {
        // Try to extract prompt/completion token counts from provider-specific response objects
        // using reflection to avoid compile-time SDK coupling. Returns true if any value was found.
        public static bool TryExtractTokens(object? responseObject, out int? promptTokens, out int? completionTokens)
        {
            promptTokens = null;
            completionTokens = null;

            if (responseObject == null)
            {
                return false;
            }

            // Common property names we try to discover
            var candidateUsageProps = new[] { "Usage", "FinalUsage", "UsageMetadata", "TokenUsage" };

            foreach (var propName in candidateUsageProps)
            {
                var usageProp = responseObject.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (usageProp == null)
                {
                    continue;
                }

                var usageValue = usageProp.GetValue(responseObject);
                if (usageValue == null)
                {
                    continue;
                }

                // Try common token property names on the usage object, including the
                // concrete names used by the OpenAI SDK (InputTokenCount/OutputTokenCount)
                // and Google.GenAI SDK (PromptTokenCount/CandidatesTokenCount).
                var promptProp = usageValue.GetType().GetProperty("PromptTokens")
                    ?? usageValue.GetType().GetProperty("Prompt")
                    ?? usageValue.GetType().GetProperty("InputTokens")
                    ?? usageValue.GetType().GetProperty("InputTokenCount")
                    ?? usageValue.GetType().GetProperty("PromptTokenCount");
                var completionProp = usageValue.GetType().GetProperty("CompletionTokens")
                    ?? usageValue.GetType().GetProperty("Completion")
                    ?? usageValue.GetType().GetProperty("OutputTokens")
                    ?? usageValue.GetType().GetProperty("OutputTokenCount")
                    ?? usageValue.GetType().GetProperty("CandidatesTokenCount");

                if (promptProp != null)
                {
                    try { promptTokens = Convert.ToInt32(promptProp.GetValue(usageValue)); } catch { }
                }

                if (completionProp != null)
                {
                    try { completionTokens = Convert.ToInt32(completionProp.GetValue(usageValue)); } catch { }
                }

                return promptTokens.HasValue || completionTokens.HasValue;
            }

            return false;
        }
    }
}
