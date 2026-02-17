namespace StripePaymentService.Domain.Enums;

/// <summary>
/// Intervalo de facturación para suscripciones
/// </summary>
public enum BillingInterval
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    SemiAnnual = 4,
    Annual = 5,
    Custom = 6
}
