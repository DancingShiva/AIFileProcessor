namespace AIFileProcessor.Core.Tests.Processing;
using System;
using System.Linq;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using Xunit;
[Trait("Category", "Unit")]
public class ResponseParserTests
{
    // A valid tab-delimited line with 7 columns (breakdown has 6 pipe-delimited fields)
    private const string ValidLine = "1\t你\tnǐ\t你|nǐ|you|pron|1|none\tN/A\tHello\t5";
    // ─── Parse: null / whitespace / empty ───────────────────────────────
    [Fact]
    public void Parse_NullInput_ReturnsEmptyList()
    {
        // Act
        var result = ResponseParser.Parse(null!);
        // Assert
        Assert.Empty(result);
    }
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyList()
    {
        // Act
        var result = ResponseParser.Parse(string.Empty);
        // Assert
        Assert.Empty(result);
    }
    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyList()
    {
        // Act
        var result = ResponseParser.Parse("   \t\n  ");
        // Assert
        Assert.Empty(result);
    }
    // ─── Parse: blank line skipping ─────────────────────────────────────
    [Fact]
    public void Parse_BlankLinesBetweenValidLines_SkipsBlanksAndParsesValid()
    {
        // Arrange
        string input = "\r\n" + ValidLine + "\r\n\r\n" + ValidLine + "\r\n";
        // Act
        var result = ResponseParser.Parse(input);
        // Assert
        Assert.Equal(2, result.Count);
    }
    // ─── Parse: status line skipping ────────────────────────────────────
    [Theory]
    [InlineData("Processing...")]
    [InlineData("Processing in progress.")]
    [InlineData("Processing cancelled.")]
    [InlineData("There were no sentences to process.")]
    [InlineData("Pipeline Error: something went wrong")]
    public void Parse_StatusLines_SkipsThemAndReturnsEmptyList(string statusLine)
    {
        // Act
        var result = ResponseParser.Parse(statusLine);
        // Assert
        Assert.Empty(result);
    }
    [Fact]
    public void Parse_StatusLineMixedWithValidLine_SkipsStatusAndParsesValid()
    {
        // Arrange
        string input = "Processing...\r\n" + ValidLine + "\r\nProcessing cancelled.";
        // Act
        var result = ResponseParser.Parse(input);
        // Assert
        Assert.Single(result);
    }
    // ─── Parse: different line separators ───────────────────────────────
    [Fact]
    public void Parse_CrOnlySeparator_ParsesLines()
    {
        // Arrange
        string input = ValidLine + "\r" + ValidLine;
        // Act
        var result = ResponseParser.Parse(input);
        // Assert
        Assert.Equal(2, result.Count);
    }
    [Fact]
    public void Parse_LfOnlySeparator_ParsesLines()
    {
        // Arrange
        string input = ValidLine + "\n" + ValidLine;
        // Act
        var result = ResponseParser.Parse(input);
        // Assert
        Assert.Equal(2, result.Count);
    }
    // ─── Parse: valid line content ──────────────────────────────────────
    [Fact]
    public void Parse_SingleValidLine_ReturnsParsedRowWithExpectedFields()
    {
        // Act
        var result = ResponseParser.Parse(ValidLine);
        // Assert
        Assert.Single(result);
        var row = result[0];
        Assert.Equal("1", row.Id);
        Assert.Equal("你", row.Chinese);
        Assert.Equal("nǐ", row.Pinyin);
        Assert.Equal("Hello", row.English);
        Assert.Equal("5", row.Length);
        Assert.Single(row.Breakdown);
        Assert.Equal("你", row.Breakdown[0].Chinese);
        Assert.Equal("nǐ", row.Breakdown[0].Pinyin);
        Assert.Equal("you", row.Breakdown[0].Gloss);
        Assert.Equal("pron", row.Breakdown[0].PartOfSpeech);
        Assert.Equal("1", row.Breakdown[0].HskLevel);
        Assert.Equal("none", row.Breakdown[0].Note);
    }
    [Fact]
    public void Parse_LineWithExtraWhitespace_TrimsBeforeStatusCheckButParsesRaw()
    {
        // Arrange – leading/trailing spaces around a valid line
        string input = "  " + ValidLine + "  ";
        // Act
        var result = ResponseParser.Parse(input);
        // Assert
        Assert.Single(result);
    }
    // ─── Parse: error / malformed lines ─────────────────────────────────
    [Fact]
    public void Parse_LineWithWrongColumnCount_ThrowsFormatException()
    {
        // Arrange – only 3 columns instead of 7
        string input = "1\t你\tnǐ";
        // Act & Assert
        Assert.Throws<FormatException>(() => ResponseParser.Parse(input));
    }
    [Fact]
    public void Parse_BreakdownEntryWithWrongPipeCount_ThrowsFormatException()
    {
        // Arrange – breakdown has only 2 pipe fields instead of 6
        string input = "1\t你\tnǐ\t你|nǐ\tn/a\tHello\t5";
        // Act & Assert
        Assert.Throws<FormatException>(() => ResponseParser.Parse(input));
    }
    // ─── IsStatusLine ───────────────────────────────────────────────────
    [Theory]
    [InlineData("Processing...")]
    [InlineData("processing...")]
    [InlineData("PROCESSING...")]
    public void IsStatusLine_ProcessingDotDotDot_ReturnsTrue(string line)
    {
        Assert.True(ResponseParser.IsStatusLine(line));
    }
    [Theory]
    [InlineData("Processing in progress.")]
    [InlineData("PROCESSING IN PROGRESS.")]
    [InlineData("processing in progress.")]
    public void IsStatusLine_ProcessingInProgress_ReturnsTrue(string line)
    {
        Assert.True(ResponseParser.IsStatusLine(line));
    }
    [Theory]
    [InlineData("Processing cancelled.")]
    [InlineData("PROCESSING CANCELLED.")]
    public void IsStatusLine_ProcessingCancelled_ReturnsTrue(string line)
    {
        Assert.True(ResponseParser.IsStatusLine(line));
    }
    [Theory]
    [InlineData("There were no sentences")]
    [InlineData("There were no sentences to process.")]
    [InlineData("there were no sentences found")]
    public void IsStatusLine_ThereWereNoSentencesPrefix_ReturnsTrue(string line)
    {
        Assert.True(ResponseParser.IsStatusLine(line));
    }
    [Theory]
    [InlineData("Pipeline Error")]
    [InlineData("Pipeline Error: timeout")]
    [InlineData("PIPELINE ERROR")]
    public void IsStatusLine_PipelineErrorPrefix_ReturnsTrue(string line)
    {
        Assert.True(ResponseParser.IsStatusLine(line));
    }
    [Theory]
    [InlineData("")]
    [InlineData("Hello world")]
    [InlineData("1\t你\tnǐ")]
    [InlineData("Processing")]
    [InlineData("Processing..")]
    public void IsStatusLine_NonStatusLine_ReturnsFalse(string line)
    {
        Assert.False(ResponseParser.IsStatusLine(line));
    }
}

