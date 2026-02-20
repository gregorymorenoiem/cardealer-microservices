using FluentAssertions;
using Video360Service.Domain.Enums;
using Xunit;

namespace Video360Service.Tests;

public class EnumsTests
{
    [Fact]
    public void Video360Provider_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<Video360Provider>().Should().HaveCount(5);
        
        Video360Provider.FfmpegApi.Should().BeDefined();
        Video360Provider.ApyHub.Should().BeDefined();
        Video360Provider.Cloudinary.Should().BeDefined();
        Video360Provider.Imgix.Should().BeDefined();
        Video360Provider.Shotstack.Should().BeDefined();
    }

    [Fact]
    public void ProcessingStatus_ShouldHaveAllExpectedValues()
    {
        // Assert
        ProcessingStatus.Pending.Should().BeDefined();
        ProcessingStatus.Uploading.Should().BeDefined();
        ProcessingStatus.Processing.Should().BeDefined();
        ProcessingStatus.Downloading.Should().BeDefined();
        ProcessingStatus.Completed.Should().BeDefined();
        ProcessingStatus.Failed.Should().BeDefined();
        ProcessingStatus.Cancelled.Should().BeDefined();
    }

    [Fact]
    public void ImageFormat_ShouldHaveAllExpectedValues()
    {
        // Assert
        ImageFormat.Jpeg.Should().BeDefined();
        ImageFormat.Png.Should().BeDefined();
        ImageFormat.WebP.Should().BeDefined();
    }

    [Fact]
    public void VideoQuality_ShouldHaveAllExpectedValues()
    {
        // Assert
        VideoQuality.Low.Should().BeDefined();
        VideoQuality.Medium.Should().BeDefined();
        VideoQuality.High.Should().BeDefined();
        VideoQuality.Ultra.Should().BeDefined();
    }

    [Theory]
    [InlineData(ProcessingStatus.Pending, 0)]
    [InlineData(ProcessingStatus.Uploading, 1)]
    [InlineData(ProcessingStatus.Processing, 2)]
    [InlineData(ProcessingStatus.Downloading, 3)]
    [InlineData(ProcessingStatus.Completed, 4)]
    [InlineData(ProcessingStatus.Failed, 5)]
    [InlineData(ProcessingStatus.Cancelled, 6)]
    public void ProcessingStatus_ShouldHaveCorrectIntValues(ProcessingStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(Video360Provider.FfmpegApi, 0)]
    [InlineData(Video360Provider.ApyHub, 1)]
    [InlineData(Video360Provider.Cloudinary, 2)]
    [InlineData(Video360Provider.Imgix, 3)]
    [InlineData(Video360Provider.Shotstack, 4)]
    public void Video360Provider_ShouldHaveCorrectIntValues(Video360Provider provider, int expectedValue)
    {
        // Assert
        ((int)provider).Should().Be(expectedValue);
    }
}
