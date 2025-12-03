using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class LocalStorageProviderTests : IDisposable
{
    private readonly Mock<ILogger<LocalStorageProvider>> _loggerMock;
    private readonly BackupOptions _options;
    private readonly LocalStorageProvider _provider;
    private readonly string _testDirectory;

    public LocalStorageProviderTests()
    {
        _loggerMock = new Mock<ILogger<LocalStorageProvider>>();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"BackupTests_{Guid.NewGuid()}");
        
        _options = new BackupOptions
        {
            LocalStoragePath = _testDirectory
        };

        Directory.CreateDirectory(_testDirectory);

        _provider = new LocalStorageProvider(
            _loggerMock.Object,
            Options.Create(_options)
        );
    }

    [Fact]
    public void StorageType_ShouldBeLocal()
    {
        // Assert
        _provider.StorageType.Should().Be(StorageType.Local);
    }

    [Fact]
    public async Task UploadAsync_WithValidFile_ShouldSucceed()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "source.txt");
        var content = "Test content";
        await File.WriteAllTextAsync(sourceFile, content);

        var destinationPath = "backups/uploaded.txt";

        // Act
        var result = await _provider.UploadAsync(sourceFile, destinationPath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FileSizeBytes.Should().BeGreaterThan(0);
        result.Checksum.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadAsync_WithNonExistentFile_ShouldFail()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");
        var destinationPath = "backups/uploaded.txt";

        // Act
        var result = await _provider.UploadAsync(nonExistentFile, destinationPath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task DownloadAsync_WithExistingFile_ShouldSucceed()
    {
        // Arrange
        var storagePath = "backups/test.txt";
        var fullStoragePath = Path.Combine(_testDirectory, storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullStoragePath)!);
        await File.WriteAllTextAsync(fullStoragePath, "Test content");

        var downloadPath = Path.Combine(_testDirectory, "downloaded.txt");

        // Act
        var result = await _provider.DownloadAsync(storagePath, downloadPath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FileSizeBytes.Should().BeGreaterThan(0);
        File.Exists(downloadPath).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_WithNonExistentFile_ShouldFail()
    {
        // Arrange
        var storagePath = "backups/nonexistent.txt";
        var downloadPath = Path.Combine(_testDirectory, "downloaded.txt");

        // Act
        var result = await _provider.DownloadAsync(storagePath, downloadPath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFile_ShouldSucceed()
    {
        // Arrange
        var storagePath = "backups/todelete.txt";
        var fullPath = Path.Combine(_testDirectory, storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "Test content");

        // Act
        var result = await _provider.DeleteAsync(storagePath);

        // Assert
        result.Should().BeTrue();
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var storagePath = "backups/nonexistent.txt";

        // Act
        var result = await _provider.DeleteAsync(storagePath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var storagePath = "backups/exists.txt";
        var fullPath = Path.Combine(_testDirectory, storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "Test content");

        // Act
        var result = await _provider.ExistsAsync(storagePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var storagePath = "backups/nonexistent.txt";

        // Act
        var result = await _provider.ExistsAsync(storagePath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileInfoAsync_WithExistingFile_ShouldReturnInfo()
    {
        // Arrange
        var storagePath = "backups/info.txt";
        var fullPath = Path.Combine(_testDirectory, storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "Test content");

        // Act
        var result = await _provider.GetFileInfoAsync(storagePath);

        // Assert
        result.Should().NotBeNull();
        result!.SizeBytes.Should().BeGreaterThan(0);
        result.ModifiedAt.Should().NotBeNull();
        result.ModifiedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetFileInfoAsync_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        var storagePath = "backups/nonexistent.txt";

        // Act
        var result = await _provider.GetFileInfoAsync(storagePath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "source.txt");
        await File.WriteAllTextAsync(sourceFile, "Test content");

        var destinationPath = "newdir/subdir/uploaded.txt";

        // Act
        var result = await _provider.UploadAsync(sourceFile, destinationPath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        var fullPath = Path.Combine(_testDirectory, "newdir/subdir");
        Directory.Exists(fullPath).Should().BeTrue();
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
