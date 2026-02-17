using FluentAssertions;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Domain;

public class VideoGenerationTests
{
    [Fact]
    public void VideoGeneration_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var video = new VideoGeneration
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrls = Enumerable.Range(1, 20)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList(),
            Style = VideoStyle.Cinematic,
            OutputFormat = VideoFormat.Mp4_1080p,
            IncludeMusic = true,
            Status = TransformationStatus.Pending
        };

        // Assert
        video.Id.Should().NotBeEmpty();
        video.SourceImageUrls.Should().HaveCount(20);
        video.Style.Should().Be(VideoStyle.Cinematic);
        video.IncludeMusic.Should().BeTrue();
    }

    [Theory]
    [InlineData(VideoStyle.Cinematic)]
    [InlineData(VideoStyle.Dynamic)]
    [InlineData(VideoStyle.Showcase)]
    [InlineData(VideoStyle.Social)]
    [InlineData(VideoStyle.Premium)]
    public void VideoGeneration_ShouldSupportAllStyles(VideoStyle style)
    {
        // Arrange & Act
        var video = new VideoGeneration
        {
            VehicleId = Guid.NewGuid(),
            Style = style
        };

        // Assert
        video.Style.Should().Be(style);
    }

    [Theory]
    [InlineData(VideoFormat.Mp4_720p)]
    [InlineData(VideoFormat.Mp4_1080p)]
    [InlineData(VideoFormat.Mp4_4K)]
    [InlineData(VideoFormat.Webm_720p)]
    [InlineData(VideoFormat.Webm_1080p)]
    public void VideoGeneration_ShouldSupportAllFormats(VideoFormat format)
    {
        // Arrange & Act
        var video = new VideoGeneration
        {
            VehicleId = Guid.NewGuid(),
            OutputFormat = format
        };

        // Assert
        video.OutputFormat.Should().Be(format);
    }

    [Fact]
    public void VideoGeneration_MarkAsCompleted_ShouldUpdateAllFields()
    {
        // Arrange
        var video = new VideoGeneration
        {
            VehicleId = Guid.NewGuid(),
            Status = TransformationStatus.Processing
        };

        // Act
        video.MarkAsCompleted(
            "https://cdn.spyne.ai/video.mp4",
            "https://cdn.spyne.ai/thumb.jpg",
            15_000_000,
            600000);

        // Assert
        video.Status.Should().Be(TransformationStatus.Completed);
        video.VideoUrl.Should().Be("https://cdn.spyne.ai/video.mp4");
        video.ThumbnailUrl.Should().NotBeNullOrEmpty();
        video.FileSizeBytes.Should().Be(15_000_000);
        video.ProcessingTimeMs.Should().Be(600000);
        video.CompletedAt.Should().NotBeNull();
    }
}
