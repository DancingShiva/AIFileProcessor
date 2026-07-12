namespace AIFileProcessor.Core.Tests.Extensions
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using AIFileProcessor.Extensions;
    using Xunit;

    public class HskLevelConverterTests
    {
        private static readonly HskLevelConverter Converter = new();
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
        public void Read_NewlineWhitespaceString_ReturnsEmptyString()
        {
            var json = Encoding.UTF8.GetBytes("\"\\n\\n\"");
            var result = ReadDirect(json);
            Assert.Equal(string.Empty, result);
        }

        // --- Single digit HSK levels ---

        [Fact]
        public void Read_SingleDigit1_ReturnsHsk1()
        {
            var result = ReadDirect("\"1\""u8.ToArray());
            Assert.Equal("HSK1", result);
        }

        [Fact]
        public void Read_SingleDigit2_ReturnsHsk2()
        {
            var result = ReadDirect("\"2\""u8.ToArray());
            Assert.Equal("HSK2", result);
        }

        [Fact]
        public void Read_SingleDigit3_ReturnsHsk3()
        {
            var result = ReadDirect("\"3\""u8.ToArray());
            Assert.Equal("HSK3", result);
        }

        [Fact]
        public void Read_SingleDigit4_ReturnsHsk4()
        {
            var result = ReadDirect("\"4\""u8.ToArray());
            Assert.Equal("HSK4", result);
        }

        [Fact]
        public void Read_SingleDigit5_ReturnsHsk5()
        {
            var result = ReadDirect("\"5\""u8.ToArray());
            Assert.Equal("HSK5", result);
        }

        [Fact]
        public void Read_SingleDigit6_ReturnsHsk6()
        {
            var result = ReadDirect("\"6\""u8.ToArray());
            Assert.Equal("HSK6", result);
        }

        [Fact]
        public void Read_SingleDigitWithWhitespace_ReturnsHsk1()
        {
            var result = ReadDirect("\" 1 \""u8.ToArray());
            Assert.Equal("HSK1", result);
        }

        // --- Single char unknown (falls through to fallback) ---

        [Fact]
        public void Read_SingleDigit7_ReturnsOriginalString()
        {
            var result = ReadDirect("\"7\""u8.ToArray());
            Assert.Equal("7", result);
        }

        [Fact]
        public void Read_SingleCharUnknown_ReturnsOriginalString()
        {
            var result = ReadDirect("\"x\""u8.ToArray());
            Assert.Equal("x", result);
        }

        // --- Fallback: HSK-prefixed values ---

        [Fact]
        public void Read_Hsk1String_ReturnsHsk1()
        {
            var result = ReadDirect("\"HSK1\""u8.ToArray());
            Assert.Equal("HSK1", result);
        }

        [Fact]
        public void Read_Hsk6String_ReturnsHsk6()
        {
            var result = ReadDirect("\"HSK6\""u8.ToArray());
            Assert.Equal("HSK6", result);
        }

        [Fact]
        public void Read_Hsk7To9String_ReturnsHsk7To9()
        {
            var result = ReadDirect("\"HSK7-9\""u8.ToArray());
            Assert.Equal("HSK7-9", result);
        }

        // --- Fallback: LEVEL-prefixed values ---

        [Fact]
        public void Read_Level1String_ReturnsHsk1()
        {
            var result = ReadDirect("\"Level1\""u8.ToArray());
            Assert.Equal("HSK1", result);
        }

        [Fact]
        public void Read_Level3String_ReturnsHsk3()
        {
            var result = ReadDirect("\"LEVEL3\""u8.ToArray());
            Assert.Equal("HSK3", result);
        }

        // --- Fallback: 7-9 variants ---

        [Fact]
        public void Read_SevenToNineString_ReturnsHsk7To9()
        {
            var result = ReadDirect("\"7-9\""u8.ToArray());
            Assert.Equal("HSK7-9", result);
        }

        [Fact]
        public void Read_SevenNineString_ReturnsHsk7To9()
        {
            var result = ReadDirect("\"79\""u8.ToArray());
            Assert.Equal("HSK7-9", result);
        }

        // --- Fallback: unknown multi-char values ---

        [Fact]
        public void Read_NaString_ReturnsOriginalString()
        {
            var result = ReadDirect("\"N/A\""u8.ToArray());
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Read_UnknownString_ReturnsOriginalString()
        {
            var result = ReadDirect("\"unknown\""u8.ToArray());
            Assert.Equal("unknown", result);
        }

        [Fact]
        public void Read_HskStringWithSpaces_ReturnsNormalized()
        {
            var result = ReadDirect("\" H S K 1 \""u8.ToArray());
            Assert.Equal("HSK1", result);
        }

        // --- Multi-segment (HasValueSequence = true) ---

        [Fact]
        public void Read_MultiSegmentSingleDigit_ReturnsHsk3()
        {
            var json = "\"3\""u8.ToArray();
            var result = ReadMultiSegment(json, 2);
            Assert.Equal("HSK3", result);
        }

        [Fact]
        public void Read_MultiSegmentFallbackString_ReturnsHsk1()
        {
            var json = "\"HSK1\""u8.ToArray();
            var result = ReadMultiSegment(json, 3);
            Assert.Equal("HSK1", result);
        }

        [Fact]
        public void Read_MultiSegmentEmptyString_ReturnsEmpty()
        {
            var json = "\"\""u8.ToArray();
            var result = ReadMultiSegment(json, 2);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Read_MultiSegmentUnknownString_ReturnsOriginal()
        {
            var json = "\"N/A\""u8.ToArray();
            var result = ReadMultiSegment(json, 2);
            Assert.Equal("N/A", result);
        }

        // --- Write ---

        [Fact]
        public void Write_WritesStringValue()
        {
            var json = WriteValue("HSK1");
            Assert.Equal("\"HSK1\"", json);
        }

        [Fact]
        public void Write_WritesEmptyStringValue()
        {
            var json = WriteValue(string.Empty);
            Assert.Equal("\"\"", json);
        }

        [Fact]
        public void Write_WritesHsk7To9Value()
        {
            var json = WriteValue("HSK7-9");
            Assert.Equal("\"HSK7-9\"", json);
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