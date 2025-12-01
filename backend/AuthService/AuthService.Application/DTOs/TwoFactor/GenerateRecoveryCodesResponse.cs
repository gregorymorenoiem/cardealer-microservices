namespace AuthService.Application.DTOs.TwoFactor;

public record GenerateRecoveryCodesResponse(
    List<string> RecoveryCodes,
    string Message = "Recovery codes generated successfully. Please save them in a secure place."
);
