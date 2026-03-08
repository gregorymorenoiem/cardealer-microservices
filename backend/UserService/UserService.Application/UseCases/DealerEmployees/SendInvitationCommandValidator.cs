using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerEmployees;

public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>
{
    public SendInvitationCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Permissions!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Permissions));

        RuleFor(x => x.InvitedBy)
            .NotEmpty().WithMessage("InvitedBy is required");
    }
}
