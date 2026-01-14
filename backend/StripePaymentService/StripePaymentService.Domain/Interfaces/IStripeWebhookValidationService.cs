namespace StripePaymentService.Domain.Interfaces;

/// <summary>
/// Servicio para validar webhooks de Stripe
/// </summary>
public interface IStripeWebhookValidationService
{
    bool ValidateWebhookSignature(string payload, string signature, string endpointSecret);
    Dictionary<string, object?> ExtractWebhookData(string payload);
    bool IsAuthenticStripeWebhook(string payload, string signature, string endpointSecret);
}
