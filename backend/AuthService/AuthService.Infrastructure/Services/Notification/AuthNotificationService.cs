using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Services;
using AuthService.Infrastructure.External;
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Shared;
using AuthService.Shared.NotificationMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Services.Notification;

public class AuthNotificationService : IAuthNotificationService
{
    private readonly NotificationServiceClient _notificationServiceClient;
    private readonly INotificationEventProducer _notificationProducer;
    private readonly NotificationServiceSettings _settings;
    private readonly NotificationServiceRabbitMQSettings _rabbitMqSettings;
    private readonly ILogger<AuthNotificationService> _logger;

    public AuthNotificationService(
        NotificationServiceClient notificationServiceClient,
        INotificationEventProducer notificationProducer,
        IOptions<NotificationServiceSettings> settings,
        IOptions<NotificationServiceRabbitMQSettings> rabbitMqSettings,
        ILogger<AuthNotificationService> logger)
    {
        _notificationServiceClient = notificationServiceClient;
        _notificationProducer = notificationProducer;
        _settings = settings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value; // .Value para obtener la instancia
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var resetUrl = $"{_settings.FrontendBaseUrl}/reset-password?token={resetToken}";

        if (_rabbitMqSettings.EnableRabbitMQ && _notificationProducer != null)
        {
            await SendPasswordResetViaRabbitMQ(email, resetToken, resetUrl);
        }
        else
        {
            await SendPasswordResetViaHttp(email, resetToken, resetUrl);
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        if (_rabbitMqSettings.EnableRabbitMQ && _notificationProducer != null)
        {
            await SendWelcomeViaRabbitMQ(email, username);
        }
        else
        {
            await SendWelcomeViaHttp(email, username);
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationToken)
    {
        var confirmUrl = $"{_settings.FrontendBaseUrl}/verify-email?token={confirmationToken}";

        if (_rabbitMqSettings.EnableRabbitMQ && _notificationProducer != null)
        {
            await SendEmailConfirmationViaRabbitMQ(email, confirmationToken, confirmUrl);
        }
        else
        {
            await SendEmailConfirmationViaHttp(email, confirmationToken, confirmUrl);
        }
    }

    public async Task SendTwoFactorCodeAsync(string destination, string code, TwoFactorAuthType method)
    {
        if (_rabbitMqSettings.EnableRabbitMQ && _notificationProducer != null)
        {
            await SendTwoFactorCodeViaRabbitMQ(destination, code, method);
        }
        else
        {
            await SendTwoFactorCodeViaHttp(destination, code, method);
        }
    }

    public async Task SendTwoFactorBackupCodesAsync(string email, List<string> backupCodes)
    {
        if (_rabbitMqSettings.EnableRabbitMQ && _notificationProducer != null)
        {
            await SendTwoFactorBackupCodesViaRabbitMQ(email, backupCodes);
        }
        else
        {
            await SendTwoFactorBackupCodesViaHttp(email, backupCodes);
        }
    }

    #region RabbitMQ Methods

    private async Task SendPasswordResetViaRabbitMQ(string email, string resetToken, string resetUrl)
    {
        try
        {
            var notification = new EmailNotificationEvent
            {
                To = email,
                Subject = "Password Reset Request",
                TemplateName = "PasswordReset",
                Body = GeneratePasswordResetEmailBody(resetUrl),
                Data = new Dictionary<string, object>
                {
                    ["resetUrl"] = resetUrl,
                    ["expiryHours"] = 1,
                    ["token"] = resetToken
                },
                IsHtml = true
            };

            await _notificationProducer.PublishNotificationAsync(notification);
            _logger.LogInformation("Password reset notification sent via RabbitMQ to {Email}", email);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset via RabbitMQ to {Email}. Falling back to HTTP.", email);
            await SendPasswordResetViaHttp(email, resetToken, resetUrl);
        }
    }

    private async Task SendWelcomeViaRabbitMQ(string email, string username)
    {
        try
        {
            var notification = new EmailNotificationEvent
            {
                To = email,
                Subject = "Welcome to Our Platform!",
                TemplateName = "Welcome",
                Body = GenerateWelcomeEmailBody(username),
                Data = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["loginUrl"] = $"{_settings.FrontendBaseUrl}/login",
                    ["supportEmail"] = "support@example.com"
                },
                IsHtml = true
            };

            await _notificationProducer.PublishNotificationAsync(notification);
            _logger.LogInformation("Welcome email sent via RabbitMQ to {Email}", email);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email via RabbitMQ to {Email}. Falling back to HTTP.", email);
            await SendWelcomeViaHttp(email, username);
        }
    }

    private async Task SendEmailConfirmationViaRabbitMQ(string email, string confirmationToken, string confirmUrl)
    {
        try
        {
            var notification = new EmailNotificationEvent
            {
                To = email,
                Subject = "Confirm Your Email Address",
                TemplateName = "EmailConfirmation",
                Body = GenerateEmailConfirmationBody(confirmUrl),
                Data = new Dictionary<string, object>
                {
                    ["confirmationUrl"] = confirmUrl,
                    ["expiryHours"] = 24,
                    ["token"] = confirmationToken
                },
                IsHtml = true
            };

            await _notificationProducer.PublishNotificationAsync(notification);
            _logger.LogInformation("Email confirmation sent via RabbitMQ to {Email}", email);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email confirmation via RabbitMQ to {Email}. Falling back to HTTP.", email);
            await SendEmailConfirmationViaHttp(email, confirmationToken, confirmUrl);
        }
    }

    private async Task SendTwoFactorCodeViaRabbitMQ(string destination, string code, TwoFactorAuthType method)
    {
        try
        {
            NotificationEvent notification = method switch
            {
                TwoFactorAuthType.SMS => new SmsNotificationEvent
                {
                    To = destination,
                    Body = $"Your verification code is: {code}. This code will expire in 5 minutes.",
                    TemplateName = "TwoFactorSMS",
                    Data = new Dictionary<string, object>
                    {
                        ["code"] = code,
                        ["expiryMinutes"] = 5
                    }
                },
                TwoFactorAuthType.Email => new EmailNotificationEvent
                {
                    To = destination,
                    Subject = "Your Verification Code",
                    Body = GenerateTwoFactorEmailBody(code),
                    TemplateName = "TwoFactorEmail",
                    Data = new Dictionary<string, object>
                    {
                        ["code"] = code,
                        ["expiryMinutes"] = 5
                    },
                    IsHtml = true
                },
                _ => throw new System.ArgumentException($"Unsupported 2FA method: {method}")
            };

            await _notificationProducer.PublishNotificationAsync(notification);
            _logger.LogInformation("2FA code sent via RabbitMQ to {Destination} using {Method}", destination, method);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send 2FA code via RabbitMQ to {Destination}. Falling back to HTTP.", destination);
            await SendTwoFactorCodeViaHttp(destination, code, method);
        }
    }

    private async Task SendTwoFactorBackupCodesViaRabbitMQ(string email, List<string> backupCodes)
    {
        try
        {
            var notification = new EmailNotificationEvent
            {
                To = email,
                Subject = "Your Two-Factor Authentication Backup Codes",
                TemplateName = "TwoFactorBackupCodes",
                Body = GenerateBackupCodesEmailBody(backupCodes),
                Data = new Dictionary<string, object>
                {
                    ["backupCodes"] = backupCodes,
                    ["generatedAt"] = System.DateTime.UtcNow
                },
                IsHtml = true
            };

            await _notificationProducer.PublishNotificationAsync(notification);
            _logger.LogInformation("2FA backup codes sent via RabbitMQ to {Email}", email);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send 2FA backup codes via RabbitMQ to {Email}. Falling back to HTTP.", email);
            await SendTwoFactorBackupCodesViaHttp(email, backupCodes);
        }
    }

    #endregion

    #region HTTP Fallback Methods

    private async Task SendPasswordResetViaHttp(string email, string resetToken, string resetUrl)
    {
        var subject = "Password Reset Request";
        var body = GeneratePasswordResetEmailBody(resetUrl);

        var success = await _notificationServiceClient.SendEmailAsync(email, subject, body, true);

        if (success)
        {
            _logger.LogInformation("Password reset email sent via HTTP to {Email}", email);
        }
        else
        {
            _logger.LogError("Failed to send password reset email via HTTP to {Email}", email);
        }
    }

    private async Task SendWelcomeViaHttp(string email, string username)
    {
        var subject = "Welcome to Our Platform!";
        var body = GenerateWelcomeEmailBody(username);

        var success = await _notificationServiceClient.SendEmailAsync(email, subject, body, true);

        if (success)
        {
            _logger.LogInformation("Welcome email sent via HTTP to {Email}", email);
        }
        else
        {
            _logger.LogError("Failed to send welcome email via HTTP to {Email}", email);
        }
    }

    private async Task SendEmailConfirmationViaHttp(string email, string confirmationToken, string confirmUrl)
    {
        var subject = "Confirm Your Email Address";
        var body = GenerateEmailConfirmationBody(confirmUrl);

        var success = await _notificationServiceClient.SendEmailAsync(email, subject, body, true);

        if (success)
        {
            _logger.LogInformation("Email confirmation sent via HTTP to {Email}", email);
        }
        else
        {
            _logger.LogError("Failed to send email confirmation via HTTP to {Email}", email);
        }
    }

    private async Task SendTwoFactorCodeViaHttp(string destination, string code, TwoFactorAuthType method)
    {
        bool success = false;

        if (method == TwoFactorAuthType.SMS)
        {
            var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
            success = await _notificationServiceClient.SendSmsAsync(destination, message);
        }
        else if (method == TwoFactorAuthType.Email)
        {
            var subject = "Your Verification Code";
            var body = GenerateTwoFactorEmailBody(code);
            success = await _notificationServiceClient.SendEmailAsync(destination, subject, body, true);
        }

        if (success)
        {
            _logger.LogInformation("2FA code sent via HTTP to {Destination} using {Method}", destination, method);
        }
        else
        {
            _logger.LogError("Failed to send 2FA code via HTTP to {Destination} using {Method}", destination, method);
        }
    }

    private async Task SendTwoFactorBackupCodesViaHttp(string email, List<string> backupCodes)
    {
        var subject = "Your Two-Factor Authentication Backup Codes";
        var body = GenerateBackupCodesEmailBody(backupCodes);

        var success = await _notificationServiceClient.SendEmailAsync(email, subject, body, true);

        if (success)
        {
            _logger.LogInformation("2FA backup codes sent via HTTP to {Email}", email);
        }
        else
        {
            _logger.LogError("Failed to send 2FA backup codes via HTTP to {Email}", email);
        }
    }

    #endregion

    #region Email Body Generators

    private string GeneratePasswordResetEmailBody(string resetUrl)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Please click the button below to reset your password:</p>
                    <p><a href='{resetUrl}' class='button'>Reset Password</a></p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you did not request this reset, please ignore this email.</p>
                    <div class='footer'>
                        <p>If you're having trouble with the button, copy and paste this link into your browser:</p>
                        <p>{resetUrl}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GenerateWelcomeEmailBody(string username)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .welcome {{ color: #28a745; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2 class='welcome'>Welcome, {username}!</h2>
                    <p>Thank you for joining our platform. We're excited to have you on board!</p>
                    <p>Your account has been successfully created and you can now start using all our features.</p>
                    <p>If you have any questions, feel free to contact our support team.</p>
                    <p>Best regards,<br/>The Team</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateEmailConfirmationBody(string confirmUrl)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Email Confirmation</h2>
                    <p>Thank you for registering with us!</p>
                    <p>Please confirm your email address by clicking the button below:</p>
                    <p><a href='{confirmUrl}' class='button'>Confirm Email Address</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you did not create an account, please ignore this email.</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateTwoFactorEmailBody(string code)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .code {{ font-size: 24px; font-weight: bold; color: #007bff; letter-spacing: 4px; padding: 10px; background-color: #f8f9fa; border-radius: 4px; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Your Verification Code</h2>
                    <p>Your verification code is:</p>
                    <div class='code'>{code}</div>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't request this code, please ignore this message.</p>
                </div>
            </body>
            </html>";
    }

    private string GenerateBackupCodesEmailBody(List<string> backupCodes)
    {
        var codesHtml = string.Join("<br/>", backupCodes);

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .codes {{ font-family: monospace; background-color: #f8f9fa; padding: 15px; border-radius: 4px; }}
                    .warning {{ color: #856404; background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Backup Codes</h2>
                    <p>Here are your two-factor authentication backup codes:</p>
                    <div class='codes'>{codesHtml}</div>
                    <div class='warning'>
                        <p><strong>Important:</strong> Save these codes in a safe place. Each code can only be used once.</p>
                        <p>If you lose your authenticator app and don't have backup codes, you may lose access to your account.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    #endregion
}
