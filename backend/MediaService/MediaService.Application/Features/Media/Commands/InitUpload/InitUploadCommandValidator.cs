using FluentValidation;
using MediaService.Shared;

namespace MediaService.Application.Features.Media.Commands.InitUpload;

public class InitUploadCommandValidator : AbstractValidator<InitUploadCommand>
{
    public InitUploadCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.FileSize).GreaterThan(0);
    }
}