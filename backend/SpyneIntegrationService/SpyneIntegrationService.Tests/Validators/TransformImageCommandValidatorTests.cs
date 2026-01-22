using FluentAssertions;
using FluentValidation.TestHelper;
using SpyneIntegrationService.Application.Features.Images.Commands;
using SpyneIntegrationService.Application.Validators;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Validators;

public class TransformImageCommandValidatorTests
{
    private readonly TransformImageCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WithValidCommand()
    {
        // Arrange
        var command = new TransformImageCommand
        {
            VehicleId = Guid.NewGuid(),
            SourceImageUrl = "https://example.com/image.jpg",
            TransformationType = TransformationType.BackgroundRemoval,
            BackgroundPreset = BackgroundPreset.Studio
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehicleIdEmpty()
    {
        // Arrange
        var command = new TransformImageCommand
        {
            VehicleId = Guid.Empty,
            SourceImageUrl = "https://example.com/image.jpg"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
    }

    [Fact]
    public void Validate_ShouldFail_WhenSourceImageUrlEmpty()
    {
        // Arrange
        var command = new TransformImageCommand
        {
            VehicleId = Guid.NewGuid(),
            OriginalImageUrl = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalImageUrl);
    }

    [Fact]
    public void Validate_ShouldFail_WhenSourceImageUrlInvalid()
    {
        // Arrange
        var command = new TransformImageCommand
        {
            VehicleId = Guid.NewGuid(),
            OriginalImageUrl = "not-a-valid-url"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalImageUrl)
            .WithErrorMessage("OriginalImageUrl must be a valid URL");
    }
}
