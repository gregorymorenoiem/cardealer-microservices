using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Commands.GetPresignedUrlsBatch;

public class GetPresignedUrlsBatchCommandValidator : AbstractValidator<GetPresignedUrlsBatchCommand>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxFileSize = 15 * 1024 * 1024; // 15MB
    private const int MaxBatchSize = 20;

    public GetPresignedUrlsBatchCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().NoSqlInjection().NoXss();
        RuleFor(x => x.Category).NotEmpty().NoSqlInjection().NoXss();
        RuleFor(x => x.Files).NotEmpty().WithMessage("Se requiere al menos un archivo");
        RuleFor(x => x.Files.Count).LessThanOrEqualTo(MaxBatchSize)
            .WithMessage($"Máximo {MaxBatchSize} archivos por lote");

        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(f => f.FileName).NotEmpty().NoSqlInjection().NoXss();
            file.RuleFor(f => f.ContentType)
                .NotEmpty()
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Tipo de archivo no permitido. Use JPEG, PNG o WebP");
            file.RuleFor(f => f.Size)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("El archivo excede el tamaño máximo de 15MB");
        });
    }
}
