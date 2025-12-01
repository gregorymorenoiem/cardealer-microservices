namespace AuthService.Application.DTOs.Auth;

public record LoginResponse(
    string UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool RequiresTwoFactor = false,
    string? TempToken = null
);
