using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.ValidateUnlinkAccount;

/// <summary>
/// Command to validate if a user can unlink their OAuth account (AUTH-EXT-008)
/// 
/// This is the first step when a user clicks "Unlink" on an OAuth provider.
/// It checks if they have a password configured and if it's the active provider.
/// </summary>
/// <param name="UserId">The authenticated user's ID</param>
/// <param name="Provider">The OAuth provider to validate for unlinking</param>
public record ValidateUnlinkAccountCommand(
    string UserId,
    string Provider
) : IRequest<ValidateUnlinkAccountResponse>;

/// <summary>
/// Response from validating unlink account request
/// </summary>
/// <param name="CanUnlink">Whether the user can proceed with unlinking</param>
/// <param name="HasPassword">Whether the user has a password configured</param>
/// <param name="IsActiveProvider">Whether this is the provider the user logged in with</param>
/// <param name="RequiresPasswordSetup">Whether user needs to set password first</param>
/// <param name="RequiresEmailVerification">Whether email verification is required (for active provider)</param>
/// <param name="Message">Human-readable message explaining next steps</param>
public record ValidateUnlinkAccountResponse(
    bool CanUnlink,
    bool HasPassword,
    bool IsActiveProvider,
    bool RequiresPasswordSetup,
    bool RequiresEmailVerification,
    string Message
);
