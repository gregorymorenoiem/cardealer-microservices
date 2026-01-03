using Microsoft.EntityFrameworkCore;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Persistence;

namespace VehiclesSaleService.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id)
    {
        return await _context.Vehicles
            .Include(v => v.Images)
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
    }

    public async Task<Vehicle?> GetByVINAsync(string vin)
    {
        return await _context.Vehicles
            .Include(v => v.Images)
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.VIN == vin && !v.IsDeleted);
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.Vehicles
            .Include(v => v.Images.Where(i => i.IsPrimary))
            .Include(v => v.Category)
            .Where(v => !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchParameters p)
    {
        var query = BuildSearchQuery(p);

        // Sorting
        query = p.SortBy?.ToLower() switch
        {
            "price" => p.SortDescending ? query.OrderByDescending(v => v.Price) : query.OrderBy(v => v.Price),
            "year" => p.SortDescending ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
            "mileage" => p.SortDescending ? query.OrderByDescending(v => v.Mileage) : query.OrderBy(v => v.Mileage),
            "make" => p.SortDescending ? query.OrderByDescending(v => v.Make) : query.OrderBy(v => v.Make),
            _ => p.SortDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
        };

        return await query
            .Skip(p.Skip)
            .Take(p.Take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetBySellerAsync(Guid sellerId)
    {
        return await _context.Vehicles
            .Include(v => v.Images.Where(i => i.IsPrimary))
            .Include(v => v.Category)
            .Where(v => v.SellerId == sellerId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetByDealerAsync(Guid dealerId)
    {
        return await _context.Vehicles
            .Include(v => v.Images.Where(i => i.IsPrimary))
            .Include(v => v.Category)
            .Where(v => v.DealerId == dealerId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetFeaturedAsync(int take = 10)
    {
        return await _context.Vehicles
            .Include(v => v.Images.Where(i => i.IsPrimary))
            .Include(v => v.Category)
            .Where(v => !v.IsDeleted && v.IsFeatured && v.Status == VehicleStatus.Active)
            .OrderByDescending(v => v.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle)
    {
        vehicle.Id = Guid.NewGuid();
        vehicle.CreatedAt = DateTime.UtcNow;
        vehicle.UpdatedAt = DateTime.UtcNow;

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return vehicle;
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        vehicle.UpdatedAt = DateTime.UtcNow;
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            vehicle.IsDeleted = true;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Vehicles
            .AnyAsync(v => v.Id == id && !v.IsDeleted);
    }

    public async Task<int> GetCountAsync(VehicleSearchParameters? parameters = null)
    {
        if (parameters == null)
        {
            return await _context.Vehicles.CountAsync(v => !v.IsDeleted);
        }

        return await BuildSearchQuery(parameters).CountAsync();
    }

    private IQueryable<Vehicle> BuildSearchQuery(VehicleSearchParameters p)
    {
        var query = _context.Vehicles
            .Include(v => v.Images.Where(i => i.IsPrimary))
            .Include(v => v.Category)
            .Where(v => !v.IsDeleted && v.Status == VehicleStatus.Active);

        if (!string.IsNullOrWhiteSpace(p.SearchTerm))
        {
            var term = p.SearchTerm.ToLower();
            query = query.Where(v =>
                v.Title.ToLower().Contains(term) ||
                v.Description!.ToLower().Contains(term) ||
                v.Make.ToLower().Contains(term) ||
                v.Model.ToLower().Contains(term));
        }

        if (p.CategoryId.HasValue)
            query = query.Where(v => v.CategoryId == p.CategoryId.Value);

        if (p.MinPrice.HasValue)
            query = query.Where(v => v.Price >= p.MinPrice.Value);

        if (p.MaxPrice.HasValue)
            query = query.Where(v => v.Price <= p.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(p.Make))
            query = query.Where(v => v.Make.ToLower() == p.Make.ToLower());

        if (!string.IsNullOrWhiteSpace(p.Model))
            query = query.Where(v => v.Model.ToLower() == p.Model.ToLower());

        if (p.MinYear.HasValue)
            query = query.Where(v => v.Year >= p.MinYear.Value);

        if (p.MaxYear.HasValue)
            query = query.Where(v => v.Year <= p.MaxYear.Value);

        if (p.MaxMileage.HasValue)
            query = query.Where(v => v.Mileage <= p.MaxMileage.Value);

        if (p.VehicleType.HasValue)
            query = query.Where(v => v.VehicleType == p.VehicleType.Value);

        if (p.BodyStyle.HasValue)
            query = query.Where(v => v.BodyStyle == p.BodyStyle.Value);

        if (p.FuelType.HasValue)
            query = query.Where(v => v.FuelType == p.FuelType.Value);

        if (p.Transmission.HasValue)
            query = query.Where(v => v.Transmission == p.Transmission.Value);

        if (p.DriveType.HasValue)
            query = query.Where(v => v.DriveType == p.DriveType.Value);

        if (p.Condition.HasValue)
            query = query.Where(v => v.Condition == p.Condition.Value);

        if (!string.IsNullOrWhiteSpace(p.ExteriorColor))
            query = query.Where(v => v.ExteriorColor!.ToLower() == p.ExteriorColor.ToLower());

        if (!string.IsNullOrWhiteSpace(p.State))
            query = query.Where(v => v.State!.ToLower() == p.State.ToLower());

        if (!string.IsNullOrWhiteSpace(p.City))
            query = query.Where(v => v.City!.ToLower() == p.City.ToLower());

        if (!string.IsNullOrWhiteSpace(p.ZipCode))
            query = query.Where(v => v.ZipCode == p.ZipCode);

        if (p.IsCertified.HasValue)
            query = query.Where(v => v.IsCertified == p.IsCertified.Value);

        if (p.HasCleanTitle.HasValue)
            query = query.Where(v => v.HasCleanTitle == p.HasCleanTitle.Value);

        return query;
    }
}
