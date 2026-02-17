using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ContactService.Application.Clients;

/// <summary>
/// HTTP Client for inter-service communication with NotificationService.
/// Uses IHttpClientFactory with Polly resilience policies (retry, circuit breaker, timeout).
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// Sends a contact form notification to the vehicle owner.
    /// </summary>
    Task<bool> SendContactNotificationAsync(ContactNotificationRequest request, CancellationToken ct = default);
}

public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendContactNotificationAsync(ContactNotificationRequest request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/notifications/send", content, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Contact notification sent for contact request {ContactRequestId}", request.ContactRequestId);
                return true;
            }

            _logger.LogWarning("⚠️ Failed to send contact notification. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending contact notification for {ContactRequestId}", request.ContactRequestId);
            return false; // Graceful degradation — don't fail the contact request
        }
    }
}

public record ContactNotificationRequest
{
    public Guid ContactRequestId { get; init; }
    public string RecipientEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? VehicleTitle { get; init; }
    public string Subject { get; init; } = "Nuevo mensaje de contacto";
}
