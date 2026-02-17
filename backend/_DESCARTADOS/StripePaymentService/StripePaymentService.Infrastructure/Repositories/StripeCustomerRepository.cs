using Microsoft.EntityFrameworkCore;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Infrastructure.Persistence;

namespace StripePaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Stripe Customer
/// </summary>
public class StripeCustomerRepository : IStripeCustomerRepository
{
    private readonly StripeDbContext _context;

    public StripeCustomerRepository(StripeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<StripeCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(x => x.PaymentMethods)
            .Include(x => x.Subscriptions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StripeCustomer?> GetByStripeIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(x => x.PaymentMethods)
            .Include(x => x.Subscriptions)
            .FirstOrDefaultAsync(x => x.StripeCustomerId == stripeCustomerId, cancellationToken);
    }

    public async Task<StripeCustomer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(x => x.PaymentMethods)
            .Include(x => x.Subscriptions)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<StripeCustomer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(x => x.PaymentMethods)
            .Include(x => x.Subscriptions)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<List<StripeCustomer>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(x => x.IsActive)
            .Include(x => x.PaymentMethods)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<StripeCustomer> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Customers.CountAsync(cancellationToken);
        var items = await _context.Customers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.PaymentMethods)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<StripeCustomer> CreateAsync(StripeCustomer entity, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<StripeCustomer> UpdateAsync(StripeCustomer entity, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Customers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
