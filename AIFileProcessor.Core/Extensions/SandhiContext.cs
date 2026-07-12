//-----------------------------------------------------------------------
// <copyright file="SandhiContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Immutable records replace mutable classes.
    /// </summary>
    /// <param name="source">The original text of the syllable or particle.</param>
    /// <param name="pos">The part of speech tag associated with the syllable, used for context-sensitive rules.</param>
    /// <param name="isParticle">Indicates whether the syllable is a particle, which affects certain sandhi rules.</param>
    public record SandhiContext(string source, string pos, bool isParticle)
    {
        // Explicitly define boundary characters.
        // Treating full-width characters as distinct, self-contained elements.
        private static readonly char[] BoundaryChars = ".?!。？！,，;；:：、".ToCharArray();

        public bool IsBoundary => source.IndexOfAny(BoundaryChars) >= 0;

        public bool IsTargetParticle => isParticle && source is "吗" or "呢" or "了" or "的" or "得" or "地";
    }
}
