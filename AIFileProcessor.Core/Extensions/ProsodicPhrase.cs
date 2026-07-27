//-----------------------------------------------------------------------
// <copyright file="ProsodicPhrase.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    /// <summary>
    /// Represents a prosodic phrase (PP) — a maximal sequence of prosodic
    /// words not interrupted by a boundary. Post-lexical sandhi (e.g., 3rd+
    /// 3rd across PW boundaries) applies within a PP but not across PP
    /// boundaries, following the cyclic/stratified application model in
    /// Chen (2000) and Duanmu (2007).
    /// </summary>
    /// <param name="StartWordIndex">The index of the first PW in this phrase within the PW list.</param>
    /// <param name="WordCount">The number of PWs belonging to this phrase.</param>
    public record ProsodicPhrase(int StartWordIndex, int WordCount);
}