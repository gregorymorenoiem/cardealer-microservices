using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using IntegrationService.Domain.Entities;

namespace IntegrationService.Infrastructure.Persistence;

public class IntegrationDbContext : MultiTenantDbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Integration> Integrations => Set<Integration>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Integration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.ApiSecret).HasMaxLength(500);
            entity.Property(e => e.AccessToken).HasMaxLength(2000);
            entity.Property(e => e.RefreshToken).HasMaxLength(2000);
            entity.Property(e => e.WebhookUrl).HasMaxLength(500);
            entity.Property(e => e.WebhookSecret).HasMaxLength(200);
            entity.Property(e => e.LastSyncStatus).HasMaxLength(50);
            entity.Property(e => e.LastError).HasMaxLength(2000);
        });

        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.IntegrationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.NextRetryAt });
            entity.Property(e => e.EventName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(e => e.Integration)
                .WithMany()
                .HasForeignKey(e => e.IntegrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SyncJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.IntegrationId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Integration)
                .WithMany()
                .HasForeignKey(e => e.IntegrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
