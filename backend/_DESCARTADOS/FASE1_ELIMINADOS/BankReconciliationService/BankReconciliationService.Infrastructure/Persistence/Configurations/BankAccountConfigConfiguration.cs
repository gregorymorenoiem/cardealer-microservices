using BankReconciliationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankReconciliationService.Infrastructure.Persistence.Configurations;

public class BankAccountConfigConfiguration : IEntityTypeConfiguration<BankAccountConfig>
{
    public void Configure(EntityTypeBuilder<BankAccountConfig> builder)
    {
        builder.ToTable("bank_account_configs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BankCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountName).HasMaxLength(200);
        builder.Property(x => x.AccountType).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("DOP");
        builder.Property(x => x.ApiClientId).HasMaxLength(200);
        builder.Property(x => x.ApiClientSecretEncrypted).HasMaxLength(500);
        builder.Property(x => x.ApiBaseUrl).HasMaxLength(500);
        builder.Property(x => x.ChartOfAccountsCode).HasMaxLength(50);
        builder.Property(x => x.AutoMatchThresholdAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.BankCode);
        builder.HasIndex(x => x.AccountNumber).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}
