namespace NotificationService.Domain.Interfaces.External;

/// <summary>
/// Interface for Slack webhook notification provider.
/// </summary>
public interface ISlackProvider
{
    /// <summary>
    /// Send a message to a Slack channel via webhook.
    /// </summary>
    Task<bool> SendMessageAsync(
        string message,
        string? channel = null,
        string? iconEmoji = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a rich block-kit message to Slack.
    /// </summary>
    Task<bool> SendBlockMessageAsync(
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? fields = null,
        CancellationToken cancellationToken = default);

    string ProviderName { get; }
}
