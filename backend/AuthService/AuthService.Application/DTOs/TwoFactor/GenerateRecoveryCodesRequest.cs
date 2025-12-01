namespace AuthService.Application.DTOs.TwoFactor;

public record GenerateRecoveryCodesRequest(
    string UserId,
    string Password
);
