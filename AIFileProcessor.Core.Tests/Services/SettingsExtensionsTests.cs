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
            RequestHeaders = new Dictionary<string, string?> { ["X-Test"] = "val" },
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
        Assert.NotNull(target.RequestHeaders);
        Assert.Equal("val", target.RequestHeaders!["X-Test"]);
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
            RequestHeaders = new Dictionary<string, string?> { ["X-Test"] = "val" },
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
        Assert.NotNull(target.RequestHeaders);
        Assert.Equal("val", target.RequestHeaders!["X-Test"]);
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

    // --- OllamaSettings.CopyOnto --------------------------------------

    [Fact]
    public void CopyOnto_Ollama_NullSource_DoesNotThrow()
    {
        OllamaSettings source = null!;
        var target = new OllamaSettings();

        source.CopyOnto(target);
    }

    [Fact]
    public void CopyOnto_Ollama_AllOptionsSet_OverwritesTarget()
    {
        var source = new OllamaSettings
        {
            ApiKey = "should-not-copy",
            Endpoint = "http://other-host:11434",
            ModelName = "qwen3:14b",
            TimeoutSeconds = 900,
            Temperature = 0.8,
            TopP = 0.9,
            TopK = 50,
            ContextWindow = 32768,
            MaxTokens = 4096,
            RepeatPenalty = 1.2,
            Seed = 7,
            Stop = ["<stop>"],
            MinP = 0.1,
            TypicalP = 0.8,
            TfsZ = 1.5,
            MiroStat = 2,
            MiroStatTau = 4.0,
            MiroStatEta = 0.1,
            PresencePenalty = 0.4,
            FrequencyPenalty = 0.2,
            RepeatLastN = 128,
            NumGpu = 40,
            NumBatch = 1024,
            NumThread = 16,
            NumKeep = 8,
            Numa = true,
            UseMlock = true,
            UseMmap = false,
            LowVram = true,
            KeepAlive = "10m",
            EnableThinking = true,
            ReasoningLevel = AIFileProcessor.Models.ReasoningLevel.High,
            MaxRetries = 4,
            BaseDelay = 2,
            MaxDelay = 90,
            RequestHeaders = new Dictionary<string, string?> { ["X-Test"] = "1" },
        };

        var target = new OllamaSettings { ApiKey = "machine-key" };
        source.CopyOnto(target);

        Assert.Equal("machine-key", target.ApiKey); // never overlaid
        Assert.Equal("http://other-host:11434", target.Endpoint);
        Assert.Equal("qwen3:14b", target.ModelName);
        Assert.Equal(900, target.TimeoutSeconds);
        Assert.Equal(0.8, target.Temperature);
        Assert.Equal(0.9, target.TopP);
        Assert.Equal(50, target.TopK);
        Assert.Equal(32768, target.ContextWindow);
        Assert.Equal(4096, target.MaxTokens);
        Assert.Equal(1.2, target.RepeatPenalty);
        Assert.Equal(7, target.Seed);
        Assert.Equal(["<stop>"], target.Stop);
        Assert.Equal(0.1, target.MinP);
        Assert.Equal(0.8, target.TypicalP);
        Assert.Equal(1.5, target.TfsZ);
        Assert.Equal(2, target.MiroStat);
        Assert.Equal(4.0, target.MiroStatTau);
        Assert.Equal(0.1, target.MiroStatEta);
        Assert.Equal(0.4, target.PresencePenalty);
        Assert.Equal(0.2, target.FrequencyPenalty);
        Assert.Equal(128, target.RepeatLastN);
        Assert.Equal(40, target.NumGpu);
        Assert.Equal(1024, target.NumBatch);
        Assert.Equal(16, target.NumThread);
        Assert.Equal(8, target.NumKeep);
        Assert.True(target.Numa);
        Assert.True(target.UseMlock);
        Assert.False(target.UseMmap);
        Assert.True(target.LowVram);
        Assert.Equal("10m", target.KeepAlive);
        Assert.True(target.EnableThinking);
        Assert.Equal(AIFileProcessor.Models.ReasoningLevel.High, target.ReasoningLevel);
        Assert.Equal(4, target.MaxRetries);
        Assert.Equal(2, target.BaseDelay);
        Assert.Equal(90, target.MaxDelay);
        Assert.Equal("1", target.RequestHeaders!["X-Test"]);
    }

    [Fact]
    public void CopyOnto_Ollama_NullOptionsNotOverwritten_PreservesTargetDefaults()
    {
        var source = new OllamaSettings(); // all options null/empty
        var target = new OllamaSettings
        {
            Temperature = 0.3,
            ContextWindow = 16384,
            KeepAlive = "5m",
        };

        source.CopyOnto(target);

        Assert.Equal(0.3, target.Temperature);
        Assert.Equal(16384, target.ContextWindow);
        Assert.Equal("5m", target.KeepAlive);
        Assert.Equal(new Uri("http://localhost:11434").AbsoluteUri.TrimEnd('/'), target.Endpoint); // default kept (empty source does not overwrite)
    }
}
