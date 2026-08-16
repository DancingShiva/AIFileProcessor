namespace AIFileProcessor.Core.Tests.Response;

using AIFileProcessor.Response;

[Trait("Category", "Unit")]
public class BreakdownEntryTests
{
    [Fact]
    public void ToString_WithAllPropertiesSet_ReturnsPipeDelimitedString()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "我",
            Pinyin = "wǒ",
            Gloss = "I",
            PartOfSpeech = "pronoun",
            HskLevel = "1",
            Note = "subject",
        };

        var result = entry.ToString();

        Assert.Equal("我|wǒ|I|pronoun|1|subject", result);
    }

    [Fact]
    public void ToString_WithDefaultValues_ReturnsPipeDelimitedEmptyString()
    {
        var entry = new BreakdownEntry();

        var result = entry.ToString();

        Assert.Equal("|||||", result);
    }

    [Fact]
    public void ToString_WithNullProperties_ReturnsPipeDelimitedWithEmptySegments()
    {
        var entry = new BreakdownEntry
        {
            Chinese = null!,
            Pinyin = null!,
            Gloss = null!,
            PartOfSpeech = null!,
            HskLevel = null!,
            Note = null!,
        };

        var result = entry.ToString();

        Assert.Equal("|||||", result);
    }

    [Fact]
    public void ToString_WithSomePropertiesSet_ReturnsPipeDelimitedWithEmptySegments()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
            Gloss = string.Empty,
            PartOfSpeech = string.Empty,
            HskLevel = "2",
            Note = string.Empty,
        };

        var result = entry.ToString();

        Assert.Equal("你好|nǐ hǎo|||2|", result);
    }

    [Fact]
    public void ToString_WithSpecialCharactersInProperties_ReturnsPipeDelimitedString()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "，",
            Pinyin = string.Empty,
            Gloss = "comma",
            PartOfSpeech = "punctuation",
            HskLevel = "N/A",
            Note = "full-width",
        };

        var result = entry.ToString();

        Assert.Equal("，||comma|punctuation|N/A|full-width", result);
    }

    [Fact]
    public void ToString_WithPipeCharactersInValues_PreservesPipesInValues()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "a|b",
            Pinyin = "c|d",
            Gloss = "e|f",
            PartOfSpeech = "g|h",
            HskLevel = "i|j",
            Note = "k|l",
        };

        var result = entry.ToString();

        Assert.Equal("a|b|c|d|e|f|g|h|i|j|k|l", result);
    }

    [Fact]
    public void ToString_WithWhitespaceOnlyValues_ReturnsWhitespaceSegments()
    {
        var entry = new BreakdownEntry
        {
            Chinese = " ",
            Pinyin = " ",
            Gloss = " ",
            PartOfSpeech = " ",
            HskLevel = " ",
            Note = " ",
        };

        var result = entry.ToString();

        Assert.Equal(" | | | | | ", result);
    }

    [Fact]
    public void ToString_WithMixedNullAndNonNullProperties_ReturnsCorrectFormat()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "是",
            Pinyin = null!,
            Gloss = "to be",
            PartOfSpeech = null!,
            HskLevel = "1",
            Note = null!,
        };

        var result = entry.ToString();

        Assert.Equal("是||to be||1|", result);
    }

    [Fact]
    public void ToString_WithOnlyChineseSet_ReturnsCorrectFormat()
    {
        var entry = new BreakdownEntry
        {
            Chinese = "们",
        };

        var result = entry.ToString();

        Assert.Equal("们|||||", result);
    }

    [Fact]
    public void ToString_WithOnlyNoteSet_ReturnsCorrectFormat()
    {
        var entry = new BreakdownEntry
        {
            Note = "plural marker",
        };

        var result = entry.ToString();

        Assert.Equal("|||||plural marker", result);
    }
}