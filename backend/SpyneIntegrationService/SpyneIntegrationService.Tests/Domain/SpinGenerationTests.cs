using FluentAssertions;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Domain;

public class SpinGenerationTests
{
    [Fact]
    public void SpinGeneration_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var spin = new SpinGeneration
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrls = new List<string>
            {
                "https://example.com/1.jpg",
                "https://example.com/2.jpg"
            },
            BackgroundPreset = BackgroundPreset.Showroom,
            RequiredImageCount = 36,
            ReceivedImageCount = 2,
            Status = TransformationStatus.Pending
        };

        // Assert
        spin.Id.Should().NotBeEmpty();
        spin.SourceImageUrls.Should().HaveCount(2);
        spin.Status.Should().Be(TransformationStatus.Pending);
    }

    [Fact]
    public void SpinGeneration_HasSufficientImages_ShouldReturnTrue_When36Images()
    {
        // Arrange
        var spin = new SpinGeneration
        {
            VehicleId = Guid.NewGuid(),
            RequiredImageCount = 36,
            ReceivedImageCount = 36
        };

        // Act & Assert
        spin.HasSufficientImages().Should().BeTrue();
    }

    [Fact]
    public void SpinGeneration_HasSufficientImages_ShouldReturnFalse_WhenInsufficient()
    {
        // Arrange
        var spin = new SpinGeneration
        {
            VehicleId = Guid.NewGuid(),
            RequiredImageCount = 36,
            ReceivedImageCount = 10
        };

        // Act & Assert
        spin.HasSufficientImages().Should().BeFalse();
    }

    [Fact]
    public void SpinGeneration_MarkAsCompleted_ShouldUpdateAllFields()
    {
        // Arrange
        var spin = new SpinGeneration
        {
            VehicleId = Guid.NewGuid(),
            Status = TransformationStatus.Processing
        };

        // Act
        spin.MarkAsCompleted(
            "https://spyne.ai/spin/viewer/123",
            "<iframe src='...'></iframe>",
            "https://spyne.ai/spin/fallback.jpg",
            300000);

        // Assert
        spin.Status.Should().Be(TransformationStatus.Completed);
        spin.SpinViewerUrl.Should().Be("https://spyne.ai/spin/viewer/123");
        spin.SpinEmbedCode.Should().Contain("iframe");
        spin.FallbackImageUrl.Should().NotBeNullOrEmpty();
        spin.ProcessingTimeMs.Should().Be(300000);
        spin.CompletedAt.Should().NotBeNull();
    }
}
