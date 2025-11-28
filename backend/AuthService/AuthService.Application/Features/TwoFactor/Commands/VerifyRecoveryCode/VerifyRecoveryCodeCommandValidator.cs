using FluentValidation;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyRecoveryCode;

public class VerifyRecoveryCodeCommandValidator : AbstractValidator<VerifyRecoveryCodeCommand>
{
    public VerifyRecoveryCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Recovery code is required.")
            .Length(8).WithMessage("Recovery code must be 8 characters long.")
            .Matches(@"^[A-Z0-9]+$").WithMessage("Recovery code must contain only uppercase letters and numbers.");
    }
}