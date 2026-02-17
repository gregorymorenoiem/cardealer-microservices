using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using ContactService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContactService.Infrastructure.Repositories;

public class ContactRequestRepository : IContactRequestRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContactRequest?> GetByIdAsync(Guid id)
    {
        return await _context.ContactRequests
            .IgnoreQueryFilters()
            .Include(cr => cr.Messages)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<List<ContactRequest>> GetByBuyerIdAsync(Guid buyerId)
    {
        return await _context.ContactRequests
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(cr => cr.Messages)
            .Where(cr => cr.BuyerId == buyerId)
            .OrderByDescending(cr => cr.CreatedAt)
            .Take(200) // Safety limit
            .ToListAsync();
    }

    public async Task<List<ContactRequest>> GetBySellerIdAsync(Guid sellerId)
    {
        return await _context.ContactRequests
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(cr => cr.Messages)
            .Where(cr => cr.SellerId == sellerId)
            .OrderByDescending(cr => cr.CreatedAt)
            .Take(200) // Safety limit
            .ToListAsync();
    }

    public async Task<List<ContactRequest>> GetByVehicleIdAsync(Guid vehicleId)
    {
        return await _context.ContactRequests
            .AsNoTracking()
            .Include(cr => cr.Messages)
            .Where(cr => cr.VehicleId == vehicleId)
            .OrderByDescending(cr => cr.CreatedAt)
            .Take(200) // Safety limit
            .ToListAsync();
    }

    public async Task<ContactRequest> CreateAsync(ContactRequest contactRequest)
    {
        _context.ContactRequests.Add(contactRequest);
        await _context.SaveChangesAsync();
        return contactRequest;
    }

    public async Task<ContactRequest> UpdateAsync(ContactRequest contactRequest)
    {
        _context.ContactRequests.Update(contactRequest);
        await _context.SaveChangesAsync();
        return contactRequest;
    }

    public async Task DeleteAsync(Guid id)
    {
        var contactRequest = await _context.ContactRequests.FindAsync(id);
        if (contactRequest != null)
        {
            _context.ContactRequests.Remove(contactRequest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountForSellerAsync(Guid sellerId)
    {
        return await _context.ContactRequests
            .AsNoTracking()
            .Where(cr => cr.SellerId == sellerId && cr.Status != "Closed")
            .CountAsync(cr => cr.Messages.Any(m => !m.IsRead && m.IsFromBuyer));
    }
}