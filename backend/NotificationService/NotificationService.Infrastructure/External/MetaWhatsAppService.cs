using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces.External;

namespace NotificationService.Infrastructure.External;

/// <summary>
/// WhatsApp provider using Meta WhatsApp Business Cloud API.
/// Reads phone_number_id and access_token from ConfigurationService.
/// Docs: https://developers.facebook.com/docs/whatsapp/cloud-api
/// </summary>
public class MetaWhatsAppService : IWhatsAppProvider
{
    private const string MetaApiBaseUrl = "https://graph.facebook.com/v18.0";

    private readonly HttpClient _httpClient;
    private readonly IConfigurationServiceClient _configClient;
    private readonly ILogger<MetaWhatsAppService> _logger;

    public MetaWhatsAppService(
        HttpClient httpClient,
        IConfigurationServiceClient configClient,
        ILogger<MetaWhatsAppService> logger)
    {
        _httpClient = httpClient;
        _configClient = configClient;
        _logger = logger;
    }

    public string ProviderName => "MetaWhatsApp";

    public async Task<(bool success, string? messageId, string? error)> SendMessageAsync(
        string to,
        string message,
        Dictionary<string, object>? metadata = null)
    {
        var (phoneNumberId, accessToken) = await GetMetaCredentials();

        if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogInformation(
                "[MOCK] Meta WhatsApp message would be sent to {To}: {Message}", to, message);
            return (true, $"mock-meta-whatsapp-{Guid.NewGuid()}", null);
        }

        try
        {
            var formattedTo = StripWhatsAppPrefix(to);
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = formattedTo,
                type = "text",
                text = new { preview_url = false, body = message }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{MetaApiBaseUrl}/{phoneNumberId}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<MetaMessageResponse>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var msgId = result?.Messages?.FirstOrDefault()?.Id;
                _logger.LogInformation(
                    "Meta WhatsApp message sent to {To}. MessageId: {MessageId}", to, msgId);
                return (true, msgId, null);
            }

            _logger.LogWarning(
                "Meta WhatsApp API error for {To}. Status: {Status}, Body: {Body}",
                to, response.StatusCode, responseBody);
            return (false, null, $"Meta API error: {response.StatusCode} - {responseBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Meta WhatsApp message to {To}", to);
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
        var (phoneNumberId, accessToken) = await GetMetaCredentials();

        if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogInformation(
                "[MOCK] Meta WhatsApp template '{Template}' would be sent to {To}",
                templateName, to);
            return (true, $"mock-meta-template-{Guid.NewGuid()}", null);
        }

        try
        {
            var formattedTo = StripWhatsAppPrefix(to);

            // Build template components from parameters
            var components = new List<object>();
            if (parameters != null && parameters.Count > 0)
            {
                var bodyParams = parameters.Select(p => new
                {
                    type = "text",
                    text = p.Value
                }).ToArray();

                components.Add(new
                {
                    type = "body",
                    parameters = bodyParams
                });
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = formattedTo,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode ?? "es" },
                    components = components.Count > 0 ? components.ToArray() : null
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{MetaApiBaseUrl}/{phoneNumberId}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<MetaMessageResponse>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var msgId = result?.Messages?.FirstOrDefault()?.Id;
                _logger.LogInformation(
                    "Meta WhatsApp template '{Template}' sent to {To}. MessageId: {MessageId}",
                    templateName, to, msgId);
                return (true, msgId, null);
            }

            _logger.LogWarning(
                "Meta WhatsApp template API error for {To}. Status: {Status}, Body: {Body}",
                to, response.StatusCode, responseBody);
            return (false, null, $"Meta API error: {response.StatusCode} - {responseBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending Meta WhatsApp template '{Template}' to {To}", templateName, to);
            return (false, null, ex.Message);
        }
    }

    private async Task<(string? phoneNumberId, string? accessToken)> GetMetaCredentials()
    {
        var phoneNumberId = await _configClient.GetValueAsync("whatsapp.meta_phone_number_id");
        var accessToken = await _configClient.GetValueAsync("whatsapp.meta_access_token");
        return (phoneNumberId, accessToken);
    }

    private static string StripWhatsAppPrefix(string phone)
    {
        if (phone.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            phone = phone["whatsapp:".Length..];

        // Strip + for Meta API (it expects country code digits)
        return phone.TrimStart('+');
    }

    // DTOs for Meta WhatsApp API response
    private sealed class MetaMessageResponse
    {
        public string? MessagingProduct { get; set; }
        public List<MetaMessageContact>? Contacts { get; set; }
        public List<MetaMessageInfo>? Messages { get; set; }
    }

    private sealed class MetaMessageContact
    {
        public string? Input { get; set; }
        public string? WaId { get; set; }
    }

    private sealed class MetaMessageInfo
    {
        public string? Id { get; set; }
    }
}
