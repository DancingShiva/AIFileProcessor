//-----------------------------------------------------------------------
// <copyright file="ProsodicWord.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    /// <summary>
    /// Represents a prosodic word (PW) — the minimal domain of obligatory
    /// tone sandhi — as a contiguous range of syllable indices into the
    /// syllable stream. PWs are formed by grouping content words with their
    /// enclitic function words (particles, measure words) per the standard
    /// prosodic hierarchy described in Chen (2000), Duanmu (2007), and
    /// Kubler &amp; Wang (2018).
    /// </summary>
    /// <param name="StartIndex">The index of the first syllable in this PW within the syllable stream.</param>
    /// <param name="SyllableCount">The number of syllables belonging to this PW.</param>
    public record ProsodicWord(int StartIndex, int SyllableCount);
}