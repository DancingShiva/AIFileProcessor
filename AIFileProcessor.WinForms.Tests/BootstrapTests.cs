//-----------------------------------------------------------------------
// <copyright file="BootstrapTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.WinForms.Tests;

using System;
using AIFileProcessor.Configuration;
using AIFileProcessor.DependencyInjection;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Processing;
using AIFileProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

[Trait("Category", "WinForms")]
public class BootstrapTests
{
    private static IServiceProvider BuildServiceProvider()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GeminiSettings:ApiKey"] = "test-key",
            ["GeminiSettings:Endpoint"] = "https://generativelanguage.googleapis.com",
            ["OpenAiSettings:ApiKey"] = "test-key",
            ["OpenAiSettings:Endpoint"] = "https://api.openai.com/v1",
            ["DeepSeekSettings:ApiKey"] = "test-key",
            ["DeepSeekSettings:Endpoint"] = "https://api.deepseek.com",
        });
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddAiFileProcessorCore(builder.Configuration);
        builder.Services.AddSingleton<IUserContext, WindowsUserContext>();
        builder.Services.AddSingleton<IDialogService, MessageBoxDialogService>();

        return builder.Build().Services;
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersProcessingOrchestrator()
    {
        var sp = BuildServiceProvider();

        Assert.NotNull(sp.GetRequiredService<ProcessingOrchestrator>());
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersIFileOps()
    {
        var sp = BuildServiceProvider();

        Assert.NotNull(sp.GetRequiredService<IFileOps>());
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersISettingsProvider()
    {
        var sp = BuildServiceProvider();

        Assert.NotNull(sp.GetRequiredService<ISettingsProvider>());
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersIAiServiceCollection()
    {
        var sp = BuildServiceProvider();

        var services = sp.GetRequiredService<IEnumerable<IAiService>>();
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersIUserContext_AsWindowsUserContext()
    {
        var sp = BuildServiceProvider();

        var userContext = sp.GetRequiredService<IUserContext>();
        Assert.IsType<WindowsUserContext>(userContext);
    }

    [Fact]
    public void AddAiFileProcessorCore_RegistersIDialogService_AsMessageBoxDialogService()
    {
        var sp = BuildServiceProvider();

        var dialogService = sp.GetRequiredService<IDialogService>();
        Assert.IsType<MessageBoxDialogService>(dialogService);
    }
}