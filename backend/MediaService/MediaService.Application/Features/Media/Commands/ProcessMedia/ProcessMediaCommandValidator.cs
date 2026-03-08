using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Commands.ProcessMedia;

public class ProcessMediaCommandValidator : AbstractValidator<ProcessMediaCommand>
{
    private static readonly string[] ValidProcessingTypes = { "resize", "compress", "watermark", "thumbnail", "optimize" };

    public ProcessMediaCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Media ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.ProcessingType)
            .Must(t => string.IsNullOrEmpty(t) || ValidProcessingTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Processing type must be one of: {string.Join(", ", ValidProcessingTypes)}.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.ProcessingType));
    }
}
