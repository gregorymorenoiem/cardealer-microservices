using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

/// <summary>
/// Request DTO for verifying 2FA code
/// UserId is extracted from JWT token in the controller, not from this request
/// </summary>
public record Verify2FARequest(
    string Code,
    TwoFactorAuthType Type
);
