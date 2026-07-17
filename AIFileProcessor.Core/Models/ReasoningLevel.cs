//-----------------------------------------------------------------------
// <copyright file="ReasoningLevel.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    /// <summary>
    /// Represents the reasoning effort/thinking level that an AI model can use.
    /// Different models support different subsets of these levels.
    /// </summary>
    public enum ReasoningLevel
    {
        /// <summary>
        /// No reasoning effort. Mapped to "none" for OpenAI/DeepSeek and not applicable for Gemini.
        /// </summary>
        None,

        /// <summary>
        /// Minimal thinking. Mapped to "MINIMAL" for Gemini and not supported by OpenAI/DeepSeek.
        /// </summary>
        Minimal,

        /// <summary>
        /// Low reasoning effort. Mapped to "low" for OpenAI/DeepSeek and "LOW" for Gemini.
        /// </summary>
        Low,

        /// <summary>
        /// Medium reasoning effort. Mapped to "medium" for OpenAI/DeepSeek and "MEDIUM" for Gemini.
        /// </summary>
        Medium,

        /// <summary>
        /// High reasoning effort. Mapped to "high" for OpenAI/DeepSeek and "HIGH" for Gemini.
        /// </summary>
        High,

        /// <summary>
        /// Extra-high reasoning effort. Mapped to "xhigh" for OpenAI/DeepSeek. Not supported by Gemini.
        /// </summary>
        XHigh,

        /// <summary>
        /// Maximum reasoning effort. Mapped to "max" for OpenAI/DeepSeek. Not supported by Gemini.
        /// </summary>
        Max,
    }
}