// =====================================================
// VehicleRegistryService - DbContext
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using Microsoft.EntityFrameworkCore;
using VehicleRegistryService.Domain.Entities;

namespace VehicleRegistryService.Infrastructure.Persistence;

public class RegistryDbContext : DbContext
{
    public RegistryDbContext(DbContextOptions<RegistryDbContext> options) : base(options) { }

    public DbSet<VehicleRegistration> Registrations => Set<VehicleRegistration>();
    public DbSet<OwnershipTransfer> Transfers => Set<OwnershipTransfer>();
    public DbSet<LienRecord> Liens => Set<LienRecord>();
    public DbSet<VinValidation> VinValidations => Set<VinValidation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // VehicleRegistration
        modelBuilder.Entity<VehicleRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Vin).IsRequired().HasMaxLength(17);
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(30);
            entity.Property(e => e.OwnerIdentification).IsRequired().HasMaxLength(15);
            entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.VehicleType).HasConversion<string>();
            entity.Property(e => e.FiscalValue).HasPrecision(18, 2);
            entity.HasIndex(e => e.PlateNumber).IsUnique();
            entity.HasIndex(e => e.Vin).IsUnique();
            entity.HasIndex(e => e.OwnerIdentification);
        });

        // OwnershipTransfer
        modelBuilder.Entity<OwnershipTransfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreviousOwnerIdentification).IsRequired().HasMaxLength(15);
            entity.Property(e => e.NewOwnerIdentification).IsRequired().HasMaxLength(15);
            entity.Property(e => e.SalePrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.VehicleRegistration)
                  .WithMany(v => v.Transfers)
                  .HasForeignKey(e => e.VehicleRegistrationId);
        });

        // LienRecord
        modelBuilder.Entity<LienRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LienHolderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LienHolderRnc).IsRequired().HasMaxLength(15);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.VehicleRegistration)
                  .WithMany(v => v.Liens)
                  .HasForeignKey(e => e.VehicleRegistrationId);
        });

        // VinValidation
        modelBuilder.Entity<VinValidation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Vin).IsRequired().HasMaxLength(17);
            entity.HasIndex(e => e.Vin);
        });
    }
}
