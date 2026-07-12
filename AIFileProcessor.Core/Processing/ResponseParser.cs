//-----------------------------------------------------------------------
// <copyright file="ResponseParser.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Response;

    /// <summary>
    /// Parses tab-delimited AI response text into <see cref="ProcessedSentenceRow"/> objects.
    /// Extracted from MainForm to enable sharing between UI frontends.
    /// </summary>
    public static class ResponseParser
    {
        private static readonly string[] LineSeparators = ["\r\n", "\r", "\n"];

        /// <summary>
        /// Parses the response text into a list of <see cref="ProcessedSentenceRow"/> objects.
        /// </summary>
        /// <param name="responseText">The raw tab-delimited response text from the AI service.</param>
        /// <returns>A list of parsed sentence rows. Returns an empty list if the input is null or whitespace.</returns>
        public static List<ProcessedSentenceRow> Parse(string responseText)
        {
            var rows = new List<ProcessedSentenceRow>();
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return rows;
            }

            string[] lines = responseText.Split(LineSeparators, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string trimmed = raw.Trim();
                if (IsStatusLine(trimmed))
                {
                    continue;
                }

                rows.Add(ParseResponseLine(raw, i + 1));
            }

            return rows;
        }

        /// <summary>
        /// Determines whether the trimmed line is a status message that should be skipped during parsing.
        /// </summary>
        /// <param name="trimmedLine">The trimmed line to check.</param>
        /// <returns>True if the line is a status message; otherwise, false.</returns>
        public static bool IsStatusLine(string trimmedLine)
        {
            return trimmedLine.Equals("Processing...", StringComparison.OrdinalIgnoreCase)
                || trimmedLine.Equals("Processing in progress.", StringComparison.OrdinalIgnoreCase)
                || trimmedLine.Equals("Processing cancelled.", StringComparison.OrdinalIgnoreCase)
                || trimmedLine.StartsWith("There were no sentences", StringComparison.OrdinalIgnoreCase)
                || trimmedLine.StartsWith("Pipeline Error", StringComparison.OrdinalIgnoreCase);
        }

        private static ProcessedSentenceRow ParseResponseLine(string raw, int lineNumber)
        {
            string[] columns = raw.Split('\t');
            if (columns.Length != 7)
            {
                throw new FormatException(
                    $"Line {lineNumber} has {columns.Length} tab-delimited columns; expected 7.");
            }

            return new ProcessedSentenceRow
            {
                Id = columns[0],
                Chinese = columns[1],
                Pinyin = columns[2],
                Breakdown = ParseBreakdownEntries(columns[3], lineNumber),
                R = columns[4],
                English = columns[5],
                Length = columns[6],
            };
        }

        private static List<BreakdownEntry> ParseBreakdownEntries(string breakdownText, int lineNumber)
        {
            var breakdown = new List<BreakdownEntry>();
            if (string.IsNullOrWhiteSpace(breakdownText))
            {
                return breakdown;
            }

            string[] entries = breakdownText.Split(
                "||",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (string entry in entries)
            {
                string[] parts = entry.Split('|', 6, StringSplitOptions.TrimEntries);
                if (parts.Length != 6)
                {
                    throw new FormatException(
                        $"Line {lineNumber} has a breakdown entry with {parts.Length} pipe-delimited fields; expected 6.");
                }

                breakdown.Add(new BreakdownEntry
                {
                    Chinese = parts[0],
                    Pinyin = parts[1],
                    Gloss = parts[2],
                    PartOfSpeech = parts[3],
                    HskLevel = parts[4],
                    Note = parts[5],
                });
            }

            return breakdown;
        }
    }
}