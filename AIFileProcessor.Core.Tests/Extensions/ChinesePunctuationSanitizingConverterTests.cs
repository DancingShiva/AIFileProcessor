namespace AIFileProcessor.Core.Tests.Extensions;
using System;
using System.Text.Json;
using AIFileProcessor.Extensions;
[Trait("Category", "Unit")]
public class ChinesePunctuationSanitizingConverterTests
{
    [Fact]
    public void Read_WithChinesePunctuationSurroundedBySpaces_RemovesSpaces()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        var json = "\"你好 ， 世界\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();
        var result = converter.Read(ref reader, typeof(string), new JsonSerializerOptions());
        Assert.Equal("你好，世界", result);
    }
    [Fact]
    public void Read_WithCleanString_ReturnsStringUnchanged()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        var json = "\"你好，世界\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();
        var result = converter.Read(ref reader, typeof(string), new JsonSerializerOptions());
        Assert.Equal("你好，世界", result);
    }
    [Fact]
    public void Read_WithNullJsonValue_ReturnsEmptyString()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        var json = "null";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();
        var result = converter.Read(ref reader, typeof(string), new JsonSerializerOptions());
        Assert.Equal(string.Empty, result);
    }
    [Fact]
    public void Write_WithNonEmptyString_WritesValueToOutput()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        using var stream = new System.IO.MemoryStream();
        var writerOptions = new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        using var writer = new Utf8JsonWriter(stream, writerOptions);
        var options = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        converter.Write(writer, "你好，世界", options);
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("你好，世界", json);
    }
    [Fact]
    public void Write_WithEmptyString_WritesEmptyStringValue()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        using var stream = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        converter.Write(writer, string.Empty, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("\"\"", json);
    }
    [Fact]
    public void Read_WithFullWidthPunctuationAndSpaces_RemovesAllSurroundingSpaces()
    {
        var converter = new ChinesePunctuationSanitizingConverter();
        var json = "\"测试 【 标题 】 结束\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();
        var result = converter.Read(ref reader, typeof(string), new JsonSerializerOptions());
        Assert.Equal("测试【标题】结束", result);
    }
}

