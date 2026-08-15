//-----------------------------------------------------------------------
// <copyright file="AiCallResult.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Response
{
    using AIFileProcessor.Telemetry;

    /// <summary>
    /// The result of a single AI provider request: the generated text plus the
    /// telemetry captured for that call. Callers use this to record performance
    /// metrics without provider-specific knowledge.
    /// </summary>
    public class AiCallResult
    {
        /// <summary>Gets or sets the raw text returned by the provider (typically JSON).</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Gets or sets the telemetry captured for the call.</summary>
        public CallTelemetry Telemetry { get; set; } = new();
    }
}
