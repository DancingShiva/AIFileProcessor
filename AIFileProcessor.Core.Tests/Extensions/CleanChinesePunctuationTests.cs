namespace AIFileProcessor.Core.Tests.Extensions
{
    using AIFileProcessor.Extensions;

    public class CleanChinesePunctuationTests
    {
        [Fact]
        public void CleanChinesePunctuation_NullInput_ReturnsEmptyString()
        {
            string input = null!;

            var result = input.CleanChinesePunctuation();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CleanChinesePunctuation_EmptyInput_ReturnsEmptyString()
        {
            var result = string.Empty.CleanChinesePunctuation();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CleanChinesePunctuation_WhitespaceInput_ReturnsEmptyString()
        {
            var result = "   ".CleanChinesePunctuation();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CleanChinesePunctuation_NoPunctuation_ReturnsOriginalString()
        {
            var input = "你好世界";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好世界", result);
        }

        [Fact]
        public void CleanChinesePunctuation_SpaceAfterPunctuation_RemovesSpace()
        {
            var input = "你好， 世界";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好，世界", result);
        }

        [Fact]
        public void CleanChinesePunctuation_SpaceBeforePunctuation_RemovesSpace()
        {
            var input = "你好 ，世界";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好，世界", result);
        }

        [Fact]
        public void CleanChinesePunctuation_MultiplePunctuationWithSpaces_RemovesAllSpaces()
        {
            var input = "你好， 世界 。 再见！ ";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好，世界。再见！", result);
        }

        [Fact]
        public void CleanChinesePunctuation_TabAfterPunctuation_RemovesTab()
        {
            var input = "你好，\t世界";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好，世界", result);
        }

        [Fact]
        public void CleanChinesePunctuation_FullWidthParenthesesWithSpaces_RemovesSpaces()
        {
            var input = "你好 （ 世界 ）";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好（世界）", result);
        }

        [Fact]
        public void CleanChinesePunctuation_QuoteMarksWithSpaces_RemovesSpaces()
        {
            var input = "你好 “ 世界 ”";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("你好“世界”", result);
        }

        [Fact]
        public void CleanChinesePunctuation_MixedPunctuationAllTypes_RemovesSurroundingSpaces()
        {
            var input = "【 、 】 ， 。 ！ ？ ： ； ” “ 《 （ ） ％ ＄ ￥";

            var result = input.CleanChinesePunctuation();

            Assert.Equal("【、】，。！？：；”“《（）％＄￥", result);
        }
    }
}