using FluentAssertions;
using FluentValidation.TestHelper;
using SpyneIntegrationService.Application.Features.Spins.Commands;
using SpyneIntegrationService.Application.Validators;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Validators;

public class GenerateSpinCommandValidatorTests
{
    private readonly GenerateSpinCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_With36Images()
    {
        // Arrange
        var command = new GenerateSpinCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = Enumerable.Range(1, 36)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList(),
            BackgroundPreset = BackgroundPreset.Studio
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WithLessThan8Images()
    {
        // Arrange
        var command = new GenerateSpinCommand
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
            .WithErrorMessage("Minimum 8 images required for spin (recommended 36)");
    }

    [Fact]
    public void Validate_ShouldFail_WithMoreThan72Images()
    {
        // Arrange
        var command = new GenerateSpinCommand
        {
            VehicleId = Guid.NewGuid(),
            ImageUrls = Enumerable.Range(1, 100)
                .Select(i => $"https://example.com/{i}.jpg")
                .ToList()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrls)
            .WithErrorMessage("Maximum 72 images per spin");
    }
}
