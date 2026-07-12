//-----------------------------------------------------------------------
// <copyright file="PartOfSpeechConverter.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class PartOfSpeechConverter : JsonConverter<string>
    {
        private static readonly string Adjective = "Adjective";
        private static readonly string Adverb = "Adverb";
        private static readonly string Conjunction = "Conjunction";
        private static readonly string Idiom = "Idiom";
        private static readonly string Interjection = "Interjection";
        private static readonly string Locative = "Locative";
        private static readonly string MeasureWord = "Measure Word";
        private static readonly string Noun = "Noun";
        private static readonly string Numeral = "Numeral";
        private static readonly string Particle = "Particle";
        private static readonly string Preposition = "Preposition";
        private static readonly string Pronoun = "Pronoun";
        private static readonly string Punctuation = "Punctuation";
        private static readonly string ProperNoun = "Proper Noun";
        private static readonly string TimeWord = "Time Word";
        private static readonly string Verb = "Verb";

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string.");
            }

            ReadOnlySpan<byte> span = reader.HasValueSequence
                ? System.Buffers.BuffersExtensions.ToArray(reader.ValueSequence)
                : reader.ValueSpan;

            span = TrimUtf8Whitespace(span);

            if (span.IsEmpty)
            {
                return string.Empty;
            }

            switch (span.Length)
            {
                case 1:
                    byte b = ToLowerAscii(span[0]);
                    if (b == (byte)'n') { return Noun; }
                    if (b == (byte)'v') { return Verb; }
                    if (b == (byte)'t') { return TimeWord; }
                    break;

                case 2:
                    if (EqualsAsciiIgnoreCase(span, "id")) { return Idiom; }
                    if (EqualsAsciiIgnoreCase(span, "mw")) { return MeasureWord; }
                    if (EqualsAsciiIgnoreCase(span, "pn")) { return ProperNoun; }
                    break;

                case 3:
                    if (EqualsAsciiIgnoreCase(span, "adj")) { return Adjective; }
                    if (EqualsAsciiIgnoreCase(span, "adv")) { return Adverb; }
                    if (EqualsAsciiIgnoreCase(span, "int")) { return Interjection; }
                    if (EqualsAsciiIgnoreCase(span, "loc")) { return Locative; }
                    if (EqualsAsciiIgnoreCase(span, "num")) { return Numeral; }
                    break;

                case 4:
                    if (EqualsAsciiIgnoreCase(span, "conj")) { return Conjunction; }
                    if (EqualsAsciiIgnoreCase(span, "part")) { return Particle; }
                    if (EqualsAsciiIgnoreCase(span, "prep")) { return Preposition; }
                    if (EqualsAsciiIgnoreCase(span, "pron")) { return Pronoun; }
                    if (EqualsAsciiIgnoreCase(span, "punc")) { return Punctuation; }
                    break;
            }

            // Fallback: Return original string if no match found
            return reader.GetString() ?? string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);

        private static ReadOnlySpan<byte> TrimUtf8Whitespace(ReadOnlySpan<byte> span)
        {
            int start = 0;
            while (start < span.Length && IsUtf8Whitespace(span[start])) { start++; }
            int end = span.Length - 1;
            while (end >= start && IsUtf8Whitespace(span[end])) { end--; }
            return span.Slice(start, end - start + 1);
        }

        private static bool IsUtf8Whitespace(byte b) => b == ' ' || b == '\t' || b == '\r' || b == '\n';

        private static bool EqualsAsciiIgnoreCase(ReadOnlySpan<byte> span, string asciiLower)
        {
            if (span.Length != asciiLower.Length) { return false; }
            for (int i = 0; i < span.Length; i++)
            {
                if (ToLowerAscii(span[i]) != (byte)asciiLower[i]) { return false; }
            }

            return true;
        }

        private static byte ToLowerAscii(byte b) => (b >= (byte)'A' && b <= (byte)'Z') ? (byte)(b + 32) : b;
    }
}