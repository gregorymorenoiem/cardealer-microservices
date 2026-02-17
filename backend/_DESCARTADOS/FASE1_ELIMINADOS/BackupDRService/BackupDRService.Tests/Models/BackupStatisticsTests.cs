using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class BackupStatisticsTests
{
    [Fact]
    public void BackupStatistics_Empty_ShouldReturnZeroValues()
    {
        // Arrange & Act
        var stats = BackupStatistics.Empty();

        // Assert
        stats.TotalJobs.Should().Be(0);
        stats.TotalBackups.Should().Be(0);
        stats.TotalRestorePoints.Should().Be(0);
        stats.TotalRestores.Should().Be(0);
        stats.TotalStorageUsedBytes.Should().Be(0);
        stats.SuccessRate.Should().Be(0);
        stats.RestoreSuccessRate.Should().Be(0);
    }

    [Fact]
    public void BackupStatistics_SuccessRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var stats = new BackupStatistics
        {
            TotalBackups = 100,
            SuccessfulBackups = 95
        };

        // Act
        var rate = stats.SuccessRate;

        // Assert
        rate.Should().Be(95);
    }

    [Fact]
    public void BackupStatistics_RestoreSuccessRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var stats = new BackupStatistics
        {
            TotalRestores = 50,
            SuccessfulRestores = 48
        };

        // Act
        var rate = stats.RestoreSuccessRate;

        // Assert
        rate.Should().Be(96);
    }

    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(2048, "2.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    public void BackupStatistics_GetFormattedStorageUsed_ShouldReturnCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var stats = new BackupStatistics { TotalStorageUsedBytes = bytes };

        // Act
        var formatted = stats.GetFormattedStorageUsed();

        // Assert
        formatted.Should().Be(expected);
    }

    [Fact]
    public void BackupStatistics_WithJobCounts_ShouldSetValues()
    {
        // Arrange
        var stats = BackupStatistics.Empty();

        // Act
        stats.WithJobCounts(10, 8, 2);

        // Assert
        stats.TotalJobs.Should().Be(10);
        stats.EnabledJobs.Should().Be(8);
        stats.DisabledJobs.Should().Be(2);
        stats.RunningJobs.Should().Be(2);
    }

    [Fact]
    public void BackupStatistics_WithBackupCounts_ShouldSetValues()
    {
        // Arrange
        var stats = BackupStatistics.Empty();

        // Act
        stats.WithBackupCounts(100, 90, 8, 2);

        // Assert
        stats.TotalBackups.Should().Be(100);
        stats.SuccessfulBackups.Should().Be(90);
        stats.FailedBackups.Should().Be(8);
        stats.PendingBackups.Should().Be(2);
    }

    [Fact]
    public void BackupStatistics_WithRestorePointCounts_ShouldSetValues()
    {
        // Arrange
        var stats = BackupStatistics.Empty();

        // Act
        stats.WithRestorePointCounts(50, 45, 5);

        // Assert
        stats.TotalRestorePoints.Should().Be(50);
        stats.AvailableRestorePoints.Should().Be(45);
        stats.ExpiredRestorePoints.Should().Be(5);
    }

    [Fact]
    public void BackupStatistics_WithRestoreCounts_ShouldSetValues()
    {
        // Arrange
        var stats = BackupStatistics.Empty();

        // Act
        stats.WithRestoreCounts(25, 24, 1);

        // Assert
        stats.TotalRestores.Should().Be(25);
        stats.SuccessfulRestores.Should().Be(24);
        stats.FailedRestores.Should().Be(1);
    }
}
