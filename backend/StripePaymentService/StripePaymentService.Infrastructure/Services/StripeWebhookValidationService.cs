using System.Security.Cryptography;
using System.Text;
using Serilog;
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

    public bool ValidateWebhookSignature(string payload, string signatureHeader)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signatureHeader))
        {
            _logger.Warning("Payload o Signature vacío");
            return false;
        }

        try
        {
            // Stripe signature format: t=timestamp,v1=signature
            var parts = signatureHeader.Split(',');
            var timestamp = parts.FirstOrDefault(p => p.StartsWith("t="))?.Substring(2);
            var signature = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Substring(3);

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
            {
                _logger.Warning("Formato de signature inválido");
                return false;
            }

            // Crear signed content: timestamp.payload
            var signedContent = $"{timestamp}.{payload}";

            // HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));
                var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                if (computedSignature != signature)
                {
                    _logger.Warning("Signature no coincide");
                    return false;
                }

                // Validar que no sea muy antiguo (5 minutos)
                if (!long.TryParse(timestamp, out var ts))
                {
                    _logger.Warning("Timestamp inválido");
                    return false;
                }

                var webhookTime = UnixTimeStampToDateTime(ts);
                if (DateTime.UtcNow - webhookTime > TimeSpan.FromMinutes(5))
                {
                    _logger.Warning("Webhook demasiado antiguo");
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validando webhook signature");
            return false;
        }
    }

    public bool IsAuthenticStripeWebhook(string payload, string signatureHeader)
    {
        return ValidateWebhookSignature(payload, signatureHeader);
    }

    public string? ExtractWebhookData(string payload)
    {
        try
        {
            if (string.IsNullOrEmpty(payload))
            {
                _logger.Warning("Payload vacío");
                return null;
            }

            return payload;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error extrayendo webhook data");
            return null;
        }
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}
