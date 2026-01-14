namespace AzulPaymentService.Domain.Interfaces;

/// <summary>
/// Interfaz para servicio de validación de webhooks de AZUL
/// </summary>
public interface IAzulWebhookValidationService
{
    /// <summary>
    /// Validar firma de webhook de AZUL
    /// </summary>
    /// <param name="payload">Payload del webhook en JSON</param>
    /// <param name="signature">Firma recibida en header</param>
    /// <returns>True si la firma es válida</returns>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>
    /// Extraer información de un webhook
    /// </summary>
    /// <param name="payload">Payload del webhook</param>
    /// <returns>Dictionary con información extraída</returns>
    Dictionary<string, object> ExtractWebhookData(string payload);

    /// <summary>
    /// Verificar que el webhook es de AZUL (no spoofing)
    /// </summary>
    bool IsAuthenticAzulWebhook(string payload, string signature);
}
