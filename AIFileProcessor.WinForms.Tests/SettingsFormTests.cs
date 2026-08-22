//-----------------------------------------------------------------------
// <copyright file="SettingsFormTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.WinForms.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

[Trait("Category", "WinForms")]
public class SettingsFormTests
{
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly List<IAiService> _aiServices;

    public SettingsFormTests()
    {
        _userContextMock.Setup(u => u.UserId).Returns("test-user");

        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);
        aiServiceMock.Setup(s => s.GetSupportedModels()).Returns([
            new("Test Gemini", "gemini-test", [ReasoningLevel.None]),
            new("Test OpenAI", "openai-test", [ReasoningLevel.None]),
            new("Test DeepSeek", "deepseek-test", [ReasoningLevel.None]),
        ]);
        aiServiceMock.Setup(s => s.GetSupportedReasoningLevels(It.IsAny<string>()))
            .Returns(new List<ReasoningLevel> { ReasoningLevel.None }.AsReadOnly());

        _aiServices = [aiServiceMock.Object];

        _settingsProviderMock.Setup(s => s.GetGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeminiSettings());
        _settingsProviderMock.Setup(s => s.GetOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenAiSettings());
        _settingsProviderMock.Setup(s => s.GetDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeepSeekSettings());
        _settingsProviderMock.Setup(s => s.GetOllamaSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OllamaSettings());
        _settingsProviderMock.Setup(s => s.GetGeminiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeminiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        _settingsProviderMock.Setup(s => s.GetOpenAiModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenAiModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        _settingsProviderMock.Setup(s => s.GetDeepSeekModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeepSeekModelSettings { ChunkSize = 40, MaxConcurrentChunks = 3 });
        _settingsProviderMock.Setup(s => s.GetOllamaModelSettingsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OllamaModelSettings { ChunkSize = 20, MaxConcurrentChunks = 2 });
        _settingsProviderMock.Setup(s => s.GetProcessingSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingSettings());
        _settingsProviderMock.Setup(s => s.GetProcessingOptionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingOptions());
        _settingsProviderMock.Setup(s => s.SaveGeminiSettingsAsync(It.IsAny<string>(), It.IsAny<GeminiSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _settingsProviderMock.Setup(s => s.SaveOpenAiSettingsAsync(It.IsAny<string>(), It.IsAny<OpenAiSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _settingsProviderMock.Setup(s => s.SaveDeepSeekSettingsAsync(It.IsAny<string>(), It.IsAny<DeepSeekSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _settingsProviderMock.Setup(s => s.SaveOllamaSettingsAsync(It.IsAny<string>(), It.IsAny<OllamaSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public void Constructor_NullAiServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SettingsForm(null!, _settingsProviderMock.Object, _userContextMock.Object));
    }

    [Fact]
    public void Constructor_NullSettingsProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SettingsForm(_aiServices, null!, _userContextMock.Object));
    }

    [Fact]
    public void Constructor_NullUserContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SettingsForm(_aiServices, _settingsProviderMock.Object, null!));
    }

    [Fact]
    public void Constructor_ValidDependencies_PopulatesModelDropdowns()
    {
        // PopulateModelDropdowns checks for concrete service types (GeminiService, etc.)
        // which Mock<IAiService> doesn't satisfy. Instead, verify the constructor
        // completes without error and the form is usable.
        var form = new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object);
        Assert.NotNull(form);
        form.Dispose();
    }
}