using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Privacy.ConfirmAccountDeletion;

public class ConfirmAccountDeletionCommandValidator : AbstractValidator<ConfirmAccountDeletionCommand>
{
    public ConfirmAccountDeletionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.ConfirmationCode)
            .NotEmpty().WithMessage("Confirmation code is required")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required for account deletion confirmation")
            .MaximumLength(128)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email!)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
