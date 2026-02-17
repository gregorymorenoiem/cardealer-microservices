using FluentValidation;
using MediaService.Shared;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Commands.InitUpload;

public class InitUploadCommandValidator : AbstractValidator<InitUploadCommand>
{
    public InitUploadCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().NoSqlInjection().NoXss();
        RuleFor(x => x.ContentType).NotEmpty().NoSqlInjection().NoXss();
        RuleFor(x => x.FileSize).GreaterThan(0);
    }
}