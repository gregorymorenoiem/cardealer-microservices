using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces.Services;

public interface IAuthNotificationService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendWelcomeEmailAsync(string email, string username);
    Task SendEmailConfirmationAsync(string email, string confirmationToken);
    Task SendTwoFactorCodeAsync(string email, string code, TwoFactorAuthType method);
    Task SendTwoFactorBackupCodesAsync(string email, List<string> backupCodes);
    
    /// <summary>
    /// Sends a new authenticator setup (secret, QR code, and recovery codes) when user 
    /// loses their device and exhausts all recovery codes.
    /// </summary>
    Task SendNewAuthenticatorSetupAsync(string email, string secret, string qrCodeUri, List<string> recoveryCodes);
    
    /// <summary>
    /// Sends a confirmation email when password has been changed.
    /// This is a security notification to alert user of account changes.
    /// </summary>
    Task SendPasswordChangedConfirmationAsync(string email);
    
    /// <summary>
    /// Sends a security alert email when suspicious activity is detected.
    /// US-18.2: Failed login attempts, lockouts, etc.
    /// </summary>
    Task SendSecurityAlertAsync(string email, SecurityAlertDto alert);
}

/// <summary>
/// DTO for security alert notifications (US-18.2)
/// </summary>
public record SecurityAlertDto(
    string AlertType,
    string IpAddress,
    int AttemptCount,
    DateTime Timestamp,
    string? Location = null,
    string? DeviceInfo = null,
    TimeSpan? LockoutDuration = null
);
