namespace ComparisonService.Domain.Entities;

/// <summary>
/// Comparación de vehículos guardada por un usuario
/// </summary>
public class Comparison
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public List<Guid> VehicleIds { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsPublic { get; private set; }
    public string? ShareToken { get; private set; }

    private Comparison() { } // EF Core

    public Comparison(Guid userId, string name, List<Guid> vehicleIds, bool isPublic = false)
    {
        if (vehicleIds == null || vehicleIds.Count == 0)
            throw new ArgumentException("At least one vehicle is required");
        
        if (vehicleIds.Count > 3)
            throw new ArgumentException("Maximum 3 vehicles can be compared");

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name ?? $"Comparison {DateTime.UtcNow:yyyy-MM-dd}";
        VehicleIds = vehicleIds;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsPublic = isPublic;
        
        if (isPublic)
        {
            ShareToken = GenerateShareToken();
        }
    }

    public void UpdateVehicles(List<Guid> vehicleIds)
    {
        if (vehicleIds == null || vehicleIds.Count == 0)
            throw new ArgumentException("At least one vehicle is required");
        
        if (vehicleIds.Count > 3)
            throw new ArgumentException("Maximum 3 vehicles can be compared");

        VehicleIds = vehicleIds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakePublic()
    {
        IsPublic = true;
        ShareToken ??= GenerateShareToken();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakePrivate()
    {
        IsPublic = false;
        ShareToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateShareToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 16);
    }
}
