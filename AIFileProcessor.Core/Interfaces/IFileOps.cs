//-----------------------------------------------------------------------
// <copyright file="IFileOps.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Threading.Tasks;

    public interface IFileOps
    {
        /// <summary>
        /// Reads the selected file asynchronously and returns the text of the file.
        /// </summary>
        /// <param name="pathOfFile">The Complete path and FileName of the file to load.</param>
        /// <returns>The selected file contents, or an empty string if canceled or failed.</returns>
        Task<string> LoadFileAsync(string pathOfFile);

        /// <summary>
        /// Writes the selected file asynchronously and returns boolean success indicator.
        /// </summary>
        /// <param name="pathOfFile">The Complete path and FileName of the file to save.</param>
        /// <param name="data">The data to be written to the file.</param>
        /// <returns>Boolean indicating success.</returns>
        Task<bool> SaveFileAsync(string pathOfFile, string data);
    }
}