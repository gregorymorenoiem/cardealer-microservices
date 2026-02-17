using Microsoft.EntityFrameworkCore;
using RealEstateService.Domain.Entities;

namespace RealEstateService.Infrastructure.Data;

/// <summary>
/// DbContext para RealEstateService
/// </summary>
public class RealEstateDbContext : DbContext
{
    private readonly Guid? _currentDealerId;

    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : base(options)
    {
    }

    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options, Guid? currentDealerId)
        : base(options)
    {
        _currentDealerId = currentDealerId;
    }

    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<PropertyAmenity> PropertyAmenities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Property configuration
        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.ListingType).HasColumnName("listing_type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.PricePerSqMeter).HasColumnName("price_per_sq_meter").HasPrecision(18, 2);
            entity.Property(e => e.MaintenanceFee).HasColumnName("maintenance_fee").HasPrecision(18, 2);
            entity.Property(e => e.IsNegotiable).HasColumnName("is_negotiable");
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(300);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasColumnName("zip_code").HasMaxLength(20);
            entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100);
            entity.Property(e => e.Neighborhood).HasColumnName("neighborhood").HasMaxLength(100);
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.TotalArea).HasColumnName("total_area").HasPrecision(18, 2);
            entity.Property(e => e.BuiltArea).HasColumnName("built_area").HasPrecision(18, 2);
            entity.Property(e => e.LotArea).HasColumnName("lot_area").HasPrecision(18, 2);
            entity.Property(e => e.Bedrooms).HasColumnName("bedrooms");
            entity.Property(e => e.Bathrooms).HasColumnName("bathrooms");
            entity.Property(e => e.HalfBathrooms).HasColumnName("half_bathrooms");
            entity.Property(e => e.ParkingSpaces).HasColumnName("parking_spaces");
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.TotalFloors).HasColumnName("total_floors");
            entity.Property(e => e.YearBuilt).HasColumnName("year_built");
            entity.Property(e => e.HasGarden).HasColumnName("has_garden");
            entity.Property(e => e.HasPool).HasColumnName("has_pool");
            entity.Property(e => e.HasGym).HasColumnName("has_gym");
            entity.Property(e => e.HasElevator).HasColumnName("has_elevator");
            entity.Property(e => e.HasSecurity).HasColumnName("has_security");
            entity.Property(e => e.IsFurnished).HasColumnName("is_furnished");
            entity.Property(e => e.AllowsPets).HasColumnName("allows_pets");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.SellerName).HasColumnName("seller_name").HasMaxLength(200);
            entity.Property(e => e.SellerPhone).HasColumnName("seller_phone").HasMaxLength(50);
            entity.Property(e => e.SellerEmail).HasColumnName("seller_email").HasMaxLength(200);
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.FavoriteCount).HasColumnName("favorite_count");
            entity.Property(e => e.InquiryCount).HasColumnName("inquiry_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            // Indexes
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ListingType);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.IsFeatured);

            // Global filter for soft delete and multi-tenant
            entity.HasQueryFilter(e => !e.IsDeleted);
            if (_currentDealerId.HasValue)
            {
                entity.HasQueryFilter(e => !e.IsDeleted && e.DealerId == _currentDealerId.Value);
            }

            // Relationships
            entity.HasMany(e => e.Images)
                .WithOne(i => i.Property)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Amenities)
                .WithOne(a => a.Property)
                .HasForeignKey(a => a.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PropertyImage configuration
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("property_images");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);
            entity.Property(e => e.Caption).HasColumnName("caption").HasMaxLength(200);
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.PropertyId);
        });

        // PropertyAmenity configuration
        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.ToTable("property_amenities");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.Icon).HasColumnName("icon").HasMaxLength(50);

            entity.HasIndex(e => e.PropertyId);
        });
    }
}
