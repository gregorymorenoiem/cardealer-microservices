using FluentValidation;
using AuthService.Domain.Enums;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.Verify2FA;

public class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
{
    public Verify2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 characters long.")
            .Matches(@"^\d+$").WithMessage("Verification code must contain only digits.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid two-factor authentication type.");
    }
}
