using FluentValidation;

namespace AuthService.Application.Features.Auth.Commands.RevokeAllSessions;

/// <summary>
/// Validator para RevokeAllSessionsCommand.
/// </summary>
public class RevokeAllSessionsCommandValidator : AbstractValidator<RevokeAllSessionsCommand>
{
    public RevokeAllSessionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .MaximumLength(50).WithMessage("Invalid user ID format.")
            .Must(BeValidGuidOrId).WithMessage("Invalid user ID format.");

        RuleFor(x => x.CurrentSessionId)
            .Must(id => string.IsNullOrEmpty(id) || BeValidGuid(id))
            .WithMessage("Current session ID format is invalid.");
    }

    private static bool BeValidGuid(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return Guid.TryParse(value, out _);
    }

    private static bool BeValidGuidOrId(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        if (Guid.TryParse(value, out _))
            return true;

        return value.All(c => char.IsLetterOrDigit(c) || c == '-');
    }
}
