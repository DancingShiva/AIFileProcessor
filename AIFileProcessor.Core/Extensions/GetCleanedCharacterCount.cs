//-----------------------------------------------------------------------
// <copyright file="GetCleanedCharacterCount.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extension methods for string analytics.
    /// </summary>
    public static partial class StringAnalyticsExtensions
    {
        /// <summary>
        /// Strips non-target characters and returns a zero-padded character count.
        /// </summary>
        /// <param name="inputText">The input string to analyze.</param>
        /// <param name="padding">The number of digits to pad the character count.</param>
        /// <returns>A zero-padded character count of the cleaned string.</returns>
        public static string GetCleanedCharacterCount(this string inputText, int padding)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return new string('0', padding) + "Chars";
            }

            string cleanedText = CleanTextRegex().Replace(inputText, string.Empty);
            return $"{cleanedText.Length.ToString($"D{padding}")}Chars";
        }

        // Removed RegexOptions.Compiled as it is handled by the source generator
        [GeneratedRegex(@"[\x00-\x1FA-Za-z\p{P}\p{S}\s\u002E\u002F\u005C\u3000\uFF0C\u3001\uFF1F\uFF01\uFF1B\uFF1A\u201C\u201D\u2018\u2019\u300C\u300D\u300E\u300F\u2026\uFF08\uFF09\u3010\u3011\u3014\u3015\u3002\u300A\u300B\u300C\u300D\u300E\u300F]")]
        private static partial Regex CleanTextRegex();
    }
}