using UserService.Domain.Entities;
using UserService.Domain.Entities.Privacy;
using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.Encryption;

namespace UserService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IFieldEncryptor? _fieldEncryptor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IFieldEncryptor fieldEncryptor) : base(options)
        {
            _fieldEncryptor = fieldEncryptor;
        }

        // Entidades principales
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<UserService.Domain.Entities.Role> Roles => Set<UserService.Domain.Entities.Role>();

        // Dealers y vendedores
        public DbSet<Dealer> Dealers => Set<Dealer>();
        public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
        public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();
        public DbSet<ContactPreferences> ContactPreferences => Set<ContactPreferences>();
        public DbSet<SellerBadgeAssignment> SellerBadgeAssignments => Set<SellerBadgeAssignment>();
        public DbSet<SellerConversion> SellerConversions => Set<SellerConversion>();

        // Entidades de empleados
        public DbSet<DealerEmployee> DealerEmployees => Set<DealerEmployee>();
        public DbSet<PlatformEmployee> PlatformEmployees => Set<PlatformEmployee>();
        public DbSet<DealerEmployeeInvitation> DealerEmployeeInvitations => Set<DealerEmployeeInvitation>();
        public DbSet<PlatformEmployeeInvitation> PlatformEmployeeInvitations => Set<PlatformEmployeeInvitation>();

        // Suscripciones de dealers
        public DbSet<DealerSubscription> DealerSubscriptions => Set<DealerSubscription>();
        public DbSet<SubscriptionHistory> SubscriptionHistory => Set<SubscriptionHistory>();

        // Módulos y suscripciones de módulos (multi-tenant)
        public DbSet<ModuleAddon> ModuleAddons => Set<ModuleAddon>();
        public DbSet<DealerModuleSubscription> DealerModuleSubscriptions => Set<DealerModuleSubscription>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<DealerModule> DealerModules => Set<DealerModule>();

        // Onboarding
        public DbSet<UserOnboarding> UserOnboardings => Set<UserOnboarding>();
        public DbSet<DealerOnboardingProcess> DealerOnboardingProcesses => Set<DealerOnboardingProcess>();
        public DbSet<DealerOnboarding> DealerOnboardings => Set<DealerOnboarding>();

        // Privacy (ARCO - Ley 172-13)
        public DbSet<PrivacyRequest> PrivacyRequests => Set<PrivacyRequest>();
        public DbSet<CommunicationPreference> CommunicationPreferences => Set<CommunicationPreference>();
        public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones desde este assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // ════════════════════════════════════════════════════════════════
            // PII ENCRYPTION AT REST — Ley 172-13 Compliance
            // Algorithm: AES-256-GCM (256-bit key, 96-bit nonce, 128-bit tag)
            // Key source: OKLA_PII_ENCRYPTION_KEY environment variable
            //
            // NOTE: Email is NOT encrypted here because it has a UNIQUE index
            // used for authentication lookups. Email encryption requires a
            // blind-index migration (Phase 12b).
            // ════════════════════════════════════════════════════════════════
            if (_fieldEncryptor != null)
            {
                var enc = _fieldEncryptor;

                // User — Phone numbers
                modelBuilder.Entity<User>().Property(u => u.PhoneNumber)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));
                modelBuilder.Entity<User>().Property(u => u.BusinessPhone)
                    .HasConversion(
                        v => v == null ? null : enc.Encrypt(v),
                        v => v == null ? null : enc.DecryptOrPassthrough(v));

                // IdentityDocument — Cédula (DocumentNumber)
                modelBuilder.Entity<IdentityDocument>().Property(d => d.DocumentNumber)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));

                // Dealer — Business phone
                modelBuilder.Entity<Dealer>().Property(d => d.Phone)
                    .HasConversion(
                        v => enc.Encrypt(v),
                        v => enc.DecryptOrPassthrough(v));
            }
        }
    }
}
