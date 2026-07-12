namespace AIFileProcessor.Core.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using AIFileProcessor;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class FileOpsTests
{
    private readonly Mock<ILogger<FileOps>> _loggerMock;
    private readonly FileOps _fileOps;

    public FileOpsTests()
    {
        _loggerMock = new Mock<ILogger<FileOps>>();
        _fileOps = new FileOps(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ValidLogger_InitializesInstance()
    {
        // Arrange & Act
        var instance = new FileOps(_loggerMock.Object);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task LoadFileAsync_ValidFile_ReturnsContent()
    {
        // Arrange
        string tempFile = Path.GetTempFileName();
        string expectedContent = "Hello, World!";
        await File.WriteAllTextAsync(tempFile, expectedContent);

        try
        {
            // Act
            string result = await _fileOps.LoadFileAsync(tempFile);

            // Assert
            Assert.Equal(expectedContent, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task LoadFileAsync_NullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.LoadFileAsync(null!));
    }

    [Fact]
    public async Task LoadFileAsync_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.LoadFileAsync(string.Empty));
    }

    [Fact]
    public async Task LoadFileAsync_WhitespacePath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.LoadFileAsync("   "));
    }

    [Fact]
    public async Task LoadFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _fileOps.LoadFileAsync(nonExistentFile));
    }

    [Fact]
    public async Task LoadFileAsync_InvalidPath_ThrowsArgumentException()
    {
        // Arrange - A path with a null character that will cause Path.GetFullPath to throw
        string invalidPath = "test\0invalid.txt";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.LoadFileAsync(invalidPath));
    }

    [Fact]
    public async Task SaveFileAsync_ValidPathAndData_ReturnsTrueAndWritesFile()
    {
        // Arrange
        string tempFile = Path.Combine(Path.GetTempPath(), $"save_test_{Guid.NewGuid()}.txt");
        string data = "Test content for saving";

        try
        {
            // Act
            bool result = await _fileOps.SaveFileAsync(tempFile, data);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(tempFile));
            string writtenContent = await File.ReadAllTextAsync(tempFile);
            Assert.Equal(data, writtenContent);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task SaveFileAsync_NullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.SaveFileAsync(null!, "data"));
    }

    [Fact]
    public async Task SaveFileAsync_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.SaveFileAsync(string.Empty, "data"));
    }

    [Fact]
    public async Task SaveFileAsync_WhitespacePath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.SaveFileAsync("   ", "data"));
    }

    [Fact]
    public async Task SaveFileAsync_InvalidPath_ThrowsArgumentException()
    {
        // Arrange - A path with a null character that will cause Path.GetFullPath to throw
        string invalidPath = "test\0invalid.txt";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fileOps.SaveFileAsync(invalidPath, "data"));
    }

    [Fact]
    public async Task SaveFileAsync_CreatesDirectoryIfNotExists_ReturnsTrue()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"save_dir_test_{Guid.NewGuid()}");
        string filePath = Path.Combine(tempDir, "testfile.txt");
        string data = "Content in new directory";

        try
        {
            // Act
            bool result = await _fileOps.SaveFileAsync(filePath, data);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(filePath));
            Assert.True(Directory.Exists(tempDir));
            string writtenContent = await File.ReadAllTextAsync(filePath);
            Assert.Equal(data, writtenContent);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}