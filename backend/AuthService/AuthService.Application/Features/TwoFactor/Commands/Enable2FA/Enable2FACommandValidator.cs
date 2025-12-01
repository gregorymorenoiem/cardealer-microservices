using FluentValidation;
using AuthService.Domain.Enums;

namespace AuthService.Application.Features.TwoFactor.Commands.Enable2FA;

public class Enable2FACommandValidator : AbstractValidator<Enable2FACommand>
{
    public Enable2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .MinimumLength(1).WithMessage("User ID cannot be empty.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid two-factor authentication type.")
            .Must(type => type != TwoFactorAuthType.SMS || type != TwoFactorAuthType.Email || type != TwoFactorAuthType.Authenticator)
            .WithMessage("Supported two-factor authentication types are: Authenticator, SMS, and Email.");
    }
}
