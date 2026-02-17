using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Shared;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationService.Infrastructure.External;

/// <summary>
/// WhatsApp provider using Twilio WhatsApp Business API.
/// Reads runtime config from ConfigurationService for dynamic settings.
/// Falls back to appsettings.json Twilio credentials if ConfigurationService is unavailable.
/// </summary>
public class TwilioWhatsAppService : IWhatsAppProvider
{
    private readonly ILogger<TwilioWhatsAppService> _logger;
    private readonly IConfigurationServiceClient _configClient;
    private readonly TwilioSettings _twilioSettings;
    private readonly bool _isConfigured;

    public TwilioWhatsAppService(
        IOptions<NotificationSettings> settings,
        IConfigurationServiceClient configClient,
        ILogger<TwilioWhatsAppService> logger)
    {
        _logger = logger;
        _configClient = configClient;
        _twilioSettings = settings.Value.Twilio;

        if (!string.IsNullOrWhiteSpace(_twilioSettings.AccountSid) &&
            !string.IsNullOrWhiteSpace(_twilioSettings.AuthToken))
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
            _isConfigured = true;
        }
        else
        {
            _logger.LogWarning("Twilio credentials not configured. WhatsApp sending will be mocked.");
            _isConfigured = false;
        }
    }

    public string ProviderName => "TwilioWhatsApp";

    public async Task<(bool success, string? messageId, string? error)> SendMessageAsync(
        string to,
        string message,
        Dictionary<string, object>? metadata = null)
    {
        var fromNumber = await GetWhatsAppFromNumber();

        if (!_isConfigured || string.IsNullOrWhiteSpace(fromNumber))
        {
            _logger.LogInformation(
                "[MOCK] WhatsApp message would be sent to {To}: {Message}", to, message);
            return (true, $"mock-whatsapp-{Guid.NewGuid()}", null);
        }

        try
        {
            var formattedTo = FormatWhatsAppNumber(to);
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(formattedTo)
            );

            if (messageResource.Status != MessageResource.StatusEnum.Failed)
            {
                _logger.LogInformation(
                    "WhatsApp message sent successfully to {To}. SID: {Sid}",
                    to, messageResource.Sid);
                return (true, messageResource.Sid, null);
            }

            _logger.LogWarning(
                "Failed to send WhatsApp message to {To}. Error: {Error}",
                to, messageResource.ErrorMessage);
            return (false, null, $"Twilio error: {messageResource.ErrorMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to {To}", to);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, string? messageId, string? error)> SendTemplateAsync(
        string to,
        string templateName,
        Dictionary<string, string>? parameters = null,
        string? languageCode = "es",
        Dictionary<string, object>? metadata = null)
    {
        // Twilio WhatsApp templates are sent as content SID
        // For now, we build the template message from parameters
        var templateBody = $"[Template: {templateName}]";
        if (parameters != null)
        {
            foreach (var (key, value) in parameters)
            {
                templateBody += $" {key}={value}";
            }
        }

        _logger.LogInformation(
            "Sending WhatsApp template '{Template}' to {To} via Twilio",
            templateName, to);

        return await SendMessageAsync(to, templateBody, metadata);
    }

    private async Task<string?> GetWhatsAppFromNumber()
    {
        // Try to get from ConfigurationService first (dynamic config)
        var configNumber = await _configClient.GetValueAsync("whatsapp.twilio_whatsapp_number");
        if (!string.IsNullOrWhiteSpace(configNumber))
            return configNumber;

        // Fall back to static config
        var businessNumber = await _configClient.GetValueAsync("whatsapp.business_number");
        if (!string.IsNullOrWhiteSpace(businessNumber))
            return $"whatsapp:{businessNumber}";

        return null;
    }

    private static string FormatWhatsAppNumber(string phoneNumber)
    {
        // Ensure the number has whatsapp: prefix
        if (phoneNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            return phoneNumber;

        // Ensure it starts with +
        if (!phoneNumber.StartsWith('+'))
            phoneNumber = $"+{phoneNumber}";

        return $"whatsapp:{phoneNumber}";
    }
}
