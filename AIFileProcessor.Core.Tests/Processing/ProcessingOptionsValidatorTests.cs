//-----------------------------------------------------------------------
// <copyright file="ProcessingOptionsValidatorTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Processing;

using AIFileProcessor.Models;
using AIFileProcessor.Processing;

[Trait("Category", "Unit")]
public class ProcessingOptionsValidatorTests
{
    private static ProcessingOptions CreateValidOptions() => new()
    {
        IdPrefix = "ID",
        IdStart = 1,
        IdPad = 3,
        LengthPad = 3,
        Prompt = "Some prompt",
        Data = "我在超市买菜。",
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
    public void Validate_DataWithNoChinese_AddsNoChineseContentError()
    {
        var options = CreateValidOptions();
        options.Data = "Hello world 12345";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The data file contains no Chinese text to process.", errors);
    }

    [Fact]
    public void Validate_DataOnlyPunctuation_AddsNoChineseContentError()
    {
        var options = CreateValidOptions();
        options.Data = "。，！？【】";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The data file contains no Chinese text to process.", errors);
    }

    [Fact]
    public void Validate_DataWithChineseMixedContent_NoChineseContentError()
    {
        var options = CreateValidOptions();
        options.Data = "Line one with English\n我在超市买菜。";

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.DoesNotContain("The data file contains no Chinese text to process.", errors);
    }

    [Fact]
    public void Validate_DataNull_AddsLoadDataErrorOnly_NoChineseContentError()
    {
        var options = CreateValidOptions();
        options.Data = null!;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("Please load a data file before processing.", errors);
        Assert.DoesNotContain("The data file contains no Chinese text to process.", errors);
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

    private static readonly string[] ExpectedAllInvalidErrors =
    [
        "Please select a model before processing.",
        "Please enter a prefix before processing.",
        "The starting index must be a positive numeric value.",
        "The value for the Id padding must be a positive integer.",
        "The value for the length padding must be a positive integer.",
        "Please load a prompt file before processing.",
        "Please load a data file before processing.",
    ];

    [Fact]
    public void Validate_AllInvalid_ErrorsInFieldOrder()
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

        // Errors are returned in field/UI order so the UI can present them in one pass.
        Assert.Equal(ExpectedAllInvalidErrors, errors);
    }

    // --- MaxInputLines validation ---

    [Fact]
    public void Validate_DataExceedsMaxInputLines_AddsLineCountError()
    {
        var options = CreateValidOptions();
        // Build data with 401 lines of Chinese text
        options.Data = string.Join("\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 401));

        var errors = ProcessingOptionsValidator.Validate(options, "model-1", maxInputLines: 400);

        Assert.Contains(errors, e => e.Contains("maximum allowed line count"));
    }

    [Fact]
    public void Validate_DataAtMaxInputLines_NoLineCountError()
    {
        var options = CreateValidOptions();
        // Build data with exactly 400 lines of Chinese text
        options.Data = string.Join("\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 400));

        var errors = ProcessingOptionsValidator.Validate(options, "model-1", maxInputLines: 400);

        Assert.DoesNotContain(errors, e => e.Contains("maximum allowed line count"));
    }

    [Fact]
    public void Validate_DataWithCRLFLineEndings_CountsLinesCorrectly()
    {
        var options = CreateValidOptions();
        // Build data with 401 lines using \r\n separators
        options.Data = string.Join("\r\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 401));

        var errors = ProcessingOptionsValidator.Validate(options, "model-1", maxInputLines: 400);

        Assert.Contains(errors, e => e.Contains("maximum allowed line count"));
    }

    [Fact]
    public void Validate_DefaultMaxInputLines_400LinesRejected()
    {
        var options = CreateValidOptions();
        options.Data = string.Join("\n", System.Linq.Enumerable.Repeat("我在超市买菜。", 401));

        // Use default parameters (no explicit maxInputLines)
        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains(errors, e => e.Contains("maximum allowed line count of 400"));
    }

    // --- Chunking validation ---

    [Fact]
    public void Validate_ChunkSizeZero_AddsChunkSizeError()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = 0, MaxConcurrentChunks = 3 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The chunk size must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_ChunkSizeNegative_AddsChunkSizeError()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = -1, MaxConcurrentChunks = 3 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The chunk size must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_MaxConcurrentChunksZero_AddsConcurrencyError()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 0 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The maximum concurrent chunks must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_MaxConcurrentChunksNegative_AddsConcurrencyError()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = -1 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Contains("The maximum concurrent chunks must be a positive integer.", errors);
    }

    [Fact]
    public void Validate_DefaultChunking_NoErrors()
    {
        var options = CreateValidOptions();
        // Default ChunkingOptions: ChunkSize=40, MaxConcurrentChunks=3

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.DoesNotContain(errors, e => e.Contains("chunk size"));
        Assert.DoesNotContain(errors, e => e.Contains("concurrent chunks"));
    }

    // --- Boundary values: minimum valid value (1) for numeric fields ---

    public static TheoryData<string, int> BoundaryValidValues => new()
    {
        { "IdStart", 1 },
        { "IdPad", 1 },
        { "LengthPad", 1 },
    };

    [Theory]
    [MemberData(nameof(BoundaryValidValues))]
    public void Validate_MinimumValidValue_NoErrors(string fieldName, int value)
    {
        var options = CreateValidOptions();
        switch (fieldName)
        {
            case "IdStart": options.IdStart = value; break;
            case "IdPad": options.IdPad = value; break;
            case "LengthPad": options.LengthPad = value; break;
        }

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IdStartLargeValue_NoErrors()
    {
        var options = CreateValidOptions();
        options.IdStart = int.MaxValue;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IdPadLargeValue_NoErrors()
    {
        var options = CreateValidOptions();
        options.IdPad = int.MaxValue;

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.Empty(errors);
    }

    // --- Boundary values: ChunkSize and MaxConcurrentChunks minimum valid (1) ---

    [Fact]
    public void Validate_ChunkSizeOne_NoErrors()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = 1, MaxConcurrentChunks = 3 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.DoesNotContain(errors, e => e.Contains("chunk size"));
    }

    [Fact]
    public void Validate_MaxConcurrentChunksOne_NoErrors()
    {
        var options = CreateValidOptions();
        options.Chunking = new ChunkingOptions { ChunkSize = 40, MaxConcurrentChunks = 1 };

        var errors = ProcessingOptionsValidator.Validate(options, "model-1");

        Assert.DoesNotContain(errors, e => e.Contains("concurrent chunks"));
    }

    // --- maxInputSize boundary tests ---

    [Fact]
    public void Validate_DataAtMaxInputSize_NoSizeError()
    {
        var options = CreateValidOptions();
        // Build data with exactly maxInputSize characters (all Chinese)
        options.Data = new string('我', 1000);

        var errors = ProcessingOptionsValidator.Validate(options, "model-1", maxInputSize: 1000);

        Assert.DoesNotContain(errors, e => e.Contains("maximum allowed size"));
    }

    [Fact]
    public void Validate_DataOneCharOverMaxInputSize_AddsSizeError()
    {
        var options = CreateValidOptions();
        // Build data with one character over the limit
        options.Data = new string('我', 1001);

        var errors = ProcessingOptionsValidator.Validate(options, "model-1", maxInputSize: 1000);

        Assert.Contains(errors, e => e.Contains("maximum allowed size of 1,000 characters"));
    }
}