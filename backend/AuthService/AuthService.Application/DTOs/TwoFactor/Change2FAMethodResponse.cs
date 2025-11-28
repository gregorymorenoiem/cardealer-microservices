using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Change2FAMethodResponse(
    bool Success,
    string Message,
    string? Secret = null,
    string? QrCodeUri = null,
    string? PhoneNumber = null
);
