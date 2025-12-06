using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Entidades principales
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<UserService.Domain.Entities.Role> Roles => Set<UserService.Domain.Entities.Role>();

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones desde este assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
