namespace AIFileProcessor.Core.Tests.Extensions;

using AIFileProcessor.Extensions;

public class PinyinUtilityTests
{
    // ---------- GetTone ----------

    [Theory]
    [InlineData("ā")]
    [InlineData("ē")]
    [InlineData("ī")]
    [InlineData("ō")]
    [InlineData("ū")]
    [InlineData("ǖ")]
    public void GetTone_FirstToneChars_ReturnsFirst(string syllable)
    {
        Assert.Equal(Tone.First, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Theory]
    [InlineData("á")]
    [InlineData("é")]
    [InlineData("í")]
    [InlineData("ó")]
    [InlineData("ú")]
    [InlineData("ǘ")]
    public void GetTone_SecondToneChars_ReturnsSecond(string syllable)
    {
        Assert.Equal(Tone.Second, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Theory]
    [InlineData("ǎ")]
    [InlineData("ě")]
    [InlineData("ǐ")]
    [InlineData("ǒ")]
    [InlineData("ǔ")]
    [InlineData("ǚ")]
    public void GetTone_ThirdToneChars_ReturnsThird(string syllable)
    {
        Assert.Equal(Tone.Third, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Theory]
    [InlineData("à")]
    [InlineData("è")]
    [InlineData("ì")]
    [InlineData("ò")]
    [InlineData("ù")]
    [InlineData("ǜ")]
    public void GetTone_FourthToneChars_ReturnsFourth(string syllable)
    {
        Assert.Equal(Tone.Fourth, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Theory]
    [InlineData("ma")]
    [InlineData("he")]
    [InlineData("li")]
    [InlineData("mo")]
    [InlineData("hu")]
    [InlineData("nü")]
    [InlineData("nv")]
    public void GetTone_PlainVowelsNoToneMark_ReturnsNeutral(string syllable)
    {
        Assert.Equal(Tone.Neutral, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("m")]
    [InlineData("sh")]
    [InlineData("zh")]
    public void GetTone_NoVowels_ReturnsUnknown(string syllable)
    {
        Assert.Equal(Tone.Unknown, PinyinUtility.GetTone(syllable.AsSpan()));
    }

    [Fact]
    public void GetTone_MixedConsonantsAndPlainVowel_ReturnsNeutral()
    {
        Assert.Equal(Tone.Neutral, PinyinUtility.GetTone("shang".AsSpan()));
    }

    [Fact]
    public void GetTone_ToneMarkInSyllable_ReturnsCorrectTone()
    {
        Assert.Equal(Tone.First, PinyinUtility.GetTone("mā".AsSpan()));
        Assert.Equal(Tone.Second, PinyinUtility.GetTone("né".AsSpan()));
        Assert.Equal(Tone.Third, PinyinUtility.GetTone("hǎo".AsSpan()));
        Assert.Equal(Tone.Fourth, PinyinUtility.GetTone("dà".AsSpan()));
    }

    // ---------- ShiftTone ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShiftTone_EmptyOrWhitespace_ReturnsEmpty(string syllable)
    {
        Assert.Equal(string.Empty, PinyinUtility.ShiftTone(syllable, Tone.First));
    }

    [Fact]
    public void ShiftTone_NeutralTone_StripsToneMark()
    {
        string result = PinyinUtility.ShiftTone("mā", Tone.Neutral);
        Assert.Equal("ma", result);
    }

    [Fact]
    public void ShiftTone_UnknownTone_StripsToneMark()
    {
        string result = PinyinUtility.ShiftTone("mā", Tone.Unknown);
        Assert.Equal("ma", result);
    }

    [Fact]
    public void ShiftTone_FirstTone_AppliesFirstToneMark()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.First);
        Assert.Equal("mā", result);
    }

    [Fact]
    public void ShiftTone_SecondTone_AppliesSecondToneMark()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.Second);
        Assert.Equal("má", result);
    }

    [Fact]
    public void ShiftTone_ThirdTone_AppliesThirdToneMark()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.Third);
        Assert.Equal("mǎ", result);
    }

    [Fact]
    public void ShiftTone_FourthTone_AppliesFourthToneMark()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.Fourth);
        Assert.Equal("mà", result);
    }

    [Fact]
    public void ShiftTone_NoVowel_ReturnsBaseSyllable()
    {
        string result = PinyinUtility.ShiftTone("sh", Tone.First);
        Assert.Equal("sh", result);
    }

    [Fact]
    public void ShiftTone_AlreadyHasTargetTone_ReturnsSameSyllable()
    {
        string result = PinyinUtility.ShiftTone("mā", Tone.First);
        Assert.Equal("mā", result);
    }

    [Fact]
    public void ShiftTone_ShiftsExistingToneToNewTone()
    {
        string result = PinyinUtility.ShiftTone("mǎ", Tone.Fourth);
        Assert.Equal("mà", result);
    }

    [Fact]
    public void ShiftTone_PreservesUppercase()
    {
        string result = PinyinUtility.ShiftTone("MA", Tone.First);
        Assert.Equal("MĀ", result);
    }

    [Fact]
    public void ShiftTone_NeutralOnAlreadyBase_ReturnsBase()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.Neutral);
        Assert.Equal("ma", result);
    }

    // ---------- StripTone ----------

    [Fact]
    public void StripTone_Null_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, PinyinUtility.StripTone(null!));
    }

    [Fact]
    public void StripTone_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, PinyinUtility.StripTone(string.Empty));
    }

    [Fact]
    public void StripTone_NoToneMarks_ReturnsSameString()
    {
        Assert.Equal("ma", PinyinUtility.StripTone("ma"));
    }

    [Fact]
    public void StripTone_FirstToneMark_Stripped()
    {
        Assert.Equal("ma", PinyinUtility.StripTone("mā"));
    }

    [Fact]
    public void StripTone_SecondToneMark_Stripped()
    {
        Assert.Equal("he", PinyinUtility.StripTone("hé"));
    }

    [Fact]
    public void StripTone_ThirdToneMark_Stripped()
    {
        Assert.Equal("hao", PinyinUtility.StripTone("hǎo"));
    }

    [Fact]
    public void StripTone_FourthToneMark_Stripped()
    {
        Assert.Equal("da", PinyinUtility.StripTone("dà"));
    }

    [Fact]
    public void StripTone_UmlautU_ToneMark_Stripped()
    {
        Assert.Equal("nü", PinyinUtility.StripTone("nǚ"));
    }

    [Fact]
    public void StripTone_MultipleToneMarks_AllStripped()
    {
        Assert.Equal("ni hao", PinyinUtility.StripTone("nǐ hǎo"));
    }
}