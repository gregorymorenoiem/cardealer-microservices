using CarDealer.Shared.MultiTenancy;

namespace VehiclesRentService.Domain.Entities;

/// <summary>
/// Vehículo para venta en el marketplace
/// </summary>
public class Vehicle : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Multi-tenant

    // ========================================
    // INFORMACIÓN BÁSICA
    // ========================================
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public VehicleStatus Status { get; set; } = VehicleStatus.Draft;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // ========================================
    // IDENTIFICACIÓN DEL VEHÍCULO
    // ========================================
    public string? VIN { get; set; } // Vehicle Identification Number
    public string? StockNumber { get; set; } // Número de inventario del dealer

    // ========================================
    // MARCA / MODELO / AÑO
    // ========================================
    public Guid? MakeId { get; set; }
    public string Make { get; set; } = string.Empty; // Toyota, Honda, Ford
    public Guid? ModelId { get; set; }
    public string Model { get; set; } = string.Empty; // Camry, Civic, F-150
    public string? Trim { get; set; } // LE, SE, XLE, Sport
    public int Year { get; set; }
    public string? Generation { get; set; } // XV70, 11th Gen

    // ========================================
    // TIPO Y CARROCERÍA
    // ========================================
    public VehicleType VehicleType { get; set; } = VehicleType.Car;
    public BodyStyle BodyStyle { get; set; } = BodyStyle.Sedan;
    public int Doors { get; set; } = 4;
    public int Seats { get; set; } = 5;

    // ========================================
    // MOTOR Y TRANSMISIÓN
    // ========================================
    public FuelType FuelType { get; set; } = FuelType.Gasoline;
    public string? EngineSize { get; set; } // 2.5L, 3.0L
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public TransmissionType Transmission { get; set; } = TransmissionType.Automatic;
    public DriveType DriveType { get; set; } = DriveType.FWD;
    public int? Cylinders { get; set; }

    // ========================================
    // KILOMETRAJE Y CONDICIÓN
    // ========================================
    public int Mileage { get; set; }
    public MileageUnit MileageUnit { get; set; } = MileageUnit.Miles;
    public VehicleCondition Condition { get; set; } = VehicleCondition.Used;
    public int? PreviousOwners { get; set; }
    public bool AccidentHistory { get; set; } = false;
    public bool CleanTitle { get; set; } = true;

    // ========================================
    // APARIENCIA
    // ========================================
    public string? ExteriorColor { get; set; }
    public string? InteriorColor { get; set; }
    public string? InteriorMaterial { get; set; } // Leather, Cloth, Vinyl

    // ========================================
    // ECONOMÍA DE COMBUSTIBLE
    // ========================================
    public int? MpgCity { get; set; }
    public int? MpgHighway { get; set; }
    public int? MpgCombined { get; set; }

    // ========================================
    // UBICACIÓN
    // ========================================
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "USA";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ========================================
    // HISTORIAL Y CERTIFICACIONES
    // ========================================
    public bool IsCertified { get; set; } = false;
    public string? CertificationProgram { get; set; } // CPO, Carfax Certified
    public string? CarfaxReportUrl { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public string? ServiceHistoryNotes { get; set; }
    public string? WarrantyInfo { get; set; }

    // ========================================
    // CARACTERÍSTICAS Y EQUIPAMIENTO
    // ========================================
    /// <summary>
    /// Lista de features como JSON array
    /// Ejemplo: ["Sunroof", "Navigation", "Leather Seats", "Backup Camera"]
    /// </summary>
    public string FeaturesJson { get; set; } = "[]";

    /// <summary>
    /// Paquetes de equipamiento
    /// Ejemplo: ["Technology Package", "Premium Package"]
    /// </summary>
    public string PackagesJson { get; set; } = "[]";

    // ========================================
    // MÉTRICAS DE ENGAGEMENT
    // ========================================
    public int ViewCount { get; set; } = 0;
    public int FavoriteCount { get; set; } = 0;
    public int InquiryCount { get; set; } = 0;

    // ========================================
    // METADATOS
    // ========================================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
}

/// <summary>
/// Estados del vehículo
/// </summary>
public enum VehicleStatus
{
    Draft = 0,
    PendingReview = 1,
    Active = 2,
    Reserved = 3,
    Sold = 4,
    Archived = 5,
    Rejected = 6
}

/// <summary>
/// Tipo de vehículo
/// </summary>
public enum VehicleType
{
    Car = 0,
    Truck = 1,
    SUV = 2,
    Van = 3,
    Motorcycle = 4,
    RV = 5,
    Boat = 6,
    ATV = 7,
    Commercial = 8,
    Other = 99
}

/// <summary>
/// Estilo de carrocería
/// </summary>
public enum BodyStyle
{
    Sedan = 0,
    Coupe = 1,
    Hatchback = 2,
    Wagon = 3,
    SUV = 4,
    Crossover = 5,
    Pickup = 6,
    Van = 7,
    Minivan = 8,
    Convertible = 9,
    SportsCar = 10,
    Other = 99
}

/// <summary>
/// Tipo de combustible
/// </summary>
public enum FuelType
{
    Gasoline = 0,
    Diesel = 1,
    Electric = 2,
    Hybrid = 3,
    PlugInHybrid = 4,
    Hydrogen = 5,
    FlexFuel = 6,
    NaturalGas = 7,
    Other = 99
}

/// <summary>
/// Tipo de transmisión
/// </summary>
public enum TransmissionType
{
    Automatic = 0,
    Manual = 1,
    CVT = 2,
    Automated = 3, // Automated Manual
    DualClutch = 4,
    Other = 99
}

/// <summary>
/// Tipo de tracción
/// </summary>
public enum DriveType
{
    FWD = 0, // Front-Wheel Drive
    RWD = 1, // Rear-Wheel Drive
    AWD = 2, // All-Wheel Drive
    FourWD = 3, // 4x4
    Other = 99
}

/// <summary>
/// Unidad de kilometraje
/// </summary>
public enum MileageUnit
{
    Miles = 0,
    Kilometers = 1
}

/// <summary>
/// Condición del vehículo
/// </summary>
public enum VehicleCondition
{
    New = 0,
    CertifiedPreOwned = 1,
    Used = 2,
    Salvage = 3,
    Rebuilt = 4
}

/// <summary>
/// Imágenes del vehículo
/// </summary>
public class VehicleImage : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid VehicleId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public ImageType ImageType { get; set; } = ImageType.Exterior;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle? Vehicle { get; set; }
}

/// <summary>
/// Tipo de imagen
/// </summary>
public enum ImageType
{
    Exterior = 0,
    Interior = 1,
    Engine = 2,
    Damage = 3,
    Documents = 4,
    Other = 99
}

/// <summary>
/// Categoría de vehículos
/// </summary>
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

/// <summary>
/// Marca de vehículos (Make)
/// </summary>
public class VehicleMake
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Toyota, Honda, Ford
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<VehicleModel> Models { get; set; } = new List<VehicleModel>();
}

/// <summary>
/// Modelo de vehículos
/// </summary>
public class VehicleModel
{
    public Guid Id { get; set; }
    public Guid MakeId { get; set; }
    public string Name { get; set; } = string.Empty; // Camry, Civic, F-150
    public string? ImageUrl { get; set; }
    public VehicleType VehicleType { get; set; }
    public BodyStyle DefaultBodyStyle { get; set; }
    public int YearStart { get; set; } // Año de inicio del modelo
    public int? YearEnd { get; set; } // Año de fin (null = aún en producción)
    public bool IsActive { get; set; } = true;

    public VehicleMake? Make { get; set; }
    public ICollection<VehicleTrim> Trims { get; set; } = new List<VehicleTrim>();
}

/// <summary>
/// Versiones/Trims del modelo
/// </summary>
public class VehicleTrim
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public string Name { get; set; } = string.Empty; // LE, SE, XLE
    public int Year { get; set; }
    public decimal? BaseMSRP { get; set; }
    public string? EngineInfo { get; set; }
    public TransmissionType DefaultTransmission { get; set; }
    public DriveType DefaultDriveType { get; set; }
    public FuelType DefaultFuelType { get; set; }
    public int? Horsepower { get; set; }
    public bool IsActive { get; set; } = true;

    public VehicleModel? Model { get; set; }
}
