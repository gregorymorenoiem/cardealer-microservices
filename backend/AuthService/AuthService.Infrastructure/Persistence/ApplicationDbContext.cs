using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Domain.Common;
using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Persistence.EntityConfigurations;

namespace AuthService.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<VerificationToken> VerificationTokens { get; set; } = null!;
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<LoginHistory> LoginHistories { get; set; } = null!;
    public DbSet<TrustedDevice> TrustedDevices { get; set; } = null!; // US-18.4

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
        builder.ApplyConfiguration(new VerificationTokenConfiguration());
        builder.ApplyConfiguration(new TwoFactorAuthConfiguration());
        builder.ApplyConfiguration(new UserSessionConfiguration());
        builder.ApplyConfiguration(new LoginHistoryConfiguration());
        builder.ApplyConfiguration(new TrustedDeviceConfiguration()); // US-18.4

        // Soft delete query filter for ApplicationUser
        builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<ApplicationUser>().HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted")
            .HasFilter("\"IsDeleted\" = false");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Handle ApplicationUser timestamps and soft delete
            if (entry.Entity is ApplicationUser user)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        user.CreatedAt = now;
                        user.UpdatedAt = now;
                        break;
                    case EntityState.Modified:
                        user.UpdatedAt = now;
                        break;
                    case EntityState.Deleted:
                        // Convert hard delete to soft delete
                        entry.State = EntityState.Modified;
                        user.IsDeleted = true;
                        user.DeletedAt = now;
                        user.UpdatedAt = now;
                        break;
                }
            }
            // Handle EntityBase-derived entities (RefreshToken, etc.)
            else if (entry.Entity is EntityBase entityBase)
            {
                if (entry.State == EntityState.Modified)
                {
                    entityBase.MarkAsUpdated();
                }
            }
            // Handle VerificationToken timestamps
            else if (entry.Entity is VerificationToken vt && entry.State == EntityState.Added)
            {
                vt.CreatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
