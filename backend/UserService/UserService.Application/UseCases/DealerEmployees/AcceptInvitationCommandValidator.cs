using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerEmployees;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required")
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Password!)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.FirstName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Phone!)
            .MaximumLength(20)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
