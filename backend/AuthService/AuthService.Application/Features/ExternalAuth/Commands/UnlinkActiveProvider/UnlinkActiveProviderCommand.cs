using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.UnlinkActiveProvider;

/// <summary>
/// Command to unlink active OAuth provider with verification code (AUTH-EXT-008)
/// 
/// This is the final step that:
/// 1. Verifies the email code
/// 2. Unlinks the OAuth provider
/// 3. Revokes ALL user sessions
/// 4. Forces re-login with email/password
/// </summary>
/// <param name="UserId">The authenticated user's ID</param>
/// <param name="Provider">The OAuth provider to unlink</param>
/// <param name="VerificationCode">The 6-digit code from email</param>
/// <param name="IpAddress">IP address for audit logging</param>
/// <param name="UserAgent">User agent for audit logging</param>
public record UnlinkActiveProviderCommand(
    string UserId,
    string Provider,
    string VerificationCode,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<UnlinkActiveProviderResponse>;

/// <summary>
/// Response from unlinking active OAuth provider
/// </summary>
/// <param name="Success">Whether the unlink was successful</param>
/// <param name="Message">Human-readable message</param>
/// <param name="Provider">The provider that was unlinked</param>
/// <param name="SessionsRevoked">Number of sessions that were revoked</param>
/// <param name="RequiresReLogin">Whether the user must re-login</param>
public record UnlinkActiveProviderResponse(
    bool Success,
    string Message,
    string Provider,
    int SessionsRevoked,
    bool RequiresReLogin
);
