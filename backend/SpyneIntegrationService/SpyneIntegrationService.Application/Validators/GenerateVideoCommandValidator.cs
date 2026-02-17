using FluentValidation;
using SpyneIntegrationService.Application.Features.Videos.Commands;

namespace SpyneIntegrationService.Application.Validators;

public class GenerateVideoCommandValidator : AbstractValidator<GenerateVideoCommand>
{
    public GenerateVideoCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("At least one image URL is required")
            .Must(urls => urls.Count >= 5)
            .WithMessage("Minimum 5 images required for video")
            .Must(urls => urls.Count <= 100)
            .WithMessage("Maximum 100 images per video");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("Image URL cannot be empty")
            .Must(BeAValidUrl)
            .WithMessage("Each URL must be a valid URL");

        RuleFor(x => x.Style)
            .IsInEnum()
            .WithMessage("Invalid video style");

        RuleFor(x => x.OutputFormat)
            .IsInEnum()
            .WithMessage("Invalid output format");

        RuleFor(x => x.BackgroundPreset)
            .IsInEnum()
            .WithMessage("Invalid background preset");

        RuleFor(x => x.DurationSeconds)
            .GreaterThanOrEqualTo(5)
            .When(x => x.DurationSeconds.HasValue)
            .WithMessage("Minimum duration is 5 seconds")
            .LessThanOrEqualTo(300)
            .When(x => x.DurationSeconds.HasValue)
            .WithMessage("Maximum duration is 300 seconds (5 minutes)");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
