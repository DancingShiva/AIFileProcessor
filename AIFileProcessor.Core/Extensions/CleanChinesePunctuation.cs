//-----------------------------------------------------------------------
// <copyright file="CleanChinesePunctuation.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// This is a partial class since the RegEx is generated at runtime.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// This makes sure that there are no extraneous spaces around the Chinese punctuation. The half-width Chinese characters are converted to full-width characters as part of the initial cleanup by the prompt.
        /// </summary>
        /// <param name="input">The input string to clean.</param>
        /// <returns>The cleaned string.</returns>
        public static string CleanChinesePunctuation(this string? input)
        {
            // Intercepts nulls, "", and whitespace, normalizing them all to a safe empty string
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return PunctuationSpaceRegex().Replace(input, string.Empty);
        }

        [GeneratedRegex(@"(?<=[【】，。！？：；”》“《（）、％＄￥])\s+|\s+(?=[【】，。！？：；”》“《（）、％＄￥])")]
        private static partial Regex PunctuationSpaceRegex();
    }
}