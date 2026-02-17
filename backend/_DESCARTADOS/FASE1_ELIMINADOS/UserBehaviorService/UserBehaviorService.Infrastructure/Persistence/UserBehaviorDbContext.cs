using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Infrastructure.Persistence;

public class UserBehaviorDbContext : DbContext
{
    public UserBehaviorDbContext(DbContextOptions<UserBehaviorDbContext> options) : base(options) { }

    public DbSet<UserBehaviorProfile> UserBehaviorProfiles { get; set; }
    public DbSet<UserAction> UserActions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserBehaviorProfile
        modelBuilder.Entity<UserBehaviorProfile>(entity =>
        {
            entity.ToTable("user_behavior_profiles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.UserSegment);
            entity.HasIndex(e => e.LastActivityAt);

            entity.Property(e => e.PreferredMakes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredModels).HasColumnType("jsonb");
            entity.Property(e => e.PreferredYears).HasColumnType("jsonb");
            entity.Property(e => e.PreferredBodyTypes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredFuelTypes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredTransmissions).HasColumnType("jsonb");
            entity.Property(e => e.RecentSearchQueries).HasColumnType("jsonb");
            entity.Property(e => e.RecentVehicleViews).HasColumnType("jsonb");
        });

        // UserAction
        modelBuilder.Entity<UserAction>(entity =>
        {
            entity.ToTable("user_actions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ActionType);
        });
    }
}
