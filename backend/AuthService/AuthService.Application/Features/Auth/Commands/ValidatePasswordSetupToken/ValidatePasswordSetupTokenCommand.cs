using MediatR;

namespace AuthService.Application.Features.Auth.Commands.ValidatePasswordSetupToken;

/// <summary>
/// Command to validate a password setup token (AUTH-PWD-001)
/// 
/// When a user clicks the link in their email, this validates the token
/// before showing the password setup form.
/// </summary>
/// <param name="Token">The token from the email link</param>
public record ValidatePasswordSetupTokenCommand(string Token) : IRequest<ValidatePasswordSetupTokenResponse>;

/// <summary>
/// Response from validating a password setup token
/// </summary>
/// <param name="IsValid">Whether the token is valid</param>
/// <param name="Message">Human-readable message</param>
/// <param name="Email">The user's email (if valid)</param>
/// <param name="Provider">The OAuth provider name (if valid)</param>
/// <param name="ExpiresAt">When the token expires</param>
public record ValidatePasswordSetupTokenResponse(
    bool IsValid,
    string Message,
    string? Email = null,
    string? Provider = null,
    DateTime? ExpiresAt = null
);
