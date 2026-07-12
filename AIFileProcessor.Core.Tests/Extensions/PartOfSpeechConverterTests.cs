namespace AIFileProcessor.Core.Tests.Extensions
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using AIFileProcessor.Extensions;
    using Xunit;

    public class PartOfSpeechConverterTests
    {
        private static readonly PartOfSpeechConverter Converter = new();
        private static readonly JsonSerializerOptions Options = new() { Converters = { Converter } };

        // --- Helpers ---

        private static string ReadDirect(byte[] jsonBytes)
        {
            var reader = new Utf8JsonReader(jsonBytes);
            reader.Read();
            return Converter.Read(ref reader, typeof(string), Options);
        }

        private static string ReadMultiSegment(byte[] jsonBytes, int splitPoint)
        {
            var first = new byte[splitPoint];
            var second = new byte[jsonBytes.Length - splitPoint];
            Array.Copy(jsonBytes, 0, first, 0, splitPoint);
            Array.Copy(jsonBytes, splitPoint, second, 0, second.Length);

            var firstSegment = new TestSegment(first);
            var secondSegment = new TestSegment(second, firstSegment);
            var sequence = new ReadOnlySequence<byte>(firstSegment, 0, secondSegment, second.Length);

            var reader = new Utf8JsonReader(sequence);
            reader.Read();
            return Converter.Read(ref reader, typeof(string), Options);
        }

        private static string WriteValue(string value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            Converter.Write(writer, value, Options);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        // --- Null token ---

        [Fact]
        public void Read_NullToken_ReturnsEmptyString()
        {
            var result = ReadDirect("null"u8.ToArray());
            Assert.Equal(string.Empty, result);
        }

        // --- Non-string tokens ---

        [Fact]
        public void Read_NumberToken_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => ReadDirect("123"u8.ToArray()));
        }

        [Fact]
        public void Read_BooleanToken_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => ReadDirect("true"u8.ToArray()));
        }

        // --- Empty and whitespace strings ---

        [Fact]
        public void Read_EmptyString_ReturnsEmptyString()
        {
            var result = ReadDirect("\"\""u8.ToArray());
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Read_WhitespaceOnlyString_ReturnsEmptyString()
        {
            var result = ReadDirect("\"   \""u8.ToArray());
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Read_MultipleSpacesString_ReturnsEmptyString()
        {
            var result = ReadDirect("\"     \""u8.ToArray());
            Assert.Equal(string.Empty, result);
        }

        // --- Single char (case 1) ---

        [Fact]
        public void Read_SingleCharN_ReturnsNoun()
        {
            var result = ReadDirect("\"n\""u8.ToArray());
            Assert.Equal("Noun", result);
        }

        [Fact]
        public void Read_SingleCharV_ReturnsVerb()
        {
            var result = ReadDirect("\"v\""u8.ToArray());
            Assert.Equal("Verb", result);
        }

        [Fact]
        public void Read_SingleCharT_ReturnsTimeWord()
        {
            var result = ReadDirect("\"t\""u8.ToArray());
            Assert.Equal("Time Word", result);
        }

        [Fact]
        public void Read_SingleCharUppercaseN_ReturnsNoun()
        {
            var result = ReadDirect("\"N\""u8.ToArray());
            Assert.Equal("Noun", result);
        }

        [Fact]
        public void Read_SingleCharUppercaseV_ReturnsVerb()
        {
            var result = ReadDirect("\"V\""u8.ToArray());
            Assert.Equal("Verb", result);
        }

        [Fact]
        public void Read_SingleCharUppercaseT_ReturnsTimeWord()
        {
            var result = ReadDirect("\"T\""u8.ToArray());
            Assert.Equal("Time Word", result);
        }

        [Fact]
        public void Read_SingleCharUnknown_ReturnsOriginalString()
        {
            var result = ReadDirect("\"x\""u8.ToArray());
            Assert.Equal("x", result);
        }

        [Fact]
        public void Read_SingleCharWithWhitespace_ReturnsNoun()
        {
            var result = ReadDirect("\" n \""u8.ToArray());
            Assert.Equal("Noun", result);
        }

        // --- Two char (case 2) ---

        [Fact]
        public void Read_TwoCharId_ReturnsIdiom()
        {
            var result = ReadDirect("\"id\""u8.ToArray());
            Assert.Equal("Idiom", result);
        }

        [Fact]
        public void Read_TwoCharMw_ReturnsMeasureWord()
        {
            var result = ReadDirect("\"mw\""u8.ToArray());
            Assert.Equal("Measure Word", result);
        }

        [Fact]
        public void Read_TwoCharPn_ReturnsProperNoun()
        {
            var result = ReadDirect("\"pn\""u8.ToArray());
            Assert.Equal("Proper Noun", result);
        }

        [Fact]
        public void Read_TwoCharUppercaseId_ReturnsIdiom()
        {
            var result = ReadDirect("\"ID\""u8.ToArray());
            Assert.Equal("Idiom", result);
        }

        [Fact]
        public void Read_TwoCharMixedCaseMw_ReturnsMeasureWord()
        {
            var result = ReadDirect("\"Mw\""u8.ToArray());
            Assert.Equal("Measure Word", result);
        }

        [Fact]
        public void Read_TwoCharUnknown_ReturnsOriginalString()
        {
            var result = ReadDirect("\"xy\""u8.ToArray());
            Assert.Equal("xy", result);
        }

        // --- Three char (case 3) ---

        [Fact]
        public void Read_ThreeCharAdj_ReturnsAdjective()
        {
            var result = ReadDirect("\"adj\""u8.ToArray());
            Assert.Equal("Adjective", result);
        }

        [Fact]
        public void Read_ThreeCharAdv_ReturnsAdverb()
        {
            var result = ReadDirect("\"adv\""u8.ToArray());
            Assert.Equal("Adverb", result);
        }

        [Fact]
        public void Read_ThreeCharInt_ReturnsInterjection()
        {
            var result = ReadDirect("\"int\""u8.ToArray());
            Assert.Equal("Interjection", result);
        }

        [Fact]
        public void Read_ThreeCharLoc_ReturnsLocative()
        {
            var result = ReadDirect("\"loc\""u8.ToArray());
            Assert.Equal("Locative", result);
        }

        [Fact]
        public void Read_ThreeCharNum_ReturnsNumeral()
        {
            var result = ReadDirect("\"num\""u8.ToArray());
            Assert.Equal("Numeral", result);
        }

        [Fact]
        public void Read_ThreeCharUppercaseAdj_ReturnsAdjective()
        {
            var result = ReadDirect("\"ADJ\""u8.ToArray());
            Assert.Equal("Adjective", result);
        }

        [Fact]
        public void Read_ThreeCharUppercaseAdv_ReturnsAdverb()
        {
            var result = ReadDirect("\"ADV\""u8.ToArray());
            Assert.Equal("Adverb", result);
        }

        [Fact]
        public void Read_ThreeCharUnknown_ReturnsOriginalString()
        {
            var result = ReadDirect("\"xyz\""u8.ToArray());
            Assert.Equal("xyz", result);
        }

        // --- Four char (case 4) ---

        [Fact]
        public void Read_FourCharConj_ReturnsConjunction()
        {
            var result = ReadDirect("\"conj\""u8.ToArray());
            Assert.Equal("Conjunction", result);
        }

        [Fact]
        public void Read_FourCharPart_ReturnsParticle()
        {
            var result = ReadDirect("\"part\""u8.ToArray());
            Assert.Equal("Particle", result);
        }

        [Fact]
        public void Read_FourCharPrep_ReturnsPreposition()
        {
            var result = ReadDirect("\"prep\""u8.ToArray());
            Assert.Equal("Preposition", result);
        }

        [Fact]
        public void Read_FourCharPron_ReturnsPronoun()
        {
            var result = ReadDirect("\"pron\""u8.ToArray());
            Assert.Equal("Pronoun", result);
        }

        [Fact]
        public void Read_FourCharPunc_ReturnsPunctuation()
        {
            var result = ReadDirect("\"punc\""u8.ToArray());
            Assert.Equal("Punctuation", result);
        }

        [Fact]
        public void Read_FourCharUppercaseConj_ReturnsConjunction()
        {
            var result = ReadDirect("\"CONJ\""u8.ToArray());
            Assert.Equal("Conjunction", result);
        }

        [Fact]
        public void Read_FourCharUppercasePunc_ReturnsPunctuation()
        {
            var result = ReadDirect("\"PUNC\""u8.ToArray());
            Assert.Equal("Punctuation", result);
        }

        [Fact]
        public void Read_FourCharUnknown_ReturnsOriginalString()
        {
            var result = ReadDirect("\"abcd\""u8.ToArray());
            Assert.Equal("abcd", result);
        }

        // --- Other lengths (fallback) ---

        [Fact]
        public void Read_FiveCharString_ReturnsOriginalString()
        {
            var result = ReadDirect("\"abcde\""u8.ToArray());
            Assert.Equal("abcde", result);
        }

        [Fact]
        public void Read_LongString_ReturnsOriginalString()
        {
            var result = ReadDirect("\"unknown\""u8.ToArray());
            Assert.Equal("unknown", result);
        }

        // --- Multi-segment (HasValueSequence = true) ---

        [Fact]
        public void Read_MultiSegmentAdjString_ReturnsAdjective()
        {
            var json = "\"adj\""u8.ToArray();
            var result = ReadMultiSegment(json, 3);
            Assert.Equal("Adjective", result);
        }

        [Fact]
        public void Read_MultiSegmentConjString_ReturnsConjunction()
        {
            var json = "\"conj\""u8.ToArray();
            var result = ReadMultiSegment(json, 3);
            Assert.Equal("Conjunction", result);
        }

        [Fact]
        public void Read_MultiSegmentUnknownString_ReturnsOriginal()
        {
            var json = "\"xyz\""u8.ToArray();
            var result = ReadMultiSegment(json, 2);
            Assert.Equal("xyz", result);
        }

        // --- Write ---

        [Fact]
        public void Write_WritesStringValue()
        {
            var json = WriteValue("Noun");
            Assert.Equal("\"Noun\"", json);
        }

        [Fact]
        public void Write_WritesEmptyStringValue()
        {
            var json = WriteValue(string.Empty);
            Assert.Equal("\"\"", json);
        }

        [Fact]
        public void Write_WritesSpecialStringValue()
        {
            var json = WriteValue("Measure Word");
            Assert.Equal("\"Measure Word\"", json);
        }

        // --- Inner class for multi-segment sequence ---

        private sealed class TestSegment : ReadOnlySequenceSegment<byte>
        {
            public TestSegment(byte[] data)
            {
                Memory = data;
                RunningIndex = 0;
            }

            public TestSegment(byte[] data, TestSegment previous)
            {
                Memory = data;
                RunningIndex = previous.RunningIndex + previous.Memory.Length;
                previous.Next = this;
            }
        }
    }
}