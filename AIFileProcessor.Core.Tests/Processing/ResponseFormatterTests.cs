//-----------------------------------------------------------------------
// <copyright file="ResponseFormatterTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Processing
{
    using System.Collections.Generic;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Response;
    using Xunit;

    public class ResponseFormatterTests
    {
        private static ProcessedSentenceRow CreateRow(
            string id = "ID001",
            string chinese = "我 好",
            string pinyin = "wǒ hǎo",
            string english = "I good",
            string length = "2",
            List<BreakdownEntry>? breakdown = null,
            List<AudioReorder>? audioReordering = null)
        {
            breakdown ??=
            [
                new BreakdownEntry { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = string.Empty },
                new BreakdownEntry { Chinese = "好", Pinyin = "hǎo", Gloss = "good", PartOfSpeech = "adj", HskLevel = "1", Note = string.Empty },
            ];

            return new ProcessedSentenceRow
            {
                Id = id,
                Chinese = chinese,
                Pinyin = pinyin,
                English = english,
                Length = length,
                Breakdown = breakdown,
                AudioReordering = audioReordering ?? [],
            };
        }

        // ---------- FormatRow tests ----------

        [Fact]
        public void FormatRow_NoAudioReordering_ReturnsTabDelimitedHtmlTable()
        {
            // Arrange
            var row = CreateRow();

            // Act
            string result = ResponseFormatter.FormatRow(row);

            // Assert
            Assert.StartsWith("ID001\t我 好\twǒ hǎo\t<table>", result);
            Assert.EndsWith("</table>\tI good\t2", result);
            Assert.Contains("<tr><td>我</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("<td>wǒ</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("<td>hǎo</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("<td>I</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("<td>pron</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("<td>1</td><td>&nbsp;&nbsp;</td>", result);
            Assert.Contains("</tr>", result);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithEmptyNote_SetsSpokenReorderingNote()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = string.Empty },
                new() { Chinese = "好", Pinyin = "hǎo", Gloss = "good", PartOfSpeech = "adj", HskLevel = "1", Note = string.Empty },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = 1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("(Spoken Reordering)", breakdown[0].Note);
            Assert.Equal("(Spoken Reordering)", breakdown[1].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithNaNote_SetsSpokenReorderingNote()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = "N/A" },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = -1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("(Spoken Reordering)", breakdown[0].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithExistingNote_AppendsSpokenReordering()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = "  some note  " },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = -1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("some note (Spoken Reordering)", breakdown[0].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithExistingSpokenReorderingNote_DoesNotDoubleAppend()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = "(Spoken Reordering)" },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = -1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("(Spoken Reordering)", breakdown[0].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithCaseInsensitiveSpokenReorderingNote_DoesNotDoubleAppend()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = "some note (spoken reordering)" },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = -1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("some note (spoken reordering)", breakdown[0].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithDuplicateIndex_OnlyProcessesOnce()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = string.Empty },
                new() { Chinese = "好", Pinyin = "hǎo", Gloss = "good", PartOfSpeech = "adj", HskLevel = "1", Note = string.Empty },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering:
            [
                new AudioReorder { StartIndex = 0, EndIndex = 0 },
                new AudioReorder { StartIndex = 0, EndIndex = 1 },
            ]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("(Spoken Reordering)", breakdown[0].Note);
            Assert.Equal("(Spoken Reordering)", breakdown[1].Note);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithNegativeIndex_DoesNotThrow()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = string.Empty },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = -1, EndIndex = -1 }]);

            // Act
            string result = ResponseFormatter.FormatRow(row);

            // Assert
            Assert.DoesNotContain("(Spoken Reordering)", result);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithIndexBeyondRange_DoesNotThrow()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = string.Empty },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 5, EndIndex = 10 }]);

            // Act
            string result = ResponseFormatter.FormatRow(row);

            // Assert
            Assert.DoesNotContain("(Spoken Reordering)", result);
        }

        [Fact]
        public void FormatRow_EmptyBreakdown_ReturnsEmptyTable()
        {
            // Arrange
            var row = CreateRow(breakdown: []);

            // Act
            string result = ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Contains("<table></table>", result);
        }

        [Fact]
        public void FormatRow_AudioReorderingWithWhitespaceOnlyNote_SetsSpokenReorderingNote()
        {
            // Arrange
            var breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", Pinyin = "wǒ", Gloss = "I", PartOfSpeech = "pron", HskLevel = "1", Note = "   " },
            };
            var row = CreateRow(breakdown: breakdown, audioReordering: [new AudioReorder { StartIndex = 0, EndIndex = -1 }]);

            // Act
            ResponseFormatter.FormatRow(row);

            // Assert
            Assert.Equal("(Spoken Reordering)", breakdown[0].Note);
        }

        // ---------- FormatRows tests ----------

        [Fact]
        public void FormatRows_SingleRow_ReturnsFormattedRowWithoutNewline()
        {
            // Arrange
            var rows = new List<ProcessedSentenceRow> { CreateRow(id: "R1") };

            // Act
            string result = ResponseFormatter.FormatRows(rows);

            // Assert
            Assert.StartsWith("R1\t", result);
            Assert.DoesNotContain('\n', result);
        }

        [Fact]
        public void FormatRows_MultipleRows_SeparatedByNewline()
        {
            // Arrange
            var rows = new List<ProcessedSentenceRow>
            {
                CreateRow(id: "R1"),
                CreateRow(id: "R2"),
            };

            // Act
            string result = ResponseFormatter.FormatRows(rows);

            // Assert
            var parts = result.Split('\n');
            Assert.Equal(2, parts.Length);
            Assert.StartsWith("R1\t", parts[0]);
            Assert.StartsWith("R2\t", parts[1]);
        }

        [Fact]
        public void FormatRows_EmptyList_ReturnsEmptyString()
        {
            // Arrange
            var rows = new List<ProcessedSentenceRow>();

            // Act
            string result = ResponseFormatter.FormatRows(rows);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void FormatRows_ThreeRows_OnlyNewlinesBetweenRows()
        {
            // Arrange
            var rows = new List<ProcessedSentenceRow>
            {
                CreateRow(id: "A"),
                CreateRow(id: "B"),
                CreateRow(id: "C"),
            };

            // Act
            string result = ResponseFormatter.FormatRows(rows);

            // Assert
            int newlineCount = result.Split('\n').Length - 1;
            Assert.Equal(2, newlineCount);
        }
    }
}