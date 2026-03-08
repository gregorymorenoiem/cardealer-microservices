using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerOnboarding;

public class RegisterDealerCommandValidator : AbstractValidator<RegisterDealerCommand>
{
    public RegisterDealerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required")
            .MaximumLength(150)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Phone!)
            .MaximumLength(20)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();
    }
}
