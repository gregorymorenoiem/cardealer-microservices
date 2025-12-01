using ConfigurationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Infrastructure.Data;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
    public DbSet<EncryptedSecret> EncryptedSecrets { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<ConfigurationHistory> ConfigurationHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConfigurationItem>(entity =>
        {
            entity.ToTable("configuration_items");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Key, e.Environment, e.TenantId }).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<EncryptedSecret>(entity =>
        {
            entity.ToTable("encrypted_secrets");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Key, e.Environment, e.TenantId }).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EncryptedValue).IsRequired();
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.ToTable("feature_flags");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Key, e.Environment, e.TenantId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ConfigurationHistory>(entity =>
        {
            entity.ToTable("configuration_histories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConfigurationItemId);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ChangedBy).IsRequired().HasMaxLength(200);
        });
    }
}
