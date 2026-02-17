using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Enums;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// WhatsApp integration service using Meta Cloud API.
/// Handles inbound webhook processing, outbound messaging,
/// and bot↔human handoff coordination.
/// 
/// Flow: Meta Cloud API → Webhook → WhatsAppService → ChatbotService pipeline
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly string _verifyToken;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;
    private readonly string _apiVersion;
    private readonly HashSet<string> _allowedCountryCodes;

    public WhatsAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WhatsApp");
        _logger = logger;
        _verifyToken = configuration["WhatsApp:VerifyToken"] ?? "okla-whatsapp-verify-2026";
        _accessToken = configuration["WhatsApp:AccessToken"] ?? "";
        _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? "";
        _apiVersion = configuration["WhatsApp:ApiVersion"] ?? "v18.0";

        // Solo RD por ahora (+1809, +1829, +1849)
        var allowedCodes = configuration["WhatsApp:AllowedCountryCodes"] ?? "1809,1829,1849";
        _allowedCountryCodes = new HashSet<string>(allowedCodes.Split(','));
    }

    /// <summary>
    /// Verifica el webhook de Meta (challenge de verificación inicial)
    /// </summary>
    public bool VerifyWebhook(string mode, string token, string challenge)
    {
        if (mode == "subscribe" && token == _verifyToken)
        {
            _logger.LogInformation("WhatsApp webhook verified successfully");
            return true;
        }

        _logger.LogWarning("WhatsApp webhook verification failed: mode={Mode}", mode);
        return false;
    }

    /// <summary>
    /// Parsea un payload de webhook entrante de Meta
    /// </summary>
    public WhatsAppInboundMessage? ParseInboundMessage(JsonElement payload)
    {
        try
        {
            var entry = payload.GetProperty("entry")[0];
            var changes = entry.GetProperty("changes")[0];
            var value = changes.GetProperty("value");

            if (!value.TryGetProperty("messages", out var messages) || messages.GetArrayLength() == 0)
            {
                // Es una notificación de status (delivered, read, etc.) — no un mensaje
                return null;
            }

            var message = messages[0];
            var contact = value.GetProperty("contacts")[0];

            var messageType = message.GetProperty("type").GetString() ?? "text";
            string? textBody = null;
            string? mediaUrl = null;

            switch (messageType)
            {
                case "text":
                    textBody = message.GetProperty("text").GetProperty("body").GetString();
                    break;
                case "image":
                    textBody = "[Imagen recibida]";
                    mediaUrl = message.GetProperty("image").GetProperty("id").GetString();
                    break;
                case "audio":
                    textBody = "[Audio recibido - por favor escribe tu mensaje]";
                    break;
                case "interactive":
                    if (message.TryGetProperty("interactive", out var interactive))
                    {
                        if (interactive.TryGetProperty("button_reply", out var button))
                            textBody = button.GetProperty("title").GetString();
                        else if (interactive.TryGetProperty("list_reply", out var list))
                            textBody = list.GetProperty("title").GetString();
                    }
                    break;
                default:
                    textBody = $"[Tipo de mensaje no soportado: {messageType}]";
                    break;
            }

            var fromPhone = message.GetProperty("from").GetString() ?? "";
            var profileName = contact.GetProperty("profile").GetProperty("name").GetString() ?? "Usuario";

            return new WhatsAppInboundMessage
            {
                MessageId = message.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                From = fromPhone,
                ProfileName = profileName,
                Body = textBody ?? "",
                MediaUrl = mediaUrl,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(
                    long.Parse(message.GetProperty("timestamp").GetString() ?? "0")).UtcDateTime,
                MessageType = messageType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing WhatsApp inbound message");
            return null;
        }
    }

    /// <summary>
    /// Envía un mensaje de texto al usuario por WhatsApp
    /// </summary>
    public async Task<bool> SendTextMessageAsync(string toPhone, string message, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhone,
                type = "text",
                text = new { body = message }
            };

            var response = await SendApiRequestAsync(payload, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp text message to {Phone}", toPhone);
            return false;
        }
    }

    /// <summary>
    /// Envía un mensaje con botones interactivos (quick replies)
    /// </summary>
    public async Task<bool> SendInteractiveMessageAsync(
        string toPhone,
        string headerText,
        string bodyText,
        List<(string Id, string Title)> buttons,
        CancellationToken ct = default)
    {
        try
        {
            var buttonsList = buttons.Take(3).Select(b => new
            {
                type = "reply",
                reply = new { id = b.Id, title = b.Title.Length > 20 ? b.Title[..20] : b.Title }
            }).ToArray();

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhone,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    header = new { type = "text", text = headerText },
                    body = new { text = bodyText },
                    action = new { buttons = buttonsList }
                }
            };

            return await SendApiRequestAsync(payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp interactive message to {Phone}", toPhone);
            return false;
        }
    }

    /// <summary>
    /// Marca un mensaje como leído
    /// </summary>
    public async Task MarkAsReadAsync(string messageId, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                status = "read",
                message_id = messageId
            };
            await SendApiRequestAsync(payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error marking WhatsApp message as read: {MessageId}", messageId);
        }
    }

    /// <summary>
    /// Valida si un número de teléfono es de RD (país soportado)
    /// </summary>
    public bool IsAllowedCountry(string phoneNumber)
    {
        var cleaned = phoneNumber.TrimStart('+');
        return _allowedCountryCodes.Any(code => cleaned.StartsWith(code));
    }

    /// <summary>
    /// Rate limiting por número de teléfono (en memoria, escala con Redis en producción)
    /// </summary>
    private static readonly Dictionary<string, (int Count, DateTime WindowStart)> _rateLimits = new();
    private const int MaxMessagesPerMinute = 10;

    public bool CheckRateLimit(string phoneNumber)
    {
        var now = DateTime.UtcNow;

        lock (_rateLimits)
        {
            if (_rateLimits.TryGetValue(phoneNumber, out var entry))
            {
                if ((now - entry.WindowStart).TotalMinutes >= 1)
                {
                    // Window expired, reset
                    _rateLimits[phoneNumber] = (1, now);
                    return true;
                }

                if (entry.Count >= MaxMessagesPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for WhatsApp number {Phone}", phoneNumber);
                    return false;
                }

                _rateLimits[phoneNumber] = (entry.Count + 1, entry.WindowStart);
                return true;
            }

            _rateLimits[phoneNumber] = (1, now);
            return true;
        }
    }

    private async Task<bool> SendApiRequestAsync(object payload, CancellationToken ct)
    {
        var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _httpClient.PostAsync(url, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("WhatsApp API error: {Status} - {Body}",
                response.StatusCode, errorBody);
            return false;
        }

        return true;
    }
}
