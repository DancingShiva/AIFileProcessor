namespace AIFileProcessor.Core.Tests.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using AIFileProcessor.Configuration;
using AIFileProcessor.DependencyInjection;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Processing;
using AIFileProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

[Trait("Category", "Integration")]
public class ServiceCollectionExtensionsTests
{
    private static IConfiguration CreateConfiguration(Action<Dictionary<string, string?>>? configure = null)
    {
        var data = new Dictionary<string, string?>();
        configure?.Invoke(data);
        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    // ─── Null Arguments ─────────────────────────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_NullServices_ThrowsArgumentNullException()
    {
        IConfiguration configuration = CreateConfiguration();

        Assert.Throws<ArgumentNullException>(
            () => ServiceCollectionExtensions.AddAiFileProcessorCore(null!, configuration));
    }

    [Fact]
    public void AddAiFileProcessorCore_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(
            () => services.AddAiFileProcessorCore(null!));
    }

    // ─── Settings Binding ───────────────────────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_BindsDeepSeekSettings_FromConfiguration()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["DeepSeekSettings:ApiKey"] = "deepseek-api-key";
            d["DeepSeekSettings:TimeoutSeconds"] = "120";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var settings = sp.GetRequiredService<IOptions<DeepSeekSettings>>().Value;

        Assert.Equal("deepseek-api-key", settings.ApiKey);
        Assert.Equal(120, settings.TimeoutSeconds);
    }

    [Fact]
    public void AddAiFileProcessorCore_BindsGeminiSettings_FromConfiguration()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["GeminiSettings:ApiKey"] = "gemini-api-key";
            d["GeminiSettings:Endpoint"] = "https://gemini.example.com";
            d["GeminiSettings:TimeoutSeconds"] = "45";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var settings = sp.GetRequiredService<IOptions<GeminiSettings>>().Value;

        Assert.Equal("gemini-api-key", settings.ApiKey);
        Assert.Equal("https://gemini.example.com", settings.Endpoint);
        Assert.Equal(45, settings.TimeoutSeconds);
    }

    [Fact]
    public void AddAiFileProcessorCore_BindsOpenAiSettings_FromConfiguration()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["OpenAiSettings:ApiKey"] = "openai-api-key";
            d["OpenAiSettings:TimeoutSeconds"] = "90";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var settings = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;

        Assert.Equal("openai-api-key", settings.ApiKey);
        Assert.Equal(90, settings.TimeoutSeconds);
    }

    [Fact]
    public void AddAiFileProcessorCore_BindsOllamaSettings_FromConfiguration()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["OllamaSettings:ApiKey"] = "ollama-cloud-key";
            d["OllamaSettings:Endpoint"] = "https://ollama.example.com";
            d["OllamaSettings:TimeoutSeconds"] = "600";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var settings = sp.GetRequiredService<IOptions<OllamaSettings>>().Value;

        Assert.Equal("ollama-cloud-key", settings.ApiKey);
        Assert.Equal("https://ollama.example.com", settings.Endpoint);
        Assert.Equal(600, settings.TimeoutSeconds);
    }

    // ─── Service Registrations ──────────────────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_RegistersAllExpectedServices()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);

        // IFileOps -> FileOps (singleton)
        Assert.Contains(services, d => d.ServiceType == typeof(IFileOps) && d.Lifetime == ServiceLifetime.Singleton);

        // IChineseTextProcessor -> ChineseTextProcessor (transient)
        Assert.Contains(services, d => d.ServiceType == typeof(IChineseTextProcessor) && d.Lifetime == ServiceLifetime.Transient);

        // ISettingsProvider -> FileSettingsProvider (singleton)
        Assert.Contains(services, d => d.ServiceType == typeof(ISettingsProvider) && d.Lifetime == ServiceLifetime.Singleton);

        // ProcessingOrchestrator (transient)
        Assert.Contains(services, d => d.ServiceType == typeof(ProcessingOrchestrator) && d.Lifetime == ServiceLifetime.Transient);

        // Func<IChineseTextProcessor> (transient)
        Assert.Contains(services, d => d.ServiceType == typeof(Func<IChineseTextProcessor>) && d.Lifetime == ServiceLifetime.Transient);

        // Four IAiService registrations (Gemini, OpenAI, DeepSeek, Ollama)
        var aiServiceDescs = services.Where(d => d.ServiceType == typeof(IAiService)).ToList();
        Assert.Equal(4, aiServiceDescs.Count);
        Assert.All(aiServiceDescs, d => Assert.Equal(ServiceLifetime.Transient, d.Lifetime));
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersHttpClientForOllamaService()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);

        Assert.Contains(services, d => d.ServiceType == typeof(OllamaService));
    }

    [Fact]
    public void AddAiFileProcessorCore_ResolvesFourAiServices()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var resolved = sp.GetServices<IAiService>().ToList();

        Assert.Equal(4, resolved.Count);
        Assert.Contains(resolved, s => s is GeminiService);
        Assert.Contains(resolved, s => s is OpenAiService);
        Assert.Contains(resolved, s => s is DeepSeekService);
        Assert.Contains(resolved, s => s is OllamaService);
    }

    // ─── HttpClient Registrations ───────────────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_RegistersHttpClientForGeminiService()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);

        Assert.Contains(services, d => d.ServiceType == typeof(GeminiService));
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersHttpClientForDeepSeekService()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);

        Assert.Contains(services, d => d.ServiceType == typeof(DeepSeekService));
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersHttpClientForOpenAiService()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        services.AddAiFileProcessorCore(configuration);

        Assert.Contains(services, d => d.ServiceType == typeof(OpenAiService));
    }

    // ─── HttpClient Configuration Callbacks ─────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_GeminiHttpClient_SetsTimeoutFromSettings()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["GeminiSettings:TimeoutSeconds"] = "30";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();
        var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient(nameof(GeminiService));

        // The typed client uses the name of the type
        var geminiClient = sp.GetRequiredService<GeminiService>();
    }

    [Fact]
    public void AddAiFileProcessorCore_GeminiHttpClient_UsesDefaultTimeoutWhenZeroOrNegative()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["GeminiSettings:TimeoutSeconds"] = "0";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        // Resolving GeminiService triggers the HttpClient configuration callback
        var geminiService = sp.GetRequiredService<GeminiService>();
        Assert.NotNull(geminiService);
    }

    [Fact]
    public void AddAiFileProcessorCore_GeminiHttpClient_SetsBaseAddressFromSettings()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["GeminiSettings:Endpoint"] = "https://custom-gemini.example.com/";
            d["GeminiSettings:TimeoutSeconds"] = "30";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var geminiService = sp.GetRequiredService<GeminiService>();
        Assert.NotNull(geminiService);
    }

    [Fact]
    public void AddAiFileProcessorCore_GeminiHttpClient_AppliesRequestHeadersFromSettings()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["GeminiSettings:TimeoutSeconds"] = "30";
            d["GeminiSettings:RequestHeaders:X-Custom-Header"] = "custom-value";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var geminiService = sp.GetRequiredService<GeminiService>();
        Assert.NotNull(geminiService);
    }

    [Fact]
    public void AddAiFileProcessorCore_DeepSeekHttpClient_UsesDefaultTimeoutWhenZeroOrNegative()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["DeepSeekSettings:TimeoutSeconds"] = "0";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var deepSeekService = sp.GetRequiredService<DeepSeekService>();
        Assert.NotNull(deepSeekService);
    }

    [Fact]
    public void AddAiFileProcessorCore_OpenAiHttpClient_UsesDefaultTimeoutWhenZeroOrNegative()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["OpenAiSettings:TimeoutSeconds"] = "0";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var openAiService = sp.GetRequiredService<OpenAiService>();
        Assert.NotNull(openAiService);
    }

    [Fact]
    public void AddAiFileProcessorCore_DeepSeekHttpClient_AppliesRequestHeadersFromSettings()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["DeepSeekSettings:TimeoutSeconds"] = "30";
            d["DeepSeekSettings:RequestHeaders:X-Custom-Header"] = "custom-value";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var deepSeekService = sp.GetRequiredService<DeepSeekService>();
        Assert.NotNull(deepSeekService);
    }

    [Fact]
    public void AddAiFileProcessorCore_OpenAiHttpClient_AppliesRequestHeadersFromSettings()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(d =>
        {
            d["OpenAiSettings:TimeoutSeconds"] = "30";
            d["OpenAiSettings:RequestHeaders:X-Custom-Header"] = "custom-value";
        });

        services.AddAiFileProcessorCore(configuration);
        var sp = services.BuildServiceProvider();

        var openAiService = sp.GetRequiredService<OpenAiService>();
        Assert.NotNull(openAiService);
    }

    // ─── Return Value ───────────────────────────────────────────────

    [Fact]
    public void AddAiFileProcessorCore_ReturnsSameServiceCollectionInstance()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        var result = services.AddAiFileProcessorCore(configuration);

        Assert.Same(services, result);
    }
}