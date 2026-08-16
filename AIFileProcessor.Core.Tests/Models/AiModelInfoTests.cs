using AIFileProcessor.Models;

namespace AIFileProcessor.Core.Tests.Models;

[Trait("Category", "Unit")]
public class AiModelInfoTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsModelName()
    {
        // Arrange
        const string modelName = "GPT-4";
        const string modelId = "gpt-4";

        // Act
        var info = new AiModelInfo(modelName, modelId);

        // Assert
        Assert.Equal(modelName, info.ModelName);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsModelId()
    {
        // Arrange
        const string modelName = "GPT-4";
        const string modelId = "gpt-4";

        // Act
        var info = new AiModelInfo(modelName, modelId);

        // Assert
        Assert.Equal(modelId, info.ModelId);
    }

    [Fact]
    public void ModelName_SetNewValue_UpdatesProperty()
    {
        // Arrange
        var info = new AiModelInfo("GPT-4", "gpt-4");

        // Act
        info.ModelName = "Claude-3";

        // Assert
        Assert.Equal("Claude-3", info.ModelName);
    }

    [Fact]
    public void ModelId_SetNewValue_UpdatesProperty()
    {
        // Arrange
        var info = new AiModelInfo("GPT-4", "gpt-4");

        // Act
        info.ModelId = "claude-3";

        // Assert
        Assert.Equal("claude-3", info.ModelId);
    }
}