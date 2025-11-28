using FluentValidation;

namespace AuthService.Application.Features.TwoFactor.Commands.Disable2FA;

public class Disable2FACommandValidator : AbstractValidator<Disable2FACommand>
{
    public Disable2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to disable two-factor authentication.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
