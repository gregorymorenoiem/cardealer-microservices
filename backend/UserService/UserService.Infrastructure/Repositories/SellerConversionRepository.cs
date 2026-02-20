using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class SellerConversionRepository : ISellerConversionRepository
{
    private readonly ApplicationDbContext _context;

    public SellerConversionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SellerConversion?> GetByIdAsync(Guid id)
    {
        return await _context.SellerConversions
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    public async Task<SellerConversion?> GetByUserIdAsync(Guid userId)
    {
        return await _context.SellerConversions
            .FirstOrDefaultAsync(sc => sc.UserId == userId);
    }

    public async Task<SellerConversion?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.SellerConversions
            .FirstOrDefaultAsync(sc => sc.IdempotencyKey == idempotencyKey);
    }

    public async Task<SellerConversion> CreateAsync(SellerConversion conversion)
    {
        _context.SellerConversions.Add(conversion);
        await _context.SaveChangesAsync();
        return conversion;
    }

    public async Task<SellerConversion> UpdateAsync(SellerConversion conversion)
    {
        _context.SellerConversions.Update(conversion);
        await _context.SaveChangesAsync();
        return conversion;
    }

    public async Task<List<SellerConversion>> GetByStatusAsync(
        SellerConversionStatus status, int page = 1, int pageSize = 20)
    {
        return await _context.SellerConversions
            .Where(sc => sc.Status == status)
            .OrderByDescending(sc => sc.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> ExistsForUserAsync(Guid userId)
    {
        return await _context.SellerConversions
            .AnyAsync(sc => sc.UserId == userId);
    }
}
