using LeadScoringService.Domain.Entities;

namespace LeadScoringService.Domain.Interfaces;

/// <summary>
/// Repositorio principal para gestión de leads
/// </summary>
public interface ILeadRepository
{
    // CRUD básico
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Lead?> GetByUserAndVehicleAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Lead> CreateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<Lead> UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Queries por dealer
    Task<List<Lead>> GetLeadsByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetHotLeadsByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetLeadsByDealerAndTemperatureAsync(Guid dealerId, LeadTemperature temperature, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetLeadsByDealerAndStatusAsync(Guid dealerId, LeadStatus status, CancellationToken cancellationToken = default);
    
    // Queries por usuario
    Task<List<Lead>> GetLeadsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Queries avanzadas
    Task<List<Lead>> GetLeadsByScoreRangeAsync(int minScore, int maxScore, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetStaleLeadsAsync(int daysSinceLastInteraction, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetConvertedLeadsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // Paginación
    Task<(List<Lead> Leads, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? dealerId = null,
        LeadTemperature? temperature = null,
        LeadStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    
    // Estadísticas
    Task<int> GetTotalCountByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<int> GetHotLeadsCountByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageScoreByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<decimal> GetConversionRateByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
}
