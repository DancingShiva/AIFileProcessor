//-----------------------------------------------------------------------
// <copyright file="AiApiResponse.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Response
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AIFileProcessor.Extensions;

    /// <summary>
    ///  This is a class to handle the response from the AI API. It is designed to be flexible and robust, capable of parsing JSON responses that may vary in structure or formatting. The class includes properties for scratchpad data, corrections made by the AI, and processed sentence rows. It also provides methods to populate its properties from a JSON string, handling various edge cases such as markdown code blocks and trailing commas.
    /// </summary>
    public class AiApiResponse
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AiApiResponse"/> class.
        /// Required for standard JSON deserialization frameworks.
        /// </summary>
        public AiApiResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiApiResponse"/> class by parsing a JSON string.
        /// </summary>
        /// <param name="jsonString">The JSON payload containing the response data.</param>
        /// <param name="options">Optional JSON serializer options to customize deserialization behavior.</param>
        /// <exception cref="ArgumentException">Thrown when the input string is null or whitespace.</exception>
        /// <exception cref="JsonException">Thrown if the JSON string is invalid or cannot be parsed.</exception>
        public AiApiResponse(string jsonString, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(jsonString));
            }

            if (!FillFromJsonResponse(jsonString, options ?? _options))
            {
                throw new JsonException("Failed to parse the JSON string into the AiApiResponse instance.");
            }
        }

        /// <summary>
        /// Gets or sets the value of the scratch pad that Gemini keeps to track the pinyin sandhi rules. This is used to help Gemini keep track of the changes that it has made to the pinyin in order to apply the correct sandhi rules. It is not used by the application, but it is included in the response for debugging purposes.
        /// </summary>
        [JsonPropertyName("SP")]
        public string Scratchpad { get; set; } = string.Empty;

        /// <summary>
        /// Gets or Sets the Corrections that the LLM has applied since the LLM is given the freedom to detect and correct common issues that may be caused by OCR errors or other inconsistencies in the input data. This property allows the LLM to return a list of any corrections it has made during processing. These corrections can be used for debugging, auditing, or even to provide feedback to users about potential issues in the original input. The application does not rely on this property for its core functionality, but it can be valuable for understanding how the LLM is interpreting and modifying the data.
        /// </summary>
        [JsonPropertyName("Corrections")]
        public List<string> Corrections { get; set; } = [];

        /// <summary>
        /// Gets or sets a list of processed sentence rows, each containing the original sentence and its corrected version.
        /// </summary>
        [JsonPropertyName("Sentences")]
        public List<ProcessedSentenceRow> ProcessedSentenceRows { get; set; } = [];

        /// <summary>
        /// Populates the current instance properties from a JSON response string.
        /// </summary>
        /// <param name="response">The JSON string containing the model response.</param>
        /// <param name="options">Optional JSON serializer options to customize deserialization behavior.</param>
        /// <returns>True if deserialization succeeded and properties were populated; otherwise, false.</returns>
        public bool FillFromJsonResponse(string response, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            // Work over a ReadOnlySpan<char>: Slice/Trim are zero-allocation views into the
            // original string, avoiding the repeated Substring/Trim copies on large responses.
            ReadOnlySpan<char> json = response.AsSpan().Trim();

            // Extract the content specifically wrapped inside markdown code blocks.
            int jsonStartIndex = json.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            if (jsonStartIndex != -1)
            {
                int contentStart = jsonStartIndex + 7;
                int jsonEndIndex = json.Slice(contentStart).IndexOf("```", StringComparison.OrdinalIgnoreCase);
                if (jsonEndIndex != -1)
                {
                    json = json.Slice(contentStart, jsonEndIndex);
                }
            }
            else
            {
                int generalCodeStartIndex = json.IndexOf("```", StringComparison.OrdinalIgnoreCase);
                if (generalCodeStartIndex != -1)
                {
                    int contentStart = generalCodeStartIndex + 3;
                    int generalCodeEndIndex = json.Slice(contentStart).IndexOf("```", StringComparison.OrdinalIgnoreCase);
                    if (generalCodeEndIndex != -1)
                    {
                        json = json.Slice(contentStart, generalCodeEndIndex);
                    }
                }
            }

            json = json.Trim();

            // Suffix and prefix fallback: extract everything between outer curly braces.
            if (!json.StartsWith('{'))
            {
                int firstBrace = json.IndexOf('{');
                int lastBrace = json.LastIndexOf('}');
                if (firstBrace != -1 && lastBrace > firstBrace)
                {
                    json = json.Slice(firstBrace, lastBrace - firstBrace + 1);
                }
            }

            try
            {
                // AI responses often have minor variations. Case-insensitivity and
                // allowing trailing commas prevent unnecessary parse failures.
                options ??= _options;

                var responseObject = JsonSerializer.Deserialize<AiApiResponse>(json, options);
                if (responseObject != null)
                {
                    this.Scratchpad = responseObject.Scratchpad;
                    this.Corrections = responseObject.Corrections;
                    this.ProcessedSentenceRows = responseObject.ProcessedSentenceRows;

                    // Apply Mandarin tone sandhi transformations to the pinyin fields
                    Processing.SandhiProcessor.ApplySandhi(this);

                    return true;
                }
            }
            catch (JsonException ex)
            {
                // Issue #11: Removed Debug.Fail which showed a modal dialog in debug builds.
                // Debug.WriteLine is sufficient for development diagnostics without
                // interrupting the application with a modal dialog.
                Debug.WriteLine($"JSON Deserialization failed: {ex.Message}. Response: {json.ToString()}");
            }

            return false;
        }
    }
}