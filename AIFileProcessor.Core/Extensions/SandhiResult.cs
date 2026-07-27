//-----------------------------------------------------------------------
// <copyright file="SandhiResult.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    /// <summary>
    /// Represents the result of a sandhi transformation on a single syllable.
    /// </summary>
    /// <param name="Transformed">The transformed pinyin syllable.</param>
    /// <param name="Note">A human-readable note describing the transformation, or null if no change occurred.</param>
    public record SandhiResult(string Transformed, string? Note);
}
