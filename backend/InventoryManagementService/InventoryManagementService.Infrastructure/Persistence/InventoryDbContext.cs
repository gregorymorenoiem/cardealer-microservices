using Microsoft.EntityFrameworkCore;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
    public DbSet<BulkImportJob> BulkImportJobs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // InventoryItem configuration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("inventory_items");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id").IsRequired();
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.Visibility).HasColumnName("visibility").IsRequired();
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes").HasMaxLength(2000);
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(200);
            entity.Property(e => e.StockNumber).HasColumnName("stock_number");
            entity.Property(e => e.VIN).HasColumnName("vin").HasMaxLength(17);
            entity.Property(e => e.CostPrice).HasColumnName("cost_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.ListPrice).HasColumnName("list_price").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TargetPrice).HasColumnName("target_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinAcceptablePrice).HasColumnName("min_acceptable_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsNegotiable).HasColumnName("is_negotiable").HasDefaultValue(true);
            entity.Property(e => e.AcquiredDate).HasColumnName("acquired_date");
            entity.Property(e => e.AcquisitionSource).HasColumnName("acquisition_source");
            entity.Property(e => e.AcquisitionDetails).HasColumnName("acquisition_details").HasMaxLength(1000);
            entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
            entity.Property(e => e.InquiryCount).HasColumnName("inquiry_count").HasDefaultValue(0);
            entity.Property(e => e.TestDriveCount).HasColumnName("test_drive_count").HasDefaultValue(0);
            entity.Property(e => e.OfferCount).HasColumnName("offer_count").HasDefaultValue(0);
            entity.Property(e => e.HighestOffer).HasColumnName("highest_offer").HasColumnType("decimal(18,2)");
            entity.Property(e => e.LastViewedAt).HasColumnName("last_viewed_at");
            entity.Property(e => e.LastInquiryAt).HasColumnName("last_inquiry_at");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
            entity.Property(e => e.FeaturedUntil).HasColumnName("featured_until");
            entity.Property(e => e.Priority).HasColumnName("priority").HasDefaultValue(0);
            entity.Property(e => e.Tags).HasColumnName("tags").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.SoldAt).HasColumnName("sold_at");
            entity.Property(e => e.SoldPrice).HasColumnName("sold_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.SoldTo).HasColumnName("sold_to").HasMaxLength(200);

            entity.HasIndex(e => e.DealerId).HasDatabaseName("ix_inventory_items_dealer_id");
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_inventory_items_vehicle_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_inventory_items_status");
            entity.HasIndex(e => new { e.DealerId, e.Status }).HasDatabaseName("ix_inventory_items_dealer_status");
        });

        // BulkImportJob configuration
        modelBuilder.Entity<BulkImportJob>(entity =>
        {
            entity.ToTable("bulk_import_jobs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(1000).IsRequired();
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FileType).HasColumnName("file_type").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.TotalRows).HasColumnName("total_rows").HasDefaultValue(0);
            entity.Property(e => e.ProcessedRows).HasColumnName("processed_rows").HasDefaultValue(0);
            entity.Property(e => e.SuccessfulRows).HasColumnName("successful_rows").HasDefaultValue(0);
            entity.Property(e => e.FailedRows).HasColumnName("failed_rows").HasDefaultValue(0);
            entity.Property(e => e.SkippedRows).HasColumnName("skipped_rows").HasDefaultValue(0);
            entity.Property(e => e.Errors).HasColumnName("errors").HasColumnType("jsonb");
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason").HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            entity.HasIndex(e => e.DealerId).HasDatabaseName("ix_bulk_import_jobs_dealer_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_bulk_import_jobs_status");
        });
    }
}
