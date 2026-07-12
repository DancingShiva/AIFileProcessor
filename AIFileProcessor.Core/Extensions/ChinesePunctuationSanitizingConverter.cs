//-----------------------------------------------------------------------
// <copyright file="ChinesePunctuationSanitizingConverter.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ChinesePunctuationSanitizingConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? rawValue = reader.GetString();

        // This is now guaranteed to return a non-null string
        return rawValue.CleanChinesePunctuation();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}