using FluentValidation;
using Vehicle360ProcessingService.Application.DTOs;
using Vehicle360ProcessingService.Application.Features.Commands;

namespace Vehicle360ProcessingService.Application.Validators;

public class StartProcessingRequestValidator : AbstractValidator<StartProcessingRequest>
{
    public StartProcessingRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.FrameCount)
            .InclusiveBetween(4, 12)
            .WithMessage("FrameCount must be between 4 and 12");

        When(x => x.Options != null, () =>
        {
            RuleFor(x => x.Options!.OutputWidth)
                .InclusiveBetween(640, 4096)
                .WithMessage("OutputWidth must be between 640 and 4096");

            RuleFor(x => x.Options!.OutputHeight)
                .InclusiveBetween(480, 2160)
                .WithMessage("OutputHeight must be between 480 and 2160");

            RuleFor(x => x.Options!.OutputFormat)
                .Must(f => new[] { "png", "jpg", "webp" }.Contains(f.ToLower()))
                .WithMessage("OutputFormat must be png, jpg, or webp");

            RuleFor(x => x.Options!.JpegQuality)
                .InclusiveBetween(50, 100)
                .WithMessage("JpegQuality must be between 50 and 100");
        });
    }
}

public class StartVehicle360ProcessingCommandValidator : AbstractValidator<StartVehicle360ProcessingCommand>
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".avi", ".webm", ".mkv" };
    private const long MaxFileSizeBytes = 500 * 1024 * 1024; // 500 MB

    public StartVehicle360ProcessingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.FrameCount)
            .InclusiveBetween(4, 12)
            .WithMessage("FrameCount must be between 4 and 12");

        // Either VideoStream or VideoUrl must be provided
        RuleFor(x => x)
            .Must(x => x.VideoStream != null || !string.IsNullOrEmpty(x.VideoUrl))
            .WithMessage("Either VideoStream or VideoUrl must be provided");

        // Validate file extension when VideoFileName is provided
        When(x => !string.IsNullOrEmpty(x.VideoFileName), () =>
        {
            RuleFor(x => x.VideoFileName)
                .Must(fileName => AllowedVideoExtensions.Contains(
                    Path.GetExtension(fileName)?.ToLower() ?? ""))
                .WithMessage($"Video must be one of: {string.Join(", ", AllowedVideoExtensions)}");
        });

        // Validate file size
        When(x => x.VideoSize.HasValue, () =>
        {
            RuleFor(x => x.VideoSize!.Value)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage($"Video size must be less than {MaxFileSizeBytes / (1024 * 1024)} MB");
        });

        // Validate VideoUrl format
        When(x => !string.IsNullOrEmpty(x.VideoUrl), () =>
        {
            RuleFor(x => x.VideoUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("VideoUrl must be a valid URL");
        });
    }
}
