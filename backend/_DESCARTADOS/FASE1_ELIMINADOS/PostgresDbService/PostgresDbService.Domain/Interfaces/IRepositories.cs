using PostgresDbService.Domain.Entities;

namespace PostgresDbService.Domain.Interfaces;

/// <summary>
/// Generic repository for all microservice data
/// Provides CRUD operations for any entity type
/// </summary>
public interface IGenericRepository
{
    // Basic CRUD operations
    Task<GenericEntity?> GetByIdAsync(string serviceName, string entityType, string entityId);
    Task<List<GenericEntity>> GetByServiceAsync(string serviceName, string entityType, int page = 1, int pageSize = 50);
    Task<GenericEntity> CreateAsync(GenericEntity entity);
    Task<GenericEntity> UpdateAsync(GenericEntity entity);
    Task<bool> DeleteAsync(string serviceName, string entityType, string entityId, string deletedBy = "System");
    Task<bool> ExistsAsync(string serviceName, string entityType, string entityId);

    // Query operations
    Task<List<GenericEntity>> QueryAsync(string serviceName, string entityType, string jsonPath, object value);
    Task<List<GenericEntity>> QueryByJsonAsync(string serviceName, string entityType, string jsonQuery);
    Task<int> CountAsync(string serviceName, string entityType);
    Task<List<GenericEntity>> GetAllByIdsAsync(string serviceName, string entityType, List<string> entityIds);

    // Bulk operations
    Task<List<GenericEntity>> BulkCreateAsync(List<GenericEntity> entities);
    Task<List<GenericEntity>> BulkUpdateAsync(List<GenericEntity> entities);
    Task<bool> BulkDeleteAsync(string serviceName, string entityType, List<string> entityIds, string deletedBy = "System");

    // Advanced queries
    Task<List<GenericEntity>> SearchAsync(string serviceName, string entityType, string searchTerm, List<string> searchFields);
    Task<List<GenericEntity>> GetByDateRangeAsync(string serviceName, string entityType, DateTime fromDate, DateTime toDate);
    Task<List<GenericEntity>> GetRecentAsync(string serviceName, string entityType, int count = 10);
}

/// <summary>
/// Service-specific repositories for type safety and business logic
/// </summary>
public interface IUserRepository
{
    Task<GenericEntity?> GetUserByEmailAsync(string email);
    Task<GenericEntity?> GetUserByIdAsync(Guid userId);
    Task<List<GenericEntity>> GetUsersByRoleAsync(string role);
    Task<GenericEntity> CreateUserAsync(object userData, string createdBy);
    Task<GenericEntity> UpdateUserAsync(Guid userId, object userData, string updatedBy);
}

public interface IVehicleRepository  
{
    Task<GenericEntity?> GetVehicleByIdAsync(Guid vehicleId);
    Task<List<GenericEntity>> GetVehiclesBySellerAsync(Guid sellerId);
    Task<List<GenericEntity>> SearchVehiclesAsync(string make, string model, int? yearFrom, int? yearTo, decimal? priceFrom, decimal? priceTo);
    Task<GenericEntity> CreateVehicleAsync(object vehicleData, string createdBy);
    Task<GenericEntity> UpdateVehicleAsync(Guid vehicleId, object vehicleData, string updatedBy);
}

public interface IContactRepository
{
    Task<GenericEntity?> GetContactRequestByIdAsync(Guid contactRequestId);
    Task<List<GenericEntity>> GetContactRequestsByBuyerAsync(Guid buyerId);
    Task<List<GenericEntity>> GetContactRequestsBySellerAsync(Guid sellerId);
    Task<List<GenericEntity>> GetContactMessagesByRequestAsync(Guid contactRequestId);
    Task<GenericEntity> CreateContactRequestAsync(object contactData, string createdBy);
    Task<GenericEntity> CreateContactMessageAsync(object messageData, string createdBy);
}