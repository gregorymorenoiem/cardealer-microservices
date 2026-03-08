using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;

public class GenerateRecoveryCodesCommandValidator : AbstractValidator<GenerateRecoveryCodesCommand>
{
    public GenerateRecoveryCodesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to generate recovery codes.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .NoSqlInjection()
            .NoXss();
    }
}
