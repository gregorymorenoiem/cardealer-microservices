// =====================================================
// TaxComplianceService - DbContext
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using Microsoft.EntityFrameworkCore;
using TaxComplianceService.Domain.Entities;

namespace TaxComplianceService.Infrastructure.Persistence;

public class TaxDbContext : DbContext
{
    public TaxDbContext(DbContextOptions<TaxDbContext> options) : base(options) { }

    public DbSet<Taxpayer> Taxpayers => Set<Taxpayer>();
    public DbSet<TaxDeclaration> TaxDeclarations => Set<TaxDeclaration>();
    public DbSet<TaxPayment> TaxPayments => Set<TaxPayment>();
    public DbSet<NcfSequence> NcfSequences => Set<NcfSequence>();
    public DbSet<Reporte606Item> Reporte606Items => Set<Reporte606Item>();
    public DbSet<Reporte607Item> Reporte607Items => Set<Reporte607Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Taxpayer
        modelBuilder.Entity<Taxpayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rnc).IsRequired().HasMaxLength(11);
            entity.Property(e => e.BusinessName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TaxpayerType).HasConversion<string>();
            entity.HasIndex(e => e.Rnc).IsUnique();
        });

        // TaxDeclaration
        modelBuilder.Entity<TaxDeclaration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rnc).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(6);
            entity.Property(e => e.DeclarationType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.GrossAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxableAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.WithholdingAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetPayable).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.TaxpayerId, e.Period, e.DeclarationType });
        });

        // TaxPayment
        modelBuilder.Entity<TaxPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.TaxDeclaration)
                  .WithMany(d => d.Payments)
                  .HasForeignKey(e => e.TaxDeclarationId);
        });

        // NcfSequence
        modelBuilder.Entity<NcfSequence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Serie).IsRequired().HasMaxLength(2);
            entity.Property(e => e.NcfType).HasConversion<string>();
            entity.HasOne(e => e.Taxpayer)
                  .WithMany(t => t.NcfSequences)
                  .HasForeignKey(e => e.TaxpayerId);
            entity.HasIndex(e => new { e.TaxpayerId, e.NcfType, e.IsActive });
        });

        // Reporte606Item
        modelBuilder.Entity<Reporte606Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RncCedula).IsRequired().HasMaxLength(11);
            entity.Property(e => e.NcfNumber).IsRequired().HasMaxLength(19);
            entity.Property(e => e.IdentificationType).HasConversion<string>();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.ItbisAmount).HasPrecision(18, 2);
            entity.Property(e => e.ItbisRetenido).HasPrecision(18, 2);
            entity.HasOne(e => e.TaxDeclaration)
                  .WithMany(d => d.Reporte606Items)
                  .HasForeignKey(e => e.TaxDeclarationId);
        });

        // Reporte607Item
        modelBuilder.Entity<Reporte607Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RncCedula).IsRequired().HasMaxLength(11);
            entity.Property(e => e.NcfNumber).IsRequired().HasMaxLength(19);
            entity.Property(e => e.IdentificationType).HasConversion<string>();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.ItbisAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.TaxDeclaration)
                  .WithMany(d => d.Reporte607Items)
                  .HasForeignKey(e => e.TaxDeclarationId);
        });
    }
}
