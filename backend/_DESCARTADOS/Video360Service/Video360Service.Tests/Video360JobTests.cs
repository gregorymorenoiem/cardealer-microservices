using FluentAssertions;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Xunit;

namespace Video360Service.Tests;

public class Video360JobTests
{
    [Fact]
    public void Video360Job_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "video.mp4"
        };

        // Assert
        job.Status.Should().Be(Video360JobStatus.Pending);
        job.FramesToExtract.Should().Be(6);
        job.Progress.Should().Be(0);
        job.Options.Should().NotBeNull();
        job.ExtractedFrames.Should().BeEmpty();
    }

    [Fact]
    public void Video360Job_StartProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "video.mp4"
        };

        // Act
        job.StartProcessing();

        // Assert
        job.Status.Should().Be(Video360JobStatus.Processing);
        job.ProcessingStartedAt.Should().NotBeNull();
        job.Progress.Should().Be(0);
    }

    [Fact]
    public void Video360Job_Complete_ShouldUpdateStatusAndCalculateDuration()
    {
        // Arrange
        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "video.mp4"
        };
        job.StartProcessing();

        // Act
        job.Complete();

        // Assert
        job.Status.Should().Be(Video360JobStatus.Completed);
        job.Progress.Should().Be(100);
        job.ProcessingCompletedAt.Should().NotBeNull();
        job.ProcessingDurationMs.Should().NotBeNull();
        job.ProcessingDurationMs.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Video360Job_Fail_ShouldSetErrorMessage()
    {
        // Arrange
        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "video.mp4"
        };
        job.StartProcessing();
        var errorMessage = "Video format not supported";

        // Act
        job.Fail(errorMessage);

        // Assert
        job.Status.Should().Be(Video360JobStatus.Failed);
        job.ErrorMessage.Should().Be(errorMessage);
        job.ProcessingCompletedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)] // Should be clamped to 100
    public void Video360Job_UpdateProgress_ShouldClampValues(int progress)
    {
        // Arrange
        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "video.mp4"
        };

        // Act
        job.UpdateProgress(progress);

        // Assert
        job.Progress.Should().BeInRange(0, 100);
    }
}

public class ExtractedFrameTests
{
    [Fact]
    public void ExtractedFrame_CreateForPosition_ShouldUseStandardViews()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var frame1 = ExtractedFrame.CreateForPosition(1, jobId);
        var frame2 = ExtractedFrame.CreateForPosition(2, jobId);
        var frame6 = ExtractedFrame.CreateForPosition(6, jobId);

        // Assert
        frame1.ViewName.Should().Be("Frente");
        frame1.AngleDegrees.Should().Be(0);
        frame1.IsPrimary.Should().BeTrue();

        frame2.ViewName.Should().Be("Frente-Derecha");
        frame2.AngleDegrees.Should().Be(60);
        frame2.IsPrimary.Should().BeFalse();

        frame6.ViewName.Should().Be("Izquierda");
        frame6.AngleDegrees.Should().Be(300);
        frame6.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void ExtractedFrame_CreateForPosition_ShouldHandleUnknownPosition()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var frame = ExtractedFrame.CreateForPosition(10, jobId);

        // Assert
        frame.ViewName.Should().Be("Vista-10");
        frame.SequenceNumber.Should().Be(10);
    }

    [Fact]
    public void ExtractedFrame_StandardViews_ShouldHaveSixViews()
    {
        // Assert
        ExtractedFrame.StandardViews.Should().HaveCount(6);
        ExtractedFrame.StandardViews.Keys.Should().Contain(new[] { 1, 2, 3, 4, 5, 6 });
    }
}

public class ProcessingOptionsTests
{
    [Fact]
    public void ProcessingOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        options.FrameCount.Should().Be(6);
        options.OutputWidth.Should().Be(1920);
        options.OutputHeight.Should().Be(1080);
        options.JpegQuality.Should().Be(90);
        options.OutputFormat.Should().Be("jpg");
        options.GenerateThumbnails.Should().BeTrue();
        options.SmartFrameSelection.Should().BeTrue();
        options.AutoCorrectExposure.Should().BeTrue();
        options.ThumbnailWidth.Should().Be(400);
    }
}

public class Video360JobStatusTests
{
    [Theory]
    [InlineData(Video360JobStatus.Pending, 0)]
    [InlineData(Video360JobStatus.Queued, 1)]
    [InlineData(Video360JobStatus.Processing, 2)]
    [InlineData(Video360JobStatus.Completed, 10)]
    [InlineData(Video360JobStatus.Failed, 20)]
    [InlineData(Video360JobStatus.Cancelled, 30)]
    public void Video360JobStatus_ShouldHaveCorrectValues(Video360JobStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }
}
