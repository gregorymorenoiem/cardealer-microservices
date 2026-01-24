using FluentValidation;

namespace AuthService.Application.Features.Auth.Commands.ValidatePasswordSetupToken;

/// <summary>
/// Validator for ValidatePasswordSetupTokenCommand (AUTH-PWD-001)
/// </summary>
public class ValidatePasswordSetupTokenCommandValidator : AbstractValidator<ValidatePasswordSetupTokenCommand>
{
    public ValidatePasswordSetupTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required.")
            .MinimumLength(20)
            .WithMessage("Invalid token format.");
    }
}
