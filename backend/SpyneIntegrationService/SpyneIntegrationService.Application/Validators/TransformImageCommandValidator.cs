using FluentValidation;
using SpyneIntegrationService.Application.Features.Images.Commands;

namespace SpyneIntegrationService.Application.Validators;

public class TransformImageCommandValidator : AbstractValidator<TransformImageCommand>
{
    public TransformImageCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.OriginalImageUrl)
            .NotEmpty()
            .WithMessage("OriginalImageUrl is required")
            .Must(BeAValidUrl)
            .WithMessage("OriginalImageUrl must be a valid URL");

        RuleFor(x => x.BackgroundPreset)
            .IsInEnum()
            .WithMessage("BackgroundPreset must be a valid value");
    }

    private static bool BeAValidUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url) && 
               (url.StartsWith("http://") || url.StartsWith("https://"));
    }
}
