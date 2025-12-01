namespace AuthService.Application.DTOs.TwoFactor;

public record TwoFactorLoginRequest(
    string TempToken,
    string TwoFactorCode
);
