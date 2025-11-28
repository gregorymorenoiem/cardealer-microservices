namespace AuthService.Application.DTOs.TwoFactor;

public record VerifyRecoveryCodeRequest(
    string Code
);