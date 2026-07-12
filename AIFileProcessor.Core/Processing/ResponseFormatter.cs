//-----------------------------------------------------------------------
// <copyright file="ResponseFormatter.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AIFileProcessor.Response;

    /// <summary>
    /// Formats <see cref="ProcessedSentenceRow"/> objects into HTML-formatted output strings.
    /// Extracted from MainForm to enable sharing between UI frontends.
    /// </summary>
    public static class ResponseFormatter
    {
        /// <summary>
        /// Formats a single <see cref="ProcessedSentenceRow"/> into a tab-delimited string with an HTML table breakdown.
        /// Updates breakdown notes with audio reordering annotations.
        /// </summary>
        /// <param name="row">The sentence row to format.</param>
        /// <returns>A tab-delimited string containing the formatted row.</returns>
        public static string FormatRow(ProcessedSentenceRow row)
        {
            // Update the notes with comment about any audio reordering that has occurred.
            var processedIndices = new HashSet<int>();

            foreach (var audioReorder in row.AudioReordering)
            {
                int[] targetIndices = [audioReorder.StartIndex, audioReorder.EndIndex];
                foreach (var idx in targetIndices)
                {
                    if (idx >= 0 && idx < row.Breakdown.Count && processedIndices.Add(idx))
                    {
                        var note = row.Breakdown[idx].Note;
                        if (string.IsNullOrWhiteSpace(note) || note == "N/A")
                        {
                            row.Breakdown[idx].Note = "(Spoken Reordering)";
                        }
                        else if (!note.Contains("(Spoken Reordering)", StringComparison.OrdinalIgnoreCase))
                        {
                            row.Breakdown[idx].Note = note.Trim() + " (Spoken Reordering)";
                        }
                    }
                }
            }

            // Add the HTML formatting
            StringBuilder sb = new("<table>");

            foreach (var entry in row.Breakdown)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{entry.Chinese}</td><td>&nbsp;&nbsp;</td>");
                sb.Append($"<td>{entry.Pinyin}</td><td>&nbsp;&nbsp;</td>");
                sb.Append($"<td>{entry.Gloss}</td><td>&nbsp;&nbsp;</td>");
                sb.Append($"<td>{entry.PartOfSpeech}</td><td>&nbsp;&nbsp;</td>");
                sb.Append($"<td>{entry.HskLevel}</td><td>&nbsp;&nbsp;</td>");
                sb.Append($"<td>{entry.Note}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            return $"{row.Id}\t{row.Chinese}\t{row.Pinyin}\t{sb.ToString()}\t{row.English}\t{row.Length}";
        }

        /// <summary>
        /// Formats a list of <see cref="ProcessedSentenceRow"/> objects into a newline-delimited string.
        /// </summary>
        /// <param name="rows">The rows to format.</param>
        /// <returns>A newline-delimited string of formatted rows.</returns>
        public static string FormatRows(IEnumerable<ProcessedSentenceRow> rows)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var row in rows)
            {
                if (!first)
                {
                    sb.AppendLine();
                }

                sb.Append(FormatRow(row));
                first = false;
            }

            return sb.ToString();
        }
    }
}