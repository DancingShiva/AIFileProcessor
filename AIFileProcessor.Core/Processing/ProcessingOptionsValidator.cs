//-----------------------------------------------------------------------
// <copyright file="ProcessingOptionsValidator.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Processing
{
    using System.Collections.Generic;
    using AIFileProcessor.Models;

    /// <summary>
    /// Validates <see cref="ProcessingOptions"/> and returns a list of validation errors.
    /// Extracted from MainForm to enable sharing between UI frontends.
    /// </summary>
    public static class ProcessingOptionsValidator
    {
        /// <summary>
        /// Validates the specified processing options and returns a list of error messages.
        /// </summary>
        /// <param name="options">The processing options to validate.</param>
        /// <param name="selectedModelId">The selected model ID, or null if no model is selected.</param>
        /// <param name="maxInputSize">The maximum allowed input size in characters.</param>
        /// <param name="maxInputLines">The maximum allowed input line count.</param>
        /// <returns>A list of validation error messages. An empty list indicates valid options.</returns>
        public static List<string> Validate(ProcessingOptions options, string? selectedModelId = null, int maxInputSize = 32_768, int maxInputLines = 400)
        {
            var errors = new List<string>();

            // Field/UI order, all errors collected so the caller can present them in one pass.
            if (string.IsNullOrWhiteSpace(selectedModelId))
            {
                errors.Add("Please select a model before processing.");
            }

            if (string.IsNullOrWhiteSpace(options.IdPrefix))
            {
                errors.Add("Please enter a prefix before processing.");
            }

            if (options.IdStart <= 0)
            {
                errors.Add("The starting index must be a positive numeric value.");
            }

            if (options.IdPad <= 0)
            {
                errors.Add("The value for the Id padding must be a positive integer.");
            }

            if (options.LengthPad <= 0)
            {
                errors.Add("The value for the length padding must be a positive integer.");
            }

            if (options.Chunking.ChunkSize <= 0)
            {
                errors.Add("The chunk size must be a positive integer.");
            }

            if (options.Chunking.MaxConcurrentChunks <= 0)
            {
                errors.Add("The maximum concurrent chunks must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(options.Prompt))
            {
                errors.Add("Please load a prompt file before processing.");
            }

            if (string.IsNullOrWhiteSpace(options.Data))
            {
                errors.Add("Please load a data file before processing.");
            }
            else if (!ContainsChineseIdeograph(options.Data))
            {
                // Pre-flight rejection for payloads with no Chinese lexical content. The
                // headless generator can only emit hallucinated rows for such input, so
                // the call is blocked before any tokens are spent.
                errors.Add("The data file contains no Chinese text to process.");
            }
            else if (options.Data.Length > maxInputSize)
            {
                errors.Add($"The data file exceeds the maximum allowed size of {maxInputSize:N0} characters.");
            }
            else
            {
                int lineCount = CountLines(options.Data);
                if (lineCount > maxInputLines)
                {
                    errors.Add($"The data file exceeds the maximum allowed line count of {maxInputLines:N0} lines.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Determines whether the text contains at least one CJK unified ideograph
        /// (the ranges that carry Chinese lexical content).
        /// </summary>
        /// <param name="text">The data text to inspect.</param>
        /// <returns><see langword="true"/> if a CJK ideograph is present; otherwise <see langword="false"/>.</returns>
        private static bool ContainsChineseIdeograph(string text)
        {
            foreach (char c in text)
            {
                // '一' and '鿿' are CJK Unified Ideographs U+4E00..U+9FFF
                // '㐀' and '䶿' are CJK Extension A U+3400..U+4DBF
                // '豈' and '﫿' are CJK Compatibility Ideographs U+F900..U+FAFF
                if (c is >= '一' and <= '鿿'
                    or >= '㐀' and <= '䶿'
                    or >= '豈' and <= '﫿')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the number of lines in the text by counting line separators
        /// (\r\n, \r, \n) without allocating a string array.
        /// </summary>
        /// <param name="text">The text to count lines in.</param>
        /// <returns>The number of lines (minimum 1 for non-empty text).</returns>
        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    count++;
                }
                else if (c == '\r')
                {
                    count++;

                    // Skip the \n in \r\n pairs to avoid double-counting
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                }
            }

            return count;
        }
    }
}