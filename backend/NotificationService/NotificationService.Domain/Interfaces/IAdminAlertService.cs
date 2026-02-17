namespace NotificationService.Domain.Interfaces;

/// <summary>
/// Service for sending admin alerts based on ConfigurationService toggles.
/// Routes alerts to the configured channels (Email, SMS, Teams, Slack).
/// </summary>
public interface IAdminAlertService
{
    /// <summary>
    /// Send an admin alert if the given alert type is enabled in configuration.
    /// The alert will be routed to the configured channels (email, sms, teams, slack).
    /// </summary>
    /// <param name="alertType">Alert type key (e.g., "new_user_registered", "payment_failed")</param>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message body</param>
    /// <param name="severity">Severity level: Info, Warning, Error, Critical</param>
    /// <param name="metadata">Additional context data</param>
    /// <param name="ct">Cancellation token</param>
    Task SendAlertAsync(
        string alertType,
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? metadata = null,
        CancellationToken ct = default);
}
