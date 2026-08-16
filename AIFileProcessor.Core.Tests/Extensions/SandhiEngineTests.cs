namespace AIFileProcessor.Core.Tests.Extensions;

using System.Collections.Generic;
using AIFileProcessor.Extensions;

[Trait("Category", "Unit")]
public class SandhiEngineTests
{
    private readonly SandhiEngine engine = new();

    // ---------- Helper factories ----------

    private static SyllableNode Node(string text, Tone tone, string pos = "other", bool isParticle = false)
        => new(text, tone, new SandhiContext(text, pos, isParticle));

    private static SyllableNode ParticleNode(string source)
        => new(source, Tone.Neutral, new SandhiContext(source, "particle", true));

    private static SyllableNode BoundaryNode(string boundary = "，")
        => new(boundary, Tone.Unknown, new SandhiContext(boundary, "boundary", false));

    // ---------- Empty / single-element stream ----------

    [Fact]
    public void ProcessSandhiPipeline_EmptyStream_ReturnsEmptyList()
    {
        var result = engine.ProcessSandhiPipeline(Array.Empty<SyllableNode>());
        Assert.Empty(result);
    }

    [Fact]
    public void ProcessSandhiPipeline_SingleElement_DefaultKeepsOriginal()
    {
        var stream = new List<SyllableNode> { Node("mā", Tone.First) };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Single(result);
        Assert.Equal("mā", result[0]);
    }

    // ---------- RULE D: Neutral Particles ----------

    [Theory]
    [InlineData("吗")]
    [InlineData("呢")]
    [InlineData("了")]
    [InlineData("的")]
    [InlineData("得")]
    [InlineData("地")]
    public void ProcessSandhiPipeline_TargetParticle_StripsTone(string particle)
    {
        // The particle source is a CJK char with no tone marks, so StripTone returns it as-is
        var stream = new List<SyllableNode> { ParticleNode(particle) };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal(particle, result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_TargetParticleWithToneMark_StripsTone()
    {
        // Use a pinyin syllable with a tone mark as the source text
        var node = new SyllableNode("mā", Tone.Neutral, new SandhiContext("吗", "particle", true));
        var stream = new List<SyllableNode> { node };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("ma", result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_NonTargetParticle_KeepsOriginal()
    {
        // isParticle=true but source is not one of the target particles
        var stream = new List<SyllableNode> { Node("a", Tone.Neutral, isParticle: true) };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("a", result[0]);
    }

    // ---------- RULE B: bù before 4th tone ----------

    public static IEnumerable<object[]> BuSandhiData => new[]
    {
        new object[] { "shì", Tone.Fourth, "bú" },
        new object[] { "hǎo", Tone.Third, "bù" },
    };

    [Theory]
    [MemberData(nameof(BuSandhiData))]
    public void ProcessSandhiPipeline_BuSandhi_AppliesExpectedToneChange(
        string nextText, Tone nextTone, string expected)
    {
        var stream = new List<SyllableNode>
        {
            Node("bù", Tone.Fourth),
            Node(nextText, nextTone),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal(expected, result[0]);
    }

    // ---------- RULE C: yī quantitative shifts ----------

    public static IEnumerable<object[]> YiSandhiData => new[]
    {
        // yī before 4th tone, non-num -> yí (2nd tone)
        new object[] { "yàng", Tone.Fourth, "other", "yí" },
        // yī before 1st/2nd/3rd tone, non-num -> yì (4th tone)
        new object[] { "ge", Tone.First, "other", "yì" },
        new object[] { "ge", Tone.Second, "other", "yì" },
        new object[] { "ge", Tone.Third, "other", "yì" },
        // yī with num pos -> keeps original
        new object[] { "yàng", Tone.Fourth, "num", "yī" },
    };

    [Theory]
    [MemberData(nameof(YiSandhiData))]
    public void ProcessSandhiPipeline_YiSandhi_AppliesExpectedToneChange(
        string nextText, Tone nextTone, string pos, string expected)
    {
        var stream = new List<SyllableNode>
        {
            Node("yī", Tone.First, pos: pos),
            Node(nextText, nextTone),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal(expected, result[0]);
    }

    // ---------- RULE A: 3rd + 3rd Sandhi ----------

    public static IEnumerable<object[]> ThirdToneSandhiData => new[]
    {
        new object[] { "hǎo", Tone.Third, "ní" },
        new object[] { "ge", Tone.Fourth, "nǐ" },
    };

    [Theory]
    [MemberData(nameof(ThirdToneSandhiData))]
    public void ProcessSandhiPipeline_ThirdToneSandhi_AppliesExpectedToneChange(
        string nextText, Tone nextTone, string expected)
    {
        var stream = new List<SyllableNode>
        {
            Node("nǐ", Tone.Third),
            Node(nextText, nextTone),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal(expected, result[0]);
    }

    // ---------- Boundary handling ----------

    [Fact]
    public void ProcessSandhiPipeline_BoundaryNext_NextToneTreatedAsUnknown()
    {
        // bù before boundary: nextTone should be Unknown (not Fourth), so no sandhi
        var stream = new List<SyllableNode>
        {
            Node("bù", Tone.Fourth),
            BoundaryNode("，"),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("bù", result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_ThirdToneBeforeBoundary_KeepsOriginal()
    {
        var stream = new List<SyllableNode>
        {
            Node("nǐ", Tone.Third),
            BoundaryNode("。"),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("nǐ", result[0]);
    }

    // ---------- Default case ----------

    [Fact]
    public void ProcessSandhiPipeline_NoMatchingRule_KeepsOriginal()
    {
        var stream = new List<SyllableNode>
        {
            Node("shì", Tone.Fourth),
            Node("mā", Tone.First),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("shì", result[0]);
        Assert.Equal("mā", result[1]);
    }

    // ---------- Multi-element stream ----------

    [Fact]
    public void ProcessSandhiPipeline_MultipleRulesAppliedInSequence()
    {
        var stream = new List<SyllableNode>
        {
            Node("nǐ", Tone.Third),      // 3rd+3rd -> ní
            Node("hǎo", Tone.Third),     // boundary next -> keeps hǎo
            BoundaryNode("，"),
            Node("bù", Tone.Fourth),     // bù before 4th -> bú
            Node("qù", Tone.Fourth),     // default -> qù
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("ní", result[0]);
        Assert.Equal("hǎo", result[1]);
        Assert.Equal("，", result[2]);
        Assert.Equal("bú", result[3]);
        Assert.Equal("qù", result[4]);
    }
}