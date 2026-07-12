//-----------------------------------------------------------------------
// <copyright file="FileOps.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AIFileProcessor.Interfaces;
    using Microsoft.Extensions.Logging;

    public class FileOps : IFileOps
    {
        private readonly ILogger<FileOps> _logger;

        public FileOps(ILogger<FileOps> logger)
        {
            _logger = logger;
        }

        public async Task<string> LoadFileAsync(string pathOfFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathOfFile))
                {
                    throw new ArgumentException("Path cannot be null or whitespace.", nameof(pathOfFile));
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(pathOfFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid file path provided: {Path}", pathOfFile);
                    throw new ArgumentException("The provided file path is invalid.", nameof(pathOfFile), ex);
                }

                _logger.LogInformation("Opening file name: {FileName}", fullPath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found: {FileName}", fullPath);
                    throw new FileNotFoundException("The specified file does not exist.", fullPath);
                }

                // Asynchronous read - frees the UI message loop while reading
                string fileContent = await File.ReadAllTextAsync(fullPath);
                _logger.LogInformation("Successfully read: {FileName}", fullPath);

                return fileContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading file: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> SaveFileAsync(string pathOfFile, string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathOfFile))
                {
                    throw new ArgumentException("Path cannot be null or whitespace.", nameof(pathOfFile));
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(pathOfFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid file path provided: {Path}", pathOfFile);
                    throw new ArgumentException("The provided file path is invalid.", nameof(pathOfFile), ex);
                }

                var directory = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directory))
                {
                    _logger.LogError("Unable to determine directory for path: {Path}", fullPath);
                    throw new ArgumentException("Unable to determine the directory for the provided path.", nameof(pathOfFile));
                }

                // Ensure the directory exists
                Directory.CreateDirectory(directory);

                _logger.LogInformation("Saving file name: {FileName}", fullPath);

                await File.WriteAllTextAsync(fullPath, data);
                _logger.LogInformation("Successfully saved: {FileName}", fullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving file: {Message}", ex.Message);
                throw;
            }
        }
    }
}