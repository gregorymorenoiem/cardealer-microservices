using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestionar métodos de pago guardados
/// Implementa operaciones CRUD y lógica de negocio para tarjetas tokenizadas
/// </summary>
public class SavedPaymentMethodRepository : ISavedPaymentMethodRepository
{
    private readonly AzulDbContext _context;

    public SavedPaymentMethodRepository(AzulDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod?> GetByIdAndUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SavedPaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SavedPaymentMethod>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsActive)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.LastUsedAt ?? pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod?> GetByTokenAsync(string token, PaymentGateway gateway, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .FirstOrDefaultAsync(pm => pm.Token == token && pm.PaymentGateway == gateway, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsSimilarAsync(Guid userId, string last4, string brand, int expMonth, int expYear, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .AnyAsync(pm => 
                pm.UserId == userId && 
                pm.CardLast4 == last4 && 
                pm.CardBrand.ToLower() == brand.ToLower() && 
                pm.ExpirationMonth == expMonth && 
                pm.ExpirationYear == expYear &&
                pm.IsActive, 
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .CountAsync(pm => pm.UserId == userId && pm.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod> CreateAsync(SavedPaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        if (paymentMethod.Id == Guid.Empty)
        {
            paymentMethod.Id = Guid.NewGuid();
        }
        
        paymentMethod.CreatedAt = DateTime.UtcNow;

        // Si es el primer método de pago del usuario, hacerlo default automáticamente
        var existingCount = await CountByUserIdAsync(paymentMethod.UserId, cancellationToken);
        if (existingCount == 0)
        {
            paymentMethod.IsDefault = true;
        }
        else if (paymentMethod.IsDefault)
        {
            // Si se marca como default, desmarcar los demás
            await ClearDefaultAsync(paymentMethod.UserId, cancellationToken);
        }

        _context.SavedPaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync(cancellationToken);

        return paymentMethod;
    }

    /// <inheritdoc />
    public async Task<SavedPaymentMethod> UpdateAsync(SavedPaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        _context.SavedPaymentMethods.Update(paymentMethod);
        await _context.SaveChangesAsync(cancellationToken);
        return paymentMethod;
    }

    /// <inheritdoc />
    public async Task SetAsDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verificar que el método existe y pertenece al usuario
        var paymentMethod = await GetByIdAndUserAsync(id, userId, cancellationToken);
        if (paymentMethod == null)
        {
            throw new InvalidOperationException($"Payment method {id} not found for user {userId}");
        }

        // Desmarcar todos los demás
        await ClearDefaultAsync(userId, cancellationToken);

        // Marcar este como default
        paymentMethod.IsDefault = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await GetByIdAndUserAsync(id, userId, cancellationToken);
        if (paymentMethod == null)
        {
            throw new InvalidOperationException($"Payment method {id} not found for user {userId}");
        }

        paymentMethod.IsActive = false;
        paymentMethod.IsDefault = false;

        // Si era el default, asignar otro como default
        var wasDefault = paymentMethod.IsDefault;
        if (wasDefault)
        {
            var nextDefault = await _context.SavedPaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsActive && pm.Id != id)
                .OrderByDescending(pm => pm.LastUsedAt ?? pm.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextDefault != null)
            {
                nextDefault.IsDefault = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await GetByIdAndUserAsync(id, userId, cancellationToken);
        if (paymentMethod == null)
        {
            throw new InvalidOperationException($"Payment method {id} not found for user {userId}");
        }

        var wasDefault = paymentMethod.IsDefault;

        _context.SavedPaymentMethods.Remove(paymentMethod);

        // Si era el default, asignar otro como default
        if (wasDefault)
        {
            var nextDefault = await _context.SavedPaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsActive && pm.Id != id)
                .OrderByDescending(pm => pm.LastUsedAt ?? pm.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextDefault != null)
            {
                nextDefault.IsDefault = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RecordUsageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await GetByIdAsync(id, cancellationToken);
        if (paymentMethod != null)
        {
            paymentMethod.RecordUsage();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<List<SavedPaymentMethod>> GetExpiringSoonAsync(int withinMonths = 2, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddMonths(withinMonths);

        return await _context.SavedPaymentMethods
            .Where(pm => pm.IsActive && pm.Type == SavedPaymentMethodType.Card)
            .ToListAsync(cancellationToken)
            // Filtrar en memoria porque EF Core no puede traducir la lógica de fechas de expiración
            .ContinueWith(task => task.Result
                .Where(pm => pm.ExpiresWithinMonths(withinMonths) && !pm.IsExpired())
                .ToList(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SavedPaymentMethod>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SavedPaymentMethods
            .Where(pm => pm.IsActive && pm.Type == SavedPaymentMethodType.Card)
            .ToListAsync(cancellationToken)
            // Filtrar en memoria porque EF Core no puede traducir la lógica de fechas de expiración
            .ContinueWith(task => task.Result
                .Where(pm => pm.IsExpired())
                .ToList(), cancellationToken);
    }

    /// <summary>
    /// Desmarca todos los métodos de pago de un usuario como default
    /// </summary>
    private async Task ClearDefaultAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var currentDefaults = await _context.SavedPaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var pm in currentDefaults)
        {
            pm.IsDefault = false;
        }
    }
}
