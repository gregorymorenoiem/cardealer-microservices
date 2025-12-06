using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using MarketingService.Domain.Entities;

namespace MarketingService.Infrastructure.Persistence;

public class MarketingDbContext : MultiTenantDbContext
{
    public MarketingDbContext(DbContextOptions<MarketingDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Audience> Audiences => Set<Audience>();
    public DbSet<AudienceMember> AudienceMembers => Set<AudienceMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            entity.Property(e => e.SpentAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.DealerId, e.Status });
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.FromName).HasMaxLength(100);
            entity.Property(e => e.FromEmail).HasMaxLength(200);
            entity.Property(e => e.ReplyToEmail).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.HasIndex(e => new { e.DealerId, e.Type });
        });

        modelBuilder.Entity<Audience>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasMany(e => e.Members)
                .WithOne()
                .HasForeignKey(m => m.AudienceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AudienceMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.HasIndex(e => new { e.AudienceId, e.Email }).IsUnique();
        });
    }
}
