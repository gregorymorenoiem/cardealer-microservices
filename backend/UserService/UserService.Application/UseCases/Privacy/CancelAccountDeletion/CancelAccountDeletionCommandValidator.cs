using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Privacy.CancelAccountDeletion;

public class CancelAccountDeletionCommandValidator : AbstractValidator<CancelAccountDeletionCommand>
{
    public CancelAccountDeletionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email!)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
