namespace AIFileProcessor.Core.Tests.Response;

using AIFileProcessor.Response;

public class ProcessedSentenceRowTests
{
    // ---------- R property (set) tests ----------

    [Fact]
    public void R_SetWithNullOrWhitespace_ProducesEmptyAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = string.Empty;

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithNullValue_ProducesEmptyAudioReordering()
    {
        // The setter passes the value to ParseAudioReordering which handles null via IsNullOrWhiteSpace.
        var row = new ProcessedSentenceRow();

        row.R = null!;

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithWhitespaceOnly_ProducesEmptyAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "   ";

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithNaString_ProducesEmptyAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "N/A";

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithNaStringCaseInsensitive_ProducesEmptyAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "n/a";

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithNaStringWithWhitespace_ProducesEmptyAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "  N/A  ";

        Assert.Empty(row.AudioReordering);
    }

    [Fact]
    public void R_SetWithSingleValidSegment_ParsesAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "100 ¥|0,1";

        Assert.Single(row.AudioReordering);
        Assert.Equal("100 ¥", row.AudioReordering[0].VisualString);
        Assert.Equal(0, row.AudioReordering[0].StartIndex);
        Assert.Equal(1, row.AudioReordering[0].EndIndex);
    }

    [Fact]
    public void R_SetWithMultipleSegments_ParsesAllAudioReordering()
    {
        var row = new ProcessedSentenceRow();

        row.R = "100 ¥|0,1||200 ¥|2,3";

        Assert.Equal(2, row.AudioReordering.Count);

        Assert.Equal("100 ¥", row.AudioReordering[0].VisualString);
        Assert.Equal(0, row.AudioReordering[0].StartIndex);
        Assert.Equal(1, row.AudioReordering[0].EndIndex);

        Assert.Equal("200 ¥", row.AudioReordering[1].VisualString);
        Assert.Equal(2, row.AudioReordering[1].StartIndex);
        Assert.Equal(3, row.AudioReordering[1].EndIndex);
    }

    [Fact]
    public void R_SetWithSegmentMissingIndices_DefaultsToMinusOne()
    {
        var row = new ProcessedSentenceRow();

        row.R = "Hello|";

        Assert.Single(row.AudioReordering);
        Assert.Equal("Hello", row.AudioReordering[0].VisualString);
        Assert.Equal(-1, row.AudioReordering[0].StartIndex);
        Assert.Equal(-1, row.AudioReordering[0].EndIndex);
    }

    [Fact]
    public void R_SetWithSegmentWithOnlyStartIndex_ParsesStartIndexOnly()
    {
        var row = new ProcessedSentenceRow();

        row.R = "Hello|5";

        Assert.Single(row.AudioReordering);
        Assert.Equal("Hello", row.AudioReordering[0].VisualString);
        Assert.Equal(5, row.AudioReordering[0].StartIndex);
        Assert.Equal(-1, row.AudioReordering[0].EndIndex);
    }

    [Fact]
    public void R_SetWithNonNumericIndices_DefaultsToMinusOne()
    {
        var row = new ProcessedSentenceRow();

        row.R = "Hello|abc,def";

        Assert.Single(row.AudioReordering);
        Assert.Equal("Hello", row.AudioReordering[0].VisualString);
        Assert.Equal(-1, row.AudioReordering[0].StartIndex);
        Assert.Equal(-1, row.AudioReordering[0].EndIndex);
    }

    [Fact]
    public void R_SetWithSegmentMissingPipe_SkipsSegment()
    {
        var row = new ProcessedSentenceRow();

        // Only one part (no pipe), so parts.Length < 2 causes continue.
        row.R = "NoPipeHere";

        Assert.Empty(row.AudioReordering);
    }

    // ---------- InitializeId tests ----------

    [Fact]
    public void InitializeId_WithPrefixAndPaddedValue_SetsFormattedId()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId("S", 5, 3);

        Assert.Equal("S005", row.Id);
    }

    [Fact]
    public void InitializeId_WithPrefixWithTrailingWhitespace_TrimsPrefix()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId("  S  ", 10, 4);

        Assert.Equal("S0010", row.Id);
    }

    [Fact]
    public void InitializeId_WithEmptyPrefix_SetsOnlyPaddedValue()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId(string.Empty, 1, 2);

        Assert.Equal("01", row.Id);
    }

    [Fact]
    public void InitializeId_WithZeroSequentialValue_SetsPaddedZero()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId("ID", 0, 5);

        Assert.Equal("ID00000", row.Id);
    }

    [Fact]
    public void InitializeId_WithLargeSequentialValue_SetsValueWithCorrectPadding()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId("R", 12345, 3);

        Assert.Equal("R12345", row.Id);
    }

    [Fact]
    public void InitializeId_WithIdPadOne_NoExtraPadding()
    {
        var row = new ProcessedSentenceRow();

        row.InitializeId("T", 7, 1);

        Assert.Equal("T7", row.Id);
    }

    [Fact]
    public void InitializeId_OverwritesExistingId_ReplacesPreviousValue()
    {
        var row = new ProcessedSentenceRow
        {
            Id = "OLD",
        };

        row.InitializeId("N", 42, 4);

        Assert.Equal("N0042", row.Id);
    }
}