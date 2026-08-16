namespace AIFileProcessor.Core.Tests.Extensions;

using AIFileProcessor.Extensions;

[Trait("Category", "Unit")]
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

    // ---------- Additional GetTone tests ----------

    [Fact]
    public void GetTone_VOnly_ReturnsNeutral()
    {
        Assert.Equal(Tone.Neutral, PinyinUtility.GetTone("v".AsSpan()));
    }

    [Fact]
    public void GetTone_UmlautUOnly_ReturnsNeutral()
    {
        Assert.Equal(Tone.Neutral, PinyinUtility.GetTone("ü".AsSpan()));
    }

    [Fact]
    public void GetTone_MultipleToneMarks_ReturnsFirstEncountered()
    {
        // The first tone-marked vowel encountered determines the tone.
        Assert.Equal(Tone.First, PinyinUtility.GetTone("āá".AsSpan()));
    }

    [Fact]
    public void GetTone_ToneMarkAfterPlainVowel_ReturnsToneFromMarkedVowel()
    {
        // Plain vowel sets hasVowel=true, but tone-marked vowel returns early.
        Assert.Equal(Tone.Third, PinyinUtility.GetTone("aǎ".AsSpan()));
    }

    [Fact]
    public void GetTone_SpecialCharactersOnly_ReturnsUnknown()
    {
        Assert.Equal(Tone.Unknown, PinyinUtility.GetTone("123".AsSpan()));
    }

    // ---------- Additional ShiftTone tests ----------

    [Fact]
    public void ShiftTone_EVowel_AppliesToneMark()
    {
        string result = PinyinUtility.ShiftTone("he", Tone.Second);
        Assert.Equal("hé", result);
    }

    [Fact]
    public void ShiftTone_IVowel_AppliesToneMark()
    {
        string result = PinyinUtility.ShiftTone("li", Tone.Third);
        Assert.Equal("lǐ", result);
    }

    [Fact]
    public void ShiftTone_OVowel_AppliesToneMark()
    {
        string result = PinyinUtility.ShiftTone("mo", Tone.First);
        Assert.Equal("mō", result);
    }

    [Fact]
    public void ShiftTone_UVowel_AppliesToneMark()
    {
        string result = PinyinUtility.ShiftTone("hu", Tone.Fourth);
        Assert.Equal("hù", result);
    }

    [Fact]
    public void ShiftTone_UmlautUVowel_AppliesToneMark()
    {
        string result = PinyinUtility.ShiftTone("nü", Tone.Third);
        Assert.Equal("nǚ", result);
    }

    [Fact]
    public void ShiftTone_VCharacter_ReturnsBaseSyllable()
    {
        // 'v' is not recognized by FindToneVowelIndex, so no tone mark is applied.
        string result = PinyinUtility.ShiftTone("nv", Tone.First);
        Assert.Equal("nv", result);
    }

    [Fact]
    public void ShiftTone_ShiftExistingToneOnEVowel()
    {
        string result = PinyinUtility.ShiftTone("hé", Tone.Fourth);
        Assert.Equal("hè", result);
    }

    [Fact]
    public void ShiftTone_ShiftExistingToneOnIVowel()
    {
        string result = PinyinUtility.ShiftTone("lǐ", Tone.First);
        Assert.Equal("lī", result);
    }

    [Fact]
    public void ShiftTone_ShiftExistingToneOnOVowel()
    {
        string result = PinyinUtility.ShiftTone("mō", Tone.Second);
        Assert.Equal("mó", result);
    }

    [Fact]
    public void ShiftTone_ShiftExistingToneOnUVowel()
    {
        string result = PinyinUtility.ShiftTone("hù", Tone.First);
        Assert.Equal("hū", result);
    }

    [Fact]
    public void ShiftTone_ShiftExistingToneOnUmlautUVowel()
    {
        string result = PinyinUtility.ShiftTone("nǚ", Tone.First);
        Assert.Equal("nǖ", result);
    }

    [Fact]
    public void ShiftTone_UnknownToneOnBase_ReturnsBase()
    {
        string result = PinyinUtility.ShiftTone("ma", Tone.Unknown);
        Assert.Equal("ma", result);
    }

    [Fact]
    public void ShiftTone_FirstToneOnUppercaseEVowel()
    {
        string result = PinyinUtility.ShiftTone("HE", Tone.First);
        Assert.Equal("HĒ", result);
    }

    [Fact]
    public void ShiftTone_FourthToneOnUppercaseIVowel()
    {
        string result = PinyinUtility.ShiftTone("LI", Tone.Fourth);
        Assert.Equal("LÌ", result);
    }

    [Fact]
    public void ShiftTone_SecondToneOnUppercaseOVowel()
    {
        string result = PinyinUtility.ShiftTone("MO", Tone.Second);
        Assert.Equal("MÓ", result);
    }

    [Fact]
    public void ShiftTone_ThirdToneOnUppercaseUVowel()
    {
        string result = PinyinUtility.ShiftTone("HU", Tone.Third);
        Assert.Equal("HǓ", result);
    }

    [Fact]
    public void ShiftTone_SecondToneOnUppercaseUmlautU()
    {
        string result = PinyinUtility.ShiftTone("NÜ", Tone.Second);
        Assert.Equal("NǗ", result);
    }

    [Fact]
    public void ShiftTone_SyllableWithBothIAndU_AccentsLatterVowel()
    {
        // When both i and u exist, the latter one is accented.
        string result = PinyinUtility.ShiftTone("niou", Tone.First);
        Assert.Equal("niōu", result);
    }

    [Fact]
    public void ShiftTone_SyllableWithBothUAndI_AccentsLatterVowel()
    {
        // When both u and i exist and u comes after i, u is accented.
        string result = PinyinUtility.ShiftTone("tuian", Tone.Second);
        Assert.Equal("tuián", result);
    }

    [Fact]
    public void ShiftTone_SyllableWithOnlyI_AccentsI()
    {
        string result = PinyinUtility.ShiftTone("shi", Tone.First);
        Assert.Equal("shī", result);
    }

    [Fact]
    public void ShiftTone_SyllableWithOnlyU_AccentsU()
    {
        string result = PinyinUtility.ShiftTone("shu", Tone.Second);
        Assert.Equal("shú", result);
    }

    [Fact]
    public void ShiftTone_AVowelTakesPriorityOverOthers()
    {
        // 'a' takes priority over e, i, o, u in FindToneVowelIndex.
        string result = PinyinUtility.ShiftTone("xiao", Tone.First);
        Assert.Equal("xiāo", result);
    }

    [Fact]
    public void ShiftTone_OVowelTakesPriorityOverE()
    {
        // FindToneVowelIndex checks 'o' before 'e', so 'o' gets the tone mark.
        string result = PinyinUtility.ShiftTone("yeo", Tone.First);
        Assert.Equal("yeō", result);
    }

    // ---------- Additional StripTone tests ----------

    [Fact]
    public void StripTone_WhitespaceOnly_ReturnsWhitespace()
    {
        Assert.Equal("   ", PinyinUtility.StripTone("   "));
    }

    [Fact]
    public void StripTone_ConsonantsOnly_ReturnsSame()
    {
        Assert.Equal("sh", PinyinUtility.StripTone("sh"));
    }

    [Fact]
    public void StripTone_UppercaseToneMark_Stripped()
    {
        Assert.Equal("MA", PinyinUtility.StripTone("MĀ"));
    }

    [Fact]
    public void StripTone_AllVowelsWithToneMarks_Stripped()
    {
        // ü is a diaeresis, not a tone mark — it is preserved.
        Assert.Equal("aeiouü", PinyinUtility.StripTone("āēīōūǖ"));
    }

    [Fact]
    public void StripTone_PlainVowelsNoToneMark_ReturnsSame()
    {
        // ü is a diaeresis, not a tone mark — it is preserved.
        Assert.Equal("aeiouü", PinyinUtility.StripTone("aeiouü"));
    }

    [Fact]
    public void StripTone_SecondToneOnUmlautU_Stripped()
    {
        // ǘ has a tone mark (acute) on ü; the diaeresis is preserved, tone mark is stripped.
        Assert.Equal("nü", PinyinUtility.StripTone("nǘ"));
    }

    [Fact]
    public void StripTone_FirstToneOnUmlautU_Stripped()
    {
        // ǖ has a tone mark (macron) on ü; the diaeresis is preserved, tone mark is stripped.
        Assert.Equal("nü", PinyinUtility.StripTone("nǖ"));
    }

    [Fact]
    public void StripTone_FourthToneOnUmlautU_Stripped()
    {
        // ǜ has a tone mark (grave) on ü; the diaeresis is preserved, tone mark is stripped.
        Assert.Equal("nü", PinyinUtility.StripTone("nǜ"));
    }

    [Fact]
    public void StripTone_MixedUppercaseAndLowercase_Stripped()
    {
        Assert.Equal("Ma He", PinyinUtility.StripTone("Mā Hé"));
    }

    [Fact]
    public void StripTone_StringWithNumbers_ReturnsSame()
    {
        Assert.Equal("ma1", PinyinUtility.StripTone("ma1"));
    }
}