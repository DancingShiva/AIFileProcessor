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

    // ---------- Edge-case: missing keys ----------

    [Fact]
    public void FillFromJsonResponse_MissingSentencesKey_ProcessedSentenceRowsStaysEmpty()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":[\"c1\"]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.NotNull(response.ProcessedSentenceRows);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    [Fact]
    public void FillFromJsonResponse_MissingSpKey_ScratchpadStaysEmpty()
    {
        var response = new AiApiResponse();
        string json = "{\"Corrections\":[\"c1\"],\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal(string.Empty, response.Scratchpad);
        Assert.Single(response.Corrections);
    }

    [Fact]
    public void FillFromJsonResponse_MissingCorrectionsKey_CorrectionsStaysEmpty()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.NotNull(response.Corrections);
        Assert.Empty(response.Corrections);
    }

    // ---------- Edge-case: null values in JSON ----------

    [Fact]
    public void FillFromJsonResponse_CorrectionsNullInJson_CorrectionsBecomesNull()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":null,\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.Null(response.Corrections);
    }

    [Fact]
    public void FillFromJsonResponse_SentencesNullInJson_ProcessedSentenceRowsBecomesNull()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":[],\"Sentences\":null}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.Null(response.ProcessedSentenceRows);
    }

    [Fact]
    public void FillFromJsonResponse_SpNullInJson_ScratchpadBecomesNull()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":null,\"Corrections\":[],\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Null(response.Scratchpad);
    }

    // ---------- Edge-case: extra unknown fields ----------

    [Fact]
    public void FillFromJsonResponse_ExtraUnknownFields_IgnoredSuccessfully()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"UnknownField\":\"value\",\"AnotherUnknown\":42,\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    // ---------- Edge-case: empty Sentences array ----------

    [Fact]
    public void FillFromJsonResponse_EmptySentencesArray_ProcessedSentenceRowsEmpty()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.NotNull(response.ProcessedSentenceRows);
        Assert.Empty(response.ProcessedSentenceRows);
    }

    // ---------- Edge-case: trailing commas ----------

    [Fact]
    public void FillFromJsonResponse_TrailingCommaInObject_ParsesSuccessfully()
    {
        var response = new AiApiResponse();
        // The serializer options allow trailing commas
        string json = "{\"SP\":\"scratch\",\"Corrections\":[\"c1\",],\"Sentences\":[],}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
        Assert.Single(response.Corrections);
        Assert.Equal("c1", response.Corrections[0]);
    }

    [Fact]
    public void FillFromJsonResponse_TrailingCommaInNestedArray_ParsesSuccessfully()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":[\"c1\",\"c2\",],\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal(2, response.Corrections.Count);
    }

    // ---------- Edge-case: Unicode escape sequences ----------

    [Fact]
    public void FillFromJsonResponse_UnicodeEscapeSequence_PopulatesScratchpad()
    {
        var response = new AiApiResponse();
        // \u4f60 is 你, \u597d is 好
        string json = "{\"SP\":\"\\u4f60\\u597d\",\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("你好", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_UnicodeEscapeInSentenceFields_PopulatesCorrectly()
    {
        var response = new AiApiResponse();
        // Use a simple sentence with Unicode escapes: \u4f60 = 你, \u597d = 好
        string json = "{\"SP\":\"sp\",\"Sentences\":[{\"C\":\"\\u4f60 \\u597d\",\"P\":\"nǐ hǎo\",\"B\":\"\\u4f60|nǐ|you|pron|1| ||\\u597d|hǎo|good|adj|1| \",\"R\":\"N/A\",\"E\":\"You good.\"}]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
        Assert.Equal("你 好", response.ProcessedSentenceRows[0].Chinese);
        Assert.Equal("You good.", response.ProcessedSentenceRows[0].English);
    }

    // ---------- Edge-case: truncated JSON ----------

    [Fact]
    public void FillFromJsonResponse_TruncatedJson_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scra";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_TruncatedJsonInSentence_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Sentences\":[{\"C\":\"我";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    [Fact]
    public void FillFromJsonResponse_TruncatedAfterBrace_ThrowsJsonException()
    {
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Corrections\":[";

        Assert.Throws<JsonException>(() => response.FillFromJsonResponse(json));
    }

    // ---------- Edge-case: BOM handling ----------

    [Fact]
    public void FillFromJsonResponse_JsonWithBom_ParsesSuccessfully()
    {
        var response = new AiApiResponse();
        // Prepend a UTF-8 BOM character (\uFEFF) to the JSON string
        string json = "\uFEFF{\"SP\":\"scratch\",\"Sentences\":[]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithBomAndCodeBlock_ParsesSuccessfully()
    {
        var response = new AiApiResponse();
        string json = "\uFEFF```json\n{\"SP\":\"scratch\"}\n```";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    [Fact]
    public void FillFromJsonResponse_JsonWithBomInBraceFallback_ParsesSuccessfully()
    {
        var response = new AiApiResponse();
        // BOM + text before JSON — brace fallback should extract between braces
        string json = "\uFEFFHere is the result: {\"SP\":\"scratch\"} done.";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal("scratch", response.Scratchpad);
    }

    // ---------- Edge-case: null element in Sentences array ----------

    [Fact]
    public void FillFromJsonResponse_SentencesWithNullElement_ThrowsNullReferenceException()
    {
        // System.Text.Json deserializes a null element in the Sentences array as a
        // null ProcessedSentenceRow. FillFromJsonResponse then calls
        // SandhiProcessor.ApplySandhi which iterates the rows and crashes on the
        // null row. This documents the current behavior — a production code fix
        // would add a null-element guard in the deserialization or sandhi pipeline.
        var response = new AiApiResponse();
        string json = "{\"SP\":\"scratch\",\"Sentences\":[null]}";

        Assert.Throws<NullReferenceException>(() => response.FillFromJsonResponse(json));
    }

    // ---------- Edge-case: sentence with missing B field ----------

    [Fact]
    public void FillFromJsonResponse_SentenceMissingBreakdownField_BreakdownStaysEmpty()
    {
        var response = new AiApiResponse();
        // Sentence with C, P, E, R but no B field
        string json = "{\"SP\":\"sp\",\"Sentences\":[{\"C\":\"你好\",\"P\":\"nǐ hǎo\",\"R\":\"N/A\",\"E\":\"Hello\"}]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
        Assert.Equal("你好", response.ProcessedSentenceRows[0].Chinese);
        // Pinyin may be transformed by sandhi but should not be null
        Assert.NotNull(response.ProcessedSentenceRows[0].Pinyin);
        // Breakdown should default to empty list (PipeDelimitedBreakdownConverter returns empty)
        Assert.NotNull(response.ProcessedSentenceRows[0].Breakdown);
        Assert.Empty(response.ProcessedSentenceRows[0].Breakdown);
    }

    // ---------- Edge-case: sandhi-aware parsing with realistic data ----------

    [Fact]
    public void FillFromJsonResponse_RealisticSingleSentence_SandhiAppliedToPinyin()
    {
        var response = new AiApiResponse();
        // The supermarket sentence has no sandhi-triggering tone patterns
        // (no 3rd+3rd adjacent, no bù, no yī, no target particles),
        // so pinyin tones are unchanged. However, sandhi reconstruction
        // rebuilds the sentence pinyin from breakdown entries, appending
        // punctuation without a leading space. ChinesePunctuationSanitizingConverter
        // also strips the space before the Chinese period in the C field.
        string json = TestDataHelper.CreateSupermarketJson();

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
        // C field: space before 。 is stripped by CleanChinesePunctuation
        Assert.Equal("我 在 超市 买菜。", response.ProcessedSentenceRows[0].Chinese);
        // P field: reconstructed from breakdown — period appended without space
        Assert.Equal("wǒ zài chāoshì mǎicài.", response.ProcessedSentenceRows[0].Pinyin);
    }

    [Fact]
    public void FillFromJsonResponse_RealisticTwoSentences_BothRowsPopulatedWithSandhi()
    {
        var response = new AiApiResponse();
        string json = TestDataHelper.CreateTwoSentenceJson();

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Equal(2, response.ProcessedSentenceRows.Count);

        // First sentence — no sandhi tone changes; punctuation space stripped in C and P
        Assert.Equal("我 在 超市 买菜。", response.ProcessedSentenceRows[0].Chinese);
        Assert.Equal("wǒ zài chāoshì mǎicài.", response.ProcessedSentenceRows[0].Pinyin);

        // Second sentence — no sandhi tone changes (tā xuéxí zhōngwén has no triggering patterns)
        Assert.Equal("他 学习 中文。", response.ProcessedSentenceRows[1].Chinese);
        Assert.Equal("tā xuéxí zhōngwén.", response.ProcessedSentenceRows[1].Pinyin);
    }

    [Fact]
    public void FillFromJsonResponse_SentenceWithThirdThirdToneSandhi_PinyinTransformed()
    {
        var response = new AiApiResponse();
        // 小雨 (xiǎo yǔ) — 3rd+3rd tone within a single word → xiáo yǔ
        string json = "{\"SP\":\"xiáo yǔ\",\"Sentences\":[{\"C\":\"小雨\",\"P\":\"xiǎo yǔ\",\"B\":\"小雨|xiǎo'yǔ|light rain|n|3| \",\"R\":\"N/A\",\"E\":\"light rain\"}]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
        // Sandhi should transform 3rd+3rd to 2nd+3rd: xiǎo → xiáo
        Assert.Contains("xiáo", response.ProcessedSentenceRows[0].Pinyin);
    }

    [Fact]
    public void FillFromJsonResponse_SentenceWithBuSandhi_PinyinTransformed()
    {
        var response = new AiApiResponse();
        // 不是 (bù shì) — bù before 4th tone → bú
        string json = "{\"SP\":\"bú shì\",\"Sentences\":[{\"C\":\"不是\",\"P\":\"bù shì\",\"B\":\"不|bù|not|adv|1| ||是|shì|is|v|2| \",\"R\":\"N/A\",\"E\":\"is not\"}]}";

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        Assert.Single(response.ProcessedSentenceRows);
        // Sandhi should transform bù → bú before 4th tone
        Assert.Contains("bú", response.ProcessedSentenceRows[0].Pinyin);
    }

    [Fact]
    public void FillFromJsonResponse_BreakdownEntriesPreserveRawWireValues()
    {
        var response = new AiApiResponse();
        // Verify that the pipe-delimited breakdown converter preserves raw wire values
        // (e.g., "pron" not "Pronoun", "1" not "HSK1") for the ResponseVerifier.
        string json = TestDataHelper.CreateSupermarketJson();

        bool result = response.FillFromJsonResponse(json);

        Assert.True(result);
        var breakdown = response.ProcessedSentenceRows[0].Breakdown;
        Assert.Equal(5, breakdown.Count);

        // First entry: 我|wǒ|I|pron|1|
        Assert.Equal("我", breakdown[0].Chinese);
        Assert.Equal("wǒ", breakdown[0].Pinyin);
        Assert.Equal("I", breakdown[0].Gloss);
        Assert.Equal("pron", breakdown[0].RawPartOfSpeech);
        Assert.Equal("1", breakdown[0].RawHskLevel);
    }
}