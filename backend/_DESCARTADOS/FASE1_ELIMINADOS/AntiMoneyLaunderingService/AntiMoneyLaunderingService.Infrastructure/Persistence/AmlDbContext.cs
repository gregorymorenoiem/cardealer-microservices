// =====================================================
// AntiMoneyLaunderingService - DbContext
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using Microsoft.EntityFrameworkCore;
using AntiMoneyLaunderingService.Domain.Entities;

namespace AntiMoneyLaunderingService.Infrastructure.Persistence;

public class AmlDbContext : DbContext
{
    public AmlDbContext(DbContextOptions<AmlDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<SuspiciousActivityReport> SuspiciousActivityReports => Set<SuspiciousActivityReport>();
    public DbSet<RosTransaction> RosTransactions => Set<RosTransaction>();
    public DbSet<AmlAlert> AmlAlerts => Set<AmlAlert>();
    public DbSet<KycDocument> KycDocuments => Set<KycDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IdentificationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IdentificationType).HasConversion<string>();
            entity.Property(e => e.RiskLevel).HasConversion<string>();
            entity.Property(e => e.KycStatus).HasConversion<string>();
            entity.Property(e => e.PepCategory).HasConversion<string>();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.IdentificationNumber);
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionReference).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(e => e.CustomerId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.TransactionDate);
        });

        // SuspiciousActivityReport
        modelBuilder.Entity<SuspiciousActivityReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReportType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.TransactionAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId);
            entity.HasIndex(e => e.ReportNumber).IsUnique();
        });

        // RosTransaction
        modelBuilder.Entity<RosTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.SuspiciousActivityReport)
                  .WithMany(r => r.RelatedTransactions)
                  .HasForeignKey(e => e.SuspiciousActivityReportId);
        });

        // AmlAlert
        modelBuilder.Entity<AmlAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).HasConversion<string>();
            entity.Property(e => e.Severity).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Alerts)
                  .HasForeignKey(e => e.CustomerId);
            entity.HasIndex(e => e.CustomerId);
        });

        // KycDocument
        modelBuilder.Entity<KycDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.KycDocuments)
                  .HasForeignKey(e => e.CustomerId);
        });
    }
}
