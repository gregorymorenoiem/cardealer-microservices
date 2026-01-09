using VehicleIntelligenceService.Domain.Entities;
using VehicleIntelligenceService.Domain.Interfaces;
using VehicleIntelligenceService.Infrastructure.Persistence;

namespace VehicleIntelligenceService.Infrastructure.Persistence.Repositories;

public class PriceSuggestionRepository(VehicleIntelligenceDbContext db) : IPriceSuggestionRepository
{
    public async Task AddAsync(PriceSuggestionRecord record, CancellationToken ct = default)
    {
        db.PriceSuggestions.Add(record);
        await db.SaveChangesAsync(ct);
    }
}
