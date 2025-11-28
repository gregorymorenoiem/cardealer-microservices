using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Add2FAMethodRequest(
    string UserId,
    string Password,
    TwoFactorAuthType Method,
    string? PhoneNumber = null
);
