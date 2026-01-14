using Microsoft.EntityFrameworkCore;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Infrastructure.Persistence;

namespace StripePaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Payment Intent
/// </summary>
public class StripePaymentIntentRepository : IStripePaymentIntentRepository
{
    private readonly StripeDbContext _context;

    public StripePaymentIntentRepository(StripeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<StripePaymentIntent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StripePaymentIntent?> GetByStripeIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .FirstOrDefaultAsync(x => x.StripePaymentIntentId == stripePaymentIntentId, cancellationToken);
    }

    public async Task<List<StripePaymentIntent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StripePaymentIntent>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StripePaymentIntent>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .Where(x => x.Status == "requires_payment_method" || x.Status == "requires_confirmation")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StripePaymentIntent>> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentIntents
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(StripePaymentIntent entity, CancellationToken cancellationToken = default)
    {
        await _context.PaymentIntents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StripePaymentIntent entity, CancellationToken cancellationToken = default)
    {
        _context.PaymentIntents.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.PaymentIntents.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
