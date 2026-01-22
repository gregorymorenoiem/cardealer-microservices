using FluentValidation;
using SpyneIntegrationService.Application.Features.Images.Commands;

namespace SpyneIntegrationService.Application.Validators;

public class TransformBatchImagesCommandValidator : AbstractValidator<TransformBatchImagesCommand>
{
    public TransformBatchImagesCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("ImageUrls is required")
            .Must(urls => urls.Count <= 50)
            .WithMessage("Maximum 50 images allowed per batch");

        RuleForEach(x => x.ImageUrls)
            .Must(BeAValidUrl)
            .WithMessage("Each ImageUrl must be a valid URL");

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
