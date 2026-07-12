using AIFileProcessor.Models;

namespace AIFileProcessor.Core.Tests.Models;

public class ProcessingOptionsTests
{
    [Fact]
    public void Constructor_DefaultValues_PromptPathIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.PromptPath);
    }

    [Fact]
    public void Constructor_DefaultValues_PromptIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.Prompt);
    }

    [Fact]
    public void Constructor_DefaultValues_DataPathIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.DataPath);
    }

    [Fact]
    public void Constructor_DefaultValues_DataIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.Data);
    }

    [Fact]
    public void Constructor_DefaultValues_ResponseIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.Response);
    }

    [Fact]
    public void Constructor_DefaultValues_ResponsePathIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.ResponsePath);
    }

    [Fact]
    public void Constructor_DefaultValues_ModelIdIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.ModelId);
    }

    [Fact]
    public void Constructor_DefaultValues_IdPrefixIsEmptyString()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(string.Empty, options.IdPrefix);
    }

    [Fact]
    public void Constructor_DefaultValues_IdStartIsOne()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(1, options.IdStart);
    }

    [Fact]
    public void Constructor_DefaultValues_IdPadIsThree()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(3, options.IdPad);
    }

    [Fact]
    public void Constructor_DefaultValues_LengthPadIsThree()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        Assert.Equal(3, options.LengthPad);
    }

    [Fact]
    public void Data_SetValue_ReturnsSameValue()
    {
        // Arrange
        var options = new ProcessingOptions();
        const string data = "some data content";

        // Act
        options.Data = data;

        // Assert
        Assert.Equal(data, options.Data);
    }

    [Fact]
    public void Data_SetValue_ClearsResponse()
    {
        // Arrange
        var options = new ProcessingOptions { Response = "previous response" };

        // Act
        options.Data = "new data";

        // Assert
        Assert.Equal(string.Empty, options.Response);
    }

    [Fact]
    public void Data_SetValue_ClearsResponsePath()
    {
        // Arrange
        var options = new ProcessingOptions { ResponsePath = "response/path.json" };

        // Act
        options.Data = "new data";

        // Assert
        Assert.Equal(string.Empty, options.ResponsePath);
    }

    [Fact]
    public void Data_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var options = new ProcessingOptions();

        // Act
        options.Data = string.Empty;

        // Assert
        Assert.Equal(string.Empty, options.Data);
    }

    [Fact]
    public void Data_SetNullValue_StoresNull()
    {
        // Arrange
        var options = new ProcessingOptions();

        // Act
        options.Data = null!;

        // Assert
        Assert.Null(options.Data);
    }

    [Fact]
    public void Response_SetValue_ReturnsSameValue()
    {
        // Arrange
        var options = new ProcessingOptions();
        const string response = "some response content";

        // Act
        options.Response = response;

        // Assert
        Assert.Equal(response, options.Response);
    }

    [Fact]
    public void Response_SetValue_ClearsData()
    {
        // Arrange
        var options = new ProcessingOptions { Data = "previous data" };

        // Act
        options.Response = "new response";

        // Assert
        Assert.Equal(string.Empty, options.Data);
    }

    [Fact]
    public void Response_SetValue_ClearsDataPath()
    {
        // Arrange
        var options = new ProcessingOptions { DataPath = "data/path.json" };

        // Act
        options.Response = "new response";

        // Assert
        Assert.Equal(string.Empty, options.DataPath);
    }

    [Fact]
    public void Response_SetEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var options = new ProcessingOptions();

        // Act
        options.Response = string.Empty;

        // Assert
        Assert.Equal(string.Empty, options.Response);
    }

    [Fact]
    public void Response_SetNullValue_StoresNull()
    {
        // Arrange
        var options = new ProcessingOptions();

        // Act
        options.Response = null!;

        // Assert
        Assert.Null(options.Response);
    }

    [Fact]
    public void Data_SetValueThenResponseSet_DataIsCleared()
    {
        // Arrange
        var options = new ProcessingOptions { Data = "initial data", DataPath = "data/path.json" };

        // Act
        options.Response = "a response";

        // Assert
        Assert.Equal(string.Empty, options.Data);
        Assert.Equal(string.Empty, options.DataPath);
        Assert.Equal("a response", options.Response);
    }

    [Fact]
    public void Response_SetValueThenDataSet_ResponseIsCleared()
    {
        // Arrange
        var options = new ProcessingOptions { Response = "initial response", ResponsePath = "response/path.json" };

        // Act
        options.Data = "new data";

        // Assert
        Assert.Equal(string.Empty, options.Response);
        Assert.Equal(string.Empty, options.ResponsePath);
        Assert.Equal("new data", options.Data);
    }
}