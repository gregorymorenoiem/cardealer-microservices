namespace AuthService.Application.DTOs.ExternalAuth;

/// <summary>
/// Request to validate if an account can be unlinked (AUTH-EXT-008)
/// </summary>
/// <param name="Provider">The OAuth provider to validate for unlinking</param>
public record ValidateUnlinkRequest(string Provider);

/// <summary>
/// Request to get a verification code for unlinking active provider (AUTH-EXT-008)
/// </summary>
/// <param name="Provider">The OAuth provider to unlink</param>
public record RequestUnlinkCodeRequest(string Provider);

/// <summary>
/// Request to unlink active OAuth provider with verification code (AUTH-EXT-008)
/// </summary>
/// <param name="Provider">The OAuth provider to unlink</param>
/// <param name="VerificationCode">The 6-digit verification code from email</param>
public record UnlinkActiveProviderRequest(string Provider, string VerificationCode);
