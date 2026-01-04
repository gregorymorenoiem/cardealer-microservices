using Microsoft.EntityFrameworkCore;
using VehiclesSaleService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Infrastructure.Persistence;

public class ApplicationDbContext : MultiTenantDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    // ========================================
    // VEHICLE ENTITIES
    // ========================================
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleImage> VehicleImages => Set<VehicleImage>();
    public DbSet<VehicleMake> VehicleMakes => Set<VehicleMake>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<VehicleTrim> VehicleTrims => Set<VehicleTrim>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // VEHICLE CONFIGURATION
        // ========================================

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles");
            entity.HasKey(v => v.Id);

            // Información básica
            entity.Property(v => v.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(v => v.Description)
                .HasMaxLength(10000);

            entity.Property(v => v.Price)
                .HasPrecision(18, 2);

            entity.Property(v => v.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(v => v.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(v => v.SellerName)
                .HasMaxLength(200);

            // Identificación
            entity.Property(v => v.VIN)
                .HasMaxLength(17);

            entity.Property(v => v.StockNumber)
                .HasMaxLength(50);

            // Marca / Modelo
            entity.Property(v => v.Make)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Trim)
                .HasMaxLength(100);

            entity.Property(v => v.Generation)
                .HasMaxLength(50);

            // Tipo y carrocería
            entity.Property(v => v.VehicleType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(v => v.BodyStyle)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Motor y transmisión
            entity.Property(v => v.FuelType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(v => v.EngineSize)
                .HasMaxLength(20);

            entity.Property(v => v.Transmission)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(v => v.DriveType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Kilometraje
            entity.Property(v => v.MileageUnit)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(v => v.Condition)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Apariencia
            entity.Property(v => v.ExteriorColor)
                .HasMaxLength(50);

            entity.Property(v => v.InteriorColor)
                .HasMaxLength(50);

            entity.Property(v => v.InteriorMaterial)
                .HasMaxLength(50);

            // Ubicación
            entity.Property(v => v.City)
                .HasMaxLength(100);

            entity.Property(v => v.State)
                .HasMaxLength(100);

            entity.Property(v => v.ZipCode)
                .HasMaxLength(20);

            entity.Property(v => v.Country)
                .HasMaxLength(100)
                .HasDefaultValue("USA");

            // Certificaciones
            entity.Property(v => v.CertificationProgram)
                .HasMaxLength(100);

            entity.Property(v => v.CarfaxReportUrl)
                .HasMaxLength(500);

            entity.Property(v => v.ServiceHistoryNotes)
                .HasMaxLength(5000);

            entity.Property(v => v.WarrantyInfo)
                .HasMaxLength(2000);

            // JSON columns
            entity.Property(v => v.FeaturesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(v => v.PackagesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            // Timestamps
            entity.Property(v => v.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(v => v.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            // Índices para búsqueda y filtrado
            entity.HasIndex(v => v.DealerId);
            entity.HasIndex(v => v.SellerId);
            entity.HasIndex(v => v.Status);
            entity.HasIndex(v => v.Price);
            entity.HasIndex(v => v.Year);
            entity.HasIndex(v => v.Make);
            entity.HasIndex(v => v.Model);
            entity.HasIndex(v => v.MakeId);
            entity.HasIndex(v => v.ModelId);
            entity.HasIndex(v => v.VehicleType);
            entity.HasIndex(v => v.BodyStyle);
            entity.HasIndex(v => v.FuelType);
            entity.HasIndex(v => v.Transmission);
            entity.HasIndex(v => v.Mileage);
            entity.HasIndex(v => v.Condition);
            entity.HasIndex(v => v.State);
            entity.HasIndex(v => v.City);
            entity.HasIndex(v => v.ZipCode);
            entity.HasIndex(v => v.CreatedAt);
            entity.HasIndex(v => v.IsFeatured);
            entity.HasIndex(v => v.IsDeleted);
            entity.HasIndex(v => v.VIN).IsUnique();

            // Índice compuesto para búsquedas comunes
            entity.HasIndex(v => new { v.Make, v.Model, v.Year });
            entity.HasIndex(v => new { v.Status, v.IsDeleted });
            entity.HasIndex(v => new { v.State, v.City });

            // Relaciones
            entity.HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(v => v.Images)
                .WithOne(i => i.Vehicle)
                .HasForeignKey(i => i.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // VEHICLE IMAGE CONFIGURATION
        // ========================================

        modelBuilder.Entity<VehicleImage>(entity =>
        {
            entity.ToTable("vehicle_images");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(i => i.ThumbnailUrl)
                .HasMaxLength(500);

            entity.Property(i => i.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(i => i.VehicleId);
            entity.HasIndex(i => i.DealerId);
            entity.HasIndex(i => new { i.VehicleId, i.SortOrder });
            entity.HasIndex(i => new { i.VehicleId, i.IsPrimary });
        });

        // ========================================
        // VEHICLE MAKE CONFIGURATION (Catálogo de marcas)
        // ========================================

        modelBuilder.Entity<VehicleMake>(entity =>
        {
            entity.ToTable("vehicle_makes");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.LogoUrl)
                .HasMaxLength(500);

            entity.Property(m => m.Country)
                .HasMaxLength(100);

            entity.HasIndex(m => m.Name).IsUnique();
            entity.HasIndex(m => m.IsActive);
            entity.HasIndex(m => m.SortOrder);

            entity.HasMany(m => m.Models)
                .WithOne(model => model.Make)
                .HasForeignKey(model => model.MakeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // VEHICLE MODEL CONFIGURATION (Catálogo de modelos)
        // ========================================

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.ToTable("vehicle_models");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.VehicleType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(m => m.DefaultBodyStyle)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasIndex(m => m.MakeId);
            entity.HasIndex(m => m.Name);
            entity.HasIndex(m => m.IsActive);
            entity.HasIndex(m => new { m.MakeId, m.Name }).IsUnique();

            entity.HasMany(m => m.Trims)
                .WithOne(t => t.Model)
                .HasForeignKey(t => t.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // VEHICLE TRIM CONFIGURATION (Versiones/Trims)
        // ========================================

        modelBuilder.Entity<VehicleTrim>(entity =>
        {
            entity.ToTable("vehicle_trims");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.BaseMSRP)
                .HasPrecision(18, 2);

            entity.Property(t => t.EngineSize)
                .HasMaxLength(200);

            entity.Property(t => t.Transmission)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(t => t.DriveType)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(t => t.FuelType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasIndex(t => t.ModelId);
            entity.HasIndex(t => t.Year);
            entity.HasIndex(t => t.IsActive);
            entity.HasIndex(t => new { t.ModelId, t.Year, t.Name }).IsUnique();
        });

        // ========================================
        // CATEGORY CONFIGURATION
        // ========================================

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.Property(c => c.IconUrl)
                .HasMaxLength(500);

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.ParentId);
            entity.HasIndex(c => c.IsActive);
            entity.HasIndex(c => c.Level);

            // Self-referencing relationship
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // SEED DATA - Categorías para vehículos en venta
        // ========================================

        SeedCategories(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var carsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var trucksId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var suvsId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var motorcyclesId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var boatsId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var rvsId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = carsId,
                Name = "Cars",
                Slug = "cars",
                Description = "Sedans, coupes, hatchbacks and sports cars",
                Level = 0,
                SortOrder = 1,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = trucksId,
                Name = "Trucks",
                Slug = "trucks",
                Description = "Pickup trucks and commercial trucks",
                Level = 0,
                SortOrder = 2,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = suvsId,
                Name = "SUVs & Crossovers",
                Slug = "suvs-crossovers",
                Description = "Sport utility vehicles and crossovers",
                Level = 0,
                SortOrder = 3,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = motorcyclesId,
                Name = "Motorcycles",
                Slug = "motorcycles",
                Description = "Sport bikes, cruisers and ATVs",
                Level = 0,
                SortOrder = 4,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = boatsId,
                Name = "Boats & Watercraft",
                Slug = "boats-watercraft",
                Description = "Boats, jet skis and watercraft",
                Level = 0,
                SortOrder = 5,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = rvsId,
                Name = "RVs & Campers",
                Slug = "rvs-campers",
                Description = "Recreational vehicles and campers",
                Level = 0,
                SortOrder = 6,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Vehicle && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((Vehicle)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
