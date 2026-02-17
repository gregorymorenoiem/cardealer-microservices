using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Interfaces;

public interface IRecommendationRepository
{
    Task<Recommendation?> GetByIdAsync(Guid id);
    Task<List<Recommendation>> GetByUserIdAsync(Guid userId, int limit = 10);
    Task<List<Recommendation>> GetByUserIdAndTypeAsync(Guid userId, RecommendationType type, int limit = 10);
    Task<List<Recommendation>> GetSimilarVehiclesAsync(Guid vehicleId, int limit = 10);
    Task<Recommendation> CreateAsync(Recommendation recommendation);
    Task<List<Recommendation>> CreateManyAsync(List<Recommendation> recommendations);
    Task<Recommendation> UpdateAsync(Recommendation recommendation);
    Task<bool> DeleteAsync(Guid id);
    Task<int> DeleteOldRecommendationsAsync(DateTime beforeDate);
}
