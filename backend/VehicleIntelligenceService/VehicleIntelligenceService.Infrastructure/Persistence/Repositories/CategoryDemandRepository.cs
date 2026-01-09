using Microsoft.EntityFrameworkCore;
using VehicleIntelligenceService.Domain.Entities;
using VehicleIntelligenceService.Domain.Interfaces;
using VehicleIntelligenceService.Infrastructure.Persistence;

namespace VehicleIntelligenceService.Infrastructure.Persistence.Repositories;

public class CategoryDemandRepository(VehicleIntelligenceDbContext db) : ICategoryDemandRepository
{
    public Task<List<CategoryDemandSnapshot>> GetAllAsync(CancellationToken ct = default)
        => db.CategoryDemandSnapshots.AsNoTracking().ToListAsync(ct);

    public async Task UpsertManyAsync(IEnumerable<CategoryDemandSnapshot> snapshots, CancellationToken ct = default)
    {
        foreach (var snapshot in snapshots)
        {
            var existing = await db.CategoryDemandSnapshots
                .FirstOrDefaultAsync(x => x.Category == snapshot.Category, ct);

            if (existing is null)
            {
                db.CategoryDemandSnapshots.Add(snapshot);
            }
            else
            {
                existing.DemandScore = snapshot.DemandScore;
                existing.Trend = snapshot.Trend;
                existing.UpdatedAt = snapshot.UpdatedAt;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
