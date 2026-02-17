using System.Net.Http.Json;
using System.Text.Json;
using CarDealer.Contracts.Events.Error;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Provider for sending Microsoft Teams notifications using Adaptive Cards.
/// </summary>
public class TeamsProvider : ITeamsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeamsProvider> _logger;
    private readonly string? _webhookUrl;

    public TeamsProvider(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TeamsProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = configuration["NotificationSettings:Teams:WebhookUrl"];

        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            _logger.LogWarning("Teams webhook URL not configured. Teams notifications will be disabled.");
        }
    }

    private bool IsConfigured => !string.IsNullOrWhiteSpace(_webhookUrl);

    public async Task<bool> SendCriticalErrorAlertAsync(
        ErrorCriticalEvent errorEvent,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogDebug("Teams not configured, skipping critical error alert for ErrorId: {ErrorId}", errorEvent.ErrorId);
            return false;
        }

        try
        {
            _logger.LogInformation(
                "Sending critical error alert to Teams for ErrorId: {ErrorId}, Service: {ServiceName}",
                errorEvent.ErrorId,
                errorEvent.ServiceName);

            var card = BuildCriticalErrorAdaptiveCard(errorEvent);
            var payload = new { type = "message", attachments = new[] { new { contentType = "application/vnd.microsoft.card.adaptive", content = card } } };

            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Teams alert sent successfully for ErrorId: {ErrorId}",
                    errorEvent.ErrorId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to send Teams alert. StatusCode: {StatusCode}, Response: {Response}",
                response.StatusCode,
                errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while sending Teams alert for ErrorId: {ErrorId}",
                errorEvent.ErrorId);
            return false;
        }
    }

    public async Task<bool> SendAdaptiveCardAsync(
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogDebug("Teams not configured, skipping alert: {Title}", title);
            return false;
        }

        try
        {
            _logger.LogInformation("Sending custom Teams alert: {Title}", title);

            var card = BuildCustomAdaptiveCard(title, message, severity, metadata);
            var payload = new { type = "message", attachments = new[] { new { contentType = "application/vnd.microsoft.card.adaptive", content = card } } };

            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Teams alert sent successfully: {Title}", title);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to send Teams alert. StatusCode: {StatusCode}, Response: {Response}",
                response.StatusCode,
                errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending Teams alert: {Title}", title);
            return false;
        }
    }

    private object BuildCriticalErrorAdaptiveCard(ErrorCriticalEvent errorEvent)
    {
        var facts = new List<object>
        {
            new { title = "Service", value = errorEvent.ServiceName },
            new { title = "Status Code", value = errorEvent.StatusCode.ToString() },
            new { title = "Exception Type", value = errorEvent.ExceptionType },
            new { title = "Endpoint", value = errorEvent.Endpoint ?? "N/A" },
            new { title = "Timestamp", value = errorEvent.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss UTC") }
        };

        if (!string.IsNullOrWhiteSpace(errorEvent.UserId))
        {
            facts.Add(new { title = "User ID", value = errorEvent.UserId });
        }

        if (errorEvent.Metadata != null && errorEvent.Metadata.ContainsKey("Environment"))
        {
            facts.Add(new { title = "Environment", value = errorEvent.Metadata["Environment"] });
        }

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            schema = "http://adaptivecards.io/schemas/adaptive-card.json",
            body = new object[]
            {
                new
                {
                    type = "Container",
                    style = "attention",
                    items = new object[]
                    {
                        new
                        {
                            type = "TextBlock",
                            text = "🚨 CRITICAL ERROR ALERT",
                            weight = "bolder",
                            size = "large",
                            color = "attention"
                        }
                    }
                },
                new
                {
                    type = "TextBlock",
                    text = errorEvent.Message,
                    wrap = true,
                    weight = "bolder"
                },
                new
                {
                    type = "FactSet",
                    facts = facts.ToArray()
                },
                new
                {
                    type = "TextBlock",
                    text = "Stack Trace:",
                    weight = "bolder",
                    spacing = "medium"
                },
                new
                {
                    type = "TextBlock",
                    text = TruncateStackTrace(errorEvent.StackTrace),
                    wrap = true,
                    fontType = "monospace",
                    isSubtle = true
                },
                new
                {
                    type = "TextBlock",
                    text = $"Error ID: {errorEvent.ErrorId}",
                    size = "small",
                    isSubtle = true,
                    spacing = "medium"
                }
            }
        };

        return card;
    }

    private object BuildCustomAdaptiveCard(
        string title,
        string message,
        string severity,
        Dictionary<string, string>? metadata)
    {
        var color = severity.ToLower() switch
        {
            "critical" => "attention",
            "error" => "attention",
            "warning" => "warning",
            _ => "accent"
        };

        var bodyItems = new List<object>
        {
            new
            {
                type = "Container",
                style = color,
                items = new object[]
                {
                    new
                    {
                        type = "TextBlock",
                        text = title,
                        weight = "bolder",
                        size = "large"
                    }
                }
            },
            new
            {
                type = "TextBlock",
                text = message,
                wrap = true
            }
        };

        if (metadata != null && metadata.Any())
        {
            var facts = metadata.Select(kv => new { title = kv.Key, value = kv.Value }).ToArray();
            bodyItems.Add(new
            {
                type = "FactSet",
                facts = facts
            });
        }

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            schema = "http://adaptivecards.io/schemas/adaptive-card.json",
            body = bodyItems.ToArray()
        };

        return card;
    }

    private string TruncateStackTrace(string? stackTrace, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return "No stack trace available";

        return stackTrace.Length > maxLength
            ? stackTrace.Substring(0, maxLength) + "\n... (truncated)"
            : stackTrace;
    }
}
