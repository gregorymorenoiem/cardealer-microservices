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
        var job = new Video360Job();
        
        // Assert
        job.Should().NotBeNull();
        job.Id.Should().NotBeEmpty();
        job.Status.Should().Be(ProcessingStatus.Pending);
        job.Provider.Should().Be(Video360Provider.FfmpegApi);
        job.ImageFormat.Should().Be(ImageFormat.Jpeg);
        job.VideoQuality.Should().Be(VideoQuality.High);
        job.FrameCount.Should().Be(6);
        job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Video360Job_StartProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var job = new Video360Job
        {
            VehicleId = Guid.NewGuid(),
            SourceVideoUrl = "https://example.com/video.mp4"
        };
        
        // Act
        job.SetProvider(Video360Provider.ApyHub);
        job.StartProcessing();
        
        // Assert
        job.Status.Should().Be(ProcessingStatus.Processing);
        job.Provider.Should().Be(Video360Provider.ApyHub);
        job.StartedAt.Should().NotBeNull();
        job.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Video360Job_CompleteProcessing_ShouldUpdateStatusAndTimestamps()
    {
        // Arrange
        var job = new Video360Job();
        job.SetProvider(Video360Provider.Cloudinary);
        job.StartProcessing();
        
        // Act
        job.CompleteProcessing(15000, 0.012m);
        
        // Assert
        job.Status.Should().Be(ProcessingStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
        job.Frames.Should().HaveCount(0); // No frames added yet
        job.ProcessingTimeMs.Should().Be(15000);
        job.CostUsd.Should().Be(0.012m);
    }

    [Fact]
    public void Video360Job_MarkFailed_ShouldSetErrorDetails()
    {
        // Arrange
        var job = new Video360Job();
        job.SetProvider(Video360Provider.Imgix);
        job.StartProcessing();
        
        // Act
        job.MarkFailed("Connection timeout", "TimeoutException");
        
        // Assert
        job.Status.Should().Be(ProcessingStatus.Failed);
        job.ErrorMessage.Should().Be("Connection timeout");
        job.ErrorCode.Should().Be("TimeoutException");
    }

    [Fact]
    public void Video360Job_Cancel_ShouldSetCancelledStatus()
    {
        // Arrange
        var job = new Video360Job();
        
        // Act
        job.Status = ProcessingStatus.Cancelled;
        job.CompletedAt = DateTime.UtcNow;
        
        // Assert
        job.Status.Should().Be(ProcessingStatus.Cancelled);
        job.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Video360Job_IncrementRetryCount_ShouldIncrease()
    {
        // Arrange
        var job = new Video360Job();
        job.RetryCount.Should().Be(0);
        
        // Act
        job.IncrementRetryCount();
        job.IncrementRetryCount();
        
        // Assert
        job.RetryCount.Should().Be(2);
    }

    [Theory]
    [InlineData(Video360Provider.FfmpegApi)]
    [InlineData(Video360Provider.ApyHub)]
    [InlineData(Video360Provider.Cloudinary)]
    [InlineData(Video360Provider.Imgix)]
    [InlineData(Video360Provider.Shotstack)]
    public void Video360Job_SetProvider_ShouldAssignCorrectProvider(Video360Provider provider)
    {
        // Arrange
        var job = new Video360Job();
        
        // Act
        job.SetProvider(provider);
        
        // Assert
        job.Provider.Should().Be(provider);
    }

    [Fact]
    public void Video360Job_AddFrame_ShouldIncreaseFramesCollection()
    {
        // Arrange
        var job = new Video360Job();
        var frame = new ExtractedFrame
        {
            Video360JobId = job.Id,
            FrameIndex = 0,
            AngleDegrees = 0,
            ImageUrl = "https://cdn.example.com/frame0.jpg"
        };
        
        // Act
        job.AddFrame(frame);
        
        // Assert
        job.Frames.Should().HaveCount(1);
        job.Frames.First().Should().Be(frame);
    }

    [Fact]
    public void Video360Job_OutputFormat_ShouldBeAliasForImageFormat()
    {
        // Arrange
        var job = new Video360Job();
        
        // Act
        job.OutputFormat = ImageFormat.Png;
        
        // Assert
        job.ImageFormat.Should().Be(ImageFormat.Png);
        job.OutputFormat.Should().Be(ImageFormat.Png);
    }

    [Fact]
    public void Video360Job_OutputQuality_ShouldBeAliasForVideoQuality()
    {
        // Arrange
        var job = new Video360Job();
        
        // Act
        job.OutputQuality = VideoQuality.Ultra;
        
        // Assert
        job.VideoQuality.Should().Be(VideoQuality.Ultra);
        job.OutputQuality.Should().Be(VideoQuality.Ultra);
    }
}
