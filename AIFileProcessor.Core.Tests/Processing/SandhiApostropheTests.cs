namespace AIFileProcessor.Core.Tests.Processing;
using System.Collections.Generic;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using Xunit;
/// <summary>
/// Client-side sandhi over apostrophe-delimited pinyin from the Gemini prompt v9.
/// </summary>
[Trait("Category", "Unit")]
public class SandhiApostropheTests
{
    private static AiApiResponse MakeResponse(string chinese, string pinyin, string breakdownRaw)
    {
        string json = $$"""
        { "SP": "", "Corrections": [], "Sentences": [
          { "C": "{{chinese}}", "P": "{{pinyin}}", "B": "{{breakdownRaw}}", "R": "N/A", "E": "x" }
        ] }
        """;
        return new AiApiResponse(json);
    }
    [Fact]
    public void ThirdThirdInsideWord_ShiftToSecond_TightRejoin()
    {
        // 小雨 xiǎo'yǔ (3rd+3rd inside one word) -> xíáo + yǔ joined tight.
        var response = MakeResponse("小雨", "xiǎoyǔ", "小雨|xiǎo'yǔ|Xiaoyu|pn|N/A| ");
        SandhiProcessor.ApplySandhi(response);
        var entry = response.ProcessedSentenceRows[0].Breakdown[0];
        Assert.Equal("xiáoyǔ", entry.Pinyin);
        Assert.Equal("xiáoyǔ", response.ProcessedSentenceRows[0].Pinyin);
    }
    [Fact]
    public void ThirdNeutralWord_NoChange_TightRejoin()
    {
        // 衣服 yī'fu (1st + neutral) -> unchanged, joined tight.
        var response = MakeResponse("衣服", "yīfu", "衣服|yī'fu|clothes|n|1| ");
        SandhiProcessor.ApplySandhi(response);
        Assert.Equal("yīfu", response.ProcessedSentenceRows[0].Breakdown[0].Pinyin);
        Assert.Equal("yīfu", response.ProcessedSentenceRows[0].Pinyin);
    }
    [Fact]
    public void BracketPlaceholder_SurvivesIntact()
    {
        // [CC] has no apostrophe/space and no vowels -> single token, untouched.
        var response = MakeResponse(
            "点击 [CC] 开启 字幕",
            "diǎnjī [CC] kāiqǐ zìmù",
            "点击|diǎn'jī|click|v|4| ||[CC]|[CC]|closed captions|n|N/A| ||开启|kāi'qǐ|open|v|6| ||字幕|zì'mù|subtitles|n|5| ");
        SandhiProcessor.ApplySandhi(response);
        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("diǎnjī [CC] kāiqǐ zìmù", row.Pinyin);
        Assert.Equal("[CC]", row.Breakdown[1].Pinyin);
        Assert.Equal("点击 [CC] 开启 字幕", row.Chinese);
    }
    [Fact]
    public void UnspacedNoApostrophe_WordKeptWhole_Passthrough()
    {
        // A word with no apostrophe stays whole (no guessing); tone rules don't split it.
        var response = MakeResponse("喜欢", "xǐhuān", "喜欢|xǐhuān|like|v|N/A| ");
        SandhiProcessor.ApplySandhi(response);
        Assert.Equal("xǐhuān", response.ProcessedSentenceRows[0].Breakdown[0].Pinyin);
        Assert.Equal("xǐhuān", response.ProcessedSentenceRows[0].Pinyin);
    }
    [Fact]
    public void NeutralParticle_Le_LosesTone()
    {
        var response = MakeResponse("了", "le", "了|le|[particle]|part|1| ");
        SandhiProcessor.ApplySandhi(response);
        Assert.Equal("le", response.ProcessedSentenceRows[0].Breakdown[0].Pinyin);
    }
    [Fact]
    public void FullSampleLine3_SandhiWithinWords()
    {
        // 父母 line: 超市 chāo'shì (unchanged), 买 mǎi / 菜 cài (separate 3rd-tone words);
        // exercises apostrophe tokenization + cross-entry spacing.
        var response = MakeResponse(
            "超市 买 菜",
            "chāoshì mǎi cài",
            "超市|chāo'shì|supermarket|n|3| ||买|mǎi|buy|v|1| ||菜|cài|groceries|n|1| ");
        SandhiProcessor.ApplySandhi(response);
        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("chāoshì", row.Breakdown[0].Pinyin);
        Assert.Equal("mǎi", row.Breakdown[1].Pinyin);
        Assert.Equal("cài", row.Breakdown[2].Pinyin);
        // "买" (3rd) before "菜" (4th-from-cài) stays 3rd; sentence pinyin joins words with spaces.
        Assert.Equal("chāoshì mǎi cài", row.Pinyin);
    }
    [Fact]
    public void ThirdThirdAcrossEntries_StaysUnshifted_WordBoundary()
    {
        // 你 / 好 as SEPARATE entries (word boundary): 3rd+3rd does NOT shift under the
        // within-word policy.
        var response = MakeResponse("你好", "nǐ hǎo", "你|nǐ|you|pron|1| ||好|hǎo|good|adj|1| ");
        SandhiProcessor.ApplySandhi(response);
        Assert.Equal("nǐ", response.ProcessedSentenceRows[0].Breakdown[0].Pinyin);
        Assert.Equal("hǎo", response.ProcessedSentenceRows[0].Breakdown[1].Pinyin);
        Assert.Equal("nǐ hǎo", response.ProcessedSentenceRows[0].Pinyin);
    }
    [Fact]
    public void XiaoyuWant_NoChainedShiftAcrossBoundary()
    {
        // 小雨 想 : xiǎo'yǔ shifts WITHIN the word to xiáoyǔ, but yǔ does NOT shift
        // before the separate word 想 (xiǎng) — no chained cross-boundary shift.
        var response = MakeResponse(
            "小雨 想",
            "xiǎoyǔ xiǎng",
            "小雨|xiǎo'yǔ|Xiaoyu|pn|N/A| ||想|xiǎng|want|v|1| ");
        SandhiProcessor.ApplySandhi(response);
        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("xiáoyǔ", row.Breakdown[0].Pinyin);
        Assert.Equal("xiǎng", row.Breakdown[1].Pinyin);
        Assert.Equal("xiáoyǔ xiǎng", row.Pinyin);
    }
}

