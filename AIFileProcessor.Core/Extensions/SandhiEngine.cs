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
    /// </summary>
    public class SandhiEngine
    {
        /// <summary>
        /// Processes a list of SyllableNodes according to Mandarin sandhi rules, returning a new list of strings with the appropriate transformations applied. The method handles various sandhi scenarios, including tone changes for consecutive third tones, neutralization of particles, and context-sensitive adjustments for "bù" and "yī". The implementation uses C# 8+ switch expressions with relational and logical patterns for clear and concise rule application.
        /// </summary>
        /// <param name="stream">The list of SyllableNodes to process.</param>
        /// <returns>A new list of strings with the sandhi rules applied.</returns>
        public IReadOnlyList<string> ProcessSandhiPipeline(IReadOnlyList<SyllableNode> stream)
        {
            var processedStream = new string[stream.Count];

            for (int i = 0; i < stream.Count; i++)
            {
                var current = stream[i];

                // STRICT BLOCK: If the next node is a boundary, treat nextTone as Unknown
                var nextTone = Tone.Unknown;
                if (i < stream.Count - 1 && !stream[i + 1].context.IsBoundary)
                {
                    nextTone = stream[i + 1].tone;
                }

                // C# 8+ Switch Expression with Relational and Logical Patterns
                processedStream[i] = current switch
                {
                    // RULE D: Neutral Particles
                    { context.IsTargetParticle: true }
                        => PinyinUtility.StripTone(current.text),

                    // RULE B: bù before 4th tone
                    { text: "bù" } when nextTone is Tone.Fourth
                        => "bú",

                    // RULE C: yī quantitative shifts
                    { text: "yī", context.pos: not "num" } when nextTone is Tone.Fourth
                        => "yí",
                    { text: "yī", context.pos: not "num" } when nextTone is Tone.First or Tone.Second or Tone.Third
                        => "yì",

                    // RULE A: 3rd + 3rd Sandhi
                    { tone: Tone.Third } when nextTone is Tone.Third
                        => PinyinUtility.ShiftTone(current.text, Tone.Second),

                    // Default: keep original
                    _ => current.text
                };
            }

            return processedStream;
        }
    }
}
