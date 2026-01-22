using FluentAssertions;
using FluentValidation.TestHelper;
using SpyneIntegrationService.Application.Features.Videos.Commands;
using SpyneIntegrationService.Application.Validators;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Validators;

public class GenerateVideoCommandValidatorTests
{
    private readonly GenerateVideoCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WithValidCommand()
    {
        // Arrange
        var command = new GenerateVideoCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = Enumerable.Range(1, 20)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList(),
            Style = VideoStyle.Cinematic,
            OutputFormat = VideoFormat.Mp4_1080p,
            DurationSeconds = 30
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WithLessThan5Images()
    {
        // Arrange
        var command = new GenerateVideoCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = new List<string>
            {
                "https://example.com/1.jpg",
                "https://example.com/2.jpg"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrls)
            .WithErrorMessage("Minimum 5 images required for video");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDurationTooShort()
    {
        // Arrange
        var command = new GenerateVideoCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = Enumerable.Range(1, 10)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList(),
            DurationSeconds = 2 // Too short
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Minimum duration is 5 seconds");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDurationTooLong()
    {
        // Arrange
        var command = new GenerateVideoCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = Enumerable.Range(1, 10)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList(),
            DurationSeconds = 600 // Too long (10 minutes)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Maximum duration is 300 seconds (5 minutes)");
    }
}
