namespace AuthService.Application.DTOs.TwoFactor;

public record TwoFactorLoginResponse(
    string UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool IsTwoFactorEnabled = true
);
