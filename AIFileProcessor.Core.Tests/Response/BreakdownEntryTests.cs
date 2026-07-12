namespace AIFileProcessor.Core.Tests.Response;

using AIFileProcessor.Response;

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
}