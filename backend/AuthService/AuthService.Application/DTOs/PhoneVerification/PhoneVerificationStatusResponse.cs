
namespace AuthService.Application.DTOs.PhoneVerification;

public record PhoneVerificationStatusResponse(
    bool IsVerified,
    string? PhoneNumber,
    DateTime LastUpdated
);