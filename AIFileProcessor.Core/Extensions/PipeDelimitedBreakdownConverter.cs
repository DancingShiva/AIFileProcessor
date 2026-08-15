//-----------------------------------------------------------------------
// <copyright file="PipeDelimitedBreakdownConverter.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AIFileProcessor.Response;

    public class PipeDelimitedBreakdownConverter : JsonConverter<List<BreakdownEntry>>
    {
        private static readonly PartOfSpeechConverter _posConverter = new();
        private static readonly HskLevelConverter _hskConverter = new();

        public override List<BreakdownEntry>? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string token for pipe-delimited breakdown, but got {reader.TokenType}.");
            }

            string rawValue = reader.GetString() ?? string.Empty;
            var list = new List<BreakdownEntry>();

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return list;
            }

            // INVALID lines have B set to "N/A" per the spec; return an empty list
            // instead of trying to parse "N/A" as a pipe-delimited breakdown entry.
            if (rawValue.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return list;
            }

            // Split array members by "||"
            string[] entryStrings = rawValue.Split("||", StringSplitOptions.RemoveEmptyEntries);

            // Use passed-through options if available so global converters are preserved
            var serializerOptions = options ?? new JsonSerializerOptions();

            foreach (var entryStr in entryStrings)
            {
                // Split components of each member by "|"
                string[] parts = entryStr.Split('|', StringSplitOptions.TrimEntries);

                var entry = new BreakdownEntry
                {
                    Chinese = parts.Length > 0 ? parts[0] : string.Empty,
                    Pinyin = parts.Length > 1 ? parts[1] : string.Empty,
                    Gloss = parts.Length > 2 ? parts[2] : string.Empty,
                    RawPartOfSpeech = parts.Length > 3 ? parts[3] : string.Empty,
                    PartOfSpeech = ConvertProperty(parts.Length > 3 ? parts[3] : string.Empty, _posConverter, serializerOptions),
                    RawHskLevel = parts.Length > 4 ? parts[4] : string.Empty,
                    HskLevel = ConvertProperty(parts.Length > 4 ? parts[4] : string.Empty, _hskConverter, serializerOptions),
                    Note = parts.Length > 5 ? parts[5] : string.Empty
                };

                list.Add(entry);
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<BreakdownEntry> value, JsonSerializerOptions options)
        {
            // Optional: If you need to serialize back to the pipe-delimited string format
            var entries = new List<string>();
            foreach (var item in value)
            {
                entries.Add($"{item.Chinese}|{item.Pinyin}|{item.Gloss}|{item.PartOfSpeech}|{item.HskLevel}|{item.Note}");
            }

            writer.WriteStringValue(string.Join("||", entries));
        }

        private static string ConvertProperty(string value, JsonConverter<string> converter, JsonSerializerOptions options)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, options);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            return converter.Read(ref reader, typeof(string), options) ?? string.Empty;
        }
    }
}