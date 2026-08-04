//-----------------------------------------------------------------------
// <copyright file="SandhiProcessor.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System.Collections.Generic;
    using System.Text;
    using AIFileProcessor.Extensions;
    using AIFileProcessor.Response;

    /// <summary>
    /// Applies Mandarin tone sandhi rules to pinyin in an <see cref="AiApiResponse"/>
    /// after JSON deserialization. Transforms pinyin in both the sentence-level
    /// <see cref="ProcessedSentenceRow.Pinyin"/> and the per-word
    /// <see cref="BreakdownEntry.Pinyin"/> fields.
    /// </summary>
    public static class SandhiProcessor
    {
        private static readonly SandhiEngine Engine = new();

        // Tokenize a breakdown entry's pinyin into syllables. The prompt instructs the model
        // to separate intra-word syllables with an apostrophe (e.g. "xiǎo'yǔ", "yī'fu"), and
        // distinct words with spaces, so we split on both. Placeholders like "[CC]" contain no
        // apostrophe or space and survive as a single token.
        private static readonly char[] SyllableDelimiters = ['\'', ' '];

        /// <summary>
        /// Applies sandhi transformations to all pinyin fields in the response, in place.
        /// For each sentence, builds a syllable stream from the breakdown entries,
        /// runs the sandhi engine, and writes the transformed syllables back.
        /// </summary>
        /// <param name="response">The parsed AI response to transform.</param>
        public static void ApplySandhi(AiApiResponse response)
        {
            if (response?.ProcessedSentenceRows == null)
            {
                return;
            }

            foreach (var row in response.ProcessedSentenceRows)
            {
                ApplySandhiToRow(row);
            }
        }

        private static string[] TokenizePinyin(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
            {
                return [pinyin ?? string.Empty];
            }

            return pinyin.Split(SyllableDelimiters, System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        }

        private static void ApplySandhiToRow(ProcessedSentenceRow row)
        {
            if (row.Breakdown == null || row.Breakdown.Count == 0)
            {
                return;
            }

            // Build a complete syllable stream from all breakdown entries in this sentence.
            // The model supplies base (dictionary) tones and apostrophe-delimited syllables, so
            // client-side sandhi always derives the sentence pinyin from the breakdown.
            var syllableStream = new List<SyllableNode>();
            var entryBoundaries = new List<(int Start, int Count)>();

            foreach (var entry in row.Breakdown)
            {
                string pinyin = entry.Pinyin ?? string.Empty;
                string[] syllables = TokenizePinyin(pinyin);
                PartOfSpeechTag pos = MapPos(entry.PartOfSpeech);
                bool isParticle = entry.PartOfSpeech == "Particle";
                string rawPos = entry.PartOfSpeech ?? string.Empty;

                int start = syllableStream.Count;

                for (int s = 0; s < syllables.Length; s++)
                {
                    string source = entry.Chinese ?? syllables[s];
                    var tone = PinyinUtility.GetTone(syllables[s]);
                    var context = new SandhiContext(source, pos, isParticle, rawPos);

                    // Last syllable of the entry's token run ends the word — 3rd-tone sandhi
                    // must not lookahead across this boundary into the next word.
                    bool endsWord = s == syllables.Length - 1;
                    syllableStream.Add(new SyllableNode(syllables[s], tone, context, endsWord));
                }

                entryBoundaries.Add((start, syllables.Length));
            }

            // Run the sandhi engine across the full sentence stream
            var transformed = Engine.ProcessSandhiPipelineWithNotes(syllableStream);

            // Write back into the breakdown and reconstruct the sentence pinyin from the
            // (sandhi-adjusted) breakdown syllables.
            row.ApplySandhiResults(transformed, entryBoundaries);
        }

        private static PartOfSpeechTag MapPos(string? partOfSpeech)
        {
            return partOfSpeech switch
            {
                "Numeral" => PartOfSpeechTag.Numeral,
                "Particle" => PartOfSpeechTag.Particle,
                "Punctuation" => PartOfSpeechTag.Punctuation,
                "Measure Word" => PartOfSpeechTag.MeasureWord,
                "Pronoun" => PartOfSpeechTag.Pronoun,
                "Preposition" => PartOfSpeechTag.Preposition,
                "Conjunction" => PartOfSpeechTag.Conjunction,
                "Locative" => PartOfSpeechTag.Locative,
                _ => PartOfSpeechTag.Other,
            };
        }
    }
}