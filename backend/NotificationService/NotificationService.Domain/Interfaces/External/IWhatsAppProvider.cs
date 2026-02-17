namespace NotificationService.Domain.Interfaces.External;

/// <summary>
/// Interface for WhatsApp Business messaging providers (Twilio WhatsApp or Meta WhatsApp Business API).
/// </summary>
public interface IWhatsAppProvider
{
    /// <summary>
    /// Send a free-form WhatsApp message (only within 24h session window).
    /// </summary>
    Task<(bool success, string? messageId, string? error)> SendMessageAsync(
        string to,
        string message,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Send a WhatsApp template message (can be sent outside session window).
    /// </summary>
    Task<(bool success, string? messageId, string? error)> SendTemplateAsync(
        string to,
        string templateName,
        Dictionary<string, string>? parameters = null,
        string? languageCode = "es",
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Provider name identifier.
    /// </summary>
    string ProviderName { get; }
}
