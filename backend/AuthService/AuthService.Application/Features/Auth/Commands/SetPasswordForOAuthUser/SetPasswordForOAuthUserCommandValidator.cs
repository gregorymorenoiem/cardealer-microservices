using FluentValidation;
using System.Text.RegularExpressions;

namespace AuthService.Application.Features.Auth.Commands.SetPasswordForOAuthUser;

/// <summary>
/// Validator for SetPasswordForOAuthUserCommand (AUTH-PWD-001)
/// 
/// Password requirements:
/// - Minimum 8 characters
/// - At least 1 uppercase letter
/// - At least 1 lowercase letter
/// - At least 1 number
/// - At least 1 special character
/// </summary>
public class SetPasswordForOAuthUserCommandValidator : AbstractValidator<SetPasswordForOAuthUserCommand>
{
    public SetPasswordForOAuthUserCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Must(HaveUppercase)
            .WithMessage("Password must contain at least one uppercase letter.")
            .Must(HaveLowercase)
            .WithMessage("Password must contain at least one lowercase letter.")
            .Must(HaveDigit)
            .WithMessage("Password must contain at least one number.")
            .Must(HaveSpecialCharacter)
            .WithMessage("Password must contain at least one special character (@$!%*?&).");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }

    private static bool HaveUppercase(string password)
    {
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[A-Z]");
    }

    private static bool HaveLowercase(string password)
    {
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[a-z]");
    }

    private static bool HaveDigit(string password)
    {
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[0-9]");
    }

    private static bool HaveSpecialCharacter(string password)
    {
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[@$!%*?&]");
    }
}
