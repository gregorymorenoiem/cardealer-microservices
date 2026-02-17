using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces.External;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Slack notification provider using Incoming Webhooks.
/// Reads webhook URL from ConfigurationService (encrypted secret).
/// Docs: https://api.slack.com/messaging/webhooks
/// </summary>
public class SlackProvider : ISlackProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationServiceClient _configClient;
    private readonly ILogger<SlackProvider> _logger;

    public SlackProvider(
        HttpClient httpClient,
        IConfigurationServiceClient configClient,
        ILogger<SlackProvider> logger)
    {
        _httpClient = httpClient;
        _configClient = configClient;
        _logger = logger;
    }

    public string ProviderName => "Slack";

    public async Task<bool> SendMessageAsync(
        string message,
        string? channel = null,
        string? iconEmoji = null,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = await GetWebhookUrl();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogInformation("[MOCK Slack] Message: {Message}", message);
            return true;
        }

        try
        {
            var payload = new Dictionary<string, object>
            {
                ["text"] = message
            };

            if (!string.IsNullOrWhiteSpace(channel))
                payload["channel"] = channel;
            if (!string.IsNullOrWhiteSpace(iconEmoji))
                payload["icon_emoji"] = iconEmoji;

            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Slack message sent successfully");
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to send Slack message. Status: {Status}, Body: {Body}",
                response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            return false;
        }
    }

    public async Task<bool> SendBlockMessageAsync(
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? fields = null,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = await GetWebhookUrl();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogInformation(
                "[MOCK Slack] Block message: {Title} - {Message}", title, message);
            return true;
        }

        try
        {
            var emoji = severity.ToLowerInvariant() switch
            {
                "critical" => "🚨",
                "error" => "❌",
                "warning" => "⚠️",
                _ => "ℹ️"
            };

            var color = severity.ToLowerInvariant() switch
            {
                "critical" => "#FF0000",
                "error" => "#E74C3C",
                "warning" => "#F39C12",
                _ => "#3498DB"
            };

            var fieldBlocks = new List<object>();
            if (fields != null)
            {
                foreach (var (key, value) in fields)
                {
                    fieldBlocks.Add(new
                    {
                        type = "mrkdwn",
                        text = $"*{key}:*\n{value}"
                    });
                }
            }

            var blocks = new List<object>
            {
                new
                {
                    type = "header",
                    text = new { type = "plain_text", text = $"{emoji} {title}", emoji = true }
                },
                new
                {
                    type = "section",
                    text = new { type = "mrkdwn", text = message }
                }
            };

            if (fieldBlocks.Count > 0)
            {
                blocks.Add(new
                {
                    type = "section",
                    fields = fieldBlocks.ToArray()
                });
            }

            blocks.Add(new
            {
                type = "context",
                elements = new object[]
                {
                    new
                    {
                        type = "mrkdwn",
                        text = $"*OKLA Platform* | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                    }
                }
            });

            var payload = new
            {
                blocks = blocks.ToArray(),
                attachments = new[]
                {
                    new { color, fallback = $"{title}: {message}" }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Slack block message sent: {Title}", title);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to send Slack block message. Status: {Status}, Body: {Body}",
                response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack block message: {Title}", title);
            return false;
        }
    }

    private async Task<string?> GetWebhookUrl()
    {
        // Read from ConfigurationService (stored as encrypted secret)
        return await _configClient.GetValueAsync("notifications.slack_webhook_url");
    }
}
