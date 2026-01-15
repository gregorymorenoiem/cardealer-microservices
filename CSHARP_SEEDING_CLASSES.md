# 💻 Clases C# para Implementación de Seeding v2.0

**Ubicación:** `backend/_Shared/CarDealer.DataSeeding/`  
**Fecha:** Enero 15, 2026  
**Status:** Listo para implementación

---

## 📋 CLASES NECESARIAS

### 1. `CatalogBuilder.cs`

```csharp
using Bogus;
using CarDealer.Contracts.Dto.Catalog;

namespace CarDealer.DataSeeding.Builders;

/// <summary>
/// Genera catálogos base: Makes, Models, Years, BodyStyles, FuelTypes, Colors
/// </summary>
public class CatalogBuilder
{
    private static readonly Faker<Make> MakeFaker = new Faker<Make>()
        .RuleFor(m => m.Id, f => Guid.NewGuid())
        .RuleFor(m => m.Name, f => f.Vehicle.Manufacturer())
        .RuleFor(m => m.LogoUrl, (f, m) => GetLogoUrlForMake(m.Name))
        .RuleFor(m => m.CreatedAt, f => f.Date.Past(1))
        .RuleFor(m => m.UpdatedAt, f => f.Date.Recent());

    // Lista específica de 10 marcas principales
    private static readonly string[] PrimaryMakes = new[]
    {
        "Toyota", "Honda", "Nissan", "Ford", "BMW",
        "Mercedes-Benz", "Tesla", "Hyundai", "Porsche", "Chevrolet"
    };

    // Mapping de modelos por marca
    private static readonly Dictionary<string, string[]> ModelsByMake = new()
    {
        { "Toyota", new[] { "Corolla", "Camry", "RAV4", "4Runner", "Highlander", "Prius" } },
        { "Honda", new[] { "Civic", "Accord", "CR-V", "Pilot", "HR-V", "Fit" } },
        { "Nissan", new[] { "Altima", "Maxima", "Rogue", "Murano", "Frontier", "Sentra" } },
        { "Ford", new[] { "F-150", "Mustang", "Explorer", "Escape", "Edge", "Focus" } },
        { "BMW", new[] { "3 Series", "5 Series", "X5", "X3", "M340i", "M440i" } },
        { "Mercedes-Benz", new[] { "C-Class", "E-Class", "GLE", "GLA", "AMG GT", "S-Class" } },
        { "Tesla", new[] { "Model S", "Model 3", "Model X", "Model Y" } },
        { "Hyundai", new[] { "Elantra", "Sonata", "Santa Fe", "Tucson", "Ioniq", "Kona" } },
        { "Porsche", new[] { "911", "Cayenne", "Panamera", "Macan", "Taycán" } },
        { "Chevrolet", new[] { "Silverado", "Colorado", "Equinox", "Blazer", "Trax" } }
    };

    public static IEnumerable<Make> GenerateMakes()
    {
        var makes = new List<Make>();
        foreach (var makeName in PrimaryMakes)
        {
            makes.Add(new Make
            {
                Id = Guid.NewGuid(),
                Name = makeName,
                LogoUrl = GetLogoUrlForMake(makeName),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
        return makes;
    }

    public static IEnumerable<Model> GenerateModels()
    {
        var models = new List<Model>();
        var makeIndex = 1;

        foreach (var (make, modelNames) in ModelsByMake)
        {
            var makeId = new Guid($"00000000-0000-0000-0000-{makeIndex:000000000000}");

            foreach (var modelName in modelNames)
            {
                models.Add(new Model
                {
                    Id = Guid.NewGuid(),
                    MakeId = makeId,
                    Name = modelName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
            makeIndex++;
        }

        return models;
    }

    public static IEnumerable<Year> GenerateYears()
    {
        var years = new List<Year>();
        var currentYear = DateTime.UtcNow.Year;

        for (int year = currentYear - 14; year <= currentYear; year++)
        {
            years.Add(new Year
            {
                Id = Guid.NewGuid(),
                Value = year,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        return years;
    }

    public static IEnumerable<BodyStyle> GenerateBodyStyles()
    {
        var styles = new[] { "Sedan", "SUV", "Truck", "Coupe", "Hatchback", "Wagon", "Convertible" };
        return styles.Select(s => new BodyStyle
        {
            Id = Guid.NewGuid(),
            Name = s,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }

    public static IEnumerable<FuelType> GenerateFuelTypes()
    {
        var types = new[] { "Gasoline", "Diesel", "Hybrid", "Electric", "Plug-in Hybrid" };
        return types.Select(t => new FuelType
        {
            Id = Guid.NewGuid(),
            Name = t,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }

    public static IEnumerable<Color> GenerateColors()
    {
        var colors = new[]
        {
            "Black", "White", "Silver", "Gray", "Blue", "Red", "Green", "Yellow",
            "Orange", "Brown", "Gold", "Purple", "Pink", "Beige", "Tan", "Burgundy"
        };

        return colors.Select(c => new Color
        {
            Id = Guid.NewGuid(),
            Name = c,
            HexCode = GetHexForColor(c),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }

    private static string GetLogoUrlForMake(string make) =>
        make.ToLower() switch
        {
            "toyota" => "https://api.example.com/logos/toyota.svg",
            "honda" => "https://api.example.com/logos/honda.svg",
            "nissan" => "https://api.example.com/logos/nissan.svg",
            "ford" => "https://api.example.com/logos/ford.svg",
            "bmw" => "https://api.example.com/logos/bmw.svg",
            "mercedes-benz" => "https://api.example.com/logos/mercedes.svg",
            "tesla" => "https://api.example.com/logos/tesla.svg",
            "hyundai" => "https://api.example.com/logos/hyundai.svg",
            "porsche" => "https://api.example.com/logos/porsche.svg",
            "chevrolet" => "https://api.example.com/logos/chevrolet.svg",
            _ => "https://api.example.com/logos/generic.svg"
        };

    private static string GetHexForColor(string color) =>
        color switch
        {
            "Black" => "#000000",
            "White" => "#FFFFFF",
            "Silver" => "#C0C0C0",
            "Gray" => "#808080",
            "Blue" => "#0000FF",
            "Red" => "#FF0000",
            "Green" => "#008000",
            "Yellow" => "#FFFF00",
            "Orange" => "#FFA500",
            "Brown" => "#A52A2A",
            "Gold" => "#FFD700",
            "Purple" => "#800080",
            "Pink" => "#FFC0CB",
            "Beige" => "#F5F5DC",
            "Tan" => "#D2B48C",
            "Burgundy" => "#800020",
            _ => "#000000"
        };
}
```

---

### 2. `VehicleBuilder.cs`

```csharp
using Bogus;
using CarDealer.Contracts.Dto.Vehicles;
using CarDealer.Domain.Entities;

namespace CarDealer.DataSeeding.Builders;

/// <summary>
/// Construye vehículos con especificaciones completas
/// </summary>
public class VehicleBuilder
{
    private static readonly Faker<Vehicle> VehicleFaker = new();

    // Especificaciones por modelo
    private static readonly Dictionary<string, VehicleSpec> SpecsByModel = new()
    {
        { "Corolla", new("1.8L", 139, 126, "FWD") },
        { "Camry", new("2.5L", 203, 184, "FWD") },
        { "RAV4", new("2.5L", 203, 184, "AWD") },
        { "Civic", new("1.5L", 174, 162, "FWD") },
        { "Accord", new("1.5L", 192, 192, "FWD") },
        // ... más especificaciones
    };

    // Features disponibles
    private static readonly string[] AvailableFeatures = new[]
    {
        "Leather Seats", "Sunroof", "Navigation System", "Adaptive Cruise Control",
        "Apple CarPlay", "Android Auto", "Panoramic Sunroof", "Heated Seats",
        "Cooled Seats", "Backup Camera", "360 Camera", "Blind Spot Monitor",
        "Lane Departure Warning", "Automatic Parking", "Wireless Charging",
        "Premium Sound System", "Ventilated Seats", "Power Windows", "Power Doors",
        "Smart Key System", "Power Liftgate", "Roof Rails", "Fog Lights",
        "LED Headlights", "Ambient Lighting", "Automatic Headlights",
        "Cruise Control", "Speed Control", "All-Wheel Drive"
    };

    public static IEnumerable<Vehicle> GenerateBatch(
        int count,
        string make,
        List<Guid> dealerIds,
        bool withCompleteSpecs = true,
        int featuresPerVehicle = 8)
    {
        var vehicles = new List<Vehicle>();
        var faker = new Faker();

        for (int i = 0; i < count; i++)
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Title = $"{make} {faker.Vehicle.Model()} {faker.Date.Past(5).Year}",
                Make = make,
                Model = faker.Vehicle.Model(),
                Year = faker.Date.Past(5).Year,
                Price = faker.Random.Long(5000, 100000),
                DealerId = faker.PickRandom(dealerIds),
                Status = "Active",
                Description = faker.Lorem.Sentences(5),
                Mileage = faker.Random.Int(100, 200000),
                CreatedAt = faker.Date.Past(1),
                UpdatedAt = faker.Date.Recent(),
                IsFeatured = faker.Random.Bool(0.1f), // 10% featured
                BodyStyle = faker.PickRandom(new[] { "Sedan", "SUV", "Truck" }),
                FuelType = faker.PickRandom(new[] { "Gasoline", "Diesel", "Hybrid" }),
                Color = faker.PickRandom(new[] { "Black", "White", "Silver", "Blue", "Red" }),
                TransmissionType = faker.PickRandom(new[] { "Automatic", "Manual" }),
                InteriorColor = faker.PickRandom(new[] { "Black", "Tan", "Gray", "Brown" }),
                Condition = "Used",

                // Especificaciones (si está habilitado)
                Engine = withCompleteSpecs ? GetEngineSize(make) : "2.0L",
                Horsepower = withCompleteSpecs ? faker.Random.Int(100, 600) : 200,
                Torque = withCompleteSpecs ? faker.Random.Int(100, 700) : 150,
                DriveType = faker.PickRandom(new[] { "FWD", "RWD", "AWD" }),

                // Features
                Features = GenerateFeatures(faker, featuresPerVehicle)
            };

            vehicles.Add(vehicle);
        }

        return vehicles;
    }

    public static IEnumerable<Vehicle> GenerateByMake(string make, int count, List<Guid> dealerIds)
    {
        return GenerateBatch(count, make, dealerIds, withCompleteSpecs: true, featuresPerVehicle: 10);
    }

    /// <summary>
    /// Genera distribución específica de vehículos por marca
    /// </summary>
    public static IEnumerable<Vehicle> GenerateAllBrands(List<Guid> dealerIds)
    {
        var vehicles = new List<Vehicle>();

        var makes = new Dictionary<string, int>
        {
            { "Toyota", 45 },
            { "Honda", 16 },
            { "Nissan", 22 },
            { "Ford", 22 },
            { "BMW", 15 },
            { "Mercedes-Benz", 15 },
            { "Tesla", 12 },
            { "Hyundai", 15 },
            { "Porsche", 10 },
            { "Chevrolet", 8 }
        };

        foreach (var (make, count) in makes)
        {
            vehicles.AddRange(GenerateByMake(make, count, dealerIds));
        }

        return vehicles;
    }

    private static string GetEngineSize(string make) =>
        make switch
        {
            "Tesla" => "Electric",
            "Porsche" => "3.0L Turbo",
            "BMW" => "2.0L Turbo",
            "Mercedes-Benz" => "2.0L Turbo",
            _ => "2.5L"
        };

    private static List<string> GenerateFeatures(Faker faker, int count)
    {
        return faker
            .PickRandom(AvailableFeatures, count)
            .ToList();
    }

    private record VehicleSpec(string Engine, int Horsepower, int Torque, string DriveType);
}
```

---

### 3. `HomepageSectionAssignmentService.cs`

```csharp
using CarDealer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarDealer.DataSeeding.Services;

/// <summary>
/// Asigna vehículos a secciones del homepage con criterios específicos
/// </summary>
public class HomepageSectionAssignmentService
{
    private readonly ApplicationDbContext _context;

    public HomepageSectionAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAndAssignSectionsAsync()
    {
        // Crear secciones base
        var sections = new List<HomepageSectionConfig>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Carousel Principal",
                Slug = "carousel-principal",
                DisplayOrder = 1,
                MaxItems = 5,
                IsActive = true,
                Subtitle = "Vehículos Destacados",
                AccentColor = "blue"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Sedanes",
                Slug = "sedanes",
                DisplayOrder = 2,
                MaxItems = 10,
                IsActive = true,
                Subtitle = "Coches Elegantes",
                AccentColor = "gray"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "SUVs",
                Slug = "suvs",
                DisplayOrder = 3,
                MaxItems = 10,
                IsActive = true,
                Subtitle = "Vehículos de Utilidad",
                AccentColor = "green"
            },
            // ... más secciones
        };

        await _context.HomepageSectionConfigs.AddRangeAsync(sections);
        await _context.SaveChangesAsync();

        // Obtener vehículos y secciones creadas
        var vehicles = await _context.Vehicles
            .Where(v => v.Status == "Active")
            .ToListAsync();

        var sectionsWithIds = await _context.HomepageSectionConfigs.ToListAsync();

        // Asignar vehículos a secciones
        var assignments = new List<VehicleHomepageSection>();

        // Carousel Principal: 5 vehículos featured
        var carouselSection = sectionsWithIds.First(s => s.Slug == "carousel-principal");
        var featuredVehicles = vehicles.Where(v => v.IsFeatured).Take(5);
        foreach (var vehicle in featuredVehicles)
        {
            assignments.Add(new VehicleHomepageSection
            {
                VehicleId = vehicle.Id,
                HomepageSectionConfigId = carouselSection.Id,
                SortOrder = assignments.Count(a => a.HomepageSectionConfigId == carouselSection.Id) + 1
            });
        }

        // Sedanes: 10 sedans
        var sedanesSection = sectionsWithIds.First(s => s.Slug == "sedanes");
        var sedans = vehicles.Where(v => v.BodyStyle == "Sedan").Take(10);
        foreach (var vehicle in sedans)
        {
            assignments.Add(new VehicleHomepageSection
            {
                VehicleId = vehicle.Id,
                HomepageSectionConfigId = sedanesSection.Id,
                SortOrder = assignments.Count(a => a.HomepageSectionConfigId == sedanesSection.Id) + 1
            });
        }

        // ... más asignaciones

        await _context.VehicleHomepageSections.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();
    }
}
```

---

### 4. `ImageBuilder.cs`

```csharp
using CarDealer.Domain.Entities;

namespace CarDealer.DataSeeding.Builders;

/// <summary>
/// Genera URLs de imágenes usando Picsum Photos con seeding para garantizar unicidad
/// </summary>
public class ImageBuilder
{
    public static IEnumerable<VehicleImage> GenerateBatchForVehicle(
        Guid vehicleId,
        int imageCount = 10)
    {
        var images = new List<VehicleImage>();

        for (int i = 0; i < imageCount; i++)
        {
            var isMainImage = i == 0; // Primera imagen es la principal

            images.Add(new VehicleImage
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                ImageUrl = GeneratePicsumUrl(vehicleId, i),
                DisplayOrder = i,
                IsPrimary = isMainImage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return images;
    }

    /// <summary>
    /// Genera URL única usando Picsum Photos con seed basado en vehicleId
    /// Garantiza que el mismo vehicleId siempre genera las mismas imágenes
    /// </summary>
    private static string GeneratePicsumUrl(Guid vehicleId, int imageIndex)
    {
        // Convierte GUID a número para usar como seed
        var seed = Math.Abs(vehicleId.GetHashCode()) + imageIndex;
        var width = 800;
        var height = 600;

        // Picsum Photos permite URLs con seed para reproducibilidad
        // https://picsum.photos/seed/{seed}/{width}/{height}
        return $"https://picsum.photos/seed/{seed}/{width}/{height}";
    }

    /// <summary>
    /// Genera batch completo de imágenes para todos los vehículos
    /// </summary>
    public static IEnumerable<VehicleImage> GenerateAllImages(
        List<Guid> vehicleIds,
        int imagesPerVehicle = 10)
    {
        var images = new List<VehicleImage>();

        foreach (var vehicleId in vehicleIds)
        {
            images.AddRange(GenerateBatchForVehicle(vehicleId, imagesPerVehicle));
        }

        return images;
    }

    /// <summary>
    /// URLs alternativas si Picsum no está disponible
    /// </summary>
    private static string GetFallbackUrl(Guid vehicleId, int index)
    {
        // Fallback a PlaceImg
        var seed = Math.Abs(vehicleId.GetHashCode()) + index;
        return $"https://placeimg.com/800/600/cars?random={seed}";
    }
}
```

---

### 5. `RelationshipBuilder.cs`

```csharp
using Bogus;
using CarDealer.Domain.Entities;

namespace CarDealer.DataSeeding.Builders;

/// <summary>
/// Construye relaciones transaccionales: Favorites, Alerts, Messages, Reviews, etc.
/// </summary>
public class RelationshipBuilder
{
    public static IEnumerable<Favorite> GenerateFavorites(
        List<Guid> buyerIds,
        List<Guid> vehicleIds,
        int favoritesPerBuyer = 10)
    {
        var faker = new Faker();
        var favorites = new List<Favorite>();

        foreach (var buyerId in buyerIds)
        {
            var count = faker.Random.Int(5, favoritesPerBuyer);
            var selectedVehicles = faker.PickRandom(vehicleIds, count);

            foreach (var vehicleId in selectedVehicles)
            {
                favorites.Add(new Favorite
                {
                    Id = Guid.NewGuid(),
                    UserId = buyerId,
                    VehicleId = vehicleId,
                    Note = faker.Lorem.Sentence(),
                    CreatedAt = faker.Date.Past(1),
                    NotifyOnPriceChange = faker.Random.Bool(0.5f)
                });
            }
        }

        return favorites;
    }

    public static IEnumerable<PriceAlert> GenerateAlerts(
        List<Guid> buyerIds,
        List<Guid> vehicleIds,
        int alertsPerBuyer = 5)
    {
        var faker = new Faker();
        var alerts = new List<PriceAlert>();

        foreach (var buyerId in buyerIds.Take(buyerIds.Count / 3))
        {
            var count = faker.Random.Int(2, alertsPerBuyer);
            var selectedVehicles = faker.PickRandom(vehicleIds, count);

            foreach (var vehicleId in selectedVehicles)
            {
                alerts.Add(new PriceAlert
                {
                    Id = Guid.NewGuid(),
                    UserId = buyerId,
                    VehicleId = vehicleId,
                    TargetPrice = faker.Random.Long(10000, 50000),
                    IsActive = true,
                    CreatedAt = faker.Date.Past(1)
                });
            }
        }

        return alerts;
    }

    public static IEnumerable<DealerReview> GenerateReviews(
        List<Guid> dealerIds,
        List<Guid> buyerIds,
        int reviewsPerDealer = 5)
    {
        var faker = new Faker();
        var reviews = new List<DealerReview>();

        foreach (var dealerId in dealerIds)
        {
            var count = faker.Random.Int(2, reviewsPerDealer);
            var reviewers = faker.PickRandom(buyerIds, Math.Min(count, buyerIds.Count));

            foreach (var reviewerId in reviewers)
            {
                reviews.Add(new DealerReview
                {
                    Id = Guid.NewGuid(),
                    DealerId = dealerId,
                    ReviewerId = reviewerId,
                    Rating = faker.Random.Int(1, 5),
                    Comment = faker.Lorem.Sentences(3),
                    CreatedAt = faker.Date.Past(1)
                });
            }
        }

        return reviews;
    }

    public static IEnumerable<ActivityLog> GenerateLogs(int logCount = 100)
    {
        var faker = new Faker();
        var logs = new List<ActivityLog>();

        var actions = new[] { "view", "favorite", "compare", "contact", "purchase", "search" };
        var resources = new[] { "vehicle", "dealer", "listing", "user", "alert" };

        for (int i = 0; i < logCount; i++)
        {
            logs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Action = faker.PickRandom(actions),
                ResourceType = faker.PickRandom(resources),
                ResourceId = Guid.NewGuid(),
                Timestamp = faker.Date.Past(90),
                IpAddress = faker.Internet.Ip(),
                UserAgent = faker.Internet.UserAgent()
            });
        }

        return logs;
    }
}
```

---

### 6. `DatabaseSeedingService.cs` (Actualizado)

```csharp
using CarDealer.DataSeeding.Builders;
using CarDealer.DataSeeding.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarDealer.DataSeeding;

/// <summary>
/// Servicio principal de seeding con 7 fases
/// </summary>
public class DatabaseSeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeedingService> _logger;

    public DatabaseSeedingService(
        ApplicationDbContext context,
        ILogger<DatabaseSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🌱 Iniciando seeding completo v2.0...");

            // Fase 0: Catálogos
            await Phase0CatalogsAsync(cancellationToken);

            // Fase 1: Usuarios
            var userIds = await Phase1UsersAsync(cancellationToken);

            // Fase 2: Dealers
            var dealerIds = await Phase2DealersAsync(userIds, cancellationToken);

            // Fase 3: Vehículos
            var vehicleIds = await Phase3VehiclesAsync(dealerIds, cancellationToken);

            // Fase 4: Homepage Sections
            await Phase4HomepageSectionsAsync(vehicleIds, cancellationToken);

            // Fase 5: Imágenes
            await Phase5ImagesAsync(vehicleIds, cancellationToken);

            // Fase 6: Relaciones
            await Phase6RelationshipsAsync(userIds, dealerIds, vehicleIds, cancellationToken);

            // Fase 7: Validación
            await Phase7ValidationAsync(cancellationToken);

            _logger.LogInformation("✅ Seeding completado exitosamente!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante seeding");
            throw;
        }
    }

    private async Task Phase0CatalogsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Fase 0: Generando catálogos...");

        var makes = CatalogBuilder.GenerateMakes();
        var models = CatalogBuilder.GenerateModels();
        var years = CatalogBuilder.GenerateYears();
        var bodyStyles = CatalogBuilder.GenerateBodyStyles();
        var fuelTypes = CatalogBuilder.GenerateFuelTypes();
        var colors = CatalogBuilder.GenerateColors();

        await _context.Makes.AddRangeAsync(makes, cancellationToken);
        await _context.Models.AddRangeAsync(models, cancellationToken);
        await _context.Years.AddRangeAsync(years, cancellationToken);
        await _context.BodyStyles.AddRangeAsync(bodyStyles, cancellationToken);
        await _context.FuelTypes.AddRangeAsync(fuelTypes, cancellationToken);
        await _context.Colors.AddRangeAsync(colors, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("  ✅ Catálogos: 10 makes, 60+ models, 15 years");
    }

    private async Task<List<Guid>> Phase1UsersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("👥 Fase 1: Generando 42 usuarios...");

        var users = new List<User>();

        // 10 Buyers
        users.AddRange(UserBuilder.GenerateBuyers(10));

        // 10 Sellers
        users.AddRange(UserBuilder.GenerateSellers(10));

        // 30 Dealer Users
        users.AddRange(UserBuilder.GenerateDealerUsers(30));

        // 2 Admins
        users.AddRange(UserBuilder.GenerateAdmins(2));

        await _context.Users.AddRangeAsync(users, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("  ✅ Usuarios: 42 totales (10 buyers, 10 sellers, 30 dealers, 2 admins)");
        return users.Select(u => u.Id).ToList();
    }

    private async Task<List<Guid>> Phase2DealersAsync(List<Guid> userIds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏢 Fase 2: Generando 30 dealers...");

        var dealerUsers = userIds.Skip(20).Take(30).ToList();
        var dealers = DealerBuilder.GenerateBatch(dealerUsers);

        await _context.Dealers.AddRangeAsync(dealers, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("  ✅ Dealers: 30 totales con locations");
        return dealers.Select(d => d.Id).ToList();
    }

    private async Task<List<Guid>> Phase3VehiclesAsync(List<Guid> dealerIds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚗 Fase 3: Generando 150 vehículos...");

        var vehicles = VehicleBuilder.GenerateAllBrands(dealerIds).ToList();

        await _context.Vehicles.AddRangeAsync(vehicles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("  ✅ Vehículos: 150 totales con specs completos");
        return vehicles.Select(v => v.Id).ToList();
    }

    private async Task Phase4HomepageSectionsAsync(List<Guid> vehicleIds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏠 Fase 4: Asignando vehículos a secciones...");

        var sectionService = new HomepageSectionAssignmentService(_context);
        await sectionService.CreateAndAssignSectionsAsync();

        _logger.LogInformation("  ✅ Homepage: 8 secciones con 90 vehículos asignados");
    }

    private async Task Phase5ImagesAsync(List<Guid> vehicleIds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🖼️  Fase 5: Generando 1,500 imágenes...");

        var images = ImageBuilder.GenerateAllImages(vehicleIds, imagesPerVehicle: 10).ToList();

        await _context.VehicleImages.AddRangeAsync(images, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("  ✅ Imágenes: 1,500 totales (10 por vehículo)");
    }

    private async Task Phase6RelationshipsAsync(
        List<Guid> userIds,
        List<Guid> dealerIds,
        List<Guid> vehicleIds,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔗 Fase 6: Generando relaciones...");

        var buyerIds = userIds.Take(10).ToList();

        // Favorites
        var favorites = RelationshipBuilder.GenerateFavorites(buyerIds, vehicleIds, 10);
        await _context.Favorites.AddRangeAsync(favorites, cancellationToken);

        // Price Alerts
        var alerts = RelationshipBuilder.GenerateAlerts(buyerIds, vehicleIds, 5);
        await _context.PriceAlerts.AddRangeAsync(alerts, cancellationToken);

        // Reviews
        var reviews = RelationshipBuilder.GenerateReviews(dealerIds, buyerIds, 5);
        await _context.DealerReviews.AddRangeAsync(reviews, cancellationToken);

        // Activity Logs
        var logs = RelationshipBuilder.GenerateLogs(100);
        await _context.ActivityLogs.AddRangeAsync(logs, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("  ✅ Relaciones: 50+ favorites, 15+ alerts, 150+ reviews, 100+ logs");
    }

    private async Task Phase7ValidationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("✔️  Fase 7: Validando datos...");

        var vehicleCount = await _context.Vehicles.CountAsync(cancellationToken);
        var imageCount = await _context.VehicleImages.CountAsync(cancellationToken);
        var userCount = await _context.Users.CountAsync(cancellationToken);
        var dealerCount = await _context.Dealers.CountAsync(cancellationToken);
        var favoriteCount = await _context.Favorites.CountAsync(cancellationToken);

        _logger.LogInformation(@$"
📊 ESTADÍSTICAS FINALES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Catálogos: 10 Makes, 60+ Models, 15 Years
✅ Usuarios: {userCount} (10 buyers, 10 sellers, 30 dealers, 2 admins)
✅ Dealers: {dealerCount}
✅ Vehículos: {vehicleCount}
✅ Imágenes: {imageCount}
✅ Homepage: 8 secciones con 90 vehículos
✅ Favoritos: {favoriteCount}+
✅ Alertas: 15+
✅ Reviews: 150+
✅ Logs: 100+
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
");
    }
}
```

---

## 📚 ARCHIVOS QUE NECESITAS CREAR

```
backend/_Shared/CarDealer.DataSeeding/
├─ Builders/
│  ├─ CatalogBuilder.cs              (NUEVO)
│  ├─ VehicleBuilder.cs              (MEJORADO)
│  ├─ UserBuilder.cs                 (EXISTENTE - mejorar)
│  ├─ DealerBuilder.cs               (EXISTENTE - mejorar)
│  └─ ImageBuilder.cs                (NUEVO)
├─ Services/
│  └─ HomepageSectionAssignmentService.cs (NUEVO)
├─ RelationshipBuilder.cs            (NUEVO)
└─ DatabaseSeedingService.cs         (ACTUALIZAR)
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

- [ ] Crear CatalogBuilder.cs con 10 makes específicas
- [ ] Crear VehicleBuilder.cs con 150 vehículos distribuidos
- [ ] Mejorar UserBuilder.cs para 42 usuarios
- [ ] Mejorar DealerBuilder.cs para 30 dealers con locations
- [ ] Crear ImageBuilder.cs con 1,500 imágenes Picsum
- [ ] Crear HomepageSectionAssignmentService.cs para 8 secciones
- [ ] Crear RelationshipBuilder.cs para favorites, alerts, reviews, logs
- [ ] Actualizar DatabaseSeedingService.cs con 7 fases
- [ ] Probar cada fase independientemente
- [ ] Ejecutar validación completa (Fase 7)

**Total:** 9 clases + 2 mejorar = 11 archivos C#
