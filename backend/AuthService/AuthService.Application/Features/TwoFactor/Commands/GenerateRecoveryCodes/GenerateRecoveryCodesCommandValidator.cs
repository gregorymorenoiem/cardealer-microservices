using FluentValidation;

namespace AuthService.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;

public class GenerateRecoveryCodesCommandValidator : AbstractValidator<GenerateRecoveryCodesCommand>
{
    public GenerateRecoveryCodesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to generate recovery codes.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
