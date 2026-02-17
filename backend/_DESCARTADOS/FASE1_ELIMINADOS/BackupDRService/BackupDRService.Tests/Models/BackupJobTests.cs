using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class BackupJobTests
{
    [Fact]
    public void BackupJob_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var job = new BackupJob();

        // Assert
        job.Id.Should().NotBeNullOrEmpty();
        job.Name.Should().BeEmpty();
        job.Type.Should().Be(BackupType.Full);
        job.Target.Should().Be(BackupTarget.PostgreSQL);
        job.StorageType.Should().Be(StorageType.Local);
        job.RetentionDays.Should().Be(30);
        job.IsEnabled.Should().BeTrue();
        job.CompressBackup.Should().BeTrue();
        job.EncryptBackup.Should().BeFalse();
        job.Status.Should().Be(BackupJobStatus.Idle);
        job.SuccessCount.Should().Be(0);
        job.FailureCount.Should().Be(0);
    }

    [Fact]
    public void BackupJob_CalculateNextRun_ShouldSetNextRunAt_WhenValidCron()
    {
        // Arrange
        var job = new BackupJob
        {
            Schedule = "0 0 * * *" // Daily at midnight
        };

        // Act
        job.CalculateNextRun();

        // Assert
        job.NextRunAt.Should().NotBeNull();
        job.NextRunAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void BackupJob_CalculateNextRun_ShouldSetNull_WhenNoSchedule()
    {
        // Arrange
        var job = new BackupJob { Schedule = "" };

        // Act
        job.CalculateNextRun();

        // Assert
        job.NextRunAt.Should().BeNull();
    }

    [Fact]
    public void BackupJob_CalculateNextRun_ShouldSetNull_WhenInvalidCron()
    {
        // Arrange
        var job = new BackupJob { Schedule = "invalid-cron" };

        // Act
        job.CalculateNextRun();

        // Assert
        job.NextRunAt.Should().BeNull();
    }

    [Fact]
    public void BackupJob_MarkSuccess_ShouldUpdateStatus()
    {
        // Arrange
        var job = new BackupJob
        {
            Status = BackupJobStatus.Running,
            SuccessCount = 5
        };

        // Act
        job.MarkSuccess();

        // Assert
        job.Status.Should().Be(BackupJobStatus.Idle);
        job.LastRunAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.SuccessCount.Should().Be(6);
    }

    [Fact]
    public void BackupJob_MarkFailure_ShouldUpdateStatus()
    {
        // Arrange
        var job = new BackupJob
        {
            Status = BackupJobStatus.Running,
            FailureCount = 2
        };

        // Act
        job.MarkFailure();

        // Assert
        job.Status.Should().Be(BackupJobStatus.Failed);
        job.LastRunAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.FailureCount.Should().Be(3);
    }

    [Fact]
    public void BackupJob_MarkRunning_ShouldSetRunningStatus()
    {
        // Arrange
        var job = new BackupJob { Status = BackupJobStatus.Idle };

        // Act
        job.MarkRunning();

        // Assert
        job.Status.Should().Be(BackupJobStatus.Running);
    }
}
