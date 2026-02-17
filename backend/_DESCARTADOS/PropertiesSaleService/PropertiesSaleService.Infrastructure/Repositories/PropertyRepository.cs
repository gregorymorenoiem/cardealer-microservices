using Microsoft.EntityFrameworkCore;
using PropertiesSaleService.Domain.Entities;
using PropertiesSaleService.Domain.Interfaces;
using PropertiesSaleService.Infrastructure.Persistence;

namespace PropertiesSaleService.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Property?> GetByMLSNumberAsync(string mlsNumber)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.MLSNumber == mlsNumber && !p.IsDeleted);
    }

    public async Task<IEnumerable<Property>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> SearchAsync(PropertySearchParameters p)
    {
        var query = BuildSearchQuery(p);

        query = p.SortBy?.ToLower() switch
        {
            "price" => p.SortDescending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "sqft" or "squarefeet" => p.SortDescending ? query.OrderByDescending(x => x.SquareFeet) : query.OrderBy(x => x.SquareFeet),
            "bedrooms" => p.SortDescending ? query.OrderByDescending(x => x.Bedrooms) : query.OrderBy(x => x.Bedrooms),
            "yearbuilt" => p.SortDescending ? query.OrderByDescending(x => x.YearBuilt) : query.OrderBy(x => x.YearBuilt),
            _ => p.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

        return await query.Skip(p.Skip).Take(p.Take).ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetByAgentAsync(Guid agentId)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Include(p => p.Category)
            .Where(p => p.AgentId == agentId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetByDealerAsync(Guid dealerId)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Include(p => p.Category)
            .Where(p => p.DealerId == dealerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetFeaturedAsync(int take = 10)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && p.IsFeatured && p.Status == PropertyStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Property> CreateAsync(Property property)
    {
        property.Id = Guid.NewGuid();
        property.CreatedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task UpdateAsync(Property property)
    {
        property.UpdatedAt = DateTime.UtcNow;
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property != null)
        {
            property.IsDeleted = true;
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Properties.AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<int> GetCountAsync(PropertySearchParameters? parameters = null)
    {
        if (parameters == null)
            return await _context.Properties.CountAsync(p => !p.IsDeleted);

        return await BuildSearchQuery(parameters).CountAsync();
    }

    private IQueryable<Property> BuildSearchQuery(PropertySearchParameters p)
    {
        var query = _context.Properties
            .Include(x => x.Images.Where(i => i.IsPrimary))
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted && x.Status == PropertyStatus.Active);

        if (!string.IsNullOrWhiteSpace(p.SearchTerm))
        {
            var term = p.SearchTerm.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Description!.ToLower().Contains(term) ||
                x.StreetAddress.ToLower().Contains(term) ||
                x.City.ToLower().Contains(term));
        }

        if (p.CategoryId.HasValue) query = query.Where(x => x.CategoryId == p.CategoryId.Value);
        if (p.MinPrice.HasValue) query = query.Where(x => x.Price >= p.MinPrice.Value);
        if (p.MaxPrice.HasValue) query = query.Where(x => x.Price <= p.MaxPrice.Value);
        if (p.PropertyType.HasValue) query = query.Where(x => x.PropertyType == p.PropertyType.Value);
        if (p.PropertySubType.HasValue) query = query.Where(x => x.PropertySubType == p.PropertySubType.Value);
        if (p.MinBedrooms.HasValue) query = query.Where(x => x.Bedrooms >= p.MinBedrooms.Value);
        if (p.MaxBedrooms.HasValue) query = query.Where(x => x.Bedrooms <= p.MaxBedrooms.Value);
        if (p.MinBathrooms.HasValue) query = query.Where(x => x.Bathrooms >= p.MinBathrooms.Value);
        if (p.MaxBathrooms.HasValue) query = query.Where(x => x.Bathrooms <= p.MaxBathrooms.Value);
        if (p.MinSquareFeet.HasValue) query = query.Where(x => x.SquareFeet >= p.MinSquareFeet.Value);
        if (p.MaxSquareFeet.HasValue) query = query.Where(x => x.SquareFeet <= p.MaxSquareFeet.Value);
        if (p.MinYearBuilt.HasValue) query = query.Where(x => x.YearBuilt >= p.MinYearBuilt.Value);
        if (p.MaxYearBuilt.HasValue) query = query.Where(x => x.YearBuilt <= p.MaxYearBuilt.Value);
        if (!string.IsNullOrWhiteSpace(p.State)) query = query.Where(x => x.State.ToLower() == p.State.ToLower());
        if (!string.IsNullOrWhiteSpace(p.City)) query = query.Where(x => x.City.ToLower() == p.City.ToLower());
        if (!string.IsNullOrWhiteSpace(p.ZipCode)) query = query.Where(x => x.ZipCode == p.ZipCode);
        if (!string.IsNullOrWhiteSpace(p.Neighborhood)) query = query.Where(x => x.Neighborhood!.ToLower() == p.Neighborhood.ToLower());
        if (p.HasPool.HasValue) query = query.Where(x => x.HasPool == p.HasPool.Value);
        if (p.HasGarage.HasValue) query = query.Where(x => x.GarageSpaces > 0 == p.HasGarage.Value);
        if (p.HasBasement.HasValue) query = query.Where(x => x.HasBasement == p.HasBasement.Value);
        if (p.HasFireplace.HasValue) query = query.Where(x => x.HasFireplace == p.HasFireplace.Value);
        if (p.HeatingType.HasValue) query = query.Where(x => x.HeatingType == p.HeatingType.Value);
        if (p.CoolingType.HasValue) query = query.Where(x => x.CoolingType == p.CoolingType.Value);

        return query;
    }
}
