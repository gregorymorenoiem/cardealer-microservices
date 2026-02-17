using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class InternalTransactionConfiguration : IEntityTypeConfiguration<InternalTransaction>
{
    public void Configure(EntityTypeBuilder<InternalTransaction> builder)
    {
        builder.ToTable("internal_transactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("DOP");
        builder.Property(x => x.SourceService).HasMaxLength(100);
        builder.Property(x => x.PaymentGateway).HasMaxLength(50);
        builder.Property(x => x.GatewayTransactionId).HasMaxLength(100);
        builder.Property(x => x.CustomerName).HasMaxLength(200);
        builder.Property(x => x.AccountCode).HasMaxLength(50);

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.InternalTransaction)
            .HasForeignKey(x => x.InternalTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.ReferenceNumber);
        builder.HasIndex(x => x.IsReconciled);
        builder.HasIndex(x => x.SourceService);
        builder.HasIndex(x => x.PaymentGateway);
    }
}
