using Microsoft.EntityFrameworkCore;
using ComparisonService.Domain.Entities;

namespace ComparisonService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comparison> Comparisons => Set<Comparison>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comparison>(entity =>
        {
            entity.ToTable("comparisons");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            
            // Store VehicleIds as JSON array
            entity.Property(c => c.VehicleIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .HasColumnType("jsonb");

            entity.Property(c => c.ShareToken).HasMaxLength(50);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).IsRequired();
            entity.Property(c => c.IsPublic).IsRequired();

            // Indexes
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.ShareToken).IsUnique();
            entity.HasIndex(c => c.CreatedAt);
        });
    }
}
