//-----------------------------------------------------------------------
// <copyright file="SandhiEngine.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Engine responsible for processing sandhi rules on a stream of syllables.
    /// Rules are applied in a single flat pass over the sequence: 3rd+3rd, bù,
    /// yī, and particle neutralization.
    /// </summary>
    public class SandhiEngine
    {
        /// <summary>
        /// Processes a list of SyllableNodes according to Mandarin sandhi rules, returning a new list of strings with the appropriate transformations applied.
        /// </summary>
        /// <param name="stream">The list of SyllableNodes to process.</param>
        /// <returns>A new list of strings with the sandhi rules applied.</returns>
        public IReadOnlyList<string> ProcessSandhiPipeline(IReadOnlyList<SyllableNode> stream)
        {
            var results = ProcessSandhiPipelineWithNotes(stream);
            var processedStream = new string[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                processedStream[i] = results[i].Transformed;
            }

            return processedStream;
        }

        /// <summary>
        /// Processes a list of SyllableNodes according to Mandarin sandhi rules
        /// in a single pass over the sequence, returning a list of
        /// <see cref="SandhiResult"/> with transformed syllables and
        /// human-readable notes describing each transformation.
        /// </summary>
        /// <param name="stream">The list of SyllableNodes to process.</param>
        /// <returns>A list of sandhi results with transformation notes.</returns>
        public IReadOnlyList<SandhiResult> ProcessSandhiPipelineWithNotes(IReadOnlyList<SyllableNode> stream)
        {
            if (stream.Count == 0)
            {
                return [];
            }

            // Establish the session context to address Data Clumps
            var session = new SandhiSession(stream);

            // Flat sequential pass: apply sandhi rules to each syllable in order.
            for (int i = 0; i < stream.Count; i++)
            {
                ApplyRulesToSyllable(session, i, stream.Count);
            }

            // Build final results
            var results = new SandhiResult[stream.Count];
            for (int i = 0; i < stream.Count; i++)
            {
                results[i] = new SandhiResult(session.WorkingText[i], session.Notes[i]);
            }

            return results;
        }

        /// <summary>
        /// Applies all sandhi rules to a single syllable, looking only at
        /// the next non-boundary syllable within the given limit.
        /// </summary>
        private static void ApplyRulesToSyllable(
            SandhiSession session,
            int i,
            int limit)
        {
            var current = session.Stream[i];

            // Determine next tone up to the processing boundary
            var nextTone = Tone.Unknown;
            if (i < limit - 1 && i < session.Stream.Count - 1 && !session.Stream[i + 1].context.IsBoundary)
            {
                nextTone = session.WorkingTone[i + 1];
            }

            string original = session.WorkingText[i];

            bool hasTransform = EvaluateRules(
                original,
                session.WorkingTone[i],
                nextTone,
                current.context,
                out string transformed,
                out string? note);

            if (hasTransform && transformed != original)
            {
                session.WorkingText[i] = transformed;
                session.WorkingTone[i] = PinyinUtility.GetTone(transformed);
            }

            if (note != null)
            {
                session.Notes[i] = AppendNote(session.Notes[i], note);
            }
        }

        /// <summary>
        /// Evaluates all sandhi rules for a single syllable, resolving the Code Duplication smell.
        /// </summary>
        private static bool EvaluateRules(
            string text,
            Tone tone,
            Tone nextTone,
            SandhiContext context,
            out string transformed,
            out string? note)
        {
            transformed = text;
            note = null;

            // RULE D: Neutral Particles
            if (context.IsTargetParticle)
            {
                transformed = PinyinUtility.StripTone(text);
                if (transformed != text)
                {
                    note = $"({text} → {transformed} neutral particle)";
                }

                return true;
            }

            // RULE B: bù before 4th tone
            if (text == "bù" && nextTone is Tone.Fourth)
            {
                transformed = "bú";
                note = $"({text} → {transformed} before 4th tone)";
                return true;
            }

            // RULE C: yī quantitative shifts (non-numeral)
            if (text == "yī" && context.pos != PartOfSpeechTag.Numeral)
            {
                if (nextTone is Tone.Fourth)
                {
                    transformed = "yí";
                    note = $"({text} → {transformed} before 4th tone)";
                    return true;
                }

                if (nextTone is Tone.First or Tone.Second or Tone.Third)
                {
                    transformed = "yì";
                    note = $"({text} → {transformed} before {ToneName(nextTone)})";
                    return true;
                }
            }

            // RULE A: 3rd + 3rd Sandhi
            if (tone is Tone.Third && nextTone is Tone.Third)
            {
                transformed = PinyinUtility.ShiftTone(text, Tone.Second);
                note = $"({text} → {transformed} before 3rd tone)";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Appends a note to an existing note (or replaces null).
        /// </summary>
        private static string? AppendNote(string? existing, string? note)
        {
            if (string.IsNullOrEmpty(note))
            {
                return existing;
            }

            return string.IsNullOrEmpty(existing) ? note : $"{existing} {note}";
        }

        private static string ToneName(Tone tone) => tone switch
        {
            Tone.First => "1st tone",
            Tone.Second => "2nd tone",
            Tone.Third => "3rd tone",
            Tone.Fourth => "4th tone",
            Tone.Neutral => "neutral tone",
            _ => "unknown tone",
        };

        /// <summary>
        /// Captures the processing state and parallel arrays for a single sandhi run, resolving the Data Clumps smell.
        /// </summary>
        private class SandhiSession
        {
            public SandhiSession(IReadOnlyList<SyllableNode> stream)
            {
                Stream = stream;
                WorkingText = new string[stream.Count];
                WorkingTone = new Tone[stream.Count];
                Notes = new string?[stream.Count];

                for (int i = 0; i < stream.Count; i++)
                {
                    WorkingText[i] = stream[i].text;
                    WorkingTone[i] = stream[i].tone;
                }
            }

            public IReadOnlyList<SyllableNode> Stream { get; }

            public string[] WorkingText { get; }

            public Tone[] WorkingTone { get; }

            public string?[] Notes { get; }
        }
    }
}
