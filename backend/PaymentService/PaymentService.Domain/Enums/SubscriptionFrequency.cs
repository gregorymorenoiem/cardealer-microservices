namespace PaymentService.Domain.Enums;

/// <summary>
/// Frecuencia de recurrencia para suscripciones
/// </summary>
public enum SubscriptionFrequency
{
    /// <summary>
    /// Cobro diario
    /// </summary>
    Daily = 0,

    /// <summary>
    /// Cobro semanal
    /// </summary>
    Weekly = 1,

    /// <summary>
    /// Cobro cada dos semanas
    /// </summary>
    BiWeekly = 2,

    /// <summary>
    /// Cobro mensual
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// Cobro trimestral (cada 3 meses)
    /// </summary>
    Quarterly = 4,

    /// <summary>
    /// Cobro semestral (cada 6 meses)
    /// </summary>
    SemiAnnual = 5,

    /// <summary>
    /// Cobro anual
    /// </summary>
    Annual = 6
}
