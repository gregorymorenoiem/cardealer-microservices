using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Clients;

public class NotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string templateKey,
        Dictionary<string, string> placeholders,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                UserId = userId,
                TemplateKey = templateKey,
                Channel = "email",
                Placeholders = placeholders
            };

            await _httpClient.PostAsJsonAsync("/api/notifications/send", request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to user {UserId}", userId);
        }
    }
}
