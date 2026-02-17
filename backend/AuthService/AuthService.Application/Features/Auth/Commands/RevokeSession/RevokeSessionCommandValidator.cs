using FluentValidation;

namespace AuthService.Application.Features.Auth.Commands.RevokeSession;

/// <summary>
/// Validator para RevokeSessionCommand.
/// Valida formato de IDs y previene inyección.
/// </summary>
public class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .MaximumLength(50).WithMessage("Invalid user ID format.")
            .Must(BeValidGuidOrId).WithMessage("Invalid user ID format.");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .Must(BeValidGuid).WithMessage("Session ID must be a valid GUID format.");

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

        // Acepta GUIDs o IDs alfanuméricos de ASP.NET Identity
        if (Guid.TryParse(value, out _))
            return true;

        // Acepta IDs alfanuméricos (típicos de Identity)
        return value.All(c => char.IsLetterOrDigit(c) || c == '-');
    }
}
