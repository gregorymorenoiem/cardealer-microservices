using CarDealer.Shared.MultiTenancy;

namespace PropertiesRentService.Domain.Entities;

/// <summary>
/// Propiedad inmobiliaria para venta
/// </summary>
public class Property : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Multi-tenant (Agencia/Broker)

    // ========================================
    // INFORMACIÓN BÁSICA
    // ========================================
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public PropertyStatus Status { get; set; } = PropertyStatus.Draft;
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;

    // ========================================
    // IDENTIFICACIÓN
    // ========================================
    public string? MLSNumber { get; set; } // MLS Listing Number
    public string? ParcelNumber { get; set; } // Tax Parcel Number
    public string? PropertyId { get; set; } // County Property ID

    // ========================================
    // TIPO DE PROPIEDAD
    // ========================================
    public PropertyType PropertyType { get; set; } = PropertyType.House;
    public PropertySubType PropertySubType { get; set; } = PropertySubType.SingleFamily;
    public OwnershipType OwnershipType { get; set; } = OwnershipType.Fee;

    // ========================================
    // TAMAÑO Y DIMENSIONES
    // ========================================
    public int? SquareFeet { get; set; }
    public int? LotSizeSquareFeet { get; set; }
    public decimal? LotSizeAcres { get; set; }
    public int? Stories { get; set; }
    public int? YearBuilt { get; set; }
    public int? YearRenovated { get; set; }

    // ========================================
    // HABITACIONES
    // ========================================
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? HalfBathrooms { get; set; }
    public int? RoomsTotal { get; set; }

    // ========================================
    // ESTACIONAMIENTO
    // ========================================
    public int? GarageSpaces { get; set; }
    public GarageType GarageType { get; set; } = GarageType.None;
    public int? ParkingSpaces { get; set; }
    public ParkingType ParkingType { get; set; } = ParkingType.None;

    // ========================================
    // CONSTRUCCIÓN Y MATERIALES
    // ========================================
    public string? ConstructionType { get; set; } // Wood Frame, Concrete, Steel
    public string? RoofType { get; set; } // Shingle, Tile, Metal
    public string? ExteriorType { get; set; } // Brick, Vinyl, Stucco
    public string? FoundationType { get; set; } // Slab, Crawl Space, Basement
    public ArchitecturalStyle ArchitecturalStyle { get; set; } = ArchitecturalStyle.Other;

    // ========================================
    // SISTEMAS
    // ========================================
    public HeatingType HeatingType { get; set; } = HeatingType.Forced;
    public CoolingType CoolingType { get; set; } = CoolingType.Central;
    public string? HeatingFuel { get; set; } // Gas, Electric, Oil
    public string? WaterSource { get; set; } // Municipal, Well
    public string? SewerType { get; set; } // Municipal, Septic

    // ========================================
    // UBICACIÓN
    // ========================================
    public string StreetAddress { get; set; } = string.Empty;
    public string? UnitNumber { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? County { get; set; }
    public string Country { get; set; } = "USA";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Neighborhood { get; set; }
    public string? Subdivision { get; set; }

    // ========================================
    // INFORMACIÓN FINANCIERA
    // ========================================
    public decimal? TaxesYearly { get; set; }
    public int? TaxYear { get; set; }
    public decimal? HOAFeesMonthly { get; set; }
    public string? HOAName { get; set; }
    public decimal? AssessedValue { get; set; }
    public decimal? PricePerSquareFoot { get; set; }

    // ========================================
    // CARACTERÍSTICAS Y AMENIDADES
    // ========================================
    /// <summary>
    /// Lista de características interiores
    /// Ejemplo: ["Hardwood Floors", "Granite Counters", "Stainless Appliances"]
    /// </summary>
    public string InteriorFeaturesJson { get; set; } = "[]";

    /// <summary>
    /// Lista de características exteriores
    /// Ejemplo: ["Pool", "Patio", "Sprinkler System", "Fence"]
    /// </summary>
    public string ExteriorFeaturesJson { get; set; } = "[]";

    /// <summary>
    /// Amenidades de la comunidad
    /// Ejemplo: ["Clubhouse", "Tennis Courts", "Gated Entry"]
    /// </summary>
    public string CommunityAmenitiesJson { get; set; } = "[]";

    /// <summary>
    /// Electrodomésticos incluidos
    /// Ejemplo: ["Refrigerator", "Dishwasher", "Washer", "Dryer"]
    /// </summary>
    public string AppliancesJson { get; set; } = "[]";

    // ========================================
    // PISCINA Y EXTERIOR
    // ========================================
    public bool HasPool { get; set; } = false;
    public PoolType PoolType { get; set; } = PoolType.None;
    public bool HasSpa { get; set; } = false;
    public bool HasFireplace { get; set; } = false;
    public int? FireplaceCount { get; set; }
    public bool HasBasement { get; set; } = false;
    public BasementType BasementType { get; set; } = BasementType.None;

    // ========================================
    // ESCUELAS Y CERCANÍAS
    // ========================================
    public string? ElementarySchool { get; set; }
    public string? MiddleSchool { get; set; }
    public string? HighSchool { get; set; }
    public string? SchoolDistrict { get; set; }

    // ========================================
    // INFORMACIÓN DE VENTA
    // ========================================
    public bool IsNewConstruction { get; set; } = false;
    public bool IsForeclosure { get; set; } = false;
    public bool IsShortSale { get; set; } = false;
    public bool VirtualTourAvailable { get; set; } = false;
    public string? VirtualTourUrl { get; set; }
    public DateTime? OpenHouseDate { get; set; }
    public DateTime? ListingDate { get; set; }
    public DateTime? ContractDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    // ========================================
    // HISTORIAL DE PRECIOS
    // ========================================
    public decimal? OriginalPrice { get; set; }
    public int? DaysOnMarket { get; set; }
    public int PriceChanges { get; set; } = 0;

    // ========================================
    // MÉTRICAS DE ENGAGEMENT
    // ========================================
    public int ViewCount { get; set; } = 0;
    public int SavedCount { get; set; } = 0;
    public int InquiryCount { get; set; } = 0;
    public int TourRequestCount { get; set; } = 0;

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
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}

/// <summary>
/// Estados de la propiedad
/// </summary>
public enum PropertyStatus
{
    Draft = 0,
    PendingReview = 1,
    Active = 2,
    UnderContract = 3,
    Pending = 4,
    Sold = 5,
    Closed = 6,
    Withdrawn = 7,
    Expired = 8,
    Archived = 9
}

/// <summary>
/// Tipo de propiedad
/// </summary>
public enum PropertyType
{
    House = 0,
    Condo = 1,
    Townhouse = 2,
    MultiFamily = 3,
    Apartment = 4,
    Land = 5,
    Commercial = 6,
    Industrial = 7,
    Farm = 8,
    MobileHome = 9,
    Other = 99
}

/// <summary>
/// Subtipo de propiedad
/// </summary>
public enum PropertySubType
{
    SingleFamily = 0,
    Duplex = 1,
    Triplex = 2,
    Fourplex = 3,
    Condo = 4,
    Loft = 5,
    Penthouse = 6,
    Studio = 7,
    Attached = 8,
    Detached = 9,
    VacantLand = 10,
    Office = 11,
    Retail = 12,
    Warehouse = 13,
    Other = 99
}

/// <summary>
/// Tipo de propiedad
/// </summary>
public enum OwnershipType
{
    Fee = 0, // Fee Simple
    Leasehold = 1,
    Coop = 2,
    Timeshare = 3,
    Other = 99
}

/// <summary>
/// Tipo de garaje
/// </summary>
public enum GarageType
{
    None = 0,
    Attached = 1,
    Detached = 2,
    Carport = 3,
    Other = 99
}

/// <summary>
/// Tipo de estacionamiento
/// </summary>
public enum ParkingType
{
    None = 0,
    Driveway = 1,
    Street = 2,
    Lot = 3,
    Covered = 4,
    Underground = 5,
    Other = 99
}

/// <summary>
/// Estilo arquitectónico
/// </summary>
public enum ArchitecturalStyle
{
    Colonial = 0,
    Contemporary = 1,
    Craftsman = 2,
    Mediterranean = 3,
    MidCenturyModern = 4,
    Ranch = 5,
    Tudor = 6,
    Victorian = 7,
    Traditional = 8,
    Modern = 9,
    Farmhouse = 10,
    Cape = 11,
    Other = 99
}

/// <summary>
/// Tipo de calefacción
/// </summary>
public enum HeatingType
{
    None = 0,
    Forced = 1,
    Radiant = 2,
    Baseboard = 3,
    HeatPump = 4,
    Geothermal = 5,
    Other = 99
}

/// <summary>
/// Tipo de refrigeración
/// </summary>
public enum CoolingType
{
    None = 0,
    Central = 1,
    Window = 2,
    Split = 3,
    Evaporative = 4,
    Geothermal = 5,
    Other = 99
}

/// <summary>
/// Tipo de piscina
/// </summary>
public enum PoolType
{
    None = 0,
    InGround = 1,
    AboveGround = 2,
    Indoor = 3,
    Community = 4,
    Other = 99
}

/// <summary>
/// Tipo de sótano
/// </summary>
public enum BasementType
{
    None = 0,
    Full = 1,
    Partial = 2,
    Finished = 3,
    Unfinished = 4,
    Crawl = 5,
    WalkOut = 6,
    Other = 99
}


