using FluentAssertions;
using Video360Service.Domain.Entities;
using Xunit;

namespace Video360Service.Tests;

public class ExtractedFrameTests
{
    [Fact]
    public void ExtractedFrame_ShouldBeCreated_WithDefaultConstructor()
    {
        // Arrange & Act
        var frame = new ExtractedFrame();
        
        // Assert
        frame.Should().NotBeNull();
        frame.Id.Should().NotBeEmpty();
        frame.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public void ExtractedFrame_ShouldBeCreated_WithParameterizedConstructor()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        
        // Act
        var frame = new ExtractedFrame(
            video360JobId: jobId,
            frameIndex: 2,
            angleDegrees: 120,
            angleLabel: "120°",
            imageUrl: "https://cdn.example.com/frame2.jpg",
            fileSizeBytes: 150000,
            contentType: "image/jpeg"
        );
        
        // Assert
        frame.Video360JobId.Should().Be(jobId);
        frame.FrameIndex.Should().Be(2);
        frame.AngleDegrees.Should().Be(120);
        frame.AngleLabel.Should().Be("120°");
        frame.ImageUrl.Should().Be("https://cdn.example.com/frame2.jpg");
        frame.FileSizeBytes.Should().Be(150000);
        frame.ContentType.Should().Be("image/jpeg");
    }

    [Theory]
    [InlineData(0, 0, "Front")]
    [InlineData(1, 60, "60°")]
    [InlineData(2, 120, "120°")]
    [InlineData(3, 180, "Rear")]
    [InlineData(4, 240, "240°")]
    [InlineData(5, 300, "300°")]
    public void ExtractedFrame_ShouldHaveCorrectAnglesFor360View(int frameIndex, int expectedAngle, string expectedLabel)
    {
        // Arrange & Act
        var frame = new ExtractedFrame(
            video360JobId: Guid.NewGuid(),
            frameIndex: frameIndex,
            angleDegrees: expectedAngle,
            angleLabel: expectedLabel,
            imageUrl: $"https://cdn.example.com/frame{frameIndex}.jpg",
            fileSizeBytes: 100000,
            contentType: "image/jpeg"
        );
        
        // Assert
        frame.FrameIndex.Should().Be(frameIndex);
        frame.AngleDegrees.Should().Be(expectedAngle);
        frame.AngleLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public void ExtractedFrame_PropertiesCanBeSet()
    {
        // Arrange
        var frame = new ExtractedFrame();
        var jobId = Guid.NewGuid();
        
        // Act
        frame.Video360JobId = jobId;
        frame.FrameIndex = 3;
        frame.AngleDegrees = 180;
        frame.TimestampSeconds = 5.5;
        frame.ImageUrl = "https://cdn.example.com/frame3.jpg";
        frame.ThumbnailUrl = "https://cdn.example.com/frame3_thumb.jpg";
        frame.FileSizeBytes = 200000;
        frame.Width = 1920;
        frame.Height = 1080;
        
        // Assert
        frame.Video360JobId.Should().Be(jobId);
        frame.FrameIndex.Should().Be(3);
        frame.AngleDegrees.Should().Be(180);
        frame.TimestampSeconds.Should().Be(5.5);
        frame.ImageUrl.Should().Be("https://cdn.example.com/frame3.jpg");
        frame.ThumbnailUrl.Should().Be("https://cdn.example.com/frame3_thumb.jpg");
        frame.FileSizeBytes.Should().Be(200000);
        frame.Width.Should().Be(1920);
        frame.Height.Should().Be(1080);
    }
}
