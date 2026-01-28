using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// Servicio de integración con WhatsApp Business API via Twilio
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;

    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _accountSid = _configuration["Twilio:AccountSid"] ?? "MOCK_ACCOUNT_SID";
        _authToken = _configuration["Twilio:AuthToken"] ?? "MOCK_AUTH_TOKEN";
        _fromNumber = _configuration["Twilio:WhatsAppNumber"] ?? "whatsapp:+18095551234";

        // Configure Twilio API
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
        _httpClient.BaseAddress = new Uri($"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
    }

    public async Task<WhatsAppHandoff> SendHandoffMessageAsync(
        string toPhoneNumber,
        string dealerName,
        string userName,
        string userPhone,
        string vehicleDetails,
        string conversationSummary,
        int leadScore,
        CancellationToken cancellationToken = default)
    {
        var handoff = new WhatsAppHandoff
        {
            DealerWhatsAppNumber = FormatWhatsAppNumber(toPhoneNumber),
            UserName = userName,
            UserPhone = userPhone,
            LeadScore = leadScore,
            LeadTemperature = DetermineTemperature(leadScore),
            ConversationSummary = conversationSummary,
            VehicleDetails = vehicleDetails,
            Status = WhatsAppStatus.Pending
        };

        var message = FormatHandoffMessage(dealerName, userName, userPhone, vehicleDetails, conversationSummary, leadScore);

        try
        {
            // In production, send actual WhatsApp message via Twilio
            if (_accountSid != "MOCK_ACCOUNT_SID")
            {
                var formData = new Dictionary<string, string>
                {
                    ["From"] = _fromNumber,
                    ["To"] = handoff.DealerWhatsAppNumber,
                    ["Body"] = message
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync("Messages.json", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                    handoff.WhatsAppMessageId = result?.RootElement.GetProperty("sid").GetString();
                    handoff.Status = WhatsAppStatus.Sent;
                    handoff.SentAt = DateTime.UtcNow;
                }
                else
                {
                    handoff.Status = WhatsAppStatus.Failed;
                    handoff.ErrorMessage = $"Twilio API error: {response.StatusCode}";
                }
            }
            else
            {
                // Mock mode for development
                _logger.LogInformation("MOCK WhatsApp message sent to {Phone}: {Message}", toPhoneNumber, message);
                handoff.WhatsAppMessageId = $"MOCK_{Guid.NewGuid():N}";
                handoff.Status = WhatsAppStatus.Sent;
                handoff.SentAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message");
            handoff.Status = WhatsAppStatus.Failed;
            handoff.ErrorMessage = ex.Message;
        }

        return handoff;
    }

    public async Task<WhatsAppStatus> GetMessageStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (_accountSid == "MOCK_ACCOUNT_SID" || messageId.StartsWith("MOCK_"))
        {
            return WhatsAppStatus.Delivered; // Mock
        }

        try
        {
            var response = await _httpClient.GetAsync($"Messages/{messageId}.json", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            var status = result?.RootElement.GetProperty("status").GetString();

            return status switch
            {
                "queued" or "sending" => WhatsAppStatus.Pending,
                "sent" => WhatsAppStatus.Sent,
                "delivered" => WhatsAppStatus.Delivered,
                "read" => WhatsAppStatus.Read,
                _ => WhatsAppStatus.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting WhatsApp message status");
            return WhatsAppStatus.Failed;
        }
    }

    public bool IsValidWhatsAppNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove all non-digits
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // WhatsApp number should be 10-15 digits (E.164 format without +)
        return digits.Length >= 10 && digits.Length <= 15;
    }

    public string FormatWhatsAppNumber(string phoneNumber)
    {
        // Remove all non-digits
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // If doesn't start with country code, assume Dominican Republic (+1809)
        if (digits.Length == 10 && !digits.StartsWith("1"))
        {
            digits = "1809" + digits;
        }

        return $"whatsapp:+{digits}";
    }

    private string FormatHandoffMessage(
        string dealerName,
        string userName,
        string userPhone,
        string vehicleDetails,
        string conversationSummary,
        int leadScore)
    {
        var temperature = leadScore >= 85 ? "🔥 HOT" : leadScore >= 70 ? "🟠 WARM-HOT" : "🟡 WARM";

        return $@"
🚗 *Nuevo Lead {temperature}* desde OKLA.com.do

👤 *Prospecto:* {userName}
📱 *Teléfono:* {userPhone}
📊 *Score:* {leadScore}/100

🚙 *Vehículo de interés:*
{vehicleDetails}

💬 *Resumen de la conversación:*
{conversationSummary}

⚡ *Acción recomendada:*
{GetActionRecommendation(leadScore)}

---
Enviado por OKLA Bot
".Trim();
    }

    private string GetActionRecommendation(int score)
    {
        return score switch
        {
            >= 85 => "Contactar INMEDIATAMENTE. Lead listo para cerrar.",
            >= 70 => "Contactar HOY. Agendar test drive o visita.",
            >= 50 => "Contactar en 24h. Nutrir con más información.",
            _ => "Seguimiento en 48-72h. Email o WhatsApp."
        };
    }

    private LeadTemperature DetermineTemperature(int score)
    {
        return score switch
        {
            >= 85 => LeadTemperature.Hot,
            >= 70 => LeadTemperature.WarmHot,
            >= 50 => LeadTemperature.Warm,
            _ => LeadTemperature.Cold
        };
    }
}
