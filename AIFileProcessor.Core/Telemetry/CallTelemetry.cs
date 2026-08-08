//-----------------------------------------------------------------------
// <copyright file="CallTelemetry.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Telemetry
{
    using System;

    /// <summary>
    /// Telemetry captured for a single AI API call (one provider request, including
    /// any internal retries the provider performs before succeeding or failing).
    /// Nullable fields are unset (null) when the provider does not surface the value
    /// (e.g., time-to-first-byte for non-streaming calls, token counts when the
    /// provider omits usage metadata).
    /// </summary>
    public class CallTelemetry
    {
        /// <summary>Gets or sets the UTC timestamp immediately before the call was issued.</summary>
        public DateTime StartUtc { get; set; }

        /// <summary>Gets or sets the UTC timestamp when the call completed (success or final failure).</summary>
        public DateTime EndUtc { get; set; }

        /// <summary>Gets or sets the total wall-clock duration of the call in milliseconds.</summary>
        public long ElapsedMs { get; set; }

        /// <summary>Gets or sets the time to first byte in milliseconds. Null when the provider does not stream.</summary>
        public long? TimeToFirstByteMs { get; set; }

        /// <summary>Gets or sets the prompt (input) token count, or null if the provider does not report usage.</summary>
        public int? PromptTokens { get; set; }

        /// <summary>Gets or sets the completion (output) token count, or null if the provider does not report usage.</summary>
        public int? CompletionTokens { get; set; }

        /// <summary>Gets or sets the number of attempts made (1 plus any retries).</summary>
        public int Attempts { get; set; } = 1;

        /// <summary>Gets or sets the outcome status (e.g., "Success" or "Failed").</summary>
        public string Status { get; set; } = "Success";
    }
}
