using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class ReconciliationConfiguration : IEntityTypeConfiguration<Reconciliation>
{
    public void Configure(EntityTypeBuilder<Reconciliation> builder)
    {
        builder.ToTable("reconciliations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalDifference).HasPrecision(18, 2);
        builder.Property(x => x.BankOpeningBalance).HasPrecision(18, 2);
        builder.Property(x => x.BankClosingBalance).HasPrecision(18, 2);
        builder.Property(x => x.SystemOpeningBalance).HasPrecision(18, 2);
        builder.Property(x => x.SystemClosingBalance).HasPrecision(18, 2);
        builder.Property(x => x.BalanceDifference).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.Reconciliation)
            .HasForeignKey(x => x.ReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Discrepancies)
            .WithOne(x => x.Reconciliation)
            .HasForeignKey(x => x.ReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.BankStatementId);
        builder.HasIndex(x => x.ReconciliationDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.PeriodFrom, x.PeriodTo });
    }
}
