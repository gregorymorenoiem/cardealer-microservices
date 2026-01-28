using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BackgroundRemovalService.Tests;

public class BackgroundRemovalJobTests
{
    [Fact]
    public void BackgroundRemovalJob_Create_ShouldInitializeWithPendingStatus()
    {
        // Arrange & Act
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            OutputFormat = OutputFormat.Png,
            OutputSize = ImageSize.Original,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        job.Status.Should().Be(ProcessingStatus.Pending);
        job.SourceImageUrl.Should().NotBeNullOrEmpty();
        job.OutputFormat.Should().Be(OutputFormat.Png);
        job.OutputSize.Should().Be(ImageSize.Original);
    }

    [Fact]
    public void BackgroundRemovalJob_MarkAsProcessing_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = ProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        job.MarkAsProcessing();

        // Assert
        job.Status.Should().Be(ProcessingStatus.Processing);
        job.ProcessingStartedAt.Should().NotBeNull();
        job.ProcessingStartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BackgroundRemovalJob_MarkAsCompleted_ShouldSetResultAndTimestamp()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = ProcessingStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            ProcessingStartedAt = DateTime.UtcNow.AddSeconds(-5)
        };

        var resultUrl = "https://s3.example.com/result.png";
        var sizeBytes = 1024 * 1024L; // 1MB
        var contentType = "image/png";
        var processingTimeMs = 5000L;

        // Act
        job.MarkAsCompleted(resultUrl, sizeBytes, contentType, processingTimeMs);

        // Assert
        job.Status.Should().Be(ProcessingStatus.Completed);
        job.ResultImageUrl.Should().Be(resultUrl);
        job.ResultFileSizeBytes.Should().Be(sizeBytes);
        job.ResultContentType.Should().Be(contentType);
        job.ProcessingTimeMs.Should().Be(processingTimeMs);
        job.ProcessingCompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void BackgroundRemovalJob_MarkAsFailed_ShouldSetError()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = ProcessingStatus.Processing,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var errorMessage = "API rate limit exceeded";
        var errorCode = "RATE_LIMIT";

        // Act
        job.MarkAsFailed(errorMessage, errorCode);

        // Assert
        job.Status.Should().Be(ProcessingStatus.Failed);
        job.ErrorMessage.Should().Be(errorMessage);
        job.ErrorCode.Should().Be(errorCode);
        job.ProcessingCompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void BackgroundRemovalJob_IncrementRetry_ShouldSetStatusAndIncrementCount()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = ProcessingStatus.Failed,
            RetryCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        job.IncrementRetry();

        // Assert
        job.Status.Should().Be(ProcessingStatus.Retrying);
        job.RetryCount.Should().Be(2);
    }

    [Fact]
    public void BackgroundRemovalJob_Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = ProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        job.Cancel();

        // Assert
        job.Status.Should().Be(ProcessingStatus.Cancelled);
    }

    [Fact]
    public void BackgroundRemovalJob_CanRetry_ShouldBeTrueWhenBelowMaxRetries()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            RetryCount = 0,
            MaxRetries = 3
        };

        // Act
        var canRetry = job.CanRetry();

        // Assert
        canRetry.Should().BeTrue();
    }

    [Fact]
    public void BackgroundRemovalJob_CanRetry_ShouldBeFalseWhenMaxRetriesReached()
    {
        // Arrange
        var job = new BackgroundRemovalJob
        {
            Id = Guid.NewGuid(),
            RetryCount = 3,
            MaxRetries = 3
        };

        // Act
        var canRetry = job.CanRetry();

        // Assert
        canRetry.Should().BeFalse();
    }
}
