using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Change2FAMethodRequest(
    string UserId,
    string Password,
    TwoFactorAuthType NewMethod,
    string? PhoneNumber = null
);

