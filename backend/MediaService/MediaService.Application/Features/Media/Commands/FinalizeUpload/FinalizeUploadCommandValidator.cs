using FluentValidation;

namespace MediaService.Application.Features.Media.Commands.FinalizeUpload;

public class FinalizeUploadCommandValidator : AbstractValidator<FinalizeUploadCommand>
{
    public FinalizeUploadCommandValidator()
    {
        RuleFor(x => x.MediaId).NotEmpty();
    }
}