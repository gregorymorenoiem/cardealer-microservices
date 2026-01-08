using CarDealer.Shared.MultiTenancy;

namespace ComparisonService.Domain.Entities;

public class VehicleComparison : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> VehicleIds { get; set; } = new();
    public string? ShareToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    protected VehicleComparison() { } // For EF Core

    public VehicleComparison(Guid userId, string name)
    {
        UserId = userId;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddVehicle(Guid vehicleId)
    {
        if (VehicleIds.Count >= 3)
            throw new InvalidOperationException("Cannot add more than 3 vehicles to a comparison");

        if (VehicleIds.Contains(vehicleId))
            throw new InvalidOperationException("Vehicle already exists in this comparison");

        VehicleIds.Add(vehicleId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveVehicle(Guid vehicleId)
    {
        VehicleIds.Remove(vehicleId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void GenerateShareToken()
    {
        ShareToken = Guid.NewGuid().ToString("N")[..16]; // 16 character token
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}