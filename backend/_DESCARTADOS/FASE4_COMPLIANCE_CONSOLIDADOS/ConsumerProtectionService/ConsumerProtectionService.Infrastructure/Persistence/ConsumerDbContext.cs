// =====================================================
// ConsumerProtectionService - DbContext
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using Microsoft.EntityFrameworkCore;
using ConsumerProtectionService.Domain.Entities;

namespace ConsumerProtectionService.Infrastructure.Persistence;

public class ConsumerDbContext : DbContext
{
    public ConsumerDbContext(DbContextOptions<ConsumerDbContext> options) : base(options) { }

    public DbSet<Warranty> Warranties => Set<Warranty>();
    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintEvidence> ComplaintEvidences => Set<ComplaintEvidence>();
    public DbSet<Mediation> Mediations => Set<Mediation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Warranty
        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WarrantyNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WarrantyType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.HasIndex(e => e.WarrantyNumber).IsUnique();
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ConsumerId);
        });

        // WarrantyClaim
        modelBuilder.Entity<WarrantyClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Resolution).HasConversion<string>();
            entity.HasOne(e => e.Warranty)
                  .WithMany(w => w.Claims)
                  .HasForeignKey(e => e.WarrantyId);
            entity.HasIndex(e => e.ClaimNumber).IsUnique();
        });

        // Complaint
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ComplaintNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ComplaintType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.HasIndex(e => e.ComplaintNumber).IsUnique();
            entity.HasIndex(e => e.ConsumerId);
            entity.HasIndex(e => e.SellerId);
        });

        // ComplaintEvidence
        modelBuilder.Entity<ComplaintEvidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Complaint)
                  .WithMany(c => c.Evidences)
                  .HasForeignKey(e => e.ComplaintId);
        });

        // Mediation
        modelBuilder.Entity<Mediation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Outcome).HasConversion<string>();
            entity.HasOne(e => e.Complaint)
                  .WithMany(c => c.Mediations)
                  .HasForeignKey(e => e.ComplaintId);
        });
    }
}
