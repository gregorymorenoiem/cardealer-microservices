using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Verify2FARequest(
    string UserId,
    string Code,
    TwoFactorAuthType Type
);