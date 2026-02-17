using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Interfaces;

namespace RecommendationService.Infrastructure.Persistence.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly RecommendationDbContext _context;

    public RecommendationRepository(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<Recommendation?> GetByIdAsync(Guid id)
    {
        return await _context.Recommendations.FindAsync(id);
    }

    public async Task<List<Recommendation>> GetByUserIdAsync(Guid userId, int limit = 10)
    {
        return await _context.Recommendations
            .Where(r => r.UserId == userId && r.IsRelevant)
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Recommendation>> GetByUserIdAndTypeAsync(Guid userId, RecommendationType type, int limit = 10)
    {
        return await _context.Recommendations
            .Where(r => r.UserId == userId && r.Type == type && r.IsRelevant)
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Recommendation>> GetSimilarVehiclesAsync(Guid vehicleId, int limit = 10)
    {
        return await _context.Recommendations
            .Where(r => r.VehicleId == vehicleId && r.Type == RecommendationType.Similar && r.IsRelevant)
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Recommendation> CreateAsync(Recommendation recommendation)
    {
        _context.Recommendations.Add(recommendation);
        await _context.SaveChangesAsync();
        return recommendation;
    }

    public async Task<List<Recommendation>> CreateManyAsync(List<Recommendation> recommendations)
    {
        _context.Recommendations.AddRange(recommendations);
        await _context.SaveChangesAsync();
        return recommendations;
    }

    public async Task<Recommendation> UpdateAsync(Recommendation recommendation)
    {
        _context.Recommendations.Update(recommendation);
        await _context.SaveChangesAsync();
        return recommendation;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var recommendation = await GetByIdAsync(id);
        if (recommendation == null)
            return false;

        _context.Recommendations.Remove(recommendation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteOldRecommendationsAsync(DateTime beforeDate)
    {
        var oldRecommendations = await _context.Recommendations
            .Where(r => r.CreatedAt < beforeDate)
            .ToListAsync();

        _context.Recommendations.RemoveRange(oldRecommendations);
        await _context.SaveChangesAsync();
        return oldRecommendations.Count;
    }
}
