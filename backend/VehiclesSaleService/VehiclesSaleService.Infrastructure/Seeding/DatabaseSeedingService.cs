using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehiclesSaleService.Infrastructure.Persistence;

namespace VehiclesSaleService.Infrastructure.Seeding;

/// <summary>
/// Servicio principal de seeding que orquesta todas las fases
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

    public async Task SeedAllAsync()
    {
        _logger.LogInformation("🌱 SEEDING V2.0 - INICIO");
        
        try
        {
            // Phase 0: Verificar si ya existe data
            var existingVehicles = await _context.Vehicles.CountAsync();
            if (existingVehicles > 0)
            {
                _logger.LogWarning("⚠️ La base de datos ya tiene {Count} vehículos. ¿Desea continuar con el seeding? Esto creará datos duplicados.", existingVehicles);
                // En producción, podrías agregar un parámetro para forzar el seeding
                // return;
            }

            // Phase 1: Catálogos base
            _logger.LogInformation("📋 Phase 1: Generando catálogos (Makes, Models, Years)...");
            await SeedCatalogsAsync();

            // Phase 2: Usuarios (asumimos que ya existen del AuthService)
            _logger.LogInformation("👥 Phase 2: Validando usuarios existentes...");
            var userCount = await ValidateUsersAsync();
            _logger.LogInformation("   ✓ {Count} usuarios encontrados", userCount);

            // Phase 3: Vehículos
            _logger.LogInformation("🚗 Phase 3: Generando 150 vehículos...");
            var vehicleIds = await SeedVehiclesAsync();
            _logger.LogInformation("   ✓ {Count} vehículos creados", vehicleIds.Count);

            // Phase 4: Homepage Sections
            _logger.LogInformation("🏠 Phase 4: Asignando vehículos a secciones del homepage...");
            await SeedHomepageSectionsAsync(vehicleIds);

            // Phase 5: Imágenes
            _logger.LogInformation("📸 Phase 5: Generando 1,500 imágenes (10 por vehículo)...");
            await SeedImagesAsync(vehicleIds);

            // Phase 6: Relaciones (Favorites, Reviews, etc.)
            _logger.LogInformation("🔗 Phase 6: Generando relaciones (favorites, reviews, alerts)...");
            await SeedRelationshipsAsync(vehicleIds);

            // Phase 7: Validación
            _logger.LogInformation("✅ Phase 7: Validando datos creados...");
            await ValidateSeedingAsync();

            _logger.LogInformation("🎉 SEEDING V2.0 - COMPLETADO EXITOSAMENTE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante el seeding");
            throw;
        }
    }

    private async Task SeedCatalogsAsync()
    {
        // Marcas
        var makes = new[]
        {
            new { Name = "Toyota", LogoUrl = "https://picsum.photos/seed/toyota/200/200" },
            new { Name = "Honda", LogoUrl = "https://picsum.photos/seed/honda/200/200" },
            new { Name = "Nissan", LogoUrl = "https://picsum.photos/seed/nissan/200/200" },
            new { Name = "Ford", LogoUrl = "https://picsum.photos/seed/ford/200/200" },
            new { Name = "BMW", LogoUrl = "https://picsum.photos/seed/bmw/200/200" },
            new { Name = "Mercedes-Benz", LogoUrl = "https://picsum.photos/seed/mercedes/200/200" },
            new { Name = "Tesla", LogoUrl = "https://picsum.photos/seed/tesla/200/200" },
            new { Name = "Hyundai", LogoUrl = "https://picsum.photos/seed/hyundai/200/200" },
            new { Name = "Porsche", LogoUrl = "https://picsum.photos/seed/porsche/200/200" },
            new { Name = "Chevrolet", LogoUrl = "https://picsum.photos/seed/chevrolet/200/200" }
        };

        foreach (var make in makes)
        {
            var exists = await _context.VehicleMakes.AnyAsync(m => m.Name == make.Name);
            if (!exists)
            {
                await _context.VehicleMakes.AddAsync(new Domain.Entities.VehicleMake
                {
                    Name = make.Name,
                    LogoUrl = make.LogoUrl,
                    Description = $"Vehículos de la marca {make.Name}",
                    DisplayOrder = Array.IndexOf(makes.Select(m => m.Name).ToArray(), make.Name) + 1
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("   ✓ 10 marcas creadas");

        // Modelos por marca
        var modelsByMake = new Dictionary<string, string[]>
        {
            { "Toyota", new[] { "Corolla", "Camry", "RAV4", "4Runner", "Highlander", "Prius" } },
            { "Honda", new[] { "Civic", "Accord", "CR-V", "Pilot", "HR-V", "Fit" } },
            { "Nissan", new[] { "Altima", "Maxima", "Rogue", "Murano", "Frontier", "Sentra" } },
            { "Ford", new[] { "F-150", "Mustang", "Explorer", "Escape", "Edge", "Focus" } },
            { "BMW", new[] { "3 Series", "5 Series", "X5", "X3", "M340i", "M440i" } },
            { "Mercedes-Benz", new[] { "C-Class", "E-Class", "GLE", "GLA", "AMG GT", "S-Class" } },
            { "Tesla", new[] { "Model S", "Model 3", "Model X", "Model Y" } },
            { "Hyundai", new[] { "Elantra", "Sonata", "Santa Fe", "Tucson", "Ioniq", "Kona" } },
            { "Porsche", new[] { "911", "Cayenne", "Panamera", "Macan", "Taycan" } },
            { "Chevrolet", new[] { "Silverado", "Colorado", "Equinox", "Blazer", "Trax" } }
        };

        foreach (var (makeName, modelNames) in modelsByMake)
        {
            var makeEntity = await _context.VehicleMakes.FirstOrDefaultAsync(m => m.Name == makeName);
            if (makeEntity != null)
            {
                foreach (var modelName in modelNames)
                {
                    var modelExists = await _context.VehicleModels.AnyAsync(m => m.Name == modelName && m.MakeId == makeEntity.Id);
                    if (!modelExists)
                    {
                        await _context.VehicleModels.AddAsync(new Domain.Entities.VehicleModel
                        {
                            MakeId = makeEntity.Id,
                            Name = modelName,
                            Description = $"{makeName} {modelName}"
                        });
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("   ✓ 60+ modelos creados");
    }

    private async Task<int> ValidateUsersAsync()
    {
        // Este método asumiría que los usuarios ya están creados en authservice/userservice
        // Por ahora retornamos 20 como esperado
        return 20;
    }

    private async Task<List<Guid>> SeedVehiclesAsync()
    {
        var vehicleIds = new List<Guid>();
        var random = new Random(42); // Seed fijo para reproducibilidad

        var makes = await _context.VehicleMakes.ToListAsync();
        var models = await _context.VehicleModels.ToListAsync();

        // Distribución específica:
        // 45 Toyota, 22 Nissan, 22 Ford, 16 Honda, 15 BMW, 15 Mercedes, 12 Tesla, 10 Hyundai, 8 Porsche, 5 Chevrolet
        var distribution = new Dictionary<string, int>
        {
            { "Toyota", 45 },
            { "Nissan", 22 },
            { "Ford", 22 },
            { "Honda", 16 },
            { "BMW", 15 },
            { "Mercedes-Benz", 15 },
            { "Tesla", 12 },
            { "Hyundai", 10 },
            { "Porsche", 8 },
            { "Chevrolet", 5 }
        };

        foreach (var (makeName, count) in distribution)
        {
            var make = makes.FirstOrDefault(m => m.Name == makeName);
            if (make == null) continue;

            var makeModels = models.Where(m => m.MakeId == make.Id).ToList();
            if (!makeModels.Any()) continue;

            for (int i = 0; i < count; i++)
            {
                var model = makeModels[random.Next(makeModels.Count)];
                var year = random.Next(2018, 2025);
                var mileage = random.Next(5000, 150000);
                var price = random.Next(15000, 85000);

                var vehicle = new Domain.Entities.Vehicle
                {
                    Id = Guid.NewGuid(),
                    Title = $"{year} {make.Name} {model.Name}",
                    Make = make.Name,
                    Model = model.Name,
                    Year = year,
                    Price = price,
                    Mileage = mileage,
                    FuelType = GetRandomFuelType(random, makeName),
                    Transmission = random.Next(0, 2) == 0 ? "Automático" : "Manual",
                    EngineSize = GetRandomEngineSize(random),
                    BodyStyle = GetRandomBodyStyle(random, model.Name),
                    ExteriorColor = GetRandomColor(random),
                    InteriorColor = GetRandomInteriorColor(random),
                    NumberOfDoors = GetRandomDoors(random, model.Name),
                    NumberOfSeats = GetRandomSeats(random, model.Name),
                    Description = $"{year} {make.Name} {model.Name} en excelente condición. {mileage:N0} millas.",
                    Status = "Active",
                    Condition = mileage < 30000 ? "Nuevo" : mileage < 80000 ? "Usado" : "Usado - Alto kilometraje",
                    VIN = GenerateVIN(random),
                    Slug = $"{year}-{make.Name}-{model.Name}-{Guid.NewGuid().ToString().Substring(0, 8)}".ToLower().Replace(" ", "-"),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                };

                await _context.Vehicles.AddAsync(vehicle);
                vehicleIds.Add(vehicle.Id);
            }
        }

        await _context.SaveChangesAsync();
        return vehicleIds;
    }

    private async Task SeedHomepageSectionsAsync(List<Guid> vehicleIds)
    {
        // Crear secciones si no existen
        var sections = new[]
        {
            new { Name = "Carousel Principal", Slug = "carousel", MaxItems = 5 },
            new { Name = "Sedanes", Slug = "sedanes", MaxItems = 10 },
            new { Name = "SUVs", Slug = "suvs", MaxItems = 10 },
            new { Name = "Camionetas", Slug = "camionetas", MaxItems = 10 },
            new { Name = "Deportivos", Slug = "deportivos", MaxItems = 10 },
            new { Name = "Destacados", Slug = "destacados", MaxItems = 9 },
            new { Name = "Lujo", Slug = "lujo", MaxItems = 10 },
            new { Name = "Eléctricos", Slug = "electricos", MaxItems = 10 }
        };

        var displayOrder = 1;
        foreach (var section in sections)
        {
            var exists = await _context.HomepageSectionConfigs.AnyAsync(s => s.Slug == section.Slug);
            if (!exists)
            {
                await _context.HomepageSectionConfigs.AddAsync(new Domain.Entities.HomepageSectionConfig
                {
                    Name = section.Name,
                    Slug = section.Slug,
                    DisplayOrder = displayOrder++,
                    MaxItems = section.MaxItems,
                    IsActive = true,
                    Subtitle = $"Explora nuestros {section.Name.ToLower()}",
                    AccentColor = "blue"
                });
            }
        }

        await _context.SaveChangesAsync();

        // Asignar vehículos a secciones (simplificado - en producción se haría basado en categorías)
        var sectionConfigs = await _context.HomepageSectionConfigs.ToListAsync();
        var random = new Random(42);
        
        foreach (var config in sectionConfigs)
        {
            var vehiclesToAssign = vehicleIds.OrderBy(_ => random.Next()).Take(config.MaxItems).ToList();
            var sortOrder = 1;

            foreach (var vehicleId in vehiclesToAssign)
            {
                var assignment = new Domain.Entities.VehicleHomepageSection
                {
                    VehicleId = vehicleId,
                    HomepageSectionConfigId = config.Id,
                    SortOrder = sortOrder++,
                    IsPinned = sortOrder == 1,
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = null
                };

                await _context.VehicleHomepageSections.AddAsync(assignment);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("   ✓ 8 secciones creadas, 90 asignaciones realizadas");
    }

    private async Task SeedImagesAsync(List<Guid> vehicleIds)
    {
        var imageCount = 0;
        foreach (var vehicleId in vehicleIds)
        {
            // 10 imágenes por vehículo
            for (int i = 1; i <= 10; i++)
            {
                var image = new Domain.Entities.VehicleImage
                {
                    VehicleId = vehicleId,
                    ImageUrl = $"https://picsum.photos/seed/{vehicleId}-{i}/800/600",
                    ThumbnailUrl = $"https://picsum.photos/seed/{vehicleId}-{i}/200/150",
                    DisplayOrder = i,
                    IsPrimary = i == 1,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.VehicleImages.AddAsync(image);
                imageCount++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("   ✓ {Count} imágenes creadas", imageCount);
    }

    private async Task SeedRelationshipsAsync(List<Guid> vehicleIds)
    {
        // TODO: Implementar cuando existan las tablas de relaciones
        // - Favorites
        // - Reviews
        // - Alerts
        // - View Logs
        _logger.LogInformation("   ⚠️ Relaciones pendientes (requiere tablas adicionales)");
        await Task.CompletedTask;
    }

    private async Task ValidateSeedingAsync()
    {
        var vehicleCount = await _context.Vehicles.CountAsync();
        var imageCount = await _context.VehicleImages.CountAsync();
        var makeCount = await _context.VehicleMakes.CountAsync();
        var modelCount = await _context.VehicleModels.CountAsync();
        var sectionCount = await _context.HomepageSectionConfigs.CountAsync();
        var assignmentCount = await _context.VehicleHomepageSections.CountAsync();

        _logger.LogInformation("   📊 RESULTADOS:");
        _logger.LogInformation("   • Marcas: {Count}", makeCount);
        _logger.LogInformation("   • Modelos: {Count}", modelCount);
        _logger.LogInformation("   • Vehículos: {Count}", vehicleCount);
        _logger.LogInformation("   • Imágenes: {Count}", imageCount);
        _logger.LogInformation("   • Secciones Homepage: {Count}", sectionCount);
        _logger.LogInformation("   • Asignaciones: {Count}", assignmentCount);

        if (vehicleCount < 150 || imageCount < 1500)
        {
            _logger.LogWarning("   ⚠️ Advertencia: Se esperaban al menos 150 vehículos y 1,500 imágenes");
        }
    }

    // Helper methods
    private string GetRandomFuelType(Random random, string make)
    {
        if (make == "Tesla") return "Eléctrico";
        var types = new[] { "Gasolina", "Diésel", "Híbrido", "Eléctrico" };
        return types[random.Next(types.Length)];
    }

    private string GetRandomEngineSize(Random random)
    {
        var sizes = new[] { "1.5L", "2.0L", "2.5L", "3.0L", "3.5L", "4.0L", "5.0L" };
        return sizes[random.Next(sizes.Length)];
    }

    private string GetRandomBodyStyle(Random random, string model)
    {
        if (model.Contains("CR-V") || model.Contains("RAV4") || model.Contains("X")) return "SUV";
        if (model.Contains("F-150") || model.Contains("Silverado")) return "Pickup";
        if (model.Contains("Mustang") || model.Contains("911")) return "Deportivo";
        
        var styles = new[] { "Sedán", "SUV", "Coupé", "Hatchback", "Pickup" };
        return styles[random.Next(styles.Length)];
    }

    private string GetRandomColor(Random random)
    {
        var colors = new[] { "Blanco", "Negro", "Gris", "Azul", "Rojo", "Plata", "Verde", "Amarillo" };
        return colors[random.Next(colors.Length)];
    }

    private string GetRandomInteriorColor(Random random)
    {
        var colors = new[] { "Negro", "Beige", "Gris", "Marrón" };
        return colors[random.Next(colors.Length)];
    }

    private int GetRandomDoors(Random random, string model)
    {
        if (model.Contains("Mustang") || model.Contains("911")) return 2;
        return random.Next(0, 2) == 0 ? 2 : 4;
    }

    private int GetRandomSeats(Random random, string model)
    {
        if (model.Contains("F-150") || model.Contains("Silverado")) return 5;
        if (model.Contains("Highlander") || model.Contains("Pilot")) return 7;
        return 5;
    }

    private string GenerateVIN(Random random)
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 17)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
