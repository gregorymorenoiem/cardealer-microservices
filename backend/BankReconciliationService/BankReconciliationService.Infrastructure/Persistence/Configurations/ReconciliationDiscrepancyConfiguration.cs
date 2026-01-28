using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class ReconciliationDiscrepancyConfiguration : IEntityTypeConfiguration<ReconciliationDiscrepancy>
{
    public void Configure(EntityTypeBuilder<ReconciliationDiscrepancy> builder)
    {
        builder.ToTable("reconciliation_discrepancies");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ResolutionNotes).HasMaxLength(1000);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReconciliationId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
    }
}
