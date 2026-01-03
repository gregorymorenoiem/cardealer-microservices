# 🏗️ Arquitectura Multi-Vertical: 4 Bases de Datos

## 📊 Resumen Ejecutivo

**Objetivo:** Separar datos por vertical de negocio usando 4 bases de datos PostgreSQL independientes.

**Razón:** Cada vertical tiene necesidades diferentes de:
- Esquema de datos (vehículos ≠ propiedades)
- Performance/scaling (ventas tiene más volumen que rentas)
- Compliance/legal (diferentes regulaciones)
- Backup strategies

---

## 🗄️ Arquitectura de Bases de Datos

### Base de Datos 1: `vehicles_sale_db`
- **Puerto:** 25460
- **Propósito:** Vehículos en venta (autos, motos, camiones, barcos)
- **Volumen estimado:** ~50,000 registros (NHTSA + Kaggle)
- **Entidad principal:** `Vehicle`

**Esquema Vehicle:**
```sql
CREATE TABLE vehicles (
    id UUID PRIMARY KEY,
    dealer_id UUID NOT NULL,
    vin VARCHAR(17) UNIQUE, -- VIN estándar 17 caracteres
    
    -- Básicos
    make VARCHAR(100) NOT NULL, -- Toyota, Ford, etc
    model VARCHAR(100) NOT NULL, -- Camry, F-150, etc
    year INT NOT NULL CHECK (year >= 1900 AND year <= 2050),
    trim VARCHAR(100), -- LX, EX, Sport, etc
    
    -- Precio y estado
    price DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    mileage INT, -- Odómetro en millas o km
    condition VARCHAR(20) CHECK (condition IN ('New', 'Used', 'Certified Pre-Owned')),
    
    -- Técnico
    transmission VARCHAR(20), -- Automatic, Manual, CVT
    fuel_type VARCHAR(20), -- Gasoline, Diesel, Electric, Hybrid
    body_type VARCHAR(30), -- Sedan, SUV, Truck, Coupe, etc
    drivetrain VARCHAR(10), -- FWD, RWD, AWD, 4WD
    engine VARCHAR(100), -- "2.5L 4-Cylinder", "3.5L V6 Turbo"
    horsepower INT,
    mpg_city INT,
    mpg_highway INT,
    
    -- Exterior/Interior
    exterior_color VARCHAR(50),
    interior_color VARCHAR(50),
    seats INT,
    doors INT,
    
    -- Features (JSON)
    features JSONB, -- ["Leather Seats", "Sunroof", "Navigation"]
    safety_features JSONB, -- ["ABS", "Airbags", "Lane Assist"]
    
    -- Metadata
    status VARCHAR(20) DEFAULT 'Active', -- Draft, Active, Sold, Reserved
    location VARCHAR(200), -- "Miami, FL", "Los Angeles, CA"
    seller_id UUID NOT NULL,
    seller_name VARCHAR(200),
    views_count INT DEFAULT 0,
    
    -- Auditoría
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    -- Full-text search
    search_vector tsvector
);

CREATE INDEX idx_vehicles_make_model ON vehicles(make, model);
CREATE INDEX idx_vehicles_year_price ON vehicles(year, price);
CREATE INDEX idx_vehicles_dealer_id ON vehicles(dealer_id);
CREATE INDEX idx_vehicles_vin ON vehicles(vin);
CREATE INDEX idx_vehicles_search ON vehicles USING GIN(search_vector);
```

---

### Base de Datos 2: `vehicles_rent_db`
- **Puerto:** 25461
- **Propósito:** Vehículos para renta (car rental)
- **Volumen estimado:** ~5,000-10,000 registros
- **Entidad principal:** `RentalVehicle`

**Esquema RentalVehicle:**
```sql
CREATE TABLE rental_vehicles (
    id UUID PRIMARY KEY,
    dealer_id UUID NOT NULL,
    
    -- Básicos (similar a vehicles pero campos específicos de renta)
    make VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INT NOT NULL,
    
    -- Pricing de renta
    daily_rate DECIMAL(8,2) NOT NULL,
    weekly_rate DECIMAL(8,2),
    monthly_rate DECIMAL(10,2),
    security_deposit DECIMAL(8,2) DEFAULT 0,
    
    -- Disponibilidad
    is_available BOOLEAN DEFAULT TRUE,
    current_renter_id UUID,
    next_available_date DATE,
    
    -- Restricciones
    minimum_age INT DEFAULT 21,
    minimum_driver_experience_years INT DEFAULT 1,
    
    -- Técnico (subset de Vehicle)
    transmission VARCHAR(20),
    fuel_type VARCHAR(20),
    body_type VARCHAR(30),
    seats INT,
    luggage_capacity INT, -- Número de maletas
    
    -- Features importantes para rentas
    has_gps BOOLEAN DEFAULT FALSE,
    has_child_seat BOOLEAN DEFAULT FALSE,
    has_unlimited_mileage BOOLEAN DEFAULT TRUE,
    mileage_limit_per_day INT, -- Si no es unlimited
    
    -- Metadata
    location VARCHAR(200),
    status VARCHAR(20) DEFAULT 'Available',
    total_rentals_count INT DEFAULT 0,
    average_rating DECIMAL(3,2),
    
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

---

### Base de Datos 3: `properties_sale_db`
- **Puerto:** 25462
- **Propósito:** Propiedades en venta (casas, apartamentos, terrenos)
- **Volumen estimado:** ~10,000-20,000 registros
- **Entidad principal:** `Property`

**Esquema Property:**
```sql
CREATE TABLE properties (
    id UUID PRIMARY KEY,
    dealer_id UUID NOT NULL, -- En este caso sería agente inmobiliario
    
    -- Tipo y básicos
    property_type VARCHAR(30) NOT NULL, -- House, Apartment, Condo, Townhouse, Land
    listing_type VARCHAR(10) DEFAULT 'sale', -- 'sale' siempre en esta DB
    
    -- Precio
    price DECIMAL(15,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    price_per_sqft DECIMAL(8,2),
    
    -- Dimensiones
    bedrooms INT,
    bathrooms DECIMAL(3,1), -- 2.5 (dos completos, uno medio)
    total_area DECIMAL(10,2) NOT NULL, -- sqft o m²
    area_unit VARCHAR(10) DEFAULT 'sqft', -- 'sqft' o 'sqm'
    lot_size DECIMAL(10,2), -- Tamaño del terreno
    
    -- Estructura
    floors INT,
    year_built INT,
    parking_spaces INT,
    garage_type VARCHAR(20), -- Attached, Detached, Carport, None
    
    -- Amenities (JSON array)
    amenities JSONB, -- ["Pool", "Gym", "Garden", "Security"]
    
    -- Servicios
    has_ac BOOLEAN DEFAULT FALSE,
    has_heating BOOLEAN DEFAULT FALSE,
    heating_type VARCHAR(30), -- Gas, Electric, Central
    
    -- Ubicación
    address VARCHAR(300) NOT NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NOT NULL,
    zip_code VARCHAR(20),
    country VARCHAR(50) DEFAULT 'USA',
    neighborhood VARCHAR(100),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    
    -- Metadata
    description TEXT,
    status VARCHAR(20) DEFAULT 'Active', -- Active, Sold, Pending, Contingent
    seller_id UUID NOT NULL,
    seller_name VARCHAR(200),
    agent_name VARCHAR(200),
    mls_number VARCHAR(50), -- Multiple Listing Service ID
    views_count INT DEFAULT 0,
    
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE,
    
    -- Full-text search
    search_vector tsvector
);

CREATE INDEX idx_properties_type ON properties(property_type);
CREATE INDEX idx_properties_location ON properties(city, state);
CREATE INDEX idx_properties_price ON properties(price);
CREATE INDEX idx_properties_bedrooms ON properties(bedrooms);
CREATE INDEX idx_properties_dealer_id ON properties(dealer_id);
CREATE INDEX idx_properties_search ON properties USING GIN(search_vector);
CREATE INDEX idx_properties_geo ON properties USING GIST(ll_to_earth(latitude, longitude));
```

---

### Base de Datos 4: `properties_rent_db`
- **Puerto:** 25463
- **Propósito:** Propiedades para renta (apartamentos, casas de alquiler)
- **Volumen estimado:** ~5,000-15,000 registros
- **Entidad principal:** `RentalProperty`

**Esquema RentalProperty:**
```sql
CREATE TABLE rental_properties (
    id UUID PRIMARY KEY,
    dealer_id UUID NOT NULL,
    
    -- Tipo
    property_type VARCHAR(30) NOT NULL, -- Apartment, House, Condo, Studio
    listing_type VARCHAR(10) DEFAULT 'rent',
    
    -- Precio de renta
    monthly_rent DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    security_deposit DECIMAL(10,2),
    first_last_month_required BOOLEAN DEFAULT FALSE,
    utilities_included BOOLEAN DEFAULT FALSE,
    
    -- Lease terms
    minimum_lease_months INT DEFAULT 12,
    maximum_lease_months INT,
    available_from DATE,
    
    -- Restricciones
    pets_allowed BOOLEAN DEFAULT FALSE,
    smoking_allowed BOOLEAN DEFAULT FALSE,
    
    -- Dimensiones (similar a properties)
    bedrooms INT NOT NULL,
    bathrooms DECIMAL(3,1) NOT NULL,
    total_area DECIMAL(10,2) NOT NULL,
    area_unit VARCHAR(10) DEFAULT 'sqft',
    
    -- Amenities
    amenities JSONB,
    furnished BOOLEAN DEFAULT FALSE,
    
    -- Ubicación
    address VARCHAR(300) NOT NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NOT NULL,
    zip_code VARCHAR(20),
    neighborhood VARCHAR(100),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    
    -- Metadata
    description TEXT,
    status VARCHAR(20) DEFAULT 'Available', -- Available, Rented, Pending
    landlord_id UUID NOT NULL,
    landlord_name VARCHAR(200),
    views_count INT DEFAULT 0,
    
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

---

## 🔧 Implementación Backend (.NET)

### Paso 1: Entidades de Dominio

**ProductService.Domain/Entities/Vehicle.cs:**
```csharp
public class Vehicle : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string? Vin { get; set; }
    
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Trim { get; set; }
    
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int? Mileage { get; set; }
    public VehicleCondition Condition { get; set; }
    
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public string? BodyType { get; set; }
    public string? Drivetrain { get; set; }
    public string? Engine { get; set; }
    public int? Horsepower { get; set; }
    public int? MpgCity { get; set; }
    public int? MpgHighway { get; set; }
    
    public string? ExteriorColor { get; set; }
    public string? InteriorColor { get; set; }
    public int? Seats { get; set; }
    public int? Doors { get; set; }
    
    public string? FeaturesJson { get; set; }
    public string? SafetyFeaturesJson { get; set; }
    
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    public string? Location { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ViewsCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    
    public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
}

public enum VehicleCondition
{
    New,
    Used,
    CertifiedPreOwned
}

public enum VehicleStatus
{
    Draft,
    Active,
    Sold,
    Reserved,
    Archived
}
```

**ProductService.Domain/Entities/Property.cs:**
```csharp
public class Property : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    
    public PropertyType PropertyType { get; set; }
    public string ListingType { get; set; } = "sale"; // 'sale' o 'rent'
    
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? PricePerSqft { get; set; }
    
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal TotalArea { get; set; }
    public string AreaUnit { get; set; } = "sqft";
    public decimal? LotSize { get; set; }
    
    public int? Floors { get; set; }
    public int? YearBuilt { get; set; }
    public int? ParkingSpaces { get; set; }
    public string? GarageType { get; set; }
    
    public string? AmenitiesJson { get; set; }
    
    public bool HasAC { get; set; }
    public bool HasHeating { get; set; }
    public string? HeatingType { get; set; }
    
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "USA";
    public string? Neighborhood { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public string? Description { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Active;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? AgentName { get; set; }
    public string? MlsNumber { get; set; }
    public int ViewsCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}

public enum PropertyType
{
    House,
    Apartment,
    Condo,
    Townhouse,
    Land,
    Commercial
}

public enum PropertyStatus
{
    Active,
    Sold,
    Pending,
    Contingent,
    Rented,
    Available
}
```

---

### Paso 2: DbContexts Separados

**ProductService.Infrastructure/Persistence/VehiclesSaleDbContext.cs:**
```csharp
public class VehiclesSaleDbContext : DbContext
{
    public VehiclesSaleDbContext(DbContextOptions<VehiclesSaleDbContext> options) 
        : base(options) { }
    
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleImage> VehicleImages => Set<VehicleImage>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles");
            entity.HasKey(v => v.Id);
            entity.HasIndex(v => v.Vin).IsUnique();
            entity.HasIndex(v => new { v.Make, v.Model });
            entity.HasIndex(v => new { v.Year, v.Price });
            entity.HasIndex(v => v.DealerId);
        });
    }
}
```

Similar para: `VehiclesRentDbContext`, `PropertiesSaleDbContext`, `PropertiesRentDbContext`

---

### Paso 3: Routing por Vertical

**ProductService.Infrastructure/Services/VerticalRouter.cs:**
```csharp
public interface IVerticalRouter
{
    DbContext GetDbContext(string vertical, string listingType);
}

public class VerticalRouter : IVerticalRouter
{
    private readonly IServiceProvider _serviceProvider;
    
    public VerticalRouter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public DbContext GetDbContext(string vertical, string listingType)
    {
        return (vertical.ToLower(), listingType.ToLower()) switch
        {
            ("vehicles", "sale") => _serviceProvider.GetRequiredService<VehiclesSaleDbContext>(),
            ("vehicles", "rent") => _serviceProvider.GetRequiredService<VehiclesRentDbContext>(),
            ("properties", "sale") => _serviceProvider.GetRequiredService<PropertiesSaleDbContext>(),
            ("properties", "rent") => _serviceProvider.GetRequiredService<PropertiesRentDbContext>(),
            _ => throw new ArgumentException($"Invalid vertical '{vertical}' or listingType '{listingType}'")
        };
    }
}
```

---

## 📦 Importación de Datos

### NHTSA API (Vehículos)

**Script:** `scripts/import-nhtsa-data.ps1`

```powershell
# Descargar makes
$makes = Invoke-RestMethod "https://vpic.nhtsa.dot.gov/api/vehicles/GetAllMakes?format=json"

foreach ($make in $makes.Results | Select-Object -First 50) {
    # Descargar modelos para cada make
    $models = Invoke-RestMethod "https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMake/$($make.Make_Name)?format=json"
    
    # Guardar en JSON
    $models.Results | ConvertTo-Json | Out-File "data/nhtsa-$($make.Make_Name).json"
}
```

### Kaggle Dataset (Vehículos con Precios)

1. Descargar: https://www.kaggle.com/datasets/nehalbirla/vehicle-dataset-from-cardekho
2. CSV → C# importer
3. Bulk insert a `vehicles_sale_db`

---

## 🚀 Próximos Pasos Concretos

1. ✅ **Diseño completado** (este documento)
2. ⏭️ **Modificar compose.yaml** (agregar 4 DBs PostgreSQL)
3. ⏭️ **Crear entidades C#** (Vehicle.cs, Property.cs)
4. ⏭️ **Crear DbContexts** (4 clases)
5. ⏭️ **Configurar DI** en Program.cs
6. ⏭️ **Generar migraciones** EF Core
7. ⏭️ **Scripts NHTSA/Kaggle** para importar datos
8. ⏭️ **Actualizar controllers** para routing

---

**Tiempo estimado total:** 6-8 horas  
**Complejidad:** Media-Alta (refactoring mayor)  
**Riesgo:** Bajo (no afecta servicios existentes, es extensión)
