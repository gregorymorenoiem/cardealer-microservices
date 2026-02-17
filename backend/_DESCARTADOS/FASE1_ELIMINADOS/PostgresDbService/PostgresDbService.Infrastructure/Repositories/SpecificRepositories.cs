using Microsoft.EntityFrameworkCore;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using PostgresDbService.Infrastructure.Persistence;
using System.Text.Json;

namespace PostgresDbService.Infrastructure.Repositories;

/// <summary>
/// User-specific repository with type-safe operations
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IGenericRepository _genericRepository;
    private const string SERVICE_NAME = "UserService";
    private const string ENTITY_TYPE = "User";

    public UserRepository(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public async Task<GenericEntity?> GetUserByEmailAsync(string email)
    {
        var users = await _genericRepository.QueryAsync(SERVICE_NAME, ENTITY_TYPE, "$.Email", email);
        return users.FirstOrDefault();
    }

    public async Task<GenericEntity?> GetUserByIdAsync(Guid userId)
    {
        return await _genericRepository.GetByIdAsync(SERVICE_NAME, ENTITY_TYPE, userId.ToString());
    }

    public async Task<List<GenericEntity>> GetUsersByRoleAsync(string role)
    {
        return await _genericRepository.QueryAsync(SERVICE_NAME, ENTITY_TYPE, "$.Role", role);
    }

    public async Task<GenericEntity> CreateUserAsync(object userData, string createdBy)
    {
        var entity = new GenericEntity
        {
            ServiceName = SERVICE_NAME,
            EntityType = ENTITY_TYPE,
            EntityId = Guid.NewGuid().ToString(),
            DataJson = JsonSerializer.Serialize(userData),
            CreatedBy = createdBy,
            IndexData = CreateUserIndexData(userData)
        };

        return await _genericRepository.CreateAsync(entity);
    }

    public async Task<GenericEntity> UpdateUserAsync(Guid userId, object userData, string updatedBy)
    {
        var entity = await GetUserByIdAsync(userId);
        if (entity == null)
            throw new InvalidOperationException($"User not found: {userId}");

        entity.DataJson = JsonSerializer.Serialize(userData);
        entity.IndexData = CreateUserIndexData(userData);
        entity.UpdatedBy = updatedBy;

        return await _genericRepository.UpdateAsync(entity);
    }

    private string CreateUserIndexData(object userData)
    {
        // Create searchable index data for users
        var userJson = JsonSerializer.Serialize(userData);
        using var doc = JsonDocument.Parse(userJson);
        var root = doc.RootElement;

        var indexData = new
        {
            Email = root.GetProperty("Email").GetString()?.ToLowerInvariant(),
            FullName = root.GetProperty("FullName").GetString()?.ToLowerInvariant(),
            Role = root.GetProperty("Role").GetString(),
            IsActive = root.TryGetProperty("IsActive", out var isActive) ? isActive.GetBoolean() : true,
            City = root.TryGetProperty("City", out var city) ? city.GetString()?.ToLowerInvariant() : null,
            Province = root.TryGetProperty("Province", out var province) ? province.GetString()?.ToLowerInvariant() : null
        };

        return JsonSerializer.Serialize(indexData);
    }
}

/// <summary>
/// Vehicle-specific repository with type-safe operations
/// </summary>
public class VehicleRepository : IVehicleRepository
{
    private readonly IGenericRepository _genericRepository;
    private const string SERVICE_NAME = "VehiclesSaleService";
    private const string ENTITY_TYPE = "Vehicle";

    public VehicleRepository(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public async Task<GenericEntity?> GetVehicleByIdAsync(Guid vehicleId)
    {
        return await _genericRepository.GetByIdAsync(SERVICE_NAME, ENTITY_TYPE, vehicleId.ToString());
    }

    public async Task<List<GenericEntity>> GetVehiclesBySellerAsync(Guid sellerId)
    {
        return await _genericRepository.QueryAsync(SERVICE_NAME, ENTITY_TYPE, "$.SellerId", sellerId.ToString());
    }

    public async Task<List<GenericEntity>> SearchVehiclesAsync(string make, string model, int? yearFrom, int? yearTo, decimal? priceFrom, decimal? priceTo)
    {
        // Complex search using JSONB query
        var conditions = new List<string>();
        
        if (!string.IsNullOrEmpty(make))
            conditions.Add($"$.Make == \"{make}\"");
        if (!string.IsNullOrEmpty(model))
            conditions.Add($"$.Model == \"{model}\"");
        if (yearFrom.HasValue)
            conditions.Add($"$.Year >= {yearFrom.Value}");
        if (yearTo.HasValue)
            conditions.Add($"$.Year <= {yearTo.Value}");
        if (priceFrom.HasValue)
            conditions.Add($"$.Price >= {priceFrom.Value}");
        if (priceTo.HasValue)
            conditions.Add($"$.Price <= {priceTo.Value}");

        if (!conditions.Any())
            return await _genericRepository.GetByServiceAsync(SERVICE_NAME, ENTITY_TYPE);

        var jsonQuery = string.Join(" && ", conditions);
        return await _genericRepository.QueryByJsonAsync(SERVICE_NAME, ENTITY_TYPE, jsonQuery);
    }

    public async Task<GenericEntity> CreateVehicleAsync(object vehicleData, string createdBy)
    {
        var entity = new GenericEntity
        {
            ServiceName = SERVICE_NAME,
            EntityType = ENTITY_TYPE,
            EntityId = Guid.NewGuid().ToString(),
            DataJson = JsonSerializer.Serialize(vehicleData),
            CreatedBy = createdBy,
            IndexData = CreateVehicleIndexData(vehicleData)
        };

        return await _genericRepository.CreateAsync(entity);
    }

    public async Task<GenericEntity> UpdateVehicleAsync(Guid vehicleId, object vehicleData, string updatedBy)
    {
        var entity = await GetVehicleByIdAsync(vehicleId);
        if (entity == null)
            throw new InvalidOperationException($"Vehicle not found: {vehicleId}");

        entity.DataJson = JsonSerializer.Serialize(vehicleData);
        entity.IndexData = CreateVehicleIndexData(vehicleData);
        entity.UpdatedBy = updatedBy;

        return await _genericRepository.UpdateAsync(entity);
    }

    private string CreateVehicleIndexData(object vehicleData)
    {
        var vehicleJson = JsonSerializer.Serialize(vehicleData);
        using var doc = JsonDocument.Parse(vehicleJson);
        var root = doc.RootElement;

        var indexData = new
        {
            Make = root.GetProperty("Make").GetString()?.ToLowerInvariant(),
            Model = root.GetProperty("Model").GetString()?.ToLowerInvariant(),
            Year = root.TryGetProperty("Year", out var year) ? year.GetInt32() : 0,
            Price = root.TryGetProperty("Price", out var price) ? price.GetDecimal() : 0m,
            Status = root.TryGetProperty("Status", out var status) ? status.GetString() : "Active",
            SellerId = root.TryGetProperty("SellerId", out var sellerId) ? sellerId.GetString() : null,
            City = root.TryGetProperty("City", out var city) ? city.GetString()?.ToLowerInvariant() : null,
            FuelType = root.TryGetProperty("FuelType", out var fuel) ? fuel.GetString()?.ToLowerInvariant() : null
        };

        return JsonSerializer.Serialize(indexData);
    }
}

/// <summary>
/// Contact-specific repository with type-safe operations
/// </summary>
public class ContactRepository : IContactRepository
{
    private readonly IGenericRepository _genericRepository;
    private const string SERVICE_NAME = "ContactService";
    private const string REQUEST_TYPE = "ContactRequest";
    private const string MESSAGE_TYPE = "ContactMessage";

    public ContactRepository(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public async Task<GenericEntity?> GetContactRequestByIdAsync(Guid contactRequestId)
    {
        return await _genericRepository.GetByIdAsync(SERVICE_NAME, REQUEST_TYPE, contactRequestId.ToString());
    }

    public async Task<List<GenericEntity>> GetContactRequestsByBuyerAsync(Guid buyerId)
    {
        return await _genericRepository.QueryAsync(SERVICE_NAME, REQUEST_TYPE, "$.BuyerId", buyerId.ToString());
    }

    public async Task<List<GenericEntity>> GetContactRequestsBySellerAsync(Guid sellerId)
    {
        return await _genericRepository.QueryAsync(SERVICE_NAME, REQUEST_TYPE, "$.SellerId", sellerId.ToString());
    }

    public async Task<List<GenericEntity>> GetContactMessagesByRequestAsync(Guid contactRequestId)
    {
        return await _genericRepository.QueryAsync(SERVICE_NAME, MESSAGE_TYPE, "$.ContactRequestId", contactRequestId.ToString());
    }

    public async Task<GenericEntity> CreateContactRequestAsync(object contactData, string createdBy)
    {
        var entity = new GenericEntity
        {
            ServiceName = SERVICE_NAME,
            EntityType = REQUEST_TYPE,
            EntityId = Guid.NewGuid().ToString(),
            DataJson = JsonSerializer.Serialize(contactData),
            CreatedBy = createdBy,
            IndexData = CreateContactIndexData(contactData)
        };

        return await _genericRepository.CreateAsync(entity);
    }

    public async Task<GenericEntity> CreateContactMessageAsync(object messageData, string createdBy)
    {
        var entity = new GenericEntity
        {
            ServiceName = SERVICE_NAME,
            EntityType = MESSAGE_TYPE,
            EntityId = Guid.NewGuid().ToString(),
            DataJson = JsonSerializer.Serialize(messageData),
            CreatedBy = createdBy
        };

        return await _genericRepository.CreateAsync(entity);
    }

    private string CreateContactIndexData(object contactData)
    {
        var contactJson = JsonSerializer.Serialize(contactData);
        using var doc = JsonDocument.Parse(contactJson);
        var root = doc.RootElement;

        var indexData = new
        {
            BuyerId = root.TryGetProperty("BuyerId", out var buyer) ? buyer.GetString() : null,
            SellerId = root.TryGetProperty("SellerId", out var seller) ? seller.GetString() : null,
            VehicleId = root.TryGetProperty("VehicleId", out var vehicle) ? vehicle.GetString() : null,
            Status = root.TryGetProperty("Status", out var status) ? status.GetString() : "Open",
            IsUrgent = root.TryGetProperty("IsUrgent", out var urgent) ? urgent.GetBoolean() : false,
            BuyerEmail = root.TryGetProperty("BuyerEmail", out var email) ? email.GetString()?.ToLowerInvariant() : null
        };

        return JsonSerializer.Serialize(indexData);
    }
}