using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class RestorePointTests
{
    [Fact]
    public void RestorePoint_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var point = new RestorePoint();

        // Assert
        point.Id.Should().NotBeNullOrEmpty();
        point.Status.Should().Be(RestorePointStatus.Available);
        point.IsAvailable.Should().BeTrue();
        point.IsVerified.Should().BeFalse();
        point.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RestorePoint_FromBackupResult_ShouldCreateCorrectly()
    {
        // Arrange
        var backupResult = BackupResult.Success("job-123", "Daily Backup", "/backups/test.backup", 1024, "checksum123");
        backupResult.BackupType = BackupType.Full;
        backupResult.Target = BackupTarget.PostgreSQL;
        backupResult.StorageType = StorageType.Local;

        // Act
        var point = RestorePoint.FromBackupResult(backupResult, "Test Point", "Test description");

        // Assert
        point.BackupResultId.Should().Be(backupResult.Id);
        point.JobId.Should().Be(backupResult.JobId);
        point.Name.Should().Be("Test Point");
        point.Description.Should().Be("Test description");
        point.FilePath.Should().Be(backupResult.FilePath);
        point.FileSizeBytes.Should().Be(backupResult.FileSizeBytes);
        point.Checksum.Should().Be(backupResult.Checksum);
        point.BackupType.Should().Be(BackupType.Full);
        point.Target.Should().Be(BackupTarget.PostgreSQL);
        point.StorageType.Should().Be(StorageType.Local);
    }

    [Fact]
    public void RestorePoint_IsExpired_ShouldReturnTrue_WhenExpired()
    {
        // Arrange
        var point = new RestorePoint
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        point.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void RestorePoint_IsExpired_ShouldReturnFalse_WhenNotExpired()
    {
        // Arrange
        var point = new RestorePoint
        {
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Act & Assert
        point.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void RestorePoint_IsExpired_ShouldReturnFalse_WhenNoExpiration()
    {
        // Arrange
        var point = new RestorePoint { ExpiresAt = null };

        // Act & Assert
        point.IsExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(2048, "2.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    public void RestorePoint_GetFormattedSize_ShouldReturnCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var point = new RestorePoint { FileSizeBytes = bytes };

        // Act
        var formatted = point.GetFormattedSize();

        // Assert
        formatted.Should().Be(expected);
    }
}
