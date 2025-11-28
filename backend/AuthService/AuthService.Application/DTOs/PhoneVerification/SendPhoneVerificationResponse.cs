namespace AuthService.Application.DTOs.PhoneVerification;

public record SendPhoneVerificationResponse(
    bool Success,
    string Message,
    DateTime? ExpiresAt = null
);
