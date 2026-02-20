using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence;

public class AdvertisingDbContext : DbContext
{
    public AdvertisingDbContext(DbContextOptions<AdvertisingDbContext> options) : base(options) { }

    public DbSet<AdCampaign> AdCampaigns => Set<AdCampaign>();
    public DbSet<AdImpression> AdImpressions => Set<AdImpression>();
    public DbSet<AdClick> AdClicks => Set<AdClick>();
    public DbSet<RotationConfig> RotationConfigs => Set<RotationConfig>();
    public DbSet<CategoryImageConfig> CategoryImageConfigs => Set<CategoryImageConfig>();
    public DbSet<BrandConfig> BrandConfigs => Set<BrandConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdvertisingDbContext).Assembly);

        // Seed RotationConfig
        modelBuilder.Entity<RotationConfig>().HasData(
            new RotationConfig
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                Section = AdPlacementType.FeaturedSpot,
                AlgorithmType = RotationAlgorithmType.WeightedRandom,
                RefreshIntervalMinutes = 30,
                MaxVehiclesShown = 8,
                WeightRemainingBudget = 0.30m,
                WeightCtr = 0.25m,
                WeightQualityScore = 0.25m,
                WeightRecency = 0.20m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new RotationConfig
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                Section = AdPlacementType.PremiumSpot,
                AlgorithmType = RotationAlgorithmType.BudgetPriority,
                RefreshIntervalMinutes = 60,
                MaxVehiclesShown = 4,
                WeightRemainingBudget = 0.40m,
                WeightCtr = 0.20m,
                WeightQualityScore = 0.20m,
                WeightRecency = 0.20m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed BrandConfig
        modelBuilder.Entity<BrandConfig>().HasData(
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), BrandKey = "toyota", DisplayName = "Toyota", LogoInitials = "TO", VehicleCount = 0, DisplayOrder = 1, IsVisible = true, Route = "/buscar?marca=toyota", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), BrandKey = "honda", DisplayName = "Honda", LogoInitials = "HO", VehicleCount = 0, DisplayOrder = 2, IsVisible = true, Route = "/buscar?marca=honda", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), BrandKey = "hyundai", DisplayName = "Hyundai", LogoInitials = "HY", VehicleCount = 0, DisplayOrder = 3, IsVisible = true, Route = "/buscar?marca=hyundai", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), BrandKey = "kia", DisplayName = "Kia", LogoInitials = "KI", VehicleCount = 0, DisplayOrder = 4, IsVisible = true, Route = "/buscar?marca=kia", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000005"), BrandKey = "nissan", DisplayName = "Nissan", LogoInitials = "NI", VehicleCount = 0, DisplayOrder = 5, IsVisible = true, Route = "/buscar?marca=nissan", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000006"), BrandKey = "mazda", DisplayName = "Mazda", LogoInitials = "MA", VehicleCount = 0, DisplayOrder = 6, IsVisible = true, Route = "/buscar?marca=mazda", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000007"), BrandKey = "ford", DisplayName = "Ford", LogoInitials = "FO", VehicleCount = 0, DisplayOrder = 7, IsVisible = true, Route = "/buscar?marca=ford", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000008"), BrandKey = "chevrolet", DisplayName = "Chevrolet", LogoInitials = "CH", VehicleCount = 0, DisplayOrder = 8, IsVisible = true, Route = "/buscar?marca=chevrolet", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-000000000009"), BrandKey = "bmw", DisplayName = "BMW", LogoInitials = "BM", VehicleCount = 0, DisplayOrder = 9, IsVisible = true, Route = "/buscar?marca=bmw", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-00000000000A"), BrandKey = "mercedes-benz", DisplayName = "Mercedes-Benz", LogoInitials = "ME", VehicleCount = 0, DisplayOrder = 10, IsVisible = true, Route = "/buscar?marca=mercedes-benz", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-00000000000B"), BrandKey = "audi", DisplayName = "Audi", LogoInitials = "AU", VehicleCount = 0, DisplayOrder = 11, IsVisible = true, Route = "/buscar?marca=audi", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new BrandConfig { Id = Guid.Parse("22222222-0000-0000-0000-00000000000C"), BrandKey = "volkswagen", DisplayName = "Volkswagen", LogoInitials = "VO", VehicleCount = 0, DisplayOrder = 12, IsVisible = true, Route = "/buscar?marca=volkswagen", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed CategoryImageConfig
        modelBuilder.Entity<CategoryImageConfig>().HasData(
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000001"), CategoryKey = "suv", DisplayName = "SUV", Description = "Versatilidad y espacio para toda la familia", ImageUrl = "", Gradient = "from-blue-600 to-blue-800", VehicleCount = 0, IsTrending = true, DisplayOrder = 1, IsVisible = true, Route = "/buscar?tipo=suv", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000002"), CategoryKey = "sedan", DisplayName = "Sedán", Description = "Elegancia y eficiencia para el día a día", ImageUrl = "", Gradient = "from-primary to-primary/90", VehicleCount = 0, IsTrending = false, DisplayOrder = 2, IsVisible = true, Route = "/buscar?tipo=sedan", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000003"), CategoryKey = "camioneta", DisplayName = "Camioneta", Description = "Potencia y capacidad de carga", ImageUrl = "", Gradient = "from-amber-600 to-amber-800", VehicleCount = 0, IsTrending = false, DisplayOrder = 3, IsVisible = true, Route = "/buscar?tipo=camioneta", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000004"), CategoryKey = "deportivo", DisplayName = "Deportivo", Description = "Rendimiento y adrenalina pura", ImageUrl = "", Gradient = "from-red-600 to-red-800", VehicleCount = 0, IsTrending = false, DisplayOrder = 4, IsVisible = true, Route = "/buscar?tipo=deportivo", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000005"), CategoryKey = "electrico", DisplayName = "Eléctrico", Description = "El futuro de la movilidad sostenible", ImageUrl = "", Gradient = "from-green-600 to-green-800", VehicleCount = 0, IsTrending = true, DisplayOrder = 5, IsVisible = true, Route = "/buscar?tipo=electrico", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new CategoryImageConfig { Id = Guid.Parse("33333333-0000-0000-0000-000000000006"), CategoryKey = "hibrido", DisplayName = "Híbrido", Description = "Lo mejor de dos mundos", ImageUrl = "", Gradient = "from-teal-600 to-teal-800", VehicleCount = 0, IsTrending = false, DisplayOrder = 6, IsVisible = true, Route = "/buscar?tipo=hibrido", UpdatedAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc) }
        );

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<AdCampaign>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
        return await base.SaveChangesAsync(ct);
    }
}
