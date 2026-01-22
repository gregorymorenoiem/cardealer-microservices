using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StripePaymentService.Domain.Interfaces;

namespace StripePaymentService.Infrastructure.Services;

/// <summary>
/// Servicio para validación de webhooks de Stripe
/// </summary>
public class StripeWebhookValidationService : IStripeWebhookValidationService
{
    private readonly string _webhookSecret;
    private readonly ILogger<StripeWebhookValidationService> _logger;

    public StripeWebhookValidationService(
        string webhookSecret,
        ILogger<StripeWebhookValidationService> logger)
    {
        if (string.IsNullOrEmpty(webhookSecret))
            throw new ArgumentNullException(nameof(webhookSecret));

        _webhookSecret = webhookSecret;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool ValidateWebhookSignature(string payload, string signatureHeader, string endpointSecret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signatureHeader))
        {
            _logger.LogWarning("Payload o Signature vacío");
            return false;
        }

        var secret = string.IsNullOrEmpty(endpointSecret) ? _webhookSecret : endpointSecret;

        try
        {
            // Stripe signature format: t=timestamp,v1=signature
            var parts = signatureHeader.Split(',');
            var timestamp = parts.FirstOrDefault(p => p.StartsWith("t="))?.Substring(2);
            var signature = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Substring(3);

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Formato de signature inválido");
                return false;
            }

            // Crear signed content: timestamp.payload
            var signedContent = $"{timestamp}.{payload}";

            // HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));
                var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                if (computedSignature != signature)
                {
                    _logger.LogWarning("Signature no coincide");
                    return false;
                }

                // Validar que no sea muy antiguo (5 minutos)
                if (!long.TryParse(timestamp, out var ts))
                {
                    _logger.LogWarning("Timestamp inválido");
                    return false;
                }

                var webhookTime = UnixTimeStampToDateTime(ts);
                if (DateTime.UtcNow - webhookTime > TimeSpan.FromMinutes(5))
                {
                    _logger.LogWarning("Webhook demasiado antiguo");
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando webhook signature");
            return false;
        }
    }

    public bool IsAuthenticStripeWebhook(string payload, string signatureHeader, string endpointSecret)
    {
        return ValidateWebhookSignature(payload, signatureHeader, endpointSecret);
    }

    public Dictionary<string, object?> ExtractWebhookData(string payload)
    {
        var result = new Dictionary<string, object?>();
        
        try
        {
            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Payload vacío");
                return result;
            }

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.TryGetProperty("id", out var id))
                result["id"] = id.GetString();
            
            if (root.TryGetProperty("type", out var type))
                result["type"] = type.GetString();
            
            if (root.TryGetProperty("created", out var created))
                result["created"] = created.GetInt64();
            
            if (root.TryGetProperty("livemode", out var livemode))
                result["livemode"] = livemode.GetBoolean();
            
            if (root.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("object", out var dataObject))
                {
                    result["data_object"] = dataObject.GetRawText();
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo webhook data");
            return result;
        }
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}
