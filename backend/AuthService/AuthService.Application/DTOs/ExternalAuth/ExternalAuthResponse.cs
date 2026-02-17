namespace AuthService.Application.DTOs.ExternalAuth;

public record ExternalAuthResponse(
    string UserId,
    string UserName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool IsNewUser = false,
    string? FirstName = null,
    string? LastName = null,
    string? ProfilePictureUrl = null
);
