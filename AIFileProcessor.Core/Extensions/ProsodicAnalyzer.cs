//-----------------------------------------------------------------------
// <copyright file="ProsodicAnalyzer.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Groups a flat syllable stream into prosodic words (PWs) and prosodic
    /// phrases (PPs) following the standard prosodic hierarchy of Mandarin
    /// Chinese as described in Chen (2000), Duanmu (2007), and Kubler &amp;
    /// Wang (2018).
    /// <para>
    /// Prosodic words are formed by grouping content words with their
    /// enclitic function words (particles, measure words, etc.). A prosodic
    /// phrase is a maximal sequence of PWs not interrupted by a boundary
    /// (punctuation). Post-lexical sandhi applies within a PP but not across
    /// PP boundaries.
    /// </para>
    /// </summary>
    public static class ProsodicAnalyzer
    {
        /// <summary>
        /// Groups a flat syllable stream into prosodic words. Each content word
        /// starts a new PW; function words that cliticize to a preceding content
        /// word (particles, measure words) are attached to it. Punctuation
        /// entries form their own single-syllable PWs that act as boundaries.
        /// </summary>
        /// <param name="syllableStream">The flat syllable stream.</param>
        /// <returns>A list of <see cref="ProsodicWord"/> covering all syllables.</returns>
        public static IReadOnlyList<ProsodicWord> GroupProsodicWords(IReadOnlyList<SyllableNode> syllableStream)
        {
            var pws = new List<ProsodicWord>();
            int currentStart = -1;
            int currentCount = 0;

            for (int i = 0; i < syllableStream.Count; i++)
            {
                SyllableNode current = syllableStream[i];

                if (current.context.IsBoundary)
                {
                    // Close pending PW
                    if (currentStart >= 0)
                    {
                        pws.Add(new ProsodicWord(currentStart, currentCount));
                        currentStart = -1;
                        currentCount = 0;
                    }

                    // Boundary forms its own PW
                    pws.Add(new ProsodicWord(i, 1));
                    continue;
                }

                bool isFunctionWord = IsFunctionWord(current.context.pos);

                if (currentStart < 0)
                {
                    // First syllable
                    currentStart = i;
                    currentCount = 1;
                }
                else if (isFunctionWord)
                {
                    // Function word cliticizes to preceding PW
                    currentCount++;
                }
                else
                {
                    // Content word: close pending PW, start new one
                    pws.Add(new ProsodicWord(currentStart, currentCount));
                    currentStart = i;
                    currentCount = 1;
                }
            }

            // Close trailing PW
            if (currentStart >= 0)
            {
                pws.Add(new ProsodicWord(currentStart, currentCount));
            }

            return pws;
        }

        /// <summary>
        /// Groups prosodic words into prosodic phrases. A PP is a maximal
        /// sequence of PWs not interrupted by a boundary PW.
        /// </summary>
        /// <param name="syllableStream">The flat syllable stream.</param>
        /// <param name="pws">The list of prosodic words covering the stream.</param>
        /// <returns>A list of <see cref="ProsodicPhrase"/> covering all PWs.</returns>
        public static IReadOnlyList<ProsodicPhrase> GroupProsodicPhrases(
            IReadOnlyList<SyllableNode> syllableStream,
            IReadOnlyList<ProsodicWord> pws)
        {
            var pps = new List<ProsodicPhrase>();
            int phraseStart = -1;
            int phraseCount = 0;

            for (int w = 0; w < pws.Count; w++)
            {
                // A PW is a boundary if its first syllable is a boundary
                int idx = pws[w].StartIndex;
                bool isBoundary = idx < syllableStream.Count && syllableStream[idx].context.IsBoundary;

                if (isBoundary)
                {
                    // Close pending PP
                    if (phraseStart >= 0)
                    {
                        pps.Add(new ProsodicPhrase(phraseStart, phraseCount));
                        phraseStart = -1;
                        phraseCount = 0;
                    }

                    // Boundary PW forms its own PP
                    pps.Add(new ProsodicPhrase(w, 1));
                    continue;
                }

                if (phraseStart < 0)
                {
                    phraseStart = w;
                    phraseCount = 1;
                }
                else
                {
                    phraseCount++;
                }
            }

            // Close trailing PP
            if (phraseStart >= 0)
            {
                pps.Add(new ProsodicPhrase(phraseStart, phraseCount));
            }

            return pps;
        }

        /// <summary>
        /// Determines whether a POS tag represents a function word that cliticizes
        /// to the preceding content word. Per Chen (2000) and Duanmu (2007),
        /// particles, measure words, pronouns in object position, and certain
        /// monosyllabic function words attach to their host.
        /// </summary>
        /// <param name="pos">The POS tag (already mapped via MapPos).</param>
        /// <returns>True if the syllable is a cliticizing function word.</returns>
        internal static bool IsFunctionWord(PartOfSpeechTag pos)
        {
            return pos is PartOfSpeechTag.Particle
                or PartOfSpeechTag.MeasureWord
                or PartOfSpeechTag.Pronoun
                or PartOfSpeechTag.Preposition
                or PartOfSpeechTag.Conjunction
                or PartOfSpeechTag.Locative;
        }
    }
}