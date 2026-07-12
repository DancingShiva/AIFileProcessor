namespace AIFileProcessor.Core.Tests.Extensions;

using AIFileProcessor.Extensions;

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

    [Fact]
    public void ProcessSandhiPipeline_BuBeforeFourthTone_ReturnsBuSecondTone()
    {
        var stream = new List<SyllableNode>
        {
            Node("bù", Tone.Fourth),
            Node("shì", Tone.Fourth),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("bú", result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_BuBeforeThirdTone_KeepsOriginal()
    {
        var stream = new List<SyllableNode>
        {
            Node("bù", Tone.Fourth),
            Node("hǎo", Tone.Third),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("bù", result[0]);
    }

    // ---------- RULE C: yī quantitative shifts ----------

    [Fact]
    public void ProcessSandhiPipeline_YiBeforeFourthToneNonNum_ReturnsYiSecondTone()
    {
        var stream = new List<SyllableNode>
        {
            Node("yī", Tone.First, pos: "other"),
            Node("yàng", Tone.Fourth),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("yí", result[0]);
    }

    [Theory]
    [InlineData(Tone.First)]
    [InlineData(Tone.Second)]
    [InlineData(Tone.Third)]
    public void ProcessSandhiPipeline_YiBeforeNonFourthNonNum_ReturnsYiFourthTone(Tone nextTone)
    {
        var stream = new List<SyllableNode>
        {
            Node("yī", Tone.First, pos: "other"),
            Node("ge", nextTone),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("yì", result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_YiWithNumPos_KeepsOriginal()
    {
        var stream = new List<SyllableNode>
        {
            Node("yī", Tone.First, pos: "num"),
            Node("yàng", Tone.Fourth),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("yī", result[0]);
    }

    // ---------- RULE A: 3rd + 3rd Sandhi ----------

    [Fact]
    public void ProcessSandhiPipeline_ThirdToneBeforeThirdTone_ReturnsSecondTone()
    {
        var stream = new List<SyllableNode>
        {
            Node("nǐ", Tone.Third),
            Node("hǎo", Tone.Third),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("ní", result[0]);
    }

    [Fact]
    public void ProcessSandhiPipeline_ThirdToneBeforeNonThird_KeepsOriginal()
    {
        var stream = new List<SyllableNode>
        {
            Node("nǐ", Tone.Third),
            Node("ge", Tone.Fourth),
        };
        var result = engine.ProcessSandhiPipeline(stream);
        Assert.Equal("nǐ", result[0]);
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