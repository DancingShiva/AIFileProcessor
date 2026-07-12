//-----------------------------------------------------------------------
// <copyright file="AiModelInfo.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Models
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AiModelInfo"/> class.
        /// </summary>
        /// <param name="modelName">The human-readable display name of the model.</param>
        /// <param name="modelId">The unique identifier used by the AI service API.</param>
        public AiModelInfo(string modelName, string modelId)
        {
            _modelName = modelName;
            _modelId = modelId;
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
    }
}