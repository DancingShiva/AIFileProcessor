namespace AIFileProcessor.Core.Tests.Processing;

using AIFileProcessor.Models;
using AIFileProcessor.Processing;

public class ProcessingOptionsValidatorTests
{
    private static ProcessingOptions CreateValidOptions() => new()
    {
        IdPrefix = "ID",
        IdStart = 1,
        IdPad = 3,
        LengthPad = 3,
        Prompt = "Some prompt",
        Data = "Some data",
    };

    [Fact]
    public void Validate_AllValid_NoErrors()
    {
        var options = CreateValidOptions();

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_SelectedModelIdNull_AddsModelError()
    {
        var options = CreateValidOptions();

        var errors = ProcessingOptionsValidator.Validate(options, null);

        Assert.Contains("Please select a model before processing.", errors);
    }

    [Fact]
    public void Validate_SelectedModelIdEmpty_AddsModelError()
    {
        var options = CreateValidOptions();

        var errors = ProcessingOptionsValidator.Validate(options, string.Empty);

        Assert.Contains("Please select a model before processing.", errors);
    }

    [Fact]
    public void Validate_SelectedModelIdWhitespace_AddsModelError()
    {
        var options = CreateValidOptions();

        var errors = ProcessingOptionsValidator.Validate(options, "   ");

        Assert.Contains("Please select a model before processing.", errors);
    }

    [Fact]
    public void Validate_SelectedModelIdNotProvided_AddsModelError()
    {
        var options = CreateValidOptions();

        var errors = ProcessingOptionsValidator.Validate(options);

        Assert.Contains("Please select a model before processing.", errors);
    }

    [Fact]
    public void Validate_IdPrefixNull_AddsPrefixError()
    {
        var options = CreateValidOptions();
        options.IdPrefix = null!;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please enter a prefix before processing.", errors);
    }

    [Fact]
    public void Validate_IdPrefixEmpty_AddsPrefixError()
    {
        var options = CreateValidOptions();
        options.IdPrefix = string.Empty;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please enter a prefix before processing.", errors);
    }

    [Fact]
    public void Validate_IdPrefixWhitespace_AddsPrefixError()
    {
        var options = CreateValidOptions();
        options.IdPrefix = "   ";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please enter a prefix before processing.", errors);
    }

    [Fact]
    public void Validate_IdStartZero_AddsStartError()
    {
        var options = CreateValidOptions();
        options.IdStart = 0;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The starting index must be a positive numeric value.", errors);
    }

    [Fact]
    public void Validate_IdStartNegative_AddsStartError()
    {
        var options = CreateValidOptions();
        options.IdStart = -5;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The starting index must be a positive numeric value.", errors);
    }

    [Fact]
    public void Validate_IdPadZero_AddsIdPadError()
    {
        var options = CreateValidOptions();
        options.IdPad = 0;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The value for the Id padding must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_IdPadNegative_AddsIdPadError()
    {
        var options = CreateValidOptions();
        options.IdPad = -1;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The value for the Id padding must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_LengthPadZero_AddsLengthPadError()
    {
        var options = CreateValidOptions();
        options.LengthPad = 0;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The value for the length padding must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_LengthPadNegative_AddsLengthPadError()
    {
        var options = CreateValidOptions();
        options.LengthPad = -2;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The value for the length padding must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_PromptNull_AddsPromptError()
    {
        var options = CreateValidOptions();
        options.Prompt = null!;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a prompt file before processing.", errors);
    }

    [Fact]
    public void Validate_PromptEmpty_AddsPromptError()
    {
        var options = CreateValidOptions();
        options.Prompt = string.Empty;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a prompt file before processing.", errors);
    }

    [Fact]
    public void Validate_PromptWhitespace_AddsPromptError()
    {
        var options = CreateValidOptions();
        options.Prompt = "   ";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a prompt file before processing.", errors);
    }

    [Fact]
    public void Validate_DataNull_AddsDataError()
    {
        var options = CreateValidOptions();
        options.Data = null!;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a data file before processing.", errors);
    }

    [Fact]
    public void Validate_DataEmpty_AddsDataError()
    {
        var options = CreateValidOptions();
        options.Data = string.Empty;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a data file before processing.", errors);
    }

    [Fact]
    public void Validate_DataWhitespace_AddsDataError()
    {
        var options = CreateValidOptions();
        options.Data = "   ";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a data file before processing.", errors);
    }

    [Fact]
    public void Validate_AllInvalid_AddsAllErrors()
    {
        var options = new ProcessingOptions
        {
            IdPrefix = string.Empty,
            IdStart = 0,
            IdPad = 0,
            LengthPad = 0,
            Prompt = string.Empty,
            Data = string.Empty,
        };

        var errors = ProcessingOptionsValidator.Validate(options, null);

        Assert.Equal(7, errors.Count);
        Assert.Contains("Please select a model before processing.", errors);
        Assert.Contains("Please enter a prefix before processing.", errors);
        Assert.Contains("The starting index must be a positive numeric value.", errors);
        Assert.Contains("The value for the Id padding must be a positive integer.", errors);
        Assert.Contains("The value for the length padding must be a positive integer.", errors);
        Assert.Contains("Please load a prompt file before processing.", errors);
        Assert.Contains("Please load a data file before processing.", errors);
    }
}