//-----------------------------------------------------------------------
// <copyright file="AiModelInfo.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents an AI model's display name and unique identifier.
    /// Used as the single source of truth for model metadata across services and UI.
    /// </summary>
    public class AiModelInfo
    {
        // Fields are initialized at declaration to satisfy nullable reference
        // type analysis, even though the constructor always sets them.
        private string _modelName = string.Empty;
        private string _modelId = string.Empty;
        private List<ReasoningLevel> _supportedReasoningLevels = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="AiModelInfo"/> class
        /// with no supported reasoning levels.
        /// </summary>
        /// <param name="modelName">The human-readable display name of the model.</param>
        /// <param name="modelId">The unique identifier used by the AI service API.</param>
        public AiModelInfo(string modelName, string modelId)
        {
            _modelName = modelName;
            _modelId = modelId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiModelInfo"/> class
        /// with the specified supported reasoning levels.
        /// </summary>
        /// <param name="modelName">The human-readable display name of the model.</param>
        /// <param name="modelId">The unique identifier used by the AI service API.</param>
        /// <param name="supportedReasoningLevels">The reasoning levels supported by this model.</param>
        public AiModelInfo(string modelName, string modelId, List<ReasoningLevel> supportedReasoningLevels)
        {
            _modelName = modelName;
            _modelId = modelId;
            _supportedReasoningLevels = supportedReasoningLevels;
        }

        /// <summary>
        /// Gets or sets the human-readable display name of the model.
        /// </summary>
        public string ModelName
        {
            get => _modelName;
            set => _modelName = value;
        }

        /// <summary>
        /// Gets or sets the unique identifier used by the AI service API.
        /// </summary>
        public string ModelId
        {
            get => _modelId;
            set => _modelId = value;
        }

        /// <summary>
        /// Gets or sets the list of reasoning levels supported by this model.
        /// </summary>
        public List<ReasoningLevel> SupportedReasoningLevels
        {
            get => _supportedReasoningLevels;
            set => _supportedReasoningLevels = value;
        }
    }
}