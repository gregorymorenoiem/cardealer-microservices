using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Domain.Interfaces;

public interface IPriceSuggestionRepository
{
    Task AddAsync(PriceSuggestionRecord record, CancellationToken ct = default);
}
