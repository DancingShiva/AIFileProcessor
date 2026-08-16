namespace AIFileProcessor.Core.Tests.Response;

using System;
using System.Collections.Generic;
using System.Text.Json;
using AIFileProcessor.Response;

[Trait("Category", "Unit")]
public class AiApiResponseTests
{
    // ---------- Parameterless constructor tests ----------

    [Fact]
    public void ParameterlessConstructor_CreatesInstanceWithDefaultValues()
    {
        var response = new AiApiResponse();

        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.Scratchpad);
        Assert.NotNull(response.Corrections);
        Assert.Empty(response.Corrections);
        Assert.NotNull(response.ProcessedSentenceRows);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    // ---------- Constructor with JSON string tests ----------

    [Fact]
    public void Constructor_ValidJsonString_PopulatesProperties()
    {
        string json = "{\"SP\":\"scratch\",\"Corrections\":[\"c1\",\"c2\"],\"Sentences\":[]}";

        var response = new AiApiResponse(json);

        Assert.Equal("scratch", response.Scratchpad);
        Assert.Equal(2, response.Corrections.Count);
        Assert.Equal("c1", response.Corrections[0]);
        Assert.Equal("c2", response.Corrections[1]);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    [Fact]
    public void Constructor_NullJsonString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AiApiResponse(null!));
    }

    [Fact]
    public void Constructor_EmptyJsonString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AiApiResponse(string.Empty));
    }

    [Fact]
    public void Constructor_WhitespaceJsonString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AiApiResponse("   "));
    }

    [Fact]
    public void Constructor_InvalidJsonString_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => new AiApiResponse("not valid json at all!!!"));
    }

    [Fact]
    public void Constructor_ValidJsonStringWithCustomOptions_PopulatesProperties()
    {
        string json = "{\"SP\":\"scratch\"}";
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        var response = new AiApiResponse(json, options);

        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void Constructor_JsonWithMarkdownCodeBlock_ParsesSuccessfully()
    {
        string json = "```json\n{\"SP\":\"scratch\"}\n```";

        var response = new AiApiResponse(json);

        Assert.Equal("scratch", response.Scratchpad);
    }

    // ---------- FillFromJsonResponse tests ----------

    [Fact]
    public void FillFromJsonResponse_NullResponse_ReturnsFalse()
    {
        var response = new AiApiResponse();

        bool result = response.FillFromJsonResponse(null!);

        Assert.False(result);
    }

    [Fact]
    public void FillFromJsonResponse_EmptyResponse_ReturnsFalse()
    {
        var response = new AiApiResponse();

        bool result = response.FillFromJsonResponse(string.Empty);

        Assert.False(result);
    }

    [Fact]
    public void FillFromJsonResponse_WhitespaceResponse_ReturnsFalse()
    {
        var response = new AiApiResponse();

        bool result = response.FillFromJsonResponse("   ");

        Assert.False(result);
    }

    [Fact]
    public void FillFromJsonResponse_ValidJson_ReturnsTrueAndPopulatesProperties()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":[\"c1\"],\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.Single(response.Corrections);
        Assert.Equal("c1", response.Corrections[0]);
    }

    [Fact]
    public void FillFromJsonResponse_JsonMarkdownBlock_ReturnsTrueAndPopulatesProperties()
    {
        var response = new AiApiResponse();
        string json = "Here is the result:\n```json\n{\"SP\":\"scratch\"}\n```\nDone.";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonMarkdownBlockCaseInsensitive_ReturnsTrueAndPopulatesProperties()
    {
        var response = new AiApiResponse();
        string json = "```JSON\n{\"SP\":\"scratch\"}\n```";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonMarkdownBlockWithoutClosingFence_StillParsesViaBraceFallback()
    {
        var response = new AiApiResponse();
        // Opening ```json but no closing ```, so the brace fallback should kick in after trim
        string json = "```json\n{\"SP\":\"scratch\"}";

        bool result = response.FillFromJsonResponse(json);

        // After extracting from ```json, no closing fence found, so response stays as-is.
        // Then the brace fallback should extract the JSON between curly braces.
        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_GeneralCodeBlock_ReturnsTrueAndPopulatesProperties()
    {
        var response = new AiApiResponse();
        string json = "```\n{\"SP\":\"scratch\"}\n```";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_GeneralCodeBlockCaseInsensitive_ReturnsTrueAndPopulatesProperties()
    {
        var response = new AiApiResponse();
        // Use uppercase backtick block marker (backticks are case-neutral but test the path)
        string json = "```\r\n{\"SP\":\"scratch\"}\r\n```";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_GeneralCodeBlockWithoutClosingFence_StillParsesViaBraceFallback()
    {
        var response = new AiApiResponse();
        string json = "```\n{\"SP\":\"scratch\"}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithPrefixAndSuffix_ExtractsViaBraceFallback()
    {
        var response = new AiApiResponse();
        string json = "Here is the data: {\"SP\":\"scratch\"} hope this helps!";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_InvalidJson_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "this is definitely not json!!!";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_InvalidJsonWithBraces_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "{not valid json}";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithCustomOptions_UsesProvidedOptions()
    {
        var response = new AiApiResponse();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        string json = "{\"SP\":\"scratch\"}";

        bool result = response.FillFromJsonResponse(json, options);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithProcessedSentenceRows_PopulatesRows()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Sentences\":[{\"S\":\"original\",\"C\":\"corrected\"}]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
    }

    [Fact]
    public void FillFromJsonResponse_InvalidJson_ThrowsJsonExceptionAndPreservesExistingProperties()
    {
        var response = new AiApiResponse();
        response.Scratchpad = "existing";
        response.Corrections = new List<string> { "existing1" };

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse("not json!!!"));
        // Properties should remain as they were (not overwritten since parse threw before assignment)
        Assert.Equal("existing", response.Scratchpad);
        Assert.Single(response.Corrections);
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithLeadingTrailingWhitespace_ReturnsTrue()
    {
        var response = new AiApiResponse();
        string json = "   {\"SP\":\"scratch\"}   ";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_EmptyJsonObject_ReturnsTrue()
    {
        var response = new AiApiResponse();
        string json = "{}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal(string.Empty, response.Scratchpad);
        Assert.Empty(response.Corrections);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    [Fact]
    public void FillFromJsonResponse_GeneralCodeBlockWithoutClosingFenceAndNoBraces_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        // General code block start but no closing fence and no curly braces
        string json = "``` some text without json";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_PrefixSuffixWithNoBraces_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "no braces here at all";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_JsonMarkdownBlockWithMultipleSentences_PopulatesAllRows()
    {
        var response = new AiApiResponse();
        string json = "```json\n{\"SP\":\"sp\",\"Sentences\":[{\"S\":\"s1\",\"C\":\"c1\"},{\"S\":\"s2\",\"C\":\"c2\"}]}\n```";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("sp", response.Scratchpad);
        Assert.Equal(2, response.ProcessedSentenceRows.Count);
    }
}