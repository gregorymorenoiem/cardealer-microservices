using Microsoft.EntityFrameworkCore;
using PropertiesRentService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;

namespace PropertiesRentService.Infrastructure.Persistence;

public class ApplicationDbContext : MultiTenantDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    // ========================================
    // PROPERTY ENTITIES
    // ========================================
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // PROPERTY CONFIGURATION
        // ========================================

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties");
            entity.HasKey(p => p.Id);

            // Información básica
            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(p => p.Description)
                .HasMaxLength(10000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(p => p.AgentName)
                .HasMaxLength(200);

            // Identificación
            entity.Property(p => p.MLSNumber)
                .HasMaxLength(20);

            entity.Property(p => p.ParcelNumber)
                .HasMaxLength(50);

            entity.Property(p => p.PropertyId)
                .HasMaxLength(50);

            // Tipo de propiedad
            entity.Property(p => p.PropertyType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.PropertySubType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.OwnershipType)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Ubicación
            entity.Property(p => p.StreetAddress)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.UnitNumber)
                .HasMaxLength(50);

            entity.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.State)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(p => p.County)
                .HasMaxLength(100);

            entity.Property(p => p.Country)
                .HasMaxLength(100)
                .HasDefaultValue("USA");

            entity.Property(p => p.Neighborhood)
                .HasMaxLength(100);

            entity.Property(p => p.Subdivision)
                .HasMaxLength(100);

            // Estructura y tamaño
            entity.Property(p => p.GarageType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.ParkingType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.ArchitecturalStyle)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.FoundationType)
                .HasMaxLength(50);

            entity.Property(p => p.RoofType)
                .HasMaxLength(50);

            entity.Property(p => p.ConstructionType)
                .HasMaxLength(100);

            entity.Property(p => p.ExteriorType)
                .HasMaxLength(100);

            // Sistemas
            entity.Property(p => p.HeatingType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.CoolingType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.HeatingFuel)
                .HasMaxLength(50);

            entity.Property(p => p.WaterSource)
                .HasMaxLength(50);

            entity.Property(p => p.SewerType)
                .HasMaxLength(50);

            // Lot info
            entity.Property(p => p.LotSizeAcres)
                .HasPrecision(18, 4);

            // Pool and basement
            entity.Property(p => p.PoolType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(p => p.BasementType)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Financiero
            entity.Property(p => p.TaxesYearly)
                .HasPrecision(18, 2);

            entity.Property(p => p.HOAFeesMonthly)
                .HasPrecision(18, 2);

            entity.Property(p => p.HOAName)
                .HasMaxLength(200);

            entity.Property(p => p.AssessedValue)
                .HasPrecision(18, 2);

            entity.Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            entity.Property(p => p.PricePerSquareFoot)
                .HasPrecision(18, 2);

            // Escuelas
            entity.Property(p => p.SchoolDistrict)
                .HasMaxLength(200);

            entity.Property(p => p.ElementarySchool)
                .HasMaxLength(200);

            entity.Property(p => p.MiddleSchool)
                .HasMaxLength(200);

            entity.Property(p => p.HighSchool)
                .HasMaxLength(200);

            // Virtual tour
            entity.Property(p => p.VirtualTourUrl)
                .HasMaxLength(500);

            // JSON columns
            entity.Property(p => p.InteriorFeaturesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(p => p.ExteriorFeaturesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(p => p.AppliancesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(p => p.CommunityAmenitiesJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            // Timestamps
            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            // Índices
            entity.HasIndex(p => p.DealerId);
            entity.HasIndex(p => p.AgentId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.Price);
            entity.HasIndex(p => p.PropertyType);
            entity.HasIndex(p => p.PropertySubType);
            entity.HasIndex(p => p.Bedrooms);
            entity.HasIndex(p => p.Bathrooms);
            entity.HasIndex(p => p.SquareFeet);
            entity.HasIndex(p => p.YearBuilt);
            entity.HasIndex(p => p.State);
            entity.HasIndex(p => p.City);
            entity.HasIndex(p => p.ZipCode);
            entity.HasIndex(p => p.Neighborhood);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.IsFeatured);
            entity.HasIndex(p => p.IsDeleted);
            entity.HasIndex(p => p.MLSNumber).IsUnique();

            // Índices compuestos
            entity.HasIndex(p => new { p.State, p.City, p.ZipCode });
            entity.HasIndex(p => new { p.PropertyType, p.Bedrooms, p.Bathrooms });
            entity.HasIndex(p => new { p.Status, p.IsDeleted });
            entity.HasIndex(p => new { p.Price, p.SquareFeet });

            // Relaciones
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Properties)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Property)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // PROPERTY IMAGE CONFIGURATION
        // ========================================

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("property_images");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(i => i.ThumbnailUrl)
                .HasMaxLength(500);

            entity.Property(i => i.Title)
                .HasMaxLength(200);

            entity.Property(i => i.AltText)
                .HasMaxLength(500);

            entity.Property(i => i.MimeType)
                .HasMaxLength(50);

            entity.Property(i => i.ImageType)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(i => i.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(i => i.PropertyId);
            entity.HasIndex(i => i.DealerId);
            entity.HasIndex(i => new { i.PropertyId, i.SortOrder });
            entity.HasIndex(i => new { i.PropertyId, i.IsPrimary });
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

            entity.Property(c => c.ImageUrl)
                .HasMaxLength(500);

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.ParentId);
            entity.HasIndex(c => c.IsActive);
            entity.HasIndex(c => c.SortOrder);

            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedCategories(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var housesId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var condosId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var townhomesId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var landId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var commercialId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var multifamilyId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = housesId,
                DealerId = Guid.Empty,
                Name = "Houses",
                Slug = "houses-rent",
                Description = "Single family homes for rent",
                SortOrder = 1,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = condosId,
                DealerId = Guid.Empty,
                Name = "Condos & Apartments",
                Slug = "condos-rent",
                Description = "Condominiums and apartments for rent",
                SortOrder = 2,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = townhomesId,
                DealerId = Guid.Empty,
                Name = "Townhomes",
                Slug = "townhomes-rent",
                Description = "Townhouses and row homes for rent",
                SortOrder = 3,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = landId,
                DealerId = Guid.Empty,
                Name = "Land & Lots",
                Slug = "land-rent",
                Description = "Vacant land and lots for rent",
                SortOrder = 4,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = commercialId,
                DealerId = Guid.Empty,
                Name = "Commercial",
                Slug = "commercial-rent",
                Description = "Commercial properties for rent",
                SortOrder = 5,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = multifamilyId,
                DealerId = Guid.Empty,
                Name = "Multi-Family",
                Slug = "multifamily-rent",
                Description = "Multi-family investment properties for rent",
                SortOrder = 6,
                IsActive = true,
                IsSystem = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
            .Where(e => e.Entity is Property && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((Property)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
