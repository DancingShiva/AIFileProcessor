namespace AIFileProcessor.Core.Tests.Extensions;
using AIFileProcessor.Extensions;
[Trait("Category", "Unit")]
public class GetCleanedCharacterCountTests
{
    [Fact]
    public void GetCleanedCharacterCount_NullInput_ReturnsPaddedZeroChars()
    {
        string input = null!;
        var result = input.GetCleanedCharacterCount(3);
        Assert.Equal("000Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_EmptyInput_ReturnsPaddedZeroChars()
    {
        var result = string.Empty.GetCleanedCharacterCount(3);
        Assert.Equal("000Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_NullInputWithPaddingOne_ReturnsSingleZeroChars()
    {
        string input = null!;
        var result = input.GetCleanedCharacterCount(1);
        Assert.Equal("0Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_NullInputWithPaddingZero_ReturnsEmptyDigitsChars()
    {
        string input = null!;
        var result = input.GetCleanedCharacterCount(0);
        Assert.Equal("Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_TextWithPunctuationAndSpaces_ReturnsCleanedCount()
    {
        var input = "Hello, World! 你好";
        var result = input.GetCleanedCharacterCount(3);
        // "Hello, World! 你好" - the regex removes ASCII letters, punctuation, symbols, spaces, and Chinese punctuation.
        // Remaining: "你好" -> 2 characters -> padded to 3 digits.
        Assert.Equal("002Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_AllCharactersStripped_ReturnsPaddedZeroChars()
    {
        var input = "Hello, World!";
        var result = input.GetCleanedCharacterCount(4);
        // All characters are stripped by the regex -> length 0 -> padded to 4 digits.
        Assert.Equal("0000Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_ChineseCharactersOnly_ReturnsCount()
    {
        var input = "你好世界";
        var result = input.GetCleanedCharacterCount(2);
        // Chinese characters are not stripped -> 4 characters -> padded to 2 digits.
        Assert.Equal("04Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_ChineseWithPunctuation_StripsPunctuationOnly()
    {
        var input = "你好，世界！";
        var result = input.GetCleanedCharacterCount(2);
        // Full-width punctuation is stripped, leaving "你好世界" -> 4 characters -> padded to 2 digits.
        Assert.Equal("04Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_LargePadding_PadsCorrectly()
    {
        var input = "你好";
        var result = input.GetCleanedCharacterCount(10);
        Assert.Equal("0000000002Chars", result);
    }
    [Fact]
    public void GetCleanedCharacterCount_DigitsRemain_ReturnsCountWithDigits()
    {
        var input = "abc123你好";
        var result = input.GetCleanedCharacterCount(3);
        // ASCII letters a-c are stripped; digits 1-3 and Chinese 你好 remain -> 5 characters -> padded to 3 digits.
        Assert.Equal("005Chars", result);
    }
}

