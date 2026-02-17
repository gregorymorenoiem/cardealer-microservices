using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Commands.UploadVehicleImage;

public class UploadVehicleImageCommandValidator : AbstractValidator<UploadVehicleImageCommand>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Exterior", "Interior", "Engine", "Damage", "Documents", "Other"
    };

    private const long MaxFileSize = 15 * 1024 * 1024; // 15MB

    public UploadVehicleImageCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("El archivo es requerido")
            .Must(f => f != null && f.Length > 0).WithMessage("El archivo está vacío")
            .Must(f => f != null && f.Length <= MaxFileSize).WithMessage("El archivo excede el tamaño máximo de 15MB")
            .Must(f => f != null && AllowedContentTypes.Contains(f.ContentType))
            .WithMessage("Tipo de archivo no permitido. Use JPEG, PNG o WebP");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId es requerido")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.ImageType)
            .Must(t => t == null || AllowedImageTypes.Contains(t))
            .WithMessage("ImageType no válido")
            .NoSqlInjection()
            .NoXss()
            .When(x => x.ImageType != null);

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 100)
            .When(x => x.SortOrder.HasValue);
    }
}
