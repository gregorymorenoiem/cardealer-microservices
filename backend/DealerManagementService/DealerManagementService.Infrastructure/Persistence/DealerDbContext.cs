using DealerManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealerManagementService.Infrastructure.Persistence;

public class DealerDbContext : DbContext
{
    public DealerDbContext(DbContextOptions<DealerDbContext> options) : base(options)
    {
    }

    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<DealerDocument> DealerDocuments => Set<DealerDocument>();
    public DbSet<DealerLocation> DealerLocations => Set<DealerLocation>();
    public DbSet<BusinessHours> BusinessHours => Set<BusinessHours>(); // Sprint 7

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dealer Configuration
        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.ToTable("dealers");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.RNC).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.VerificationStatus);
            entity.HasIndex(e => e.CurrentPlan);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Slug).IsUnique(); // Sprint 7
            entity.HasIndex(e => e.IsTrustedDealer); // Sprint 7
            entity.HasIndex(e => e.IsFoundingMember); // Sprint 7
            
            entity.Property(e => e.BusinessName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RNC).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Province).HasMaxLength(100).IsRequired();
            
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Relationships
            entity.HasMany(e => e.Documents)
                .WithOne(d => d.Dealer)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Locations)
                .WithOne(l => l.Dealer)
                .HasForeignKey(l => l.DealerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DealerDocument Configuration
        modelBuilder.Entity<DealerDocument>(entity =>
        {
            entity.ToTable("dealer_documents");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.VerificationStatus);
            entity.HasIndex(e => e.UploadedAt);
            
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DealerLocation Configuration
        modelBuilder.Entity<DealerLocation>(entity =>
        {
            entity.ToTable("dealer_locations");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Province);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.IsPrimary);
            
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(300).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Province).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Sprint 7: BusinessHours Configuration
        modelBuilder.Entity<BusinessHours>(entity =>
        {
            entity.ToTable("business_hours");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.DealerLocationId);
            entity.HasIndex(e => e.DayOfWeek);
            
            entity.HasOne(e => e.DealerLocation)
                .WithMany(l => l.BusinessHours)
                .HasForeignKey(e => e.DealerLocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

