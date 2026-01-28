using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Interfaz para operaciones de repositorio de suscripciones AZUL
/// </summary>
public interface IAzulSubscriptionRepository
{
    /// <summary>
    /// Crear una nueva suscripción
    /// </summary>
    Task<AzulSubscription> CreateAsync(AzulSubscription subscription, CancellationToken ct = default);

    /// <summary>
    /// Obtener suscripción por ID
    /// </summary>
    Task<AzulSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtener suscripción por ID de AZUL
    /// </summary>
    Task<AzulSubscription?> GetByAzulIdAsync(string azulSubscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Obtener todas las suscripciones de un usuario
    /// </summary>
    Task<List<AzulSubscription>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Obtener suscripciones activas
    /// </summary>
    Task<List<AzulSubscription>> GetActiveSubscriptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtener suscripciones cuyo próximo cobro es hoy
    /// </summary>
    Task<List<AzulSubscription>> GetDueForChargeAsync(CancellationToken ct = default);

    /// <summary>
    /// Actualizar suscripción
    /// </summary>
    Task<AzulSubscription> UpdateAsync(AzulSubscription subscription, CancellationToken ct = default);

    /// <summary>
    /// Cancelar suscripción
    /// </summary>
    Task<bool> CancelAsync(Guid id, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Obtener suscripciones por estado
    /// </summary>
    Task<List<AzulSubscription>> GetByStatusAsync(string status, CancellationToken ct = default);

    /// <summary>
    /// Obtener MRR (Monthly Recurring Revenue)
    /// </summary>
    Task<decimal> GetMRRAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtener conteo de suscripciones activas
    /// </summary>
    Task<int> GetActiveSubscriptionCountAsync(CancellationToken ct = default);
}
