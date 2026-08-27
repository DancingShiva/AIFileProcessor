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

    // ─── GetHardMax: configured path (case-insensitive provider name lookup) ──

    /// <summary>
    /// Configures ModelTokenLimits with a known map for all four providers
    /// and a non-default default, so the configured lookup path is exercised.
    /// </summary>
    private static void ConfigureForTest()
    {
        ModelTokenLimits.Configure(
            new Dictionary<string, int>
            {
                ["Gemini"] = 1000000,
                ["OpenAI"] = 2000000,
                ["DeepSeek"] = 64000,
                ["Ollama"] = 8192,
            },
            defaultTokenLimit: 50000);
    }

    /// <summary>
    /// Resets ModelTokenLimits to the unconfigured state to avoid
    /// cross-test contamination from the static dictionary.
    /// </summary>
    private static void ResetToUnconfigured()
    {
        // Configure with empty map and default 262144, which simulates the
        // unconfigured fallback for all unknown provider/model names.
        ModelTokenLimits.Configure([], 262144);
    }

    public static TheoryData<string, int> ConfiguredProviderCaseVariants => new()
    {
        { "Gemini", 1000000 },
        { "gemini", 1000000 },
        { "GEMINI", 1000000 },
        { "GeMiNi", 1000000 },
        { "OpenAI", 2000000 },
        { "openai", 2000000 },
        { "OPENAI", 2000000 },
        { "DeepSeek", 64000 },
        { "deepseek", 64000 },
        { "DEEPSEEK", 64000 },
        { "Ollama", 8192 },
        { "ollama", 8192 },
        { "OLLAMA", 8192 },
    };

    [Theory]
    [MemberData(nameof(ConfiguredProviderCaseVariants))]
    public void GetHardMax_Configured_ProviderNameCaseInsensitive_ReturnsConfiguredLimit(
        string providerName, int expectedLimit)
    {
        ConfigureForTest();
        try
        {
            int result = ModelTokenLimits.GetHardMax(providerName, null);
            Assert.Equal(expectedLimit, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void GetHardMax_Configured_ModelSpecificLimit_ReturnsModelLimit()
    {
        ConfigureForTest();
        try
        {
            // Model-specific limit takes priority over provider-level limit.
            int result = ModelTokenLimits.GetHardMax("Gemini", "gemini-3-flash");
            // No model-specific entry; falls back to provider-level (1000000).
            Assert.Equal(1000000, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void GetHardMax_Configured_UnknownProvider_ReturnsConfiguredDefault()
    {
        ConfigureForTest();
        try
        {
            int result = ModelTokenLimits.GetHardMax("Anthropic", "claude-3");
            Assert.Equal(50000, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    // ─── RegisterModelLimit ─────────────────────────────────────────────

    [Fact]
    public void RegisterModelLimit_ValidModel_UpdatesLimit()
    {
        ResetToUnconfigured();
        try
        {
            ModelTokenLimits.RegisterModelLimit("custom-model", 99999);
            int result = ModelTokenLimits.GetHardMax("UnknownProvider", "custom-model");
            Assert.Equal(99999, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterModelLimit_InvalidModelId_DoesNotRegister(string? modelId)
    {
        ResetToUnconfigured();
        try
        {
            ModelTokenLimits.RegisterModelLimit(modelId!, 99999);
            // Should return the default (262144) since nothing was registered.
            int result = ModelTokenLimits.GetHardMax("UnknownProvider", modelId);
            Assert.Equal(262144, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void RegisterModelLimit_ZeroOrNegativeLimit_DoesNotRegister()
    {
        ResetToUnconfigured();
        try
        {
            ModelTokenLimits.RegisterModelLimit("bad-model", 0);
            ModelTokenLimits.RegisterModelLimit("bad-model-2", -1);
            int result = ModelTokenLimits.GetHardMax("UnknownProvider", "bad-model");
            Assert.Equal(262144, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void RegisterModelLimit_OverwritesExistingLimit()
    {
        ResetToUnconfigured();
        try
        {
            ModelTokenLimits.RegisterModelLimit("my-model", 10000);
            ModelTokenLimits.RegisterModelLimit("my-model", 20000);
            int result = ModelTokenLimits.GetHardMax("UnknownProvider", "my-model");
            Assert.Equal(20000, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    // ─── Configure: edge cases ──────────────────────────────────────────

    [Fact]
    public void Configure_NullMap_DoesNotThrow_UsesDefault()
    {
        ModelTokenLimits.Configure(null!, 100000);
        try
        {
            int result = ModelTokenLimits.GetHardMax("AnyProvider", null);
            Assert.Equal(100000, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void Configure_ZeroOrNegativeDefault_FallsBackTo262144()
    {
        ModelTokenLimits.Configure([], 0);
        try
        {
            int result = ModelTokenLimits.GetHardMax("AnyProvider", null);
            Assert.Equal(262144, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }

    [Fact]
    public void Configure_EmptyOrNullKeyEntries_AreIgnored()
    {
        ModelTokenLimits.Configure(
            new Dictionary<string, int>
            {
                [""] = 999,
                ["   "] = 888,
                ["ValidModel"] = 777,
            },
            50000);
        try
        {
            // Empty/whitespace keys should be ignored; valid key should work.
            int result = ModelTokenLimits.GetHardMax("AnyProvider", "ValidModel");
            Assert.Equal(777, result);
        }
        finally
        {
            ResetToUnconfigured();
        }
    }
}
