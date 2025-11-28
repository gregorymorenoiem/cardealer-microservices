using FluentValidation;

namespace MediaService.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaCommandValidator : AbstractValidator<DeleteMediaCommand>
{
    public DeleteMediaCommandValidator()
    {
        RuleFor(x => x.MediaId).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty();
    }
}