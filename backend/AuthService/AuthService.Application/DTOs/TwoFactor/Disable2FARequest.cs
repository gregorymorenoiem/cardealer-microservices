namespace AuthService.Application.DTOs.TwoFactor;

public record Disable2FARequest(
    string Password
);