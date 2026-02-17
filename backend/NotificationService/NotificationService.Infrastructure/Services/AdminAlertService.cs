using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.External;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// Admin alert service that reads configuration toggles from ConfigurationService
/// and routes alerts to the configured channels (Email, SMS, Push, Teams, Slack).
///
/// Configuration keys consumed:
///   notifications.{alertType}        → toggle (true/false) per alert type
///   notifications.admin_channel      → channel selection (email, email+sms, email+push, all)
///   notifications.admin_email        → email for admin alerts
///   notifications.admin_phone        → phone for SMS alerts
///   notifications.teams_webhook_url  → Teams webhook URL (secret)
///   notifications.slack_webhook_url  → Slack webhook URL (secret)
/// </summary>
public class AdminAlertService : IAdminAlertService
{
    private readonly IConfigurationServiceClient _configClient;
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;
    private readonly ITeamsProvider _teamsProvider;
    private readonly ISlackProvider _slackProvider;
    private readonly ILogger<AdminAlertService> _logger;

    // Map of alert types to their ConfigurationService keys
    private static readonly Dictionary<string, string> AlertTypeKeys = new()
    {
        ["new_user_registered"] = "notifications.new_user_registered",
        ["new_listing_pending"] = "notifications.new_listing_pending",
        ["new_dealer_registered"] = "notifications.new_dealer_registered",
        ["user_report"] = "notifications.user_report",
        ["payment_failed"] = "notifications.payment_failed",
        ["daily_summary"] = "notifications.daily_summary",
        ["kyc_pending_review"] = "notifications.kyc_pending_review",
        ["system_errors"] = "notifications.system_errors",
    };

    public AdminAlertService(
        IConfigurationServiceClient configClient,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider,
        ITeamsProvider teamsProvider,
        ISlackProvider slackProvider,
        ILogger<AdminAlertService> logger)
    {
        _configClient = configClient;
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
        _teamsProvider = teamsProvider;
        _slackProvider = slackProvider;
        _logger = logger;
    }

    public async Task SendAlertAsync(
        string alertType,
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        // 1. Check if this alert type is enabled
        if (!await IsAlertEnabledAsync(alertType, ct))
        {
            _logger.LogDebug(
                "Admin alert type '{AlertType}' is disabled in configuration. Skipping.",
                alertType);
            return;
        }

        _logger.LogInformation(
            "Sending admin alert: Type={AlertType}, Title={Title}, Severity={Severity}",
            alertType, title, severity);

        // 2. Get channel configuration
        var channel = await _configClient.GetValueAsync("notifications.admin_channel", ct) ?? "email";
        var adminEmail = await _configClient.GetValueAsync("notifications.admin_email", ct) ?? "admin@okla.com.do";
        var adminPhone = await _configClient.GetValueAsync("notifications.admin_phone", ct);

        // 3. Route to configured channels
        var tasks = new List<Task>();

        // Email is always sent (all channel options include email)
        tasks.Add(SendEmailAlertAsync(adminEmail, title, message, severity, alertType, metadata, ct));

        // SMS for email+sms or all
        if (channel is "email+sms" or "all" && !string.IsNullOrWhiteSpace(adminPhone))
        {
            tasks.Add(SendSmsAlertAsync(adminPhone, title, message, severity, ct));
        }

        // Teams webhook (always attempted if configured, independent of channel selection)
        tasks.Add(SendTeamsAlertAsync(title, message, severity, metadata, ct));

        // Slack webhook (always attempted if configured, independent of channel selection)
        tasks.Add(SendSlackAlertAsync(title, message, severity, metadata, ct));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Admin alert dispatched: Type={AlertType}, Channels={Channel}", alertType, channel);
    }

    private async Task<bool> IsAlertEnabledAsync(string alertType, CancellationToken ct)
    {
        if (AlertTypeKeys.TryGetValue(alertType, out var configKey))
        {
            return await _configClient.IsEnabledAsync(configKey, ct);
        }

        // Unknown alert types are enabled by default (fail-open)
        _logger.LogDebug("Unknown alert type '{AlertType}', defaulting to enabled", alertType);
        return true;
    }

    private async Task SendEmailAlertAsync(
        string adminEmail,
        string title,
        string message,
        string severity,
        string alertType,
        Dictionary<string, string>? metadata,
        CancellationToken ct)
    {
        try
        {
            var emoji = severity.ToLowerInvariant() switch
            {
                "critical" => "🚨",
                "error" => "❌",
                "warning" => "⚠️",
                _ => "ℹ️"
            };

            var htmlBody = BuildEmailHtml(emoji, title, message, severity, alertType, metadata);

            var (success, messageId, error) = await _emailProvider.SendAsync(
                adminEmail,
                $"{emoji} [OKLA Admin] {title}",
                htmlBody,
                isHtml: true);

            if (!success)
            {
                _logger.LogWarning(
                    "Failed to send admin email alert to {Email}. Error: {Error}",
                    adminEmail, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin email alert to {Email}", adminEmail);
        }
    }

    private async Task SendSmsAlertAsync(
        string phone, string title, string message, string severity, CancellationToken ct)
    {
        try
        {
            var smsText = $"[OKLA {severity}] {title}: {message}";
            if (smsText.Length > 160)
                smsText = smsText[..157] + "...";

            var (success, _, error) = await _smsProvider.SendAsync(phone, smsText);
            if (!success)
            {
                _logger.LogWarning(
                    "Failed to send admin SMS alert to {Phone}. Error: {Error}", phone, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin SMS alert to {Phone}", phone);
        }
    }

    private async Task SendTeamsAlertAsync(
        string title, string message, string severity,
        Dictionary<string, string>? metadata, CancellationToken ct)
    {
        try
        {
            // TeamsProvider reads webhook URL from its own config
            await _teamsProvider.SendAdaptiveCardAsync(title, message, severity, metadata, ct);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex,
                "Teams alert not sent (webhook may not be configured): {Title}", title);
        }
    }

    private async Task SendSlackAlertAsync(
        string title, string message, string severity,
        Dictionary<string, string>? metadata, CancellationToken ct)
    {
        try
        {
            await _slackProvider.SendBlockMessageAsync(title, message, severity, metadata, ct);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex,
                "Slack alert not sent (webhook may not be configured): {Title}", title);
        }
    }

    private static string BuildEmailHtml(
        string emoji, string title, string message, string severity,
        string alertType, Dictionary<string, string>? metadata)
    {
        var color = severity.ToLowerInvariant() switch
        {
            "critical" => "#DC3545",
            "error" => "#E74C3C",
            "warning" => "#F39C12",
            _ => "#3498DB"
        };

        var metadataRows = "";
        if (metadata != null)
        {
            foreach (var (key, value) in metadata)
            {
                metadataRows += $"<tr><td style='padding:4px 8px;font-weight:bold;'>{key}</td><td style='padding:4px 8px;'>{value}</td></tr>";
            }
        }

        return $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;margin:0;padding:20px;background:#f5f5f5;'>
  <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <div style='background:{color};padding:16px 24px;color:#fff;'>
      <h2 style='margin:0;font-size:18px;'>{emoji} {title}</h2>
      <p style='margin:4px 0 0;font-size:12px;opacity:0.9;'>Severidad: {severity} | Tipo: {alertType}</p>
    </div>
    <div style='padding:24px;'>
      <p style='margin:0 0 16px;color:#333;line-height:1.5;'>{message}</p>
      {(metadata != null ? $"<table style='width:100%;border-collapse:collapse;margin-top:16px;'><tbody>{metadataRows}</tbody></table>" : "")}
    </div>
    <div style='padding:12px 24px;background:#f8f9fa;border-top:1px solid #e9ecef;'>
      <p style='margin:0;font-size:11px;color:#6c757d;'>OKLA Platform Admin Alert — {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
    </div>
  </div>
</body>
</html>";
    }
}
