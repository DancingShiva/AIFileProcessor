// <copyright file="TelemetryUtilsTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>

namespace AIFileProcessor.Core.Tests.Telemetry
{
    using AIFileProcessor.Telemetry;

    /// <summary>
    /// Regression tests for <see cref="TelemetryUtils.TryExtractTokens(object, out int?, out int?)"/>.
    /// These mirror the property shapes of the OpenAI SDK (Usage with InputTokenCount/OutputTokenCount)
    /// and the Google.GenAI SDK (UsageMetadata with PromptTokenCount/CandidatesTokenCount), which
    /// previously caused token extraction to silently fail and leave the metrics table empty.
    /// </summary>
    public class TelemetryUtilsTests
    {
        private class OpenAiShapedUsage
        {
            public int InputTokenCount { get; set; }

            public int OutputTokenCount { get; set; }

            public int TotalTokenCount { get; set; }
        }

        private class OpenAiShapedResponse
        {
            public OpenAiShapedUsage? Usage { get; set; }
        }

        private class GeminiShapedUsageMetadata
        {
            public int PromptTokenCount { get; set; }

            public int CandidatesTokenCount { get; set; }

            public int TotalTokenCount { get; set; }
        }

        private class GeminiShapedResponse
        {
            public GeminiShapedUsageMetadata? UsageMetadata { get; set; }
        }

        [Fact]
        public void TryExtractTokens_Extracts_OpenAiSdkShape()
        {
            var response = new OpenAiShapedResponse
            {
                Usage = new OpenAiShapedUsage { InputTokenCount = 123, OutputTokenCount = 456 },
            };

            bool found = TelemetryUtils.TryExtractTokens(response, out int? prompt, out int? completion);

            Assert.True(found);
            Assert.Equal(123, prompt);
            Assert.Equal(456, completion);
        }

        [Fact]
        public void TryExtractTokens_Extracts_GeminiSdkShape()
        {
            var response = new GeminiShapedResponse
            {
                UsageMetadata = new GeminiShapedUsageMetadata { PromptTokenCount = 321, CandidatesTokenCount = 654 },
            };

            bool found = TelemetryUtils.TryExtractTokens(response, out int? prompt, out int? completion);

            Assert.True(found);
            Assert.Equal(321, prompt);
            Assert.Equal(654, completion);
        }

        [Fact]
        public void TryExtractTokens_ReturnsFalse_WhenNoUsagePresent()
        {
            var response = new OpenAiShapedResponse { Usage = null };

            bool found = TelemetryUtils.TryExtractTokens(response, out int? prompt, out int? completion);

            Assert.False(found);
            Assert.Null(prompt);
            Assert.Null(completion);
        }
    }
}
