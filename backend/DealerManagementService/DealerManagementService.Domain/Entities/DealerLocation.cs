namespace DealerManagementService.Domain.Entities;

public class DealerLocation
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty; // e.g., "Casa Matriz", "Sucursal Santiago"
    public LocationType Type { get; set; } = LocationType.Branch;
    public bool IsPrimary { get; set; }
    
    // Address
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "República Dominicana";
    
    // Coordinates (for map)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Contact
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    
    // Hours
    public string? WorkingHours { get; set; } // JSON: {"monday": "8:00-18:00", ...}
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public enum LocationType
{
    Headquarters = 0,    // Casa matriz
    Branch = 1,          // Sucursal
    Showroom = 2,        // Sala de exhibición
    ServiceCenter = 3,   // Centro de servicio
    Warehouse = 4        // Bodega/Almacén
}
