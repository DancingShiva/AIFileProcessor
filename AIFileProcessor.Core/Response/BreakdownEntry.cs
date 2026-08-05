//-----------------------------------------------------------------------
// <copyright file="BreakdownEntry.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Response
{
    using System.Text.Json.Serialization;
    using AIFileProcessor.Extensions;

    /// <summary>
    /// This is a class to hold the breakdown of a sentence into its constituent parts. Each entry in the breakdown corresponds to a segment of the original sentence, and contains information about the Chinese characters, their pinyin, gloss, part of speech, HSK level, and any additional notes. This structured format allows for more detailed analysis and processing of the sentence components compared to a simple pipe-delimited string. The ToString() method reconstructs the original pipe-delimited format for compatibility with existing systems that expect that format, while still allowing for richer data representation within the application.
    /// </summary>
    public class BreakdownEntry
    {
        /// <summary>
        /// gets or sets the spaced Chinese semantic units in their original order, with punctuation sanitized to a standard form. The sanitization is handled by the custom JSON converter 'ChinesePunctuationSanitizingConverter' which ensures that all punctuation is normalized to a consistent set of characters for easier processing and comparison.
        /// </summary>
        public string Chinese { get; set; } = string.Empty;

        /// <summary>
        /// Gets or setsthe pinyin for the sentence with Sandhi and acoustic reordering applied. In cases such as ¥100 this will be 100 ¥ to reflect the actual spoken order. This allows for more accurate representation of how the sentence would be pronounced, which can be important for language learning applications or any use case where phonetic information is relevant.
        /// </summary>
        public string Pinyin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the gloss for the sentence, providing a translation or explanation of the meaning.
        /// </summary>
        public string Gloss { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the part of speech for the sentence. JSON deserialization inflates
        /// the spec's wire abbreviation (e.g. <c>pron</c>) to a display name via
        /// <see cref="PartOfSpeechConverter"/>; the raw wire value is preserved in
        /// <see cref="RawPartOfSpeech"/>.
        /// </summary>
        [JsonConverter(typeof(PartOfSpeechConverter))]
        public string PartOfSpeech { get; set; } = string.Empty; // PS

        /// <summary>
        /// Gets or sets the raw wire part-of-speech value as it arrived (e.g. <c>pron</c>),
        /// before display inflation. Set when deserializing through
        /// <see cref="PipeDelimitedBreakdownConverter"/>.
        /// </summary>
        [JsonIgnore]
        public string RawPartOfSpeech { get; set; } = string.Empty;

        /// <summary>
        /// Gets the display (inflated) part-of-speech name for output, falling back to
        /// <see cref="PartOfSpeech"/> when no raw value was captured.
        /// </summary>
        [JsonIgnore]
        public string DisplayPartOfSpeech => string.IsNullOrEmpty(RawPartOfSpeech)
            ? PartOfSpeech
            : PartOfSpeechConverter.DisplayName(RawPartOfSpeech);

        /// <summary>
        /// Gets or sets the HSK level for the Chinesee semantic unit, which indicates the proficiency level required to understand it. HSK (Hanyu Shuiping Kaoshi) is a standardized test of Chinese language proficiency for non-native speakers, and the levels range from 1 to 6, with 1 being the most basic and 6 being the most advanced. Including the HSK level in the breakdown allows for better categorization and filtering of sentence components based on language proficiency, which can be useful for educational applications or any scenario where understanding the difficulty level of the language is important.
        /// </summary>
        [JsonConverter(typeof(HskLevelConverter))]
        public string HskLevel { get; set; } = string.Empty;             // H

        /// <summary>
        /// Gets or sets the raw wire HSK level value as it arrived (e.g. <c>1</c>),
        /// before display inflation. Set when deserializing through
        /// <see cref="PipeDelimitedBreakdownConverter"/>.
        /// </summary>
        [JsonIgnore]
        public string RawHskLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets the display (inflated) HSK level for output, falling back to
        /// <see cref="HskLevel"/> when no raw value was captured.
        /// </summary>
        [JsonIgnore]
        public string DisplayHskLevel => string.IsNullOrEmpty(RawHskLevel)
            ? HskLevel
            : HskLevelConverter.DisplayName(RawHskLevel);

        /// <summary>
        /// Gets or sets any additional notes about the sentence. This includes Sandhi and reordering notes, as well as any other relevant information that may be useful for understanding the sentence or its components. This field allows for flexibility in capturing a wide range of information that may not fit neatly into the other structured fields, while still keeping it associated with the specific segment of the sentence it pertains to.
        /// </summary>
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Reconstructs the pipe-delimited string for this entry.
        /// </summary>
        /// <returns>The pipe delimited string.</returns>
        public override string ToString()
        {
            return $"{Chinese}|{Pinyin}|{Gloss}|{PartOfSpeech}|{HskLevel}|{Note}";
        }
    }
}