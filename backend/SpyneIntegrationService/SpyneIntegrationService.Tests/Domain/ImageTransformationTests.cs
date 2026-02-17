using FluentAssertions;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Domain;

public class ImageTransformationTests
{
    [Fact]
    public void ImageTransformation_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var transformation = new ImageTransformation
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            TransformationType = TransformationType.BackgroundRemoval,
            BackgroundPreset = BackgroundPreset.Studio,
            Status = TransformationStatus.Pending
        };

        // Assert
        transformation.Id.Should().NotBeEmpty();
        transformation.Status.Should().Be(TransformationStatus.Pending);
        transformation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ImageTransformation_MarkAsProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var transformation = new ImageTransformation
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = TransformationStatus.Pending
        };

        // Act
        transformation.MarkAsProcessing("job-123");

        // Assert
        transformation.Status.Should().Be(TransformationStatus.Processing);
        transformation.SpyneJobId.Should().Be("job-123");
    }

    [Fact]
    public void ImageTransformation_MarkAsCompleted_ShouldUpdateAllFields()
    {
        // Arrange
        var transformation = new ImageTransformation
        {
            VehicleId = Guid.NewGuid(),
            OriginalImageUrl = "https://example.com/image.jpg",
            Status = TransformationStatus.Processing
        };

        // Act - MarkAsCompleted(transformedUrl, transformedUrlHd, processingTimeMs)
        transformation.MarkAsCompleted(
            "https://result.com/transformed.jpg",
            "https://result.com/transformed-hd.jpg",
            1500);

        // Assert
        transformation.Status.Should().Be(TransformationStatus.Completed);
        transformation.TransformedImageUrl.Should().Be("https://result.com/transformed.jpg");
        transformation.TransformedImageUrlHd.Should().Be("https://result.com/transformed-hd.jpg");
        transformation.ProcessingTimeMs.Should().Be(1500);
        transformation.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void ImageTransformation_MarkAsFailed_ShouldSetErrorMessage()
    {
        // Arrange
        var transformation = new ImageTransformation
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            Status = TransformationStatus.Processing
        };

        // Act
        transformation.MarkAsFailed("API timeout");

        // Assert
        transformation.Status.Should().Be(TransformationStatus.Failed);
        transformation.ErrorMessage.Should().Be("API timeout");
    }

    [Theory]
    [InlineData(TransformationType.BackgroundRemoval)]
    [InlineData(TransformationType.BackgroundReplacement)]
    [InlineData(TransformationType.Enhancement)]
    [InlineData(TransformationType.PlateMasking)]
    public void ImageTransformation_ShouldSupportAllTransformationTypes(TransformationType type)
    {
        // Arrange & Act
        var transformation = new ImageTransformation
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            TransformationType = type
        };

        // Assert
        transformation.TransformationType.Should().Be(type);
    }
}
