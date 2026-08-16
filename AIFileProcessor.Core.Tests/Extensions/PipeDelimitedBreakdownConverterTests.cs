namespace AIFileProcessor.Core.Tests.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using AIFileProcessor.Extensions;
using AIFileProcessor.Response;
using Xunit;

[Trait("Category", "Unit")]
public class PipeDelimitedBreakdownConverterTests
{
    private static readonly PipeDelimitedBreakdownConverter Converter = new();
    private static readonly JsonSerializerOptions Options = new() { Converters = { Converter } };

    // --- Helpers ---

    private static List<BreakdownEntry>? ReadDirect(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read();
        return Converter.Read(ref reader, typeof(List<BreakdownEntry>), Options);
    }

    private static string WriteValue(List<BreakdownEntry> value)
    {
        using var stream = new MemoryStream();
        var writerOptions = new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        using var writer = new Utf8JsonWriter(stream, writerOptions);
        Converter.Write(writer, value, Options);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // --- Read: Null token ---

    [Fact]
    public void Read_NullToken_ReturnsNull()
    {
        var result = ReadDirect("null");
        Assert.Null(result);
    }

    // --- Read: Non-string tokens ---

    [Fact]
    public void Read_NumberToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => ReadDirect("123"));
    }

    [Fact]
    public void Read_BooleanToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => ReadDirect("true"));
    }

    [Fact]
    public void Read_ArrayToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => ReadDirect("[]"));
    }

    [Fact]
    public void Read_ObjectToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => ReadDirect("{}"));
    }

    // --- Read: Empty and whitespace strings ---

    [Fact]
    public void Read_EmptyString_ReturnsEmptyList()
    {
        var result = ReadDirect("\"\"");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_WhitespaceOnlyString_ReturnsEmptyList()
    {
        var result = ReadDirect("\"   \"");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // --- Read: Single entry with all fields ---

    [Fact]
    public void Read_SingleEntryAllFields_ReturnsPopulatedList()
    {
        var result = ReadDirect("\"我|wǒ|I|n|1|some note\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal("I", entry.Gloss);
        Assert.Equal("Noun", entry.PartOfSpeech);
        Assert.Equal("HSK1", entry.HskLevel);
        Assert.Equal("some note", entry.Note);
    }

    // --- Read: Single entry with fewer fields ---

    [Fact]
    public void Read_SingleEntryOnlyChinese_ReturnsDefaultsForOtherFields()
    {
        var result = ReadDirect("\"我\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal(string.Empty, entry.Pinyin);
        Assert.Equal(string.Empty, entry.Gloss);
        Assert.Equal(string.Empty, entry.PartOfSpeech);
        Assert.Equal(string.Empty, entry.HskLevel);
        Assert.Equal(string.Empty, entry.Note);
    }

    [Fact]
    public void Read_SingleEntryTwoFields_ReturnsChineseAndPinyin()
    {
        var result = ReadDirect("\"我|wǒ\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal(string.Empty, entry.Gloss);
    }

    [Fact]
    public void Read_SingleEntryThreeFields_ReturnsChinesePinyinGloss()
    {
        var result = ReadDirect("\"我|wǒ|I\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal("I", entry.Gloss);
        Assert.Equal(string.Empty, entry.PartOfSpeech);
    }

    [Fact]
    public void Read_SingleEntryFourFields_ReturnsChinesePinyinGlossPos()
    {
        var result = ReadDirect("\"我|wǒ|I|v\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal("I", entry.Gloss);
        Assert.Equal("Verb", entry.PartOfSpeech);
        Assert.Equal(string.Empty, entry.HskLevel);
    }

    [Fact]
    public void Read_SingleEntryFiveFields_ReturnsChinesePinyinGlossPosHsk()
    {
        var result = ReadDirect("\"我|wǒ|I|n|3\"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal("I", entry.Gloss);
        Assert.Equal("Noun", entry.PartOfSpeech);
        Assert.Equal("HSK3", entry.HskLevel);
        Assert.Equal(string.Empty, entry.Note);
    }

    // --- Read: Multiple entries ---

    [Fact]
    public void Read_MultipleEntries_ReturnsAllEntries()
    {
        var result = ReadDirect("\"我|wǒ|I|n|1||你|nǐ|you|n|1\"");
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);

        Assert.Equal("我", result[0].Chinese);
        Assert.Equal("wǒ", result[0].Pinyin);
        Assert.Equal("I", result[0].Gloss);
        Assert.Equal("Noun", result[0].PartOfSpeech);
        Assert.Equal("HSK1", result[0].HskLevel);

        Assert.Equal("你", result[1].Chinese);
        Assert.Equal("nǐ", result[1].Pinyin);
        Assert.Equal("you", result[1].Gloss);
        Assert.Equal("Noun", result[1].PartOfSpeech);
        Assert.Equal("HSK1", result[1].HskLevel);
    }

    // --- Read: Trimming of parts ---

    [Fact]
    public void Read_EntryWithSpacesAroundParts_TrimsParts()
    {
        var result = ReadDirect("\" 我 | wǒ | I | n | 1 | note \"");
        Assert.NotNull(result);
        Assert.Single(result);

        var entry = result![0];
        Assert.Equal("我", entry.Chinese);
        Assert.Equal("wǒ", entry.Pinyin);
        Assert.Equal("I", entry.Gloss);
        Assert.Equal("Noun", entry.PartOfSpeech);
        Assert.Equal("HSK1", entry.HskLevel);
        Assert.Equal("note", entry.Note);
    }

    // --- Read: Empty entries split by || ---

    [Fact]
    public void Read_EntriesWithOnlySeparators_ReturnsEmptyList()
    {
        var result = ReadDirect("\"||\"");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // --- Read: Null options ---

    [Fact]
    public void Read_WithNullOptions_ReturnsPopulatedList()
    {
        var bytes = Encoding.UTF8.GetBytes("\"我|wǒ|I|n|1|note\"");
        var reader = new Utf8JsonReader(bytes);
        reader.Read();
        var result = Converter.Read(ref reader, typeof(List<BreakdownEntry>), null!);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("我", result![0].Chinese);
    }

    // --- Write ---

    [Fact]
    public void Write_EmptyList_WritesEmptyString()
    {
        var json = WriteValue(new List<BreakdownEntry>());
        Assert.Equal("\"\"", json);
    }

    [Fact]
    public void Write_SingleEntry_WritesPipeDelimitedString()
    {
        var entries = new List<BreakdownEntry>
        {
            new()
            {
                Chinese = "我",
                Pinyin = "wǒ",
                Gloss = "I",
                PartOfSpeech = "Noun",
                HskLevel = "HSK1",
                Note = "some note",
            },
        };

        var json = WriteValue(entries);
        Assert.Equal("\"我|wǒ|I|Noun|HSK1|some note\"", json);
    }

    [Fact]
    public void Write_MultipleEntries_WritesDoublePipeSeparatedString()
    {
        var entries = new List<BreakdownEntry>
        {
            new()
            {
                Chinese = "我",
                Pinyin = "wǒ",
                Gloss = "I",
                PartOfSpeech = "Noun",
                HskLevel = "HSK1",
                Note = string.Empty,
            },
            new()
            {
                Chinese = "你",
                Pinyin = "nǐ",
                Gloss = "you",
                PartOfSpeech = "Noun",
                HskLevel = "HSK1",
                Note = string.Empty,
            },
        };

        var json = WriteValue(entries);
        Assert.Equal("\"我|wǒ|I|Noun|HSK1|||你|nǐ|you|Noun|HSK1|\"", json);
    }

    [Fact]
    public void Write_EntryWithEmptyFields_WritesEmptyPipeSegments()
    {
        var entries = new List<BreakdownEntry>
        {
            new()
            {
                Chinese = "我",
                Pinyin = string.Empty,
                Gloss = string.Empty,
                PartOfSpeech = string.Empty,
                HskLevel = string.Empty,
                Note = string.Empty,
            },
        };

        var json = WriteValue(entries);
        Assert.Equal("\"我|||||\"", json);
    }
}
