//-----------------------------------------------------------------------
// <copyright file="IChineseTextProcessor.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AIFileProcessor.Models;
    using AIFileProcessor.Response;

    /// <summary>
    /// Interface for processing Chinese text blocks by chunking and executing them in parallel.
    /// </summary>
    public interface IChineseTextProcessor
    {
        /// <summary>
        /// Processes Chinese text blocks by chunking and executing them in parallel,
        /// using the constructor-injected (default) settings for each AI service.
        /// </summary>
        /// <param name="modelId">The ID of the AI model to use for processing.</param>
        /// <param name="prompt">The prompt to execute for each chunk of text.</param>
        /// <param name="data">The block of Chinese text to process.</param>
        /// <param name="chunking">The chunking configuration (lines per chunk and max concurrency).</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the AI API response.</returns>
        Task<AiApiResponse> ProcessChineseTextAsync(
            string modelId,
            string prompt,
            string data,
            ChunkingOptions chunking,
            CancellationToken ct = default);

        /// <summary>
        /// Processes Chinese text blocks by chunking and executing them in parallel,
        /// using per-user settings resolved from the <paramref name="providerSettings"/>
        /// dictionary.
        /// </summary>
        /// <param name="modelId">The ID of the AI model to use for processing.</param>
        /// <param name="prompt">The prompt to execute for each chunk of text.</param>
        /// <param name="data">The block of Chinese text to process.</param>
        /// <param name="providerSettings">A dictionary mapping provider names (e.g., "Gemini", "OpenAI", "DeepSeek") to their resolved settings objects.</param>
        /// <param name="chunking">The chunking configuration (lines per chunk and max concurrency).</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the AI API response.</returns>
        Task<AiApiResponse> ProcessChineseTextAsync(
            string modelId,
            string prompt,
            string data,
            IDictionary<string, IAiServiceSettings> providerSettings,
            ChunkingOptions chunking,
            CancellationToken ct = default);
    }
}