//-----------------------------------------------------------------------
// <copyright file="ProcessingOptions.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    public class ProcessingOptions
    {
        private string _data = string.Empty;
        private string _response = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingOptions"/> class with default values.
        /// </summary>
        /// <remarks>This class is intended to contain the options chosen for the class by the user. It is intended to be used as a configuration object for the processing logic. It is defined so that test cases may be easily implmented. We use Data and DataPath when a data file is loaded, and Response and ResponseData when a Response file is loaded.</remarks>
        public ProcessingOptions()
        {
            PromptPath = string.Empty;
            Prompt = string.Empty;
            DataPath = string.Empty;
            Data = string.Empty;
            Response = string.Empty;
            ResponsePath = string.Empty;
            ModelId = string.Empty;
            IdPrefix = string.Empty;
            IdStart = 1;
            IdPad = 3;
            LengthPad = 3;
            ModelReasoningLevels = new Dictionary<string, ReasoningLevel>();
            ModelMaxTokens = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets or sets the prompt string to be sent to the AI. This is expected to be read from the file specified in PromptPath, but is separated here for easier testing and flexibility.
        /// </summary>
        [JsonIgnore]
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the complete path to the prompt file.
        /// </summary>
        public string PromptPath { get; set; }

        /// <summary>
        /// Gets or sets the data string to be sent to the AI. This is expected to be read from the file specified in DataPath, but is separated here for easier testing and flexibility.
        /// </summary>
        [JsonIgnore]
        public string Data
        {
            get => _data;
            set
            {
                _response = string.Empty;
                ResponsePath = string.Empty;
                _data = value;
            }
        }

        /// <summary>
        /// Gets or sets the complete path to the data file.
        /// </summary>
        public string DataPath { get; set; }

        /// <summary>
        /// Gets or sets the response string to be sent to the AI. This is expected to be read from the file specified in ResponsePath, but is separated here for easier testing and flexibility.
        /// </summary>
        [JsonIgnore]
        public string Response
        {
            get => _response;
            set
            {
                _data = string.Empty;
                DataPath = string.Empty;
                _response = value;
            }
        }

        /// <summary>
        /// Gets or sets the complete path to the data file.
        /// </summary>
        [JsonIgnore]
        public string ResponsePath { get; set; }

        /// <summary>
        /// Gets or sets the ID of the AI model to use for processing. This is expected to be chosen by the user from a list of supported models, but is separated here for easier testing and flexibility.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Gets or sets the prefix to be used for generating unique IDs for each sentence. This is intended to help with tracking and reordering sentences after processing, especially when dealing with large blocks of text that are split into chunks. The prefix can be set by the user to easily identify and manage the generated IDs.
        /// </summary>
        public string IdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the starting number for generating unique IDs.
        /// </summary>
        public int IdStart { get; set; }

        /// <summary>
        /// Gets or sets the padding length for the numeric part of the generated IDs. This ensures that all IDs have a consistent format with leading zeros, which can be important for proper sorting and organization of sentences after processing.
        /// </summary>
        public int IdPad { get; set; }

        /// <summary>
        /// Gets or sets the padding length for the length indicator of each sentence. After importing into Anki, this may be used to filter the complexity of the sentences based on their length, allowing the user to focus on simpler sentences for easier learning or more complex sentences for advanced practice.
        /// </summary>
        public int LengthPad { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping model IDs to the user's last-selected
        /// reasoning level for that model. This enables per-model persistence: when
        /// the user switches between models, their previously selected reasoning
        /// level is restored. Keys are model IDs (e.g., "gpt-5.6-sol").
        /// </summary>
        public Dictionary<string, ReasoningLevel> ModelReasoningLevels { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping model IDs to the user's preferred
        /// maximum token count for that model. When a model is processed, the
        /// orchestrator looks up the value here and applies it to the provider
        /// settings. Keys are model IDs (e.g., "gemini-3.5-flash").
        /// </summary>
        public Dictionary<string, int> ModelMaxTokens { get; set; }
    }
}