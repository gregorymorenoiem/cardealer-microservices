namespace AuthService.Application.DTOs.Auth;

public record RegisterResponse(
    string UserId,
    string UserName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);