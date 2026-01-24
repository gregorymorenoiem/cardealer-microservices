namespace AuthService.Application.DTOs.Auth;

/// <summary>
/// Response from login attempt.
/// May indicate:
/// - Successful login (tokens provided)
/// - 2FA required (RequiresTwoFactor = true, TempToken provided)
/// - Revoked device verification required (RequiresRevokedDeviceVerification = true)
/// </summary>
public record LoginResponse(
    string UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool RequiresTwoFactor = false,
    string? TempToken = null,
    string? TwoFactorType = null, // "authenticator", "sms", or "email"
    bool RequiresRevokedDeviceVerification = false, // AUTH-SEC-005: Device was previously revoked
    string? DeviceFingerprint = null // AUTH-SEC-005: Fingerprint of revoked device
);
