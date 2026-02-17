using Microsoft.EntityFrameworkCore;
using ContactService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;

namespace ContactService.Infrastructure.Persistence
{
    public class ApplicationDbContext : MultiTenantDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
            : base(options, tenantContext)
        {
        }

        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ContactRequest configuration
            modelBuilder.Entity<ContactRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BuyerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BuyerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BuyerPhone).HasMaxLength(20);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Open");
                
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.BuyerId);
                entity.HasIndex(e => e.SellerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // ContactMessage configuration
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                
                entity.HasOne(e => e.ContactRequest)
                      .WithMany(e => e.Messages)
                      .HasForeignKey(e => e.ContactRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.ContactRequestId);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.IsRead);
            });
        }

        // ✅ AUDIT FIX: Auto-update timestamps on all entities
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<ContactRequest>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedAt == default)
                        entry.Entity.CreatedAt = utcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
