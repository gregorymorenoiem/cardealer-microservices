using MediatR;

namespace AuthService.Application.Features.Auth.Commands.RequestPasswordSetup;

/// <summary>
/// Command to request password setup for OAuth users (AUTH-PWD-001)
/// 
/// When an OAuth user needs to set a password (e.g., before unlinking their OAuth provider),
/// this command generates a secure token and sends an email with a link to set their password.
/// </summary>
/// <param name="UserId">The authenticated user's ID</param>
/// <param name="Email">The user's email address</param>
/// <param name="IpAddress">IP address for audit logging</param>
/// <param name="UserAgent">User agent for audit logging</param>
public record RequestPasswordSetupCommand(
    string UserId,
    string Email,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RequestPasswordSetupResponse>;

/// <summary>
/// Response from requesting password setup
/// </summary>
/// <param name="Success">Whether the email was sent successfully</param>
/// <param name="Message">Human-readable message</param>
/// <param name="ExpiresAt">When the token expires</param>
public record RequestPasswordSetupResponse(
    bool Success,
    string Message,
    DateTime ExpiresAt
);
