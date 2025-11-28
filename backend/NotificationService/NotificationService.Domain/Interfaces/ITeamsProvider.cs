using CarDealer.Contracts.Events.Error;

namespace NotificationService.Domain.Interfaces;

/// <summary>
/// Interface for Microsoft Teams notification provider.
/// Sends Adaptive Card notifications to Teams webhooks.
/// </summary>
public interface ITeamsProvider
{
    /// <summary>
    /// Sends an Adaptive Card alert to Microsoft Teams for critical errors.
    /// </summary>
    /// <param name="errorEvent">The critical error event containing error details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendCriticalErrorAlertAsync(ErrorCriticalEvent errorEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a custom Adaptive Card to Microsoft Teams.
    /// </summary>
    /// <param name="title">Card title</param>
    /// <param name="message">Card message</param>
    /// <param name="severity">Severity level (Info, Warning, Error, Critical)</param>
    /// <param name="metadata">Additional metadata to include in the card</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendAdaptiveCardAsync(
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}
