using Microsoft.EntityFrameworkCore;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Infrastructure.Persistence;

namespace AzulPaymentService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de suscripciones AZUL
/// </summary>
public class AzulSubscriptionRepository : IAzulSubscriptionRepository
{
    private readonly AzulDbContext _context;

    public AzulSubscriptionRepository(AzulDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una nueva suscripción
    /// </summary>
    public async Task<AzulSubscription> CreateAsync(AzulSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.AzulSubscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    /// <summary>
    /// Obtiene suscripción por ID interno
    /// </summary>
    public async Task<AzulSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AzulSubscriptions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene suscripción por ID de AZUL
    /// </summary>
    public async Task<AzulSubscription?> GetByAzulIdAsync(string azulSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.AzulSubscriptions
            .FirstOrDefaultAsync(x => x.AzulSubscriptionId == azulSubscriptionId, cancellationToken);
    }

    /// <summary>
    /// Obtiene todas las suscripciones de un usuario
    /// </summary>
    public async Task<List<AzulSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AzulSubscriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene todas las suscripciones activas
    /// </summary>
    public async Task<List<AzulSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AzulSubscriptions
            .Where(x => x.Status == "Active" && (x.EndDate == null || x.EndDate > DateTime.UtcNow))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene suscripciones que vencen hoy (próximas a cobrar)
    /// </summary>
    public async Task<List<AzulSubscription>> GetDueForChargeAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.AzulSubscriptions
            .Where(x => x.Status == "Active" &&
                       x.NextChargeDate <= now &&
                       (x.EndDate == null || x.EndDate > now))
            .OrderBy(x => x.NextChargeDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza una suscripción
    /// </summary>
    public async Task<AzulSubscription> UpdateAsync(AzulSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.AzulSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    /// <summary>
    /// Cancela una suscripción
    /// </summary>
    public async Task<bool> CancelAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return false;

        subscription.Status = "Cancelled";
        subscription.CancellationReason = reason ?? "Cancelada por usuario";
        subscription.CancelledAt = DateTime.UtcNow;

        await UpdateAsync(subscription, cancellationToken);
        return true;
    }

    /// <summary>
    /// Obtiene suscripciones por estado
    /// </summary>
    public async Task<List<AzulSubscription>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.AzulSubscriptions
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula el MRR (Monthly Recurring Revenue)
    /// </summary>
    public async Task<decimal> GetMRRAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var activeSubscriptions = await _context.AzulSubscriptions
            .Where(x => x.Status == "Active" && (x.EndDate == null || x.EndDate > now))
            .ToListAsync(cancellationToken);

        // Normalizar a MRR (asumiendo que todas son mensuales por simplicidad)
        return activeSubscriptions.Sum(x => x.Amount);
    }

    /// <summary>
    /// Obtiene cantidad de suscripciones activas
    /// </summary>
    public async Task<int> GetActiveSubscriptionCountAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.AzulSubscriptions
            .CountAsync(x => x.Status == "Active" && (x.EndDate == null || x.EndDate > now), cancellationToken);
    }
}
