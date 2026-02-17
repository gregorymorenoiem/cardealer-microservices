using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("bank_statements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BankCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountName).HasMaxLength(200);
        builder.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        builder.Property(x => x.ClosingBalance).HasPrecision(18, 2);
        builder.Property(x => x.TotalDebits).HasPrecision(18, 2);
        builder.Property(x => x.TotalCredits).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ImportSource).HasMaxLength(50);
        builder.Property(x => x.ApiTransactionId).HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.BankStatement)
            .HasForeignKey(x => x.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reconciliations)
            .WithOne(x => x.BankStatement)
            .HasForeignKey(x => x.BankStatementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BankCode);
        builder.HasIndex(x => x.AccountNumber);
        builder.HasIndex(x => new { x.BankCode, x.PeriodFrom, x.PeriodTo });
    }
}
