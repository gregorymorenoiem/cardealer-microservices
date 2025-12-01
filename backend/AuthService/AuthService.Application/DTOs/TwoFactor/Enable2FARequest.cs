using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Enable2FARequest(
    string UserId,
    TwoFactorAuthType Type
);
