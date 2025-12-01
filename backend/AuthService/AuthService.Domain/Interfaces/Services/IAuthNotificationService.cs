using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces.Services;

public interface IAuthNotificationService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendWelcomeEmailAsync(string email, string username);
    Task SendEmailConfirmationAsync(string email, string confirmationToken);
    Task SendTwoFactorCodeAsync(string email, string code, TwoFactorAuthType method);
    Task SendTwoFactorBackupCodesAsync(string email, List<string> backupCodes);
}
