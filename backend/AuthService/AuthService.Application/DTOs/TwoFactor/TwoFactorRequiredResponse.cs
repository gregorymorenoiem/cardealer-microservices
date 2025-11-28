namespace AuthService.Application.DTOs.TwoFactor;

public record TwoFactorRequiredResponse(
    string UserId,
    string TempToken,
    DateTime ExpiresAt,
    string[] AvailableMethods,
    string Message = "Two-factor authentication required"
);