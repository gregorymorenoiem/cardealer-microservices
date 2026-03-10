using Microsoft.EntityFrameworkCore;
using ContactService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Encryption;

namespace ContactService.Infrastructure.Persistence
{
    public class ApplicationDbContext : MultiTenantDbContext
    {
        private readonly IFieldEncryptor? _encryptor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
            : base(options, tenantContext)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext, IFieldEncryptor encryptor)
            : base(options, tenantContext)
        {
            _encryptor = encryptor;
        }

        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // ═══════════════════════════════════════════════════════════
            // PII ENCRYPTION AT REST — Ley 172-13 Compliance
            // AES-256-GCM via ValueConverter. DecryptOrPassthrough enables
            // zero-downtime migration from plaintext → encrypted.
            // ═══════════════════════════════════════════════════════════
            if (_encryptor != null)
            {
                var enc = _encryptor;

                // ContactRequest — buyer/seller PII
                modelBuilder.Entity<ContactRequest>().Property(c => c.BuyerName)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.BuyerEmail)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.BuyerPhone)
                    .HasConversion(
                        v => v == null ? null : enc.Encrypt(v),
                        v => v == null ? null : enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.Name)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.Email)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.Phone)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                modelBuilder.Entity<ContactRequest>().Property(c => c.Message)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                // ContactMessage — message body (PII content)
                modelBuilder.Entity<ContactMessage>().Property(m => m.Message)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));
            }
        }
    }
}
