using Microsoft.EntityFrameworkCore;
using RealEstateService.Domain.Entities;
using RealEstateService.Domain.Interfaces;
using RealEstateService.Infrastructure.Data;

namespace RealEstateService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de propiedades
/// </summary>
public class PropertyRepository : IPropertyRepository
{
    private readonly RealEstateDbContext _context;

    public PropertyRepository(RealEstateDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetByDealerIdAsync(Guid dealerId)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.DealerId == dealerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetActiveAsync()
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.Status == PropertyStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetFeaturedAsync(int limit = 10)
    {
        return await _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.Status == PropertyStatus.Active && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> SearchAsync(PropertySearchCriteria criteria)
    {
        var query = _context.Properties
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var searchTerm = criteria.Query.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Address.ToLower().Contains(searchTerm) ||
                p.City.ToLower().Contains(searchTerm) ||
                (p.Neighborhood != null && p.Neighborhood.ToLower().Contains(searchTerm))
            );
        }

        if (criteria.Type.HasValue)
            query = query.Where(p => p.Type == criteria.Type.Value);

        if (criteria.ListingType.HasValue)
            query = query.Where(p => p.ListingType == criteria.ListingType.Value);

        if (criteria.Status.HasValue)
            query = query.Where(p => p.Status == criteria.Status.Value);
        else
            query = query.Where(p => p.Status == PropertyStatus.Active);

        if (criteria.MinPrice.HasValue)
            query = query.Where(p => p.Price >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= criteria.MaxPrice.Value);

        if (criteria.MinArea.HasValue)
            query = query.Where(p => p.TotalArea >= criteria.MinArea.Value);

        if (criteria.MaxArea.HasValue)
            query = query.Where(p => p.TotalArea <= criteria.MaxArea.Value);

        if (criteria.MinBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms >= criteria.MinBedrooms.Value);

        if (criteria.MaxBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms <= criteria.MaxBedrooms.Value);

        if (criteria.MinBathrooms.HasValue)
            query = query.Where(p => p.Bathrooms >= criteria.MinBathrooms.Value);

        if (criteria.MaxBathrooms.HasValue)
            query = query.Where(p => p.Bathrooms <= criteria.MaxBathrooms.Value);

        if (!string.IsNullOrWhiteSpace(criteria.City))
            query = query.Where(p => p.City.ToLower() == criteria.City.ToLower());

        if (!string.IsNullOrWhiteSpace(criteria.State))
            query = query.Where(p => p.State.ToLower() == criteria.State.ToLower());

        if (!string.IsNullOrWhiteSpace(criteria.Neighborhood))
            query = query.Where(p => p.Neighborhood != null && p.Neighborhood.ToLower().Contains(criteria.Neighborhood.ToLower()));

        if (criteria.HasPool.HasValue)
            query = query.Where(p => p.HasPool == criteria.HasPool.Value);

        if (criteria.HasGarden.HasValue)
            query = query.Where(p => p.HasGarden == criteria.HasGarden.Value);

        if (criteria.HasGym.HasValue)
            query = query.Where(p => p.HasGym == criteria.HasGym.Value);

        if (criteria.HasSecurity.HasValue)
            query = query.Where(p => p.HasSecurity == criteria.HasSecurity.Value);

        if (criteria.IsFurnished.HasValue)
            query = query.Where(p => p.IsFurnished == criteria.IsFurnished.Value);

        if (criteria.AllowsPets.HasValue)
            query = query.Where(p => p.AllowsPets == criteria.AllowsPets.Value);

        if (criteria.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == criteria.IsFeatured.Value);

        if (criteria.DealerId.HasValue)
            query = query.Where(p => p.DealerId == criteria.DealerId.Value);

        // Apply sorting
        query = criteria.SortBy?.ToLower() switch
        {
            "price" => criteria.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "area" => criteria.SortDescending ? query.OrderByDescending(p => p.TotalArea) : query.OrderBy(p => p.TotalArea),
            "bedrooms" => criteria.SortDescending ? query.OrderByDescending(p => p.Bedrooms) : query.OrderBy(p => p.Bedrooms),
            "date" => criteria.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        // Apply pagination
        return await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(Guid? dealerId = null)
    {
        var query = _context.Properties.AsQueryable();

        if (dealerId.HasValue)
            query = query.Where(p => p.DealerId == dealerId.Value);

        return await query.CountAsync();
    }

    public async Task<Property> AddAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property> UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        return property;
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
        return await _context.Properties.AnyAsync(p => p.Id == id);
    }
}

/// <summary>
/// Implementación del repositorio de imágenes
/// </summary>
public class PropertyImageRepository : IPropertyImageRepository
{
    private readonly RealEstateDbContext _context;

    public PropertyImageRepository(RealEstateDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyImage?> GetByIdAsync(Guid id)
    {
        return await _context.PropertyImages.FindAsync(id);
    }

    public async Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(Guid propertyId)
    {
        return await _context.PropertyImages
            .Where(i => i.PropertyId == propertyId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();
    }

    public async Task<PropertyImage> AddAsync(PropertyImage image)
    {
        // If this is marked as primary, unmark others
        if (image.IsPrimary)
        {
            var existingPrimary = await _context.PropertyImages
                .Where(i => i.PropertyId == image.PropertyId && i.IsPrimary)
                .ToListAsync();

            foreach (var img in existingPrimary)
            {
                img.IsPrimary = false;
            }
        }

        // Set sort order if not specified
        if (image.SortOrder == 0)
        {
            var maxOrder = await _context.PropertyImages
                .Where(i => i.PropertyId == image.PropertyId)
                .MaxAsync(i => (int?)i.SortOrder) ?? 0;
            image.SortOrder = maxOrder + 1;
        }

        _context.PropertyImages.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<PropertyImage> UpdateAsync(PropertyImage image)
    {
        _context.PropertyImages.Update(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task DeleteAsync(Guid id)
    {
        var image = await _context.PropertyImages.FindAsync(id);
        if (image != null)
        {
            _context.PropertyImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderAsync(Guid propertyId, IEnumerable<Guid> orderedIds)
    {
        var images = await _context.PropertyImages
            .Where(i => i.PropertyId == propertyId)
            .ToListAsync();

        var order = 0;
        foreach (var id in orderedIds)
        {
            var image = images.FirstOrDefault(i => i.Id == id);
            if (image != null)
            {
                image.SortOrder = order++;
            }
        }

        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// Implementación del repositorio de amenidades
/// </summary>
public class PropertyAmenityRepository : IPropertyAmenityRepository
{
    private readonly RealEstateDbContext _context;

    public PropertyAmenityRepository(RealEstateDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PropertyAmenity>> GetByPropertyIdAsync(Guid propertyId)
    {
        return await _context.PropertyAmenities
            .Where(a => a.PropertyId == propertyId)
            .ToListAsync();
    }

    public async Task<PropertyAmenity> AddAsync(PropertyAmenity amenity)
    {
        _context.PropertyAmenities.Add(amenity);
        await _context.SaveChangesAsync();
        return amenity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var amenity = await _context.PropertyAmenities.FindAsync(id);
        if (amenity != null)
        {
            _context.PropertyAmenities.Remove(amenity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SyncAmenitiesAsync(Guid propertyId, IEnumerable<string> amenityNames)
    {
        // Remove existing amenities
        var existing = await _context.PropertyAmenities
            .Where(a => a.PropertyId == propertyId)
            .ToListAsync();

        _context.PropertyAmenities.RemoveRange(existing);

        // Get property for DealerId
        var property = await _context.Properties.FindAsync(propertyId);
        if (property == null) return;

        // Add new amenities
        foreach (var name in amenityNames)
        {
            var category = CategorizeAmenity(name);
            _context.PropertyAmenities.Add(new PropertyAmenity
            {
                Id = Guid.NewGuid(),
                DealerId = property.DealerId,
                PropertyId = propertyId,
                Name = name,
                Category = category
            });
        }

        await _context.SaveChangesAsync();
    }

    private AmenityCategory CategorizeAmenity(string name)
    {
        var lowerName = name.ToLower();

        if (lowerName.Contains("security") || lowerName.Contains("guard") || lowerName.Contains("cctv") ||
            lowerName.Contains("alarm") || lowerName.Contains("seguridad") || lowerName.Contains("vigilancia"))
            return AmenityCategory.Security;

        if (lowerName.Contains("pool") || lowerName.Contains("gym") || lowerName.Contains("tennis") ||
            lowerName.Contains("playground") || lowerName.Contains("alberca") || lowerName.Contains("gimnasio"))
            return AmenityCategory.Recreation;

        if (lowerName.Contains("air") || lowerName.Contains("heating") || lowerName.Contains("elevator") ||
            lowerName.Contains("ac") || lowerName.Contains("clima") || lowerName.Contains("elevador"))
            return AmenityCategory.Comfort;

        if (lowerName.Contains("garden") || lowerName.Contains("terrace") || lowerName.Contains("balcony") ||
            lowerName.Contains("jardín") || lowerName.Contains("terraza") || lowerName.Contains("balcón"))
            return AmenityCategory.Outdoor;

        if (lowerName.Contains("internet") || lowerName.Contains("wifi") || lowerName.Contains("smart") ||
            lowerName.Contains("fiber") || lowerName.Contains("fibra"))
            return AmenityCategory.Technology;

        return AmenityCategory.Services;
    }
}
