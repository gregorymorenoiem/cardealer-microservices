using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Interfaces;

public class StorageModelsTests
{
    #region StorageUploadResult Tests

    [Fact]
    public void StorageUploadResult_Succeeded_ShouldCreateSuccessResult()
    {
        // Act
        var result = StorageUploadResult.Succeeded("/path/to/file.bak", 2048, "checksum123");

        // Assert
        result.Success.Should().BeTrue();
        result.StoragePath.Should().Be("/path/to/file.bak");
        result.FileSizeBytes.Should().Be(2048);
        result.Checksum.Should().Be("checksum123");
        result.ErrorMessage.Should().BeNull();
        result.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StorageUploadResult_Failed_ShouldCreateFailedResult()
    {
        // Act
        var result = StorageUploadResult.Failed("Upload failed");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Upload failed");
    }

    [Fact]
    public void StorageUploadResult_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var result = new StorageUploadResult();

        // Assert
        result.Success.Should().BeFalse();
        result.StoragePath.Should().BeEmpty();
        result.FileSizeBytes.Should().Be(0);
        result.Checksum.Should().BeEmpty();
        result.ErrorMessage.Should().BeNull();
        result.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StorageUploadResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var uploadedAt = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = new StorageUploadResult
        {
            Success = true,
            StoragePath = "s3://bucket/file.bak",
            FileSizeBytes = 5000,
            Checksum = "sha256hash",
            ErrorMessage = null,
            UploadedAt = uploadedAt
        };

        // Assert
        result.Success.Should().BeTrue();
        result.StoragePath.Should().Be("s3://bucket/file.bak");
        result.FileSizeBytes.Should().Be(5000);
        result.Checksum.Should().Be("sha256hash");
        result.UploadedAt.Should().Be(uploadedAt);
    }

    #endregion

    #region StorageDownloadResult Tests

    [Fact]
    public void StorageDownloadResult_Succeeded_ShouldCreateSuccessResult()
    {
        // Act
        var result = StorageDownloadResult.Succeeded("/local/path/file.bak", 4096);

        // Assert
        result.Success.Should().BeTrue();
        result.LocalPath.Should().Be("/local/path/file.bak");
        result.FileSizeBytes.Should().Be(4096);
        result.ErrorMessage.Should().BeNull();
        result.DownloadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StorageDownloadResult_Failed_ShouldCreateFailedResult()
    {
        // Act
        var result = StorageDownloadResult.Failed("Download failed");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Download failed");
    }

    [Fact]
    public void StorageDownloadResult_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var result = new StorageDownloadResult();

        // Assert
        result.Success.Should().BeFalse();
        result.LocalPath.Should().BeEmpty();
        result.FileSizeBytes.Should().Be(0);
        result.ErrorMessage.Should().BeNull();
        result.DownloadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StorageDownloadResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var downloadedAt = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = new StorageDownloadResult
        {
            Success = true,
            LocalPath = "C:\\Downloads\\file.bak",
            FileSizeBytes = 8192,
            ErrorMessage = null,
            DownloadedAt = downloadedAt
        };

        // Assert
        result.Success.Should().BeTrue();
        result.LocalPath.Should().Be("C:\\Downloads\\file.bak");
        result.FileSizeBytes.Should().Be(8192);
        result.DownloadedAt.Should().Be(downloadedAt);
    }

    #endregion

    #region StorageFileInfo Tests

    [Fact]
    public void StorageFileInfo_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var info = new StorageFileInfo();

        // Assert
        info.Name.Should().BeEmpty();
        info.Path.Should().BeEmpty();
        info.SizeBytes.Should().Be(0);
        info.Checksum.Should().BeNull();
        info.ModifiedAt.Should().BeNull();
        info.Metadata.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void StorageFileInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-7);
        var modifiedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var info = new StorageFileInfo
        {
            Name = "backup_20240101.bak",
            Path = "/backups/daily/backup_20240101.bak",
            SizeBytes = 1024 * 1024 * 512, // 512 MB
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt,
            Checksum = "sha256checksumhere",
            StorageType = StorageType.AzureBlob,
            Metadata = new Dictionary<string, string> { ["retention"] = "30" }
        };

        // Assert
        info.Name.Should().Be("backup_20240101.bak");
        info.Path.Should().Be("/backups/daily/backup_20240101.bak");
        info.SizeBytes.Should().Be(1024 * 1024 * 512);
        info.CreatedAt.Should().Be(createdAt);
        info.ModifiedAt.Should().Be(modifiedAt);
        info.Checksum.Should().Be("sha256checksumhere");
        info.StorageType.Should().Be(StorageType.AzureBlob);
        info.Metadata.Should().ContainKey("retention");
    }

    [Fact]
    public void StorageFileInfo_Metadata_ShouldBeModifiable()
    {
        // Arrange
        var info = new StorageFileInfo();

        // Act
        info.Metadata["key1"] = "value1";
        info.Metadata["key2"] = "value2";

        // Assert
        info.Metadata.Should().HaveCount(2);
        info.Metadata["key1"].Should().Be("value1");
        info.Metadata["key2"].Should().Be("value2");
    }

    [Theory]
    [InlineData(StorageType.Local)]
    [InlineData(StorageType.AzureBlob)]
    [InlineData(StorageType.S3)]
    [InlineData(StorageType.Ftp)]
    public void StorageFileInfo_StorageType_ShouldAcceptAllTypes(StorageType storageType)
    {
        // Arrange & Act
        var info = new StorageFileInfo { StorageType = storageType };

        // Assert
        info.StorageType.Should().Be(storageType);
    }

    #endregion
}
