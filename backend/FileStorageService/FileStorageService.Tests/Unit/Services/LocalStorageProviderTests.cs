using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using FileStorageService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FileStorageService.Tests.Unit.Services;

public class LocalStorageProviderTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalStorageProvider _sut;
    private readonly Mock<ILogger<LocalStorageProvider>> _loggerMock;

    public LocalStorageProviderTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testBasePath);

        var config = Options.Create(new StorageProviderConfig
        {
            BasePath = _testBasePath,
            ProviderType = StorageProviderType.Local
        });

        _loggerMock = new Mock<ILogger<LocalStorageProvider>>();
        _sut = new LocalStorageProvider(config, _loggerMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public void ProviderType_ShouldBeLocal()
    {
        _sut.ProviderType.Should().Be(StorageProviderType.Local);
    }

    [Fact]
    public async Task UploadAsync_ShouldCreateFile()
    {
        // Arrange
        var storageKey = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        await _sut.UploadAsync(storageKey, stream, "text/plain");

        // Assert
        var exists = await _sut.ExistsAsync(storageKey);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnContent()
    {
        // Arrange
        var storageKey = "test/download.txt";
        var content = "Test content for download"u8.ToArray();
        using var uploadStream = new MemoryStream(content);
        await _sut.UploadAsync(storageKey, uploadStream, "text/plain");

        // Act
        await using var downloadStream = await _sut.DownloadAsync(storageKey);
        using var reader = new StreamReader(downloadStream);
        var result = await reader.ReadToEndAsync();

        // Assert
        result.Should().Be("Test content for download");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        // Arrange
        var storageKey = "test/delete.txt";
        using var stream = new MemoryStream("To be deleted"u8.ToArray());
        await _sut.UploadAsync(storageKey, stream, "text/plain");

        // Act
        await _sut.DeleteAsync(storageKey);

        // Assert
        var exists = await _sut.ExistsAsync(storageKey);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CopyAsync_ShouldDuplicateFile()
    {
        // Arrange
        var sourceKey = "test/source.txt";
        var destKey = "test/copy.txt";
        using var stream = new MemoryStream("Original content"u8.ToArray());
        await _sut.UploadAsync(sourceKey, stream, "text/plain");

        // Act
        await _sut.CopyAsync(sourceKey, destKey);

        // Assert
        var sourceExists = await _sut.ExistsAsync(sourceKey);
        var destExists = await _sut.ExistsAsync(destKey);
        sourceExists.Should().BeTrue();
        destExists.Should().BeTrue();
    }

    [Fact]
    public async Task MoveAsync_ShouldRelocateFile()
    {
        // Arrange
        var sourceKey = "test/original.txt";
        var destKey = "test/moved.txt";
        using var stream = new MemoryStream("Content to move"u8.ToArray());
        await _sut.UploadAsync(sourceKey, stream, "text/plain");

        // Act
        await _sut.MoveAsync(sourceKey, destKey);

        // Assert
        var sourceExists = await _sut.ExistsAsync(sourceKey);
        var destExists = await _sut.ExistsAsync(destKey);
        sourceExists.Should().BeFalse();
        destExists.Should().BeTrue();
    }

    [Fact]
    public async Task GetFileSizeAsync_ShouldReturnCorrectSize()
    {
        // Arrange
        var storageKey = "test/sized.txt";
        var content = "1234567890"u8.ToArray(); // 10 bytes
        using var stream = new MemoryStream(content);
        await _sut.UploadAsync(storageKey, stream, "text/plain");

        // Act
        var size = await _sut.GetFileSizeAsync(storageKey);

        // Assert
        size.Should().Be(10);
    }

    [Fact]
    public async Task SetMetadataAsync_ShouldPersistMetadata()
    {
        // Arrange
        var storageKey = "test/metadata.txt";
        using var stream = new MemoryStream("Content"u8.ToArray());
        await _sut.UploadAsync(storageKey, stream, "text/plain");

        var metadata = new Dictionary<string, string>
        {
            ["author"] = "Test User",
            ["version"] = "1.0"
        };

        // Act
        await _sut.SetMetadataAsync(storageKey, metadata);
        var result = await _sut.GetMetadataAsync(storageKey);

        // Assert
        result.Should().ContainKey("author").WhoseValue.Should().Be("Test User");
        result.Should().ContainKey("version").WhoseValue.Should().Be("1.0");
    }

    [Fact]
    public async Task ListAsync_ShouldReturnMatchingFiles()
    {
        // Arrange
        var keys = new[] { "list/file1.txt", "list/file2.txt", "list/sub/file3.txt" };
        foreach (var key in keys)
        {
            using var stream = new MemoryStream("content"u8.ToArray());
            await _sut.UploadAsync(key, stream, "text/plain");
        }

        // Act
        var result = await _sut.ListAsync("list/");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task IsHealthyAsync_ShouldReturnTrue_WhenStorageIsAccessible()
    {
        // Act
        var result = await _sut.IsHealthyAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_ShouldReturnValidUrl()
    {
        // Arrange
        var storageKey = "test/presigned.txt";
        var expiry = TimeSpan.FromMinutes(30);

        // Act
        var result = await _sut.GenerateUploadUrlAsync(storageKey, "text/plain", expiry);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().NotBeNullOrEmpty();
        result.Type.Should().Be(PresignedUrlType.Upload);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_ShouldReturnValidUrl()
    {
        // Arrange
        var storageKey = "test/download-url.txt";
        var expiry = TimeSpan.FromMinutes(30);

        // Act
        var result = await _sut.GenerateDownloadUrlAsync(storageKey, expiry);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().NotBeNullOrEmpty();
        result.Type.Should().Be(PresignedUrlType.Download);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
