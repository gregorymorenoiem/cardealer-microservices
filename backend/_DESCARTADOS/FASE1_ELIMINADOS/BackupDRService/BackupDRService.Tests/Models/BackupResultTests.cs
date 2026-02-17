using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class BackupResultTests
{
    [Fact]
    public void BackupResult_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var result = new BackupResult();

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(BackupExecutionStatus.Pending);
        result.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CompletedAt.Should().BeNull();
        result.Duration.Should().BeNull();
    }

    [Fact]
    public void BackupResult_Success_ShouldCreateCompletedResult()
    {
        // Arrange
        var jobId = "job-123";
        var jobName = "Daily Backup";
        var filePath = "/backups/test.backup";
        var sizeBytes = 1024L * 1024L; // 1 MB
        var checksum = "abc123";

        // Act
        var result = BackupResult.Success(jobId, jobName, filePath, sizeBytes, checksum);

        // Assert
        result.JobId.Should().Be(jobId);
        result.JobName.Should().Be(jobName);
        result.Status.Should().Be(BackupExecutionStatus.Completed);
        result.FilePath.Should().Be(filePath);
        result.FileSizeBytes.Should().Be(sizeBytes);
        result.Checksum.Should().Be(checksum);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void BackupResult_Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var jobId = "job-123";
        var jobName = "Daily Backup";
        var errorMessage = "Connection failed";
        var errorDetails = "Timeout after 30 seconds";

        // Act
        var result = BackupResult.Failure(jobId, jobName, errorMessage, errorDetails);

        // Assert
        result.JobId.Should().Be(jobId);
        result.Status.Should().Be(BackupExecutionStatus.Failed);
        result.ErrorMessage.Should().Be(errorMessage);
        result.ErrorDetails.Should().Be(errorDetails);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void BackupResult_Running_ShouldCreateRunningResult()
    {
        // Arrange
        var jobId = "job-123";
        var jobName = "Daily Backup";

        // Act
        var result = BackupResult.Running(jobId, jobName);

        // Assert
        result.JobId.Should().Be(jobId);
        result.Status.Should().Be(BackupExecutionStatus.Running);
        result.CompletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(2048, "2.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    public void BackupResult_GetFormattedSize_ShouldReturnCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var result = new BackupResult { FileSizeBytes = bytes };

        // Act
        var formatted = result.GetFormattedSize();

        // Assert
        formatted.Should().Be(expected);
    }

    [Fact]
    public void BackupResult_Duration_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = new BackupResult
        {
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var duration = result.Duration;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.TotalMinutes.Should().BeApproximately(5, 0.1);
    }
}
