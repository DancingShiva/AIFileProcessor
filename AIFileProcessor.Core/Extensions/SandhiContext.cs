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
    /// Represents the strong type for Mandarin parts of speech used during sandhi processing.
    /// </summary>
    public enum PartOfSpeechTag
    {
        /// <summary>
        /// Other parts of speech (nouns, verbs, adjectives, etc.) that do not trigger function word sandhi.
        /// </summary>
        Other = 0,

        /// <summary>
        /// Numerals (numbers), which prevent quantitative sandhi.
        /// </summary>
        Numeral,

        /// <summary>
        /// Particles (e.g., structural, aspect, modal particles).
        /// </summary>
        Particle,

        /// <summary>
        /// Punctuation boundaries.
        /// </summary>
        Punctuation,

        /// <summary>
        /// Measure words.
        /// </summary>
        MeasureWord,

        /// <summary>
        /// Pronouns.
        /// </summary>
        Pronoun,

        /// <summary>
        /// Prepositions.
        /// </summary>
        Preposition,

        /// <summary>
        /// Conjunctions.
        /// </summary>
        Conjunction,

        /// <summary>
        /// Locatives.
        /// </summary>
        Locative
    }

    /// <summary>
    /// Immutable records replace mutable classes.
    /// </summary>
    /// <param name="source">The original text of the syllable or particle.</param>
    /// <param name="pos">The part of speech tag associated with the syllable, used for context-sensitive rules.</param>
    /// <param name="isParticle">Indicates whether the syllable is a particle, which affects certain sandhi rules.</param>
    /// <param name="rawPartOfSpeech">The original part-of-speech label from the breakdown entry (e.g., "Measure Word").</param>
    public record SandhiContext(string source, PartOfSpeechTag pos, bool isParticle, string rawPartOfSpeech = "")
    {
        // Explicitly define boundary characters.
        // Treating full-width characters as distinct, self-contained elements.
        private static readonly char[] BoundaryChars = ".?!。？！,，;；:：、".ToCharArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="SandhiContext"/> class.
        /// </summary>
        /// <param name="source">The original text of the syllable or particle.</param>
        /// <param name="pos">The part of speech string associated with the syllable.</param>
        /// <param name="isParticle">Indicates whether the syllable is a particle.</param>
        /// <param name="rawPartOfSpeech">The original raw part-of-speech label.</param>
        public SandhiContext(string source, string pos, bool isParticle, string rawPartOfSpeech = "")
            : this(source, ParsePos(pos), isParticle, rawPartOfSpeech)
        {
        }

        public bool IsBoundary => source.IndexOfAny(BoundaryChars) >= 0;

        public bool IsTargetParticle => isParticle && source is "吗" or "呢" or "了" or "的" or "得" or "地";

        private static PartOfSpeechTag ParsePos(string? pos)
        {
            if (string.IsNullOrEmpty(pos))
            {
                return PartOfSpeechTag.Other;
            }

            return pos.ToLowerInvariant().Trim() switch
            {
                "num" or "numeral" => PartOfSpeechTag.Numeral,
                "particle" or "part" => PartOfSpeechTag.Particle,
                "boundary" or "punctuation" or "punct" => PartOfSpeechTag.Punctuation,
                "mw" or "measure word" => PartOfSpeechTag.MeasureWord,
                "pron" or "pronoun" => PartOfSpeechTag.Pronoun,
                "prep" or "preposition" => PartOfSpeechTag.Preposition,
                "conj" or "conjunction" => PartOfSpeechTag.Conjunction,
                "loc" or "locative" => PartOfSpeechTag.Locative,
                _ => PartOfSpeechTag.Other
            };
        }
    }
}
