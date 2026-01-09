using Microsoft.EntityFrameworkCore;
using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Infrastructure.Persistence;

public class VehicleIntelligenceDbContext(DbContextOptions<VehicleIntelligenceDbContext> options) : DbContext(options)
{
    public DbSet<PriceSuggestionRecord> PriceSuggestions => Set<PriceSuggestionRecord>();
    public DbSet<CategoryDemandSnapshot> CategoryDemandSnapshots => Set<CategoryDemandSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceSuggestionRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Make).HasMaxLength(64);
            e.Property(x => x.Model).HasMaxLength(64);
            e.Property(x => x.BodyType).HasMaxLength(64);
            e.Property(x => x.Location).HasMaxLength(128);
            e.Property(x => x.ModelVersion).HasMaxLength(64);
        });

        modelBuilder.Entity<CategoryDemandSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasMaxLength(64);
            e.Property(x => x.Trend).HasMaxLength(16);
            e.HasIndex(x => x.Category).IsUnique();
        });
    }
}
