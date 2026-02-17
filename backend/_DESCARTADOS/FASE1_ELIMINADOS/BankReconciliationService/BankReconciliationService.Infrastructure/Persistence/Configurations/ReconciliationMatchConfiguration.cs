using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class ReconciliationMatchConfiguration : IEntityTypeConfiguration<ReconciliationMatch>
{
    public void Configure(EntityTypeBuilder<ReconciliationMatch> builder)
    {
        builder.ToTable("reconciliation_matches");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MatchConfidence).HasPrecision(5, 4);
        builder.Property(x => x.AmountDifference).HasPrecision(18, 2);
        builder.Property(x => x.MatchReason).HasMaxLength(500);
        builder.Property(x => x.AdjustmentAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdjustmentReason).HasMaxLength(500);

        builder.Property(x => x.MatchType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReconciliationId);
        builder.HasIndex(x => x.BankStatementLineId);
        builder.HasIndex(x => x.InternalTransactionId);
        builder.HasIndex(x => x.MatchedAt);
    }
}
