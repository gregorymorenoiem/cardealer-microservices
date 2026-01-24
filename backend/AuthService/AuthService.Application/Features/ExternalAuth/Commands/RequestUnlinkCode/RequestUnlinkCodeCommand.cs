using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.RequestUnlinkCode;

/// <summary>
/// Command to request a verification code for unlinking active OAuth provider (AUTH-EXT-008)
/// 
/// When the user confirms they want to unlink their active OAuth provider,
/// this sends a 6-digit verification code to their email.
/// </summary>
/// <param name="UserId">The authenticated user's ID</param>
/// <param name="Provider">The OAuth provider to unlink</param>
/// <param name="IpAddress">IP address for audit logging</param>
/// <param name="UserAgent">User agent for audit logging</param>
public record RequestUnlinkCodeCommand(
    string UserId,
    string Provider,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RequestUnlinkCodeResponse>;

/// <summary>
/// Response from requesting unlink verification code
/// </summary>
/// <param name="Success">Whether the code was sent successfully</param>
/// <param name="Message">Human-readable message</param>
/// <param name="MaskedEmail">Masked email where code was sent (e.g., j***@gmail.com)</param>
/// <param name="ExpiresInMinutes">How many minutes until the code expires</param>
public record RequestUnlinkCodeResponse(
    bool Success,
    string Message,
    string MaskedEmail,
    int ExpiresInMinutes
);
