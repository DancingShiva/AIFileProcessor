//-----------------------------------------------------------------------
// <copyright file="ResponseVerifier.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Response;

    /// <summary>
    /// Deterministic, always-on post-generation verifier for the headless
    /// sentence-processing spec. Checks the parsed response for format violations
    /// that are mechanically checkable (POS whitelist, HSK range, punctuation
    /// format, segment-count alignment) without calling the AI again. Sandhi-safe:
    /// only rules that hold regardless of tone state are checked, because the
    /// pipeline transforms pinyin client-side after parsing.
    /// </summary>
    public static class ResponseVerifier
    {
        private const int SentenceTokenEstimateMax = 256;

        // Mirrors the PartOfSpeech whitelist the spec defines for the PS element.
        // 'punc' is included because the punctuation rule mandates it for all
        // punctuation segments.
        private static readonly HashSet<string> ValidPartOfSpeech = new(StringComparer.Ordinal)
        {
            "n", "v", "adj", "pron", "num", "mw", "adv", "prep", "conj",
            "part", "int", "loc", "t", "pn", "id", "punc",
        };

        /// <summary>
        /// Verifies the response and returns a list of formatted violation messages,
        /// one per rule breach. An empty list means no format violations were found.
        /// </summary>
        /// <param name="response">The parsed AI response to verify.</param>
        /// <returns>A read-only list of <c>VERIFY:</c>-prefixed violation messages.</returns>
        public static IReadOnlyList<string> Verify(AiApiResponse response)
        {
            var violations = new List<string>();

            if (response?.ProcessedSentenceRows == null)
            {
                return violations;
            }

            for (int rowIndex = 0; rowIndex < response.ProcessedSentenceRows.Count; rowIndex++)
            {
                ProcessedSentenceRow row = response.ProcessedSentenceRows[rowIndex];
                if (row.Breakdown == null || row.Breakdown.Count == 0)
                {
                    continue;
                }

                int lineNumber = rowIndex + 1; // 1-based, matches the Corrections log convention

                VerifySegmentCount(row, lineNumber, violations);

                for (int segIndex = 0; segIndex < row.Breakdown.Count; segIndex++)
                {
                    VerifyEntry(row.Breakdown[segIndex], lineNumber, segIndex, violations);
                }
            }

            return violations;
        }

        /// <summary>
        /// The number of <c>||</c>-delimited segments in B should equal the number of
        /// semantic units (word tokens + grouped punctuation) that C tokenizes to.
        /// C's raw string cannot be whitespace-split naively: per Section 6, the
        /// spacing bypass glues words and punctuation together (e.g., <c>我，你</c> is
        /// a single whitespace unit but three B segments). This tokenizer mirrors the
        /// B segmentation rules — words delimited by spaces, each punctuation run
        /// grouped per Section 7 — so the segment-count comparison is apples-to-apples.
        /// </summary>
        private static int CountSemanticUnits(string chinese)
        {
            int units = 0;
            int i = 0;

            while (i < chinese.Length)
            {
                char c = chinese[i];

                if (c == ' ')
                {
                    i++;
                    continue;
                }

                if (IsPunctuation(c))
                {
                    // Contiguous punctuation run = one B segment (Section 7). Consume
                    // all consecutive punctuation to count it once.
                    while (i < chinese.Length && IsPunctuation(chinese[i]))
                    {
                        i++;
                    }

                    units++;
                }
                else
                {
                    // A word token extends until the next space or punctuation boundary.
                    while (i < chinese.Length && chinese[i] != ' ' && !IsPunctuation(chinese[i]))
                    {
                        i++;
                    }

                    units++;
                }
            }

            return units;
        }

        // The spec Section 5 Punctuation Set, expressed as per-code-point rules.
        // Multi-char marks (……, ——) count as one semantic unit each, so their
        // component code points (…, —) are treated as individual punctuation
        // characters here; grouping of contiguous punctuation is what merges them.
        private static bool IsPunctuation(char c)
        {
            return c is '【' or '】' or '，' or '。' or '！' or '？' or '：' or '；'
                or '”' or '》' or '“' or '《' or '（' or '）' or '、'
                or '-' or '—' or '…';
        }

        private static void VerifySegmentCount(ProcessedSentenceRow row, int lineNumber, List<string> violations)
        {
            if (string.IsNullOrWhiteSpace(row.Chinese))
            {
                return;
            }

            int expected = CountSemanticUnits(row.Chinese);
            int actual = row.Breakdown.Count;

            if (actual != expected && actual > 0 && actual <= SentenceTokenEstimateMax)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment count mismatch — C tokenizes to {expected} semantic unit(s) but B has {actual} segment(s).");
            }
        }

        private static void VerifyEntry(BreakdownEntry entry, int lineNumber, int segIndex, List<string> violations)
        {
            // Deserialization inflates the spec's wire values to display names (e.g.
            // pron → Pronoun, 1 → HSK1). Verification must run against the wire form:
            // prefer the preserved raw value, falling back to the stored value.
            string pos = !string.IsNullOrEmpty(entry.RawPartOfSpeech)
                ? entry.RawPartOfSpeech
                : entry.PartOfSpeech ?? string.Empty;

            string hsk = !string.IsNullOrEmpty(entry.RawHskLevel)
                ? entry.RawHskLevel
                : entry.HskLevel ?? string.Empty;

            // PS whitelist — anything else violates the spec's abbreviation map.
            if (!ValidPartOfSpeech.Contains(pos))
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} ('{entry.Chinese}') has a PS value not in the spec whitelist: '{pos}'.");
            }

            // H must be 1-9 or exactly "N/A".
            bool validHsk = hsk == "N/A"
                || (hsk.Length == 1 && hsk[0] is >= '1' and <= '9');
            if (!validHsk)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} ('{entry.Chinese}') has an H value outside 1-9/N-A: '{hsk}'.");
            }

            // Punctuation rule: when PS is 'punc', the 4th/5th elements are fixed
            // ('punc' itself is already enforced above) — the 6th (Note) must be
            // exactly one ASCII space (spec: "MUST be exactly one ASCII space").
            if (string.Equals(pos, "punc", StringComparison.Ordinal)
                && entry.Note != " ")
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} ('{entry.Chinese}') is marked punc but its Note element is not exactly one ASCII space.");
            }
        }
    }
}
