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
        /// <returns>A list of validation error messages. An empty list indicates valid options.</returns>
        public static List<string> Validate(ProcessingOptions options, string? selectedModelId = null)
        {
            var errors = new List<string>();

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

            if (string.IsNullOrWhiteSpace(options.Prompt))
            {
                errors.Add("Please load a prompt file before processing.");
            }

            if (string.IsNullOrWhiteSpace(options.Data))
            {
                errors.Add("Please load a data file before processing.");
            }

            return errors;
        }
    }
}