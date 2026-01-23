using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthService.Domain.Entities;
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
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ApplicationUser && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var user = (ApplicationUser)entityEntry.Entity;
            user.MarkAsUpdated();
        }
    }
}
