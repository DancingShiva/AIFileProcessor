namespace AIFileProcessor.Core.Tests.Extensions;
using AIFileProcessor.Extensions;
[Trait("Category", "Unit")]
public class SandhiContextTests
{
    [Fact]
    public void IsBoundary_SourceContainsAsciiPeriod_ReturnsTrue()
    {
        var context = new SandhiContext(".", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsAsciiQuestionMark_ReturnsTrue()
    {
        var context = new SandhiContext("?", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsAsciiExclamationMark_ReturnsTrue()
    {
        var context = new SandhiContext("!", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthPeriod_ReturnsTrue()
    {
        var context = new SandhiContext("。", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthQuestionMark_ReturnsTrue()
    {
        var context = new SandhiContext("？", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthExclamationMark_ReturnsTrue()
    {
        var context = new SandhiContext("！", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsAsciiComma_ReturnsTrue()
    {
        var context = new SandhiContext(",", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthComma_ReturnsTrue()
    {
        var context = new SandhiContext("，", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsAsciiSemicolon_ReturnsTrue()
    {
        var context = new SandhiContext(";", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthSemicolon_ReturnsTrue()
    {
        var context = new SandhiContext("；", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsAsciiColon_ReturnsTrue()
    {
        var context = new SandhiContext(":", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsFullWidthColon_ReturnsTrue()
    {
        var context = new SandhiContext("：", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsIdeographicComma_ReturnsTrue()
    {
        var context = new SandhiContext("、", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsBoundaryCharWithinText_ReturnsTrue()
    {
        var context = new SandhiContext("你好。世界", "n", false);
        Assert.True(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceContainsNoBoundaryChar_ReturnsFalse()
    {
        var context = new SandhiContext("你好", "n", false);
        Assert.False(context.IsBoundary);
    }
    [Fact]
    public void IsBoundary_SourceIsEmpty_ReturnsFalse()
    {
        var context = new SandhiContext(string.Empty, "n", false);
        Assert.False(context.IsBoundary);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsMa_ReturnsTrue()
    {
        var context = new SandhiContext("吗", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsNe_ReturnsTrue()
    {
        var context = new SandhiContext("呢", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsLe_ReturnsTrue()
    {
        var context = new SandhiContext("了", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsDe_ReturnsTrue()
    {
        var context = new SandhiContext("的", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsDe2_ReturnsTrue()
    {
        var context = new SandhiContext("得", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsDi_ReturnsTrue()
    {
        var context = new SandhiContext("地", "part", true);
        Assert.True(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_NotParticleAndSourceIsTarget_ReturnsFalse()
    {
        var context = new SandhiContext("吗", "n", false);
        Assert.False(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_IsParticleAndSourceIsNotTarget_ReturnsFalse()
    {
        var context = new SandhiContext("我", "part", true);
        Assert.False(context.IsTargetParticle);
    }
    [Fact]
    public void IsTargetParticle_NotParticleAndSourceIsNotTarget_ReturnsFalse()
    {
        var context = new SandhiContext("我", "n", false);
        Assert.False(context.IsTargetParticle);
    }
}

