using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Servicio para validar webhooks de AZUL
/// </summary>
public class AzulWebhookValidationService : IAzulWebhookValidationService
{
    private readonly string _webhookSecret;
    private readonly ILogger<AzulWebhookValidationService> _logger;

    public AzulWebhookValidationService(
        IConfiguration configuration,
        ILogger<AzulWebhookValidationService> logger)
    {
        _webhookSecret = configuration["Azul:WebhookSecret"] ?? throw new InvalidOperationException("Azul:WebhookSecret not configured");
        _logger = logger;
    }

    /// <summary>
    /// Valida la firma del webhook
    /// </summary>
    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            // AZUL usa SHA256 con la clave secreta
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToHexString(hash).ToLower();
                
                // Comparación segura (constant-time)
                var isValid = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computedSignature),
                    Encoding.UTF8.GetBytes(signature.ToLower())
                );

                if (!isValid)
                {
                    _logger.LogWarning("Firma de webhook inválida. Esperada: {Expected}, Recibida: {Received}",
                        computedSignature, signature);
                }

                return isValid;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar firma de webhook");
            return false;
        }
    }

    /// <summary>
    /// Extrae datos del payload del webhook
    /// </summary>
    public Dictionary<string, object> ExtractWebhookData(string payload)
    {
        try
        {
            using (var jsonDoc = JsonDocument.Parse(payload))
            {
                var result = new Dictionary<string, object>();
                var root = jsonDoc.RootElement;

                foreach (var property in root.EnumerateObject())
                {
                    result[property.Name] = property.Value.GetRawText();
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al extraer datos del webhook");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Verifica si el webhook es auténtico de AZUL
    /// </summary>
    public bool IsAuthenticAzulWebhook(string payload, string signature)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Payload o firma vacíos en webhook");
            return false;
        }

        return ValidateWebhookSignature(payload, signature);
    }
}
