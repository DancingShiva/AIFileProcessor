namespace AIFileProcessor.Core.Tests.Services;
using System;
using System.Collections.Generic;
using AIFileProcessor.Models;
using AIFileProcessor.Services;
using Xunit;
[Trait("Category", "Unit")]
public class AiModelCatalogTests
{
    private static readonly IReadOnlyList<AiModelInfo> Models =
    [
        new AiModelInfo("Gemini 3 Flash", "gemini-3-flash", [ReasoningLevel.Low, ReasoningLevel.High]),
        new AiModelInfo("GPT-5.5", "gpt-5.5", [ReasoningLevel.None, ReasoningLevel.Low]),
    ];
    // ─── SupportsModel ──────────────────────────────────────────────
    [Fact]
    public void SupportsModel_MatchesByModelId_IsCaseInsensitiveAndTrims()
    {
        Assert.True(AiModelCatalog.SupportsModel(Models, "  GEMINI-3-FLASH  "));
    }
    [Fact]
    public void SupportsModel_MatchesByModelName_IsCaseInsensitiveAndTrims()
    {
        Assert.True(AiModelCatalog.SupportsModel(Models, "gemini 3 flash"));
    }
    [Fact]
    public void SupportsModel_UnknownModel_ReturnsFalse()
    {
        Assert.False(AiModelCatalog.SupportsModel(Models, "no-such-model"));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SupportsModel_NullOrWhitespace_ReturnsFalse(string? query)
    {
        Assert.False(AiModelCatalog.SupportsModel(Models, query));
    }
    [Fact]
    public void SupportsModel_NullModels_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AiModelCatalog.SupportsModel(null!, "x"));
    }
    // ─── GetReasoningLevels ─────────────────────────────────────────
    [Fact]
    public void GetReasoningLevels_KnownModelId_IsCaseInsensitive_ReturnsLevels()
    {
        var levels = AiModelCatalog.GetReasoningLevels(Models, "GEMINI-3-FLASH");
        Assert.Equal([ReasoningLevel.Low, ReasoningLevel.High], levels);
    }
    [Fact]
    public void GetReasoningLevels_UnknownModelId_ReturnsEmpty()
    {
        Assert.Empty(AiModelCatalog.GetReasoningLevels(Models, "nope"));
    }
    [Fact]
    public void GetReasoningLevels_NullModelId_ReturnsEmpty()
    {
        Assert.Empty(AiModelCatalog.GetReasoningLevels(Models, null));
    }
    [Fact]
    public void GetReasoningLevels_NullModels_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AiModelCatalog.GetReasoningLevels(null!, "x"));
    }
}

