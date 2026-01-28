using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("bank_statement_lines");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DebitAmount).HasPrecision(18, 2);
        builder.Property(x => x.CreditAmount).HasPrecision(18, 2);
        builder.Property(x => x.Balance).HasPrecision(18, 2);
        builder.Property(x => x.BankCategory).HasMaxLength(100);
        builder.Property(x => x.Beneficiary).HasMaxLength(200);
        builder.Property(x => x.OriginAccount).HasMaxLength(50);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.BankLine)
            .HasForeignKey(x => x.BankStatementLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BankStatementId);
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.ReferenceNumber);
        builder.HasIndex(x => x.IsReconciled);
    }
}
