//-----------------------------------------------------------------------
// <copyright file="ModelTokenLimitsTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using AIFileProcessor.Services;
using Xunit;

[Trait("Category", "Unit")]
public class ModelTokenLimitsTests
{
    // ─── GetHardMax: null/whitespace providerName ──────────────────────────────

    [Fact]
    public void GetHardMax_NullProviderName_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax(null!, "gemini-3-flash");
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_EmptyProviderName_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax(string.Empty, "gemini-3-flash");
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_WhitespaceProviderName_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("   ", "gemini-3-flash");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: unknown provider ──────────────────────────────────────────

    [Fact]
    public void GetHardMax_UnknownProvider_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Anthropic", "claude-3-opus");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: Gemini models all return 262144 ───────────────────────────

    [Fact]
    public void GetHardMax_Gemini_Flash_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Gemini", "gemini-3-flash");
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_Gemini_Pro_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Gemini", "gemini-3.1-pro-preview");
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_Gemini_NullModel_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Gemini", null);
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_Gemini_EmptyModel_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Gemini", string.Empty);
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_Gemini_UnknownModel_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("Gemini", "unknown-model");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: OpenAI models all return 262144 ───────────────────────────

    [Fact]
    public void GetHardMax_OpenAi_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("OpenAi", "gpt-5.6-sol");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: DeepSeek models all return 262144 ─────────────────────────

    [Fact]
    public void GetHardMax_DeepSeek_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("DeepSeek", "deepseek-v4-pro");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: provider name casing ──────────────────────────────────────

    [Fact]
    public void GetHardMax_ProviderNameMixedCase_Gemini_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("GeMiNi", null);
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_ProviderNameUpperCase_OPENAI_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("OPENAI", "gpt-5.6-terra");
        Assert.Equal(262144, result);
    }

    [Fact]
    public void GetHardMax_ProviderNameLowerCase_deepseek_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("deepseek", "deepseek-v4-flash");
        Assert.Equal(262144, result);
    }

    // ─── GetHardMax: provider name with whitespace ─────────────────────────────

    [Fact]
    public void GetHardMax_ProviderNameWithSpaces_Gemini_Returns262144()
    {
        int result = ModelTokenLimits.GetHardMax("  Gemini  ", null);
        Assert.Equal(262144, result);
    }
}
