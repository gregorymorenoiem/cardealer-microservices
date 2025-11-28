namespace AuthService.Application.DTOs.TwoFactor;

public record Verify2FAResponse(
    bool Success,
    string Message,
    bool IsSetupComplete = false
);