//-----------------------------------------------------------------------
// <copyright file="PromptInjectionDetectedException.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Exceptions
{
    /// <summary>
    /// Thrown when user-supplied data contains a known prompt-injection pattern.
    /// The data is rejected before it is sent to the AI model. Inherits from
    /// <see cref="System.InvalidOperationException"/> for backward compatibility.
    /// </summary>
    public class PromptInjectionDetectedException : System.InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptInjectionDetectedException"/> class.
        /// </summary>
        /// <param name="pattern">The injection pattern that was detected.</param>
        public PromptInjectionDetectedException(string pattern)
            : base(
                $"Input rejected: potential prompt injection detected (pattern: '{pattern}'). " +
                "The data contains text that matches a known prompt-injection pattern and will not be sent to the AI model.")
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Gets the injection pattern that triggered the rejection.
        /// </summary>
        public string Pattern { get; }
    }
}