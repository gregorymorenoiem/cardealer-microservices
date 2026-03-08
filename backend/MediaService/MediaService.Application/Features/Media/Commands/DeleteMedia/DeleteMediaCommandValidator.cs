using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaCommandValidator : AbstractValidator<DeleteMediaCommand>
{
    public DeleteMediaCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty()
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.RequestedBy)
            .NotEmpty()
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}