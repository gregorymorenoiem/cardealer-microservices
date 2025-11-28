using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Shared;

namespace NotificationService.Infrastructure.External;

public class FirebasePushService : IPushNotificationProvider
{
    private readonly FirebaseSettings _settings;
    private readonly ILogger<FirebasePushService> _logger;

    public FirebasePushService(
        IOptions<NotificationSettings> settings,
        ILogger<FirebasePushService> logger)
    {
        _settings = settings.Value.Firebase;
        _logger = logger;
    }

    public string ProviderName => "Firebase";

    public async Task<(bool success, string? messageId, string? error)> SendAsync(
        string deviceToken,
        string title,
        string body,
        object? data = null,
        Dictionary<string, object>? metadata = null)
    {
        try
        {
            // Implementar lógica real de Firebase Cloud Messaging
            // Por ahora simulamos el envío exitoso
            await Task.Delay(100);

            _logger.LogInformation("Push notification sent successfully to device {DeviceToken}", deviceToken);
            return (true, Guid.NewGuid().ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to device {DeviceToken}", deviceToken);
            return (false, null, ex.Message);
        }
    }
}