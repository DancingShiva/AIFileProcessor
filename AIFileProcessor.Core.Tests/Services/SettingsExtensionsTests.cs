namespace AIFileProcessor.Core.Tests.Services;

using System.Collections.Generic;
using AIFileProcessor.Configuration;
using AIFileProcessor.Services;
using Xunit;

[Trait("Category", "Unit")]
public class SettingsExtensionsTests
{
    // --- GeminiSettings.CopyOnto --------------------------------------

    [Fact]
    public void CopyOnto_Gemini_NullSource_ReturnsWithoutThrowing()
    {
        GeminiSettings source = null!;
        var target = new GeminiSettings();

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_Gemini_NullTarget_ReturnsWithoutThrowing()
    {
        var source = new GeminiSettings();
        GeminiSettings target = null!;

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_Gemini_NullSourceAndNullTarget_ReturnsWithoutThrowing()
    {
        GeminiSettings source = null!;
        GeminiSettings target = null!;

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_Gemini_AllPropertiesSet_OverwritesTarget()
    {
        var source = new GeminiSettings
        {
            Endpoint = "https://override.example.com",
            ModelName = "override-model",
            TimeoutSeconds = 60,
            ProjectId = "override-project",
            Location = "override-location",
            Temperature = 0.7,
            TopP = 0.9,
            StreamingEnabled = true,
            MaxRetries = 5,
            BaseDelay = 2,
            MaxDelay = 120,
            RequestHeaders = new Dictionary<string, string?> { ["X-Test"] = "val" },
            AdditionalParameters = new Dictionary<string, string?> { ["param"] = "val" },
            EnableLogging = true,
            RateLimitPerMinute = 100,
            ProxyUrl = "https://proxy.example.com",
            UseManagedIdentity = true,
            CredentialType = "Default",
        };
        var target = new GeminiSettings { ApiKey = "machine-level-key" };

        source.CopyOnto(target);

        // ApiKey is never overlaid from per-user settings
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://override.example.com", target.Endpoint);
        Assert.Equal("override-model", target.ModelName);
        Assert.Equal(60, target.TimeoutSeconds);
        Assert.Equal("override-project", target.ProjectId);
        Assert.Equal("override-location", target.Location);
        Assert.Equal(0.7, target.Temperature);
        Assert.Equal(0.9, target.TopP);
        Assert.True(target.StreamingEnabled);
        Assert.Equal(5, target.MaxRetries);
        Assert.Equal(2, target.BaseDelay);
        Assert.Equal(120, target.MaxDelay);
        Assert.NotNull(target.RequestHeaders);
        Assert.NotNull(target.AdditionalParameters);
        Assert.True(target.EnableLogging);
        Assert.Equal(100, target.RateLimitPerMinute);
        Assert.Equal("https://proxy.example.com", target.ProxyUrl);
        Assert.True(target.UseManagedIdentity);
        Assert.Equal("Default", target.CredentialType);
    }

    [Fact]
    public void CopyOnto_Gemini_AllDefaults_PreservesNonOverwritingTarget()
    {
        var source = new GeminiSettings(); // all defaults
        var target = new GeminiSettings
        {
            ApiKey = "machine-level-key",
            Endpoint = "https://target.example.com",
            ModelName = "target-model",
            TimeoutSeconds = 45,
            ProjectId = "target-project",
            Location = "target-location",
            MaxTokens = 2048,
            Temperature = 0.5,
            TopP = 0.8,
            StreamingEnabled = true,
            MaxRetries = 7,
            BaseDelay = 3,
            MaxDelay = 90,
            RequestHeaders = new Dictionary<string, string?> { ["X-Target"] = "val" },
            AdditionalParameters = new Dictionary<string, string?> { ["p"] = "v" },
            EnableLogging = true,
            RateLimitPerMinute = 50,
            ProxyUrl = "https://target-proxy.example.com",
            UseManagedIdentity = true,
            CredentialType = "ManagedIdentity",
        };

        source.CopyOnto(target);

        // ApiKey is never overlaid.
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://target.example.com", target.Endpoint);
        Assert.Equal("target-model", target.ModelName);
        Assert.Equal(300, target.TimeoutSeconds);
        Assert.Equal("target-project", target.ProjectId);
        Assert.Equal("target-location", target.Location);
        Assert.Equal(2048, target.MaxTokens);
        Assert.Equal(0.5, target.Temperature);
        Assert.Equal(0.8, target.TopP);
        Assert.True(target.StreamingEnabled);
        Assert.Equal(3, target.MaxRetries);
        Assert.Equal(1, target.BaseDelay);
        Assert.Equal(60, target.MaxDelay);
        Assert.Empty(target.RequestHeaders!);
        Assert.Empty(target.AdditionalParameters!);
        Assert.True(target.EnableLogging);
        Assert.Equal(50, target.RateLimitPerMinute);
        Assert.Equal("https://target-proxy.example.com", target.ProxyUrl);
        Assert.True(target.UseManagedIdentity);
        Assert.Equal("ManagedIdentity", target.CredentialType);
    }

    // --- OpenAiSettings.CopyOnto --------------------------------------

    [Fact]
    public void CopyOnto_OpenAi_NullSource_ReturnsWithoutThrowing()
    {
        OpenAiSettings source = null!;
        var target = new OpenAiSettings();

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_OpenAi_NullTarget_ReturnsWithoutThrowing()
    {
        var source = new OpenAiSettings();
        OpenAiSettings target = null!;

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_OpenAi_AllPropertiesSet_OverwritesTarget()
    {
        var source = new OpenAiSettings
        {
            Endpoint = "https://override.example.com",
            ModelName = "override-model",
            TimeoutSeconds = 60,
            OrganizationId = "override-org",
            Temperature = 0.7,
        };
        var target = new OpenAiSettings { ApiKey = "machine-level-key" };

        source.CopyOnto(target);

        // ApiKey is never overlaid from per-user settings
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://override.example.com", target.Endpoint);
        Assert.Equal("override-model", target.ModelName);
        Assert.Equal(60, target.TimeoutSeconds);
        Assert.Equal("override-org", target.OrganizationId);
        Assert.Equal(0.7, target.Temperature);
    }

    [Fact]
    public void CopyOnto_OpenAi_AllDefaults_PreservesNonOverwritingTarget()
    {
        var source = new OpenAiSettings();
        var target = new OpenAiSettings
        {
            ApiKey = "machine-level-key",
            Endpoint = "https://target.example.com",
            ModelName = "target-model",
            TimeoutSeconds = 45,
            OrganizationId = "target-org",
            MaxTokens = 2048,
            Temperature = 0.5,
        };

        source.CopyOnto(target);

        // TimeoutSeconds=45 is non-default, so it overwrites. Others stay.
        // ApiKey is never overlaid.
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://target.example.com", target.Endpoint);
        Assert.Equal("target-model", target.ModelName);
        Assert.Equal(300, target.TimeoutSeconds);
        Assert.Equal("target-org", target.OrganizationId);
        Assert.Equal(2048, target.MaxTokens);
        Assert.Equal(0.5, target.Temperature);
    }

    // --- DeepSeekSettings.CopyOnto ----

    [Fact]
    public void CopyOnto_DeepSeek_NullSource_ReturnsWithoutThrowing()
    {
        DeepSeekSettings source = null!;
        var target = new DeepSeekSettings();

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_DeepSeek_NullTarget_ReturnsWithoutThrowing()
    {
        var source = new DeepSeekSettings();
        DeepSeekSettings target = null!;

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_DeepSeek_AllPropertiesSet_OverwritesTarget()
    {
        var source = new DeepSeekSettings
        {
            Endpoint = "https://override.example.com",
            TimeoutSeconds = 60,
            Temperature = 0.7,
            ModelName = "override-model",
            EnableReasoning = true,
        };
        var target = new DeepSeekSettings { ApiKey = "machine-level-key" };

        source.CopyOnto(target);

        // ApiKey is never overlaid from per-user settings
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://override.example.com", target.Endpoint);
        Assert.Equal(60, target.TimeoutSeconds);
        Assert.Equal(0.7, target.Temperature);
        Assert.Equal("override-model", target.ModelName);
        Assert.True(target.EnableReasoning);
    }

    [Fact]
    public void CopyOnto_DeepSeek_AllDefaults_PreservesNonOverwritingTarget()
    {
        var source = new DeepSeekSettings();
        var target = new DeepSeekSettings
        {
            ApiKey = "machine-level-key",
            Endpoint = "https://target.example.com",
            TimeoutSeconds = 45,
            MaxTokens = 2048,
            Temperature = 0.5,
            ModelName = "target-model",
            EnableReasoning = true,
        };

        source.CopyOnto(target);

        // TimeoutSeconds=45 is non-default, so it overwrites. Others stay.
        // ApiKey is never overlaid.
        Assert.Equal("machine-level-key", target.ApiKey);
        Assert.Equal("https://target.example.com", target.Endpoint);
        Assert.Equal(300, target.TimeoutSeconds);
        Assert.Equal(2048, target.MaxTokens);
        Assert.Equal(0.5, target.Temperature);
        Assert.Equal("target-model", target.ModelName);
        Assert.True(target.EnableReasoning);
    }
}
