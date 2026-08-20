//-----------------------------------------------------------------------
// <copyright file="ResponseVerifier.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Exceptions;
    using AIFileProcessor.Response;
    using AIFileProcessor.Services;

    /// <summary>
    /// Deterministic, always-on post-generation verifier for the headless
    /// sentence-processing spec. Checks the parsed response for format violations
    /// that are mechanically checkable (POS whitelist, HSK range, punctuation
    /// format, segment-count alignment) without calling the AI again. Sandhi-safe:
    /// only rules that hold regardless of tone state are checked, because the
    /// pipeline transforms pinyin client-side after parsing. Also detects
    /// prompt-injection aftermath — injection patterns or data-delimiter tags
    /// that leaked into the model's output fields.
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
        /// <param name="maxFieldLength">The maximum allowed length for any single AI-generated string field.</param>
        /// <returns>A read-only list of <c>VERIFY:</c>-prefixed violation messages.</returns>
        public static IReadOnlyList<string> Verify(AiApiResponse response, int maxFieldLength = 32_768)
        {
            var violations = new List<string>();

            if (response?.ProcessedSentenceRows == null)
            {
                return violations;
            }

            for (int rowIndex = 0; rowIndex < response.ProcessedSentenceRows.Count; rowIndex++)
            {
                ProcessedSentenceRow row = response.ProcessedSentenceRows[rowIndex];
                int lineNumber = rowIndex + 1; // 1-based, matches the Corrections log convention

                // Injection-aftermath checks run on every row regardless of
                // whether Breakdown is present — the C and E fields are always populated.
                VerifyFieldContent(row.Chinese, "Chinese", lineNumber, violations);
                VerifyFieldContent(row.English, "English", lineNumber, violations);
                VerifyDataTagLeak(row.Chinese, "Chinese", lineNumber, violations);
                VerifyDataTagLeak(row.English, "English", lineNumber, violations);

                // Structural and size checks (Gaps 2 and 3) run before Breakdown
                // enumeration so that malformed rows are caught even when the
                // Breakdown list is null or empty.
                ValidateStructuralIntegrity(row, rowIndex, violations, maxFieldLength);

                // INVALID line consistency: if P is "N/A", then B and R must also
                // be "N/A" (empty breakdown, no audio reordering entries).
                VerifyInvalidLineConsistency(row, lineNumber, violations);

                if (row.Breakdown == null || row.Breakdown.Count == 0)
                {
                    continue;
                }

                VerifySegmentCount(row, lineNumber, violations);

                for (int segIndex = 0; segIndex < row.Breakdown.Count; segIndex++)
                {
                    VerifyEntry(row.Breakdown[segIndex], lineNumber, segIndex, violations);
                    VerifyFieldContent(row.Breakdown[segIndex].Gloss, "Gloss", lineNumber, segIndex, violations);
                    VerifyDataTagLeak(row.Breakdown[segIndex].Gloss, "Gloss", lineNumber, segIndex, violations);
                }
            }

            // Response-level injection checks (not per-row)
            VerifyFieldContent(response.Scratchpad, "Scratchpad", violations);
            VerifyDataTagLeak(response.Scratchpad, "Scratchpad", violations);

            if (response.Corrections != null)
            {
                for (int i = 0; i < response.Corrections.Count; i++)
                {
                    VerifyFieldContent(response.Corrections[i], $"Correction[{i}]", violations);
                    VerifyDataTagLeak(response.Corrections[i], $"Correction[{i}]", violations);
                }
            }

            return violations;
        }

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if any violations contain
        /// an INJECTION marker, indicating prompt-injection aftermath. When injection
        /// is detected the entire processing run must be aborted — all returned data
        /// is untrusted.
        /// </summary>
        /// <param name="violations">The violation list produced by <see cref="Verify"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when at least one violation message contains <c>INJECTION</c>.
        /// </exception>
        public static void AbortOnInjectionViolations(IReadOnlyList<string> violations)
        {
            ArgumentNullException.ThrowIfNull(violations);

            bool hasInjection = false;
            foreach (string v in violations)
            {
                if (v.Contains("INJECTION", StringComparison.OrdinalIgnoreCase))
                {
                    hasInjection = true;
                    break;
                }
            }

            if (hasInjection)
            {
                throw new InjectionAftermathException(violations);
            }
        }

        /// <summary>
        /// Sanitizes all string fields on the parsed response by stripping detected
        /// injection patterns and data-delimiter tags. Call after any INJECTION
        /// findings to ensure downstream consumers receive clean data.
        /// </summary>
        /// <param name="response">The parsed AI response to sanitize in place.</param>
        public static void SanitizeResponse(AiApiResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            if (response.ProcessedSentenceRows == null)
            {
                return;
            }

            foreach (ProcessedSentenceRow row in response.ProcessedSentenceRows)
            {
                row.Chinese = PromptInjectionGuard.SanitizeField(row.Chinese) ?? string.Empty;
                row.English = PromptInjectionGuard.SanitizeField(row.English) ?? string.Empty;
                row.Pinyin = PromptInjectionGuard.SanitizeField(row.Pinyin) ?? string.Empty;

                if (row.Breakdown != null)
                {
                    foreach (BreakdownEntry entry in row.Breakdown)
                    {
                        entry.Chinese = PromptInjectionGuard.SanitizeField(entry.Chinese) ?? string.Empty;
                        entry.Pinyin = PromptInjectionGuard.SanitizeField(entry.Pinyin) ?? string.Empty;
                        entry.Gloss = PromptInjectionGuard.SanitizeField(entry.Gloss) ?? string.Empty;
                        entry.PartOfSpeech = PromptInjectionGuard.SanitizeField(entry.PartOfSpeech) ?? string.Empty;
                        entry.HskLevel = PromptInjectionGuard.SanitizeField(entry.HskLevel) ?? string.Empty;
                        entry.Note = PromptInjectionGuard.SanitizeField(entry.Note) ?? string.Empty;
                    }
                }
            }

            response.Scratchpad = PromptInjectionGuard.SanitizeField(response.Scratchpad) ?? string.Empty;

            if (response.Corrections != null)
            {
                for (int i = 0; i < response.Corrections.Count; i++)
                {
                    response.Corrections[i] = PromptInjectionGuard.SanitizeField(response.Corrections[i]) ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Structural integrity and size ceiling checks that run before any
        /// semantic rule evaluation. These catch injection attempts that try to
        /// break the tab-delimited format or flood fields with oversized content.
        /// </summary>
        private static void ValidateStructuralIntegrity(ProcessedSentenceRow row, int rowIndex, List<string> violations, int maxFieldLength)
        {
            int lineNumber = rowIndex + 1;

            // Field length ceiling. Applies to C, P, E, L, Id.
            ValidateFieldLength(row.Id, "Id", lineNumber, violations, maxFieldLength);
            ValidateFieldLength(row.Chinese, "Chinese", lineNumber, violations, maxFieldLength);
            ValidateFieldLength(row.Pinyin, "Pinyin", lineNumber, violations, maxFieldLength);
            ValidateFieldLength(row.English, "English", lineNumber, violations, maxFieldLength);
            ValidateFieldLength(row.Length, "Length", lineNumber, violations, maxFieldLength);

            // Tab-delimited structural integrity. The pipe delimiter '|' must
            // never appear inside a tab-delimited column, and tabs/newlines are
            // structural boundaries that must not be present in field content.
            if (row.Chinese.Contains('\t'))
            {
                violations.Add($"VERIFY: line {lineNumber} Chinese contains a structural tab character.");
            }

            if (row.Pinyin.Contains('\t'))
            {
                violations.Add($"VERIFY: line {lineNumber} Pinyin contains a structural tab character.");
            }

            if (row.English.Contains('\t'))
            {
                violations.Add($"VERIFY: line {lineNumber} English contains a structural tab character.");
            }

            if (row.Length.Contains('\t'))
            {
                violations.Add($"VERIFY: line {lineNumber} Length contains a structural tab character.");
            }

            // Breakdown fields: validate '||' delimiters and structural integrity.
            if (row.Breakdown != null)
            {
                for (int i = 0; i < row.Breakdown.Count; i++)
                {
                    BreakdownEntry entry = row.Breakdown[i];
                    int segIndex = i + 1;

                    ValidateFieldLength(entry.Chinese, $"segment {segIndex} Chinese", lineNumber, violations, maxFieldLength);
                    ValidateFieldLength(entry.Pinyin, $"segment {segIndex} Pinyin", lineNumber, violations, maxFieldLength);
                    ValidateFieldLength(entry.Gloss, $"segment {segIndex} Gloss", lineNumber, violations, maxFieldLength);
                    ValidateFieldLength(entry.PartOfSpeech, $"segment {segIndex} PartOfSpeech", lineNumber, violations, maxFieldLength);
                    ValidateFieldLength(entry.HskLevel, $"segment {segIndex} HskLevel", lineNumber, violations, maxFieldLength);
                    ValidateFieldLength(entry.Note, $"segment {segIndex} Note", lineNumber, violations, maxFieldLength);

                    // The Breakdown column is pipe-delimited; raw '||' in a field is a
                    // structural violation (the field should contain single '|' only).
                    if (entry.Gloss.Contains("||", StringComparison.Ordinal))
                    {
                        violations.Add($"VERIFY: line {lineNumber} segment {segIndex} Gloss contains structural '||' delimiter.");
                    }

                    // Tab/newline characters are structural boundaries that belong
                    // to the outer tab-delimited format, not inside breakdown fields.
                    if (entry.Chinese.Contains('\t') || entry.Pinyin.Contains('\t') ||
                        entry.Gloss.Contains('\t') || entry.PartOfSpeech.Contains('\t') ||
                        entry.HskLevel.Contains('\t') || entry.Note.Contains('\t'))
                    {
                        violations.Add($"VERIFY: line {lineNumber} segment {segIndex} contains a structural tab character in a breakdown field.");
                    }
                }
            }
        }

        /// <summary>
        /// Checks that a single field does not exceed the maximum length ceiling.
        /// </summary>
        private static void ValidateFieldLength(string? field, string fieldName, int lineNumber, List<string> violations, int maxFieldLength)
        {
            if (field != null && field.Length > maxFieldLength)
            {
                violations.Add($"VERIFY: line {lineNumber} {fieldName} exceeds maximum length of {maxFieldLength} characters ({field.Length}).");
            }
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
            // empty or a single ASCII space. The spec mandates one space to keep
            // the ||-delimited breakdown parseable, but the space is trimmed after
            // parsing so an empty string is also acceptable.
            if (string.Equals(pos, "punc", StringComparison.Ordinal)
                && !string.IsNullOrEmpty(entry.Note) && entry.Note != " ")
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} ('{entry.Chinese}') is marked punc but its Note element is not empty or a single ASCII space.");
            }
        }

        /// <summary>
        /// Checks a per-row output field for known prompt-injection patterns.
        /// </summary>
        private static void VerifyFieldContent(string? field, string fieldName, int lineNumber, List<string> violations)
        {
            string? pattern = PromptInjectionGuard.FindInjectionPattern(field);
            if (pattern != null)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} INJECTION: {fieldName} field contains injection pattern '{pattern}'.");
            }
        }

        /// <summary>
        /// Checks a per-row output field for data-delimiter tag leaks.
        /// </summary>
        private static void VerifyDataTagLeak(string? field, string fieldName, int lineNumber, List<string> violations)
        {
            string? tag = PromptInjectionGuard.FindDataTagLeak(field);
            if (tag != null)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} INJECTION: {tag} tag leaked into {fieldName} field — model may have confused data with instructions.");
            }
        }

        /// <summary>
        /// Checks a per-segment output field (e.g. Gloss) for known prompt-injection patterns.
        /// </summary>
        private static void VerifyFieldContent(string? field, string fieldName, int lineNumber, int segIndex, List<string> violations)
        {
            string? pattern = PromptInjectionGuard.FindInjectionPattern(field);
            if (pattern != null)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} INJECTION: {fieldName} field contains injection pattern '{pattern}'.");
            }
        }

        /// <summary>
        /// Checks a per-segment output field (e.g. Gloss) for data-delimiter tag leaks.
        /// </summary>
        private static void VerifyDataTagLeak(string? field, string fieldName, int lineNumber, int segIndex, List<string> violations)
        {
            string? tag = PromptInjectionGuard.FindDataTagLeak(field);
            if (tag != null)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} segment {segIndex + 1} INJECTION: {tag} tag leaked into {fieldName} field — model may have confused data with instructions.");
            }
        }

        /// <summary>
        /// Checks that an INVALID line (P == "N/A") has the correct sentinel
        /// pattern: B must be empty (no breakdown entries) and R must have no
        /// audio reordering entries. The spec requires that INVALID lines emit
        /// P/B/R/E all set to "N/A".
        /// </summary>
        private static void VerifyInvalidLineConsistency(ProcessedSentenceRow row, int lineNumber, List<string> violations)
        {
            if (!string.Equals(row.Pinyin, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (row.Breakdown != null && row.Breakdown.Count > 0)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} P is 'N/A' (INVALID line) but B has {row.Breakdown.Count} breakdown entry/entries — expected none.");
            }

            if (row.AudioReordering != null && row.AudioReordering.Count > 0)
            {
                violations.Add(
                    $"VERIFY: line {lineNumber} P is 'N/A' (INVALID line) but R has {row.AudioReordering.Count} audio reordering entry/entries — expected none.");
            }
        }

        /// <summary>
        /// Checks a response-level output field (e.g. Scratchpad, Corrections) for injection patterns.
        /// </summary>
        private static void VerifyFieldContent(string? field, string fieldName, List<string> violations)
        {
            string? pattern = PromptInjectionGuard.FindInjectionPattern(field);
            if (pattern != null)
            {
                violations.Add(
                    $"VERIFY: INJECTION: {fieldName} contains injection pattern '{pattern}'.");
            }
        }

        /// <summary>
        /// Checks a response-level output field for data-delimiter tag leaks.
        /// </summary>
        private static void VerifyDataTagLeak(string? field, string fieldName, List<string> violations)
        {
            string? tag = PromptInjectionGuard.FindDataTagLeak(field);
            if (tag != null)
            {
                violations.Add(
                    $"VERIFY: INJECTION: {tag} tag leaked into {fieldName} — model may have confused data with instructions.");
            }
        }
    }
}
