//-----------------------------------------------------------------------
// <copyright file="HskLevelConverter.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Buffers;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// This purpose of this class is to automatically convert the HSK level values from the JSON response into a more user-friendly format. The original JSON may contain values like "1", "2", "3", "4", "5", "6", or "N/A". This converter will transform numeric levels into the format "HSK1", "HSK2", etc., while leaving "N/A" unchanged and converting empty or whitespace values to an empty string. This allows for consistent and clear representation of HSK levels in the application without requiring manual formatting after deserialization.
    /// </summary>
    public class HskLevelConverter : JsonConverter<string>
    {
        private static readonly string Hsk1 = "HSK1";
        private static readonly string Hsk2 = "HSK2";
        private static readonly string Hsk3 = "HSK3";
        private static readonly string Hsk4 = "HSK4";
        private static readonly string Hsk5 = "HSK5";
        private static readonly string Hsk6 = "HSK6";
        private static readonly string Hsk7To9 = "HSK7-9";

        /// <summary>
        /// Maps a spec wire HSK value to its display form, mirroring the deserialization
        /// inflation. Single digits become <c>HSK1</c>..<c>HSK6</c>; other shapes fall
        /// back to the same normalization used during deserialization.
        /// </summary>
        /// <param name="value">The raw wire value (e.g. <c>1</c>).</param>
        /// <returns>The display form (e.g. <c>HSK1</c>).</returns>
        public static string DisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            if (trimmed.Length == 1)
            {
                switch (trimmed[0])
                {
                    case '1': return Hsk1;
                    case '2': return Hsk2;
                    case '3': return Hsk3;
                    case '4': return Hsk4;
                    case '5': return Hsk5;
                    case '6': return Hsk6;
                }
            }

            return NormalizeHskLevelFallback(trimmed);
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string token for HSK level.");
            }

            ReadOnlySpan<byte> span = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan;

            span = TrimUtf8Whitespace(span);

            if (span.IsEmpty)
            {
                return string.Empty;
            }

            if (span.Length == 1)
            {
                switch (span[0])
                {
                    case (byte)'1': return Hsk1;
                    case (byte)'2': return Hsk2;
                    case (byte)'3': return Hsk3;
                    case (byte)'4': return Hsk4;
                    case (byte)'5': return Hsk5;
                    case (byte)'6': return Hsk6;
                }
            }

            string rawValue = reader.GetString() ?? string.Empty;
            return NormalizeHskLevelFallback(rawValue);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        private static ReadOnlySpan<byte> TrimUtf8Whitespace(ReadOnlySpan<byte> span)
        {
            int start = 0;
            while (start < span.Length && IsUtf8Whitespace(span[start]))
            {
                start++;
            }

            int end = span.Length - 1;
            while (end >= start && IsUtf8Whitespace(span[end]))
            {
                end--;
            }

            return span.Slice(start, end - start + 1);
        }

        private static bool IsUtf8Whitespace(byte b)
        {
            return b == ' ' || b == '\t' || b == '\r' || b == '\n';
        }

        private static string NormalizeHskLevelFallback(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return string.Empty;
            }

            string normalized = val.Replace(" ", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "HSK1" or "LEVEL1" => Hsk1,
                "HSK2" or "LEVEL2" => Hsk2,
                "HSK3" or "LEVEL3" => Hsk3,
                "HSK4" or "LEVEL4" => Hsk4,
                "HSK5" or "LEVEL5" => Hsk5,
                "HSK6" or "LEVEL6" => Hsk6,
                "HSK7-9" or "7-9" or "79" => Hsk7To9,
                _ => val
            };
        }
    }
}