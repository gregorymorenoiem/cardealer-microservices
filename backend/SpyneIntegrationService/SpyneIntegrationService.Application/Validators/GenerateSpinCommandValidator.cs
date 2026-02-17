using FluentValidation;
using SpyneIntegrationService.Application.Features.Spins.Commands;

namespace SpyneIntegrationService.Application.Validators;

public class GenerateSpinCommandValidator : AbstractValidator<GenerateSpinCommand>
{
    public GenerateSpinCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("At least one image URL is required")
            .Must(urls => urls.Count >= 8)
            .WithMessage("Minimum 8 images required for spin (recommended 36)")
            .Must(urls => urls.Count <= 72)
            .WithMessage("Maximum 72 images per spin");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("Image URL cannot be empty")
            .Must(BeAValidUrl)
            .WithMessage("Each URL must be a valid URL");

        RuleFor(x => x.BackgroundPreset)
            .IsInEnum()
            .WithMessage("Invalid background preset");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
