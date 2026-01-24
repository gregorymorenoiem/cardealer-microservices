using MediatR;

namespace AuthService.Application.Features.Auth.Commands.SetPasswordForOAuthUser;

/// <summary>
/// Command to set password for an OAuth user (AUTH-PWD-001)
/// 
/// This is the final step where the user submits their new password.
/// The token is consumed (deleted) after successful password setup.
/// </summary>
/// <param name="Token">The token from the password setup email</param>
/// <param name="NewPassword">The new password</param>
/// <param name="ConfirmPassword">Confirmation of the new password</param>
/// <param name="IpAddress">IP address for audit logging</param>
/// <param name="UserAgent">User agent for audit logging</param>
public record SetPasswordForOAuthUserCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<SetPasswordForOAuthUserResponse>;

/// <summary>
/// Response from setting password for OAuth user
/// </summary>
/// <param name="Success">Whether the password was set successfully</param>
/// <param name="Message">Human-readable message</param>
/// <param name="Email">The user's email</param>
/// <param name="CanNowUnlinkProvider">Whether user can now unlink their OAuth provider</param>
public record SetPasswordForOAuthUserResponse(
    bool Success,
    string Message,
    string? Email = null,
    bool CanNowUnlinkProvider = false
);
