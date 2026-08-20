//-----------------------------------------------------------------------
// <copyright file="InjectionAftermathException.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Exceptions
{
    using System.Collections.Generic;

    /// <summary>
    /// Thrown when post-generation verification detects injection aftermath in
    /// the AI's response — the response contains data-delimiter tags or injection
    /// patterns that indicate the model confused data boundaries with instructions.
    /// Inherits from <see cref="System.InvalidOperationException"/> for backward
    /// compatibility with existing catch blocks.
    /// </summary>
    public class InjectionAftermathException : System.InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionAftermathException"/> class.
        /// </summary>
        /// <param name="violations">The violation list produced by <see cref="Processing.ResponseVerifier.Verify"/>.</param>
        public InjectionAftermathException(IReadOnlyList<string> violations)
            : base(
                "Processing aborted: prompt-injection aftermath detected in AI response. " +
                "One or more output fields contained injection patterns or data-delimiter tags.")
        {
            Violations = violations;
        }

        /// <summary>
        /// Gets the list of violation messages that triggered the abort, including
        /// INJECTION findings and any other violations found. Retained for
        /// diagnostic value in crash dumps and logs.
        /// </summary>
        public IReadOnlyList<string> Violations { get; }
    }
}