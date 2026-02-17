using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Interfaces;

public interface IVehicleInteractionRepository
{
    Task<VehicleInteraction?> GetByIdAsync(Guid id);
    Task<List<VehicleInteraction>> GetByUserIdAsync(Guid userId, int limit = 100);
    Task<List<VehicleInteraction>> GetRecentByUserIdAsync(Guid userId, int days = 30, int limit = 50);
    Task<List<VehicleInteraction>> GetByVehicleIdAsync(Guid vehicleId);
    Task<VehicleInteraction> CreateAsync(VehicleInteraction interaction);
    Task<int> GetUserInteractionCountAsync(Guid userId, InteractionType type);
}
