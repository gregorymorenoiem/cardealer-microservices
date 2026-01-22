// =====================================================
// ComplianceReportingService - DbContext
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using Microsoft.EntityFrameworkCore;
using ComplianceReportingService.Domain.Entities;

namespace ComplianceReportingService.Infrastructure.Persistence;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

    public DbSet<ComplianceReport> ComplianceReports => Set<ComplianceReport>();
    public DbSet<ReportItem> ReportItems => Set<ReportItem>();
    public DbSet<ReportSubmission> ReportSubmissions => Set<ReportSubmission>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ComplianceReport
        modelBuilder.Entity<ComplianceReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.ReportNumber).IsUnique();
            entity.HasIndex(e => new { e.RegulatoryBody, e.Period });
            entity.HasIndex(e => e.Status);

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.ComplianceReport)
                  .HasForeignKey(i => i.ComplianceReportId);

            entity.HasMany(e => e.Submissions)
                  .WithOne(s => s.ComplianceReport)
                  .HasForeignKey(s => s.ComplianceReportId);
        });

        // ReportItem
        modelBuilder.Entity<ReportItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ComplianceReportId);
        });

        // ReportSubmission
        modelBuilder.Entity<ReportSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ComplianceReportId);
            entity.HasIndex(e => e.SubmissionReference);
        });

        // ReportSchedule
        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.ReportType, e.RegulatoryBody });
        });

        // ReportTemplate
        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ReportType, e.RegulatoryBody, e.IsActive });
        });
    }
}
