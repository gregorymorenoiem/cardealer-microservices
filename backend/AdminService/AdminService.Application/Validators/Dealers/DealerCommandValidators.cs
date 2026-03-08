using FluentValidation;
using AdminService.Application.UseCases.Dealers;

namespace AdminService.Application.Validators.Dealers;

/// <summary>
/// Validator for SuspendDealerCommand.
/// Validates Reason with NoSqlInjection/NoXss.
/// </summary>
public class SuspendDealerCommandValidator : AbstractValidator<SuspendDealerCommand>
{
    public SuspendDealerCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("Dealer ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Suspension reason is required.")
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss();
    }
}

/// <summary>
/// Validator for CreateDealerProfileForUserCommand.
/// Validates BusinessName, Email, Phone with NoSqlInjection/NoXss.
/// </summary>
public class CreateDealerProfileForUserCommandValidator : AbstractValidator<CreateDealerProfileForUserCommand>
{
    public CreateDealerProfileForUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20)
            .Matches(@"^\+?[\d\s\-()]{7,20}$").WithMessage("Invalid phone number format.")
            .NoSqlInjection()
            .NoXss();
    }
}
