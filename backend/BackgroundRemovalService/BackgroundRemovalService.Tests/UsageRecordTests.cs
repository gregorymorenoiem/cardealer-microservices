using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BackgroundRemovalService.Tests;

public class UsageRecordTests
{
    [Fact]
    public void UsageRecord_Create_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var record = new UsageRecord
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            JobId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CostUsd = 0.20m,
            IsSuccess = true,
            ProcessingTimeMs = 1500,
            InputSizeBytes = 1024 * 1024,
            OutputSizeBytes = 512 * 1024,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        record.Id.Should().NotBeEmpty();
        record.Provider.Should().Be(BackgroundRemovalProvider.RemoveBg);
        record.CostUsd.Should().Be(0.20m);
        record.IsSuccess.Should().BeTrue();
        record.ProcessingTimeMs.Should().Be(1500);
    }

    [Fact]
    public void UsageRecord_ForFailedJob_ShouldHaveSuccessFalse()
    {
        // Arrange & Act
        var record = new UsageRecord
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.Slazzer,
            JobId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CostUsd = 0.0m, // No cost for failed jobs
            IsSuccess = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        record.IsSuccess.Should().BeFalse();
        record.CostUsd.Should().Be(0.0m);
    }

    [Theory]
    [InlineData(OutputFormat.Png)]
    [InlineData(OutputFormat.WebP)]
    [InlineData(OutputFormat.Jpeg)]
    [InlineData(OutputFormat.Auto)]
    public void OutputFormat_Enum_ShouldHaveExpectedValues(OutputFormat format)
    {
        // Assert
        Enum.IsDefined(typeof(OutputFormat), format).Should().BeTrue();
    }

    [Theory]
    [InlineData(ImageSize.Original)]
    [InlineData(ImageSize.Preview)]
    [InlineData(ImageSize.Small)]
    [InlineData(ImageSize.Medium)]
    [InlineData(ImageSize.Large)]
    [InlineData(ImageSize.FullHD)]
    [InlineData(ImageSize.UltraHD)]
    public void ImageSize_Enum_ShouldHaveExpectedValues(ImageSize size)
    {
        // Assert
        Enum.IsDefined(typeof(ImageSize), size).Should().BeTrue();
    }

    [Theory]
    [InlineData(ProcessingStatus.Pending)]
    [InlineData(ProcessingStatus.Processing)]
    [InlineData(ProcessingStatus.Completed)]
    [InlineData(ProcessingStatus.Failed)]
    [InlineData(ProcessingStatus.Cancelled)]
    [InlineData(ProcessingStatus.Retrying)]
    public void ProcessingStatus_Enum_ShouldHaveExpectedValues(ProcessingStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(ProcessingStatus), status).Should().BeTrue();
    }
}
