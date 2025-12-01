namespace AuthService.Application.DTOs.TwoFactor;

public record Enable2FAResponse(
    string Secret,
    string QrCodeUri,
    List<string> RecoveryCodes,
    string Message = "Two-factor authentication setup completed successfully. Please verify your authenticator app."
);
