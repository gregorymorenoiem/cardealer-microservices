using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;

namespace ProductService.Infrastructure.Persistence;

public class ApplicationDbContext : MultiTenantDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductCustomField> ProductCustomFields => Set<ProductCustomField>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // PRODUCT CONFIGURATION
        // ========================================

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(5000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            entity.Property(p => p.SellerName)
                .HasMaxLength(200);

            entity.Property(p => p.CategoryName)
                .HasMaxLength(100);

            // JSON column para campos personalizados
            entity.Property(p => p.CustomFieldsJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            // Índices
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.SellerId);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.Price);
            entity.HasIndex(p => p.CreatedAt);

            // Relaciones
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.CustomFields)
                .WithOne(cf => cf.Product)
                .HasForeignKey(cf => cf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // PRODUCT IMAGE CONFIGURATION
        // ========================================

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(i => i.ThumbnailUrl)
                .HasMaxLength(500);

            entity.HasIndex(i => i.ProductId);
            entity.HasIndex(i => new { i.ProductId, i.SortOrder });
        });

        // ========================================
        // PRODUCT CUSTOM FIELD CONFIGURATION
        // ========================================

        modelBuilder.Entity<ProductCustomField>(entity =>
        {
            entity.ToTable("product_custom_fields");
            entity.HasKey(cf => cf.Id);

            entity.Property(cf => cf.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(cf => cf.Value)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(cf => cf.DataType)
                .HasMaxLength(20)
                .HasDefaultValue("string");

            entity.Property(cf => cf.Unit)
                .HasMaxLength(50);

            entity.HasIndex(cf => cf.ProductId);
            entity.HasIndex(cf => cf.Key);
            entity.HasIndex(cf => new { cf.ProductId, cf.Key });
            entity.HasIndex(cf => cf.IsSearchable);
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

            entity.Property(c => c.CustomFieldsSchemaJson)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.ParentId);
            entity.HasIndex(c => c.IsActive);

            // Auto-referencia para jerarquía
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // SEED DATA
        // ========================================

        SeedCategories(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var vehiclesId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var realEstateId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var electronicsId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = vehiclesId,
                Name = "Vehículos",
                Slug = "vehiculos",
                Description = "Autos, motos y vehículos comerciales",
                Level = 0,
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CustomFieldsSchemaJson = @"[
                    {""key"":""make"",""label"":""Marca"",""type"":""string"",""required"":true},
                    {""key"":""model"",""label"":""Modelo"",""type"":""string"",""required"":true},
                    {""key"":""year"",""label"":""Año"",""type"":""number"",""required"":true},
                    {""key"":""mileage"",""label"":""Kilometraje"",""type"":""number"",""unit"":""km""},
                    {""key"":""transmission"",""label"":""Transmisión"",""type"":""string""},
                    {""key"":""fuelType"",""label"":""Combustible"",""type"":""string""},
                    {""key"":""color"",""label"":""Color"",""type"":""string""}
                ]"
            },
            new Category
            {
                Id = realEstateId,
                Name = "Inmuebles",
                Slug = "inmuebles",
                Description = "Casas, departamentos y terrenos",
                Level = 0,
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CustomFieldsSchemaJson = @"[
                    {""key"":""bedrooms"",""label"":""Habitaciones"",""type"":""number"",""required"":true},
                    {""key"":""bathrooms"",""label"":""Baños"",""type"":""number"",""required"":true},
                    {""key"":""sqft"",""label"":""Área"",""type"":""number"",""unit"":""m²""},
                    {""key"":""parking"",""label"":""Estacionamiento"",""type"":""boolean""},
                    {""key"":""furnished"",""label"":""Amueblado"",""type"":""boolean""}
                ]"
            },
            new Category
            {
                Id = electronicsId,
                Name = "Electrónicos",
                Slug = "electronicos",
                Description = "Computadoras, celulares y gadgets",
                Level = 0,
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CustomFieldsSchemaJson = @"[
                    {""key"":""brand"",""label"":""Marca"",""type"":""string"",""required"":true},
                    {""key"":""condition"",""label"":""Condición"",""type"":""string""},
                    {""key"":""warranty"",""label"":""Garantía"",""type"":""boolean""}
                ]"
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
            .Where(e => e.Entity is Product && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((Product)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
