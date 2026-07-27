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

        private static void ApplySandhiToRow(ProcessedSentenceRow row)
        {
            if (row.Breakdown == null || row.Breakdown.Count == 0)
            {
                return;
            }

            // Build a complete syllable stream from all breakdown entries in this sentence
            var syllableStream = new List<SyllableNode>();
            var entryBoundaries = new List<(int Start, int Count)>();

            foreach (var entry in row.Breakdown)
            {
                // Attempt to split the entry-level pinyin into individual syllables.
                // If the pinyin already contains spaces, SplitPinyinSyllables will
                // respect them. If not (e.g. "xǐhuān"), use the number of Chinese
                // characters as a hint so we can split into per-character syllables
                // and preserve tone marks for sandhi processing.
                string[] syllables = SplitPinyinSyllables(entry.Pinyin, entry.Chinese?.Length ?? 0);
                PartOfSpeechTag pos = MapPos(entry.PartOfSpeech);
                bool isParticle = entry.PartOfSpeech == "Particle";
                string rawPos = entry.PartOfSpeech ?? string.Empty;

                int start = syllableStream.Count;

                for (int i = 0; i < syllables.Length; i++)
                {
                    // Use the Chinese character for context (first char for multi-char entries)
                    string source = (entry.Chinese != null && entry.Chinese.Length > i)
                        ? entry.Chinese[i].ToString()
                        : ((entry.Chinese != null && entry.Chinese.Length > 0) ? entry.Chinese[0].ToString() : syllables[i]);

                    var tone = PinyinUtility.GetTone(syllables[i]);
                    var context = new SandhiContext(source, pos, isParticle, rawPos);
                    syllableStream.Add(new SyllableNode(syllables[i], tone, context));
                }

                entryBoundaries.Add((start, syllables.Length));
            }

            // Run the sandhi engine across the full sentence stream
            var transformed = Engine.ProcessSandhiPipelineWithNotes(syllableStream);

            // Let the row update its breakdown entries and reconstruct its sentence-level pinyin
            row.ApplySandhiResults(transformed, entryBoundaries);
        }

        private static string[] SplitPinyinSyllables(string pinyin, int expectedSyllableCount = 0)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
            {
                return [];
            }

            // If the pinyin already contains spaces, use them.
            if (pinyin.Contains(' '))
            {
                return pinyin.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            }

            // If we have an expected syllable count (from the number of Chinese
            // characters) try to split the pinyin accordingly by locating vowel
            // positions (including accented vowels). This handles cases like
            // "xǐhuān" -> ["xǐ","huān"]. If we cannot produce the expected
            // count, fall back to returning the whole pinyin as a single element.
            if (expectedSyllableCount > 1)
            {
                const string vowels = "aeiouüāáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜv";
                var vowelIndices = new System.Collections.Generic.List<int>();
                for (int i = 0; i < pinyin.Length; i++)
                {
                    if (vowels.IndexOf(char.ToLowerInvariant(pinyin[i])) >= 0)
                    {
                        vowelIndices.Add(i);
                    }
                }

                if (vowelIndices.Count >= expectedSyllableCount)
                {
                    var parts = new string[expectedSyllableCount];
                    for (int s = 0; s < expectedSyllableCount; s++)
                    {
                        int start = s == 0 ? 0 : vowelIndices[s - 1] + 1;
                        int end = s == expectedSyllableCount - 1 ? pinyin.Length - 1 : vowelIndices[s];
                        int length = end - start + 1;
                        if (length <= 0)
                        {
                            parts[s] = string.Empty;
                        }
                        else
                        {
                            parts[s] = pinyin.Substring(start, length);
                        }
                    }

                    // Ensure no empty parts; if any are empty, fall back.
                    bool valid = true;
                    foreach (var part in parts)
                    {
                        if (string.IsNullOrWhiteSpace(part))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        return parts;
                    }
                }
            }

            return new[] { pinyin };
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