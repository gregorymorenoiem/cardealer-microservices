using FluentValidation;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;

namespace Video360Service.Application.Validators;

public class CreateVideo360JobRequestValidator : AbstractValidator<CreateVideo360JobRequest>
{
    public CreateVideo360JobRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("El ID del vehículo es requerido");

        RuleFor(x => x.FrameCount)
            .InclusiveBetween(4, 12)
            .WithMessage("El número de frames debe estar entre 4 y 12");

        RuleFor(x => x.OutputWidth)
            .InclusiveBetween(640, 4096)
            .When(x => x.OutputWidth.HasValue)
            .WithMessage("El ancho debe estar entre 640 y 4096 píxeles");

        RuleFor(x => x.OutputHeight)
            .InclusiveBetween(480, 2160)
            .When(x => x.OutputHeight.HasValue)
            .WithMessage("El alto debe estar entre 480 y 2160 píxeles");

        RuleFor(x => x.JpegQuality)
            .InclusiveBetween(50, 100)
            .When(x => x.JpegQuality.HasValue)
            .WithMessage("La calidad JPEG debe estar entre 50 y 100");

        RuleFor(x => x.OutputFormat)
            .Must(f => string.IsNullOrEmpty(f) || new[] { "jpg", "png", "webp" }.Contains(f.ToLower()))
            .WithMessage("El formato de salida debe ser jpg, png o webp");
    }
}

public class CreateVideo360JobCommandValidator : AbstractValidator<CreateVideo360JobCommand>
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".avi", ".webm", ".mkv" };
    private const long MaxFileSizeBytes = 500 * 1024 * 1024; // 500 MB

    public CreateVideo360JobCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("El ID del vehículo es requerido");

        RuleFor(x => x.VideoUrl)
            .NotEmpty()
            .WithMessage("La URL del video es requerida");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage("El nombre del archivo es requerido")
            .Must(HaveValidExtension)
            .WithMessage($"El archivo debe tener una extensión válida: {string.Join(", ", AllowedVideoExtensions)}");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("El tamaño del archivo debe ser mayor a 0")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"El archivo no puede exceder {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.Options.FrameCount)
            .InclusiveBetween(4, 12)
            .WithMessage("El número de frames debe estar entre 4 y 12");
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedVideoExtensions.Contains(extension);
    }
}
