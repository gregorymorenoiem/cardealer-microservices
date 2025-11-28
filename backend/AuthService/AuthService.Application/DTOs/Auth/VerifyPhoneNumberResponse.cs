namespace AuthService.Application.DTOs.Auth;

public record VerifyPhoneNumberResponse(
    bool Success,
    string Message,
    bool IsVerified = false
);
