using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class RestoreResultTests
{
    [Fact]
    public void RestoreResult_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var result = new RestoreResult();

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(RestoreExecutionStatus.Pending);
        result.Mode.Should().Be(RestoreMode.InPlace);
        result.InitiatedBy.Should().Be("system");
        result.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RestoreResult_Success_ShouldCreateCompletedResult()
    {
        // Arrange
        var restorePointId = "rp-123";
        var restorePointName = "Daily Backup Point";
        var bytesRestored = 1024L * 1024L;

        // Act
        var result = RestoreResult.Success(restorePointId, restorePointName, bytesRestored);

        // Assert
        result.RestorePointId.Should().Be(restorePointId);
        result.RestorePointName.Should().Be(restorePointName);
        result.Status.Should().Be(RestoreExecutionStatus.Completed);
        result.BytesRestored.Should().Be(bytesRestored);
        result.VerifiedIntegrity.Should().BeTrue();
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void RestoreResult_Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var restorePointId = "rp-123";
        var restorePointName = "Daily Backup Point";
        var errorMessage = "Database connection failed";
        var errorDetails = "Timeout";

        // Act
        var result = RestoreResult.Failure(restorePointId, restorePointName, errorMessage, errorDetails);

        // Assert
        result.RestorePointId.Should().Be(restorePointId);
        result.Status.Should().Be(RestoreExecutionStatus.Failed);
        result.ErrorMessage.Should().Be(errorMessage);
        result.ErrorDetails.Should().Be(errorDetails);
    }

    [Fact]
    public void RestoreResult_Running_ShouldCreateRunningResult()
    {
        // Arrange
        var restorePointId = "rp-123";
        var restorePointName = "Daily Backup Point";
        var initiatedBy = "admin";

        // Act
        var result = RestoreResult.Running(restorePointId, restorePointName, initiatedBy);

        // Assert
        result.RestorePointId.Should().Be(restorePointId);
        result.Status.Should().Be(RestoreExecutionStatus.Running);
        result.InitiatedBy.Should().Be(initiatedBy);
        result.CompletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(2048, "2.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    public void RestoreResult_GetFormattedBytesRestored_ShouldReturnCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var result = new RestoreResult { BytesRestored = bytes };

        // Act
        var formatted = result.GetFormattedBytesRestored();

        // Assert
        formatted.Should().Be(expected);
    }

    [Fact]
    public void RestoreResult_Duration_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = new RestoreResult
        {
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var duration = result.Duration;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.TotalMinutes.Should().BeApproximately(10, 0.1);
    }
}
