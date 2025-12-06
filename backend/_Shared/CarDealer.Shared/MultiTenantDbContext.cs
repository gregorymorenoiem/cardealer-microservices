using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarDealer.Shared.MultiTenancy;

public abstract class MultiTenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected MultiTenantDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected Guid? CurrentDealerId => _tenantContext.CurrentDealerId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Solo aplicar filtro a entidades que no tengan un tipo base que también sea tenant entity
            // Esto evita aplicar filtros duplicados en jerarquías TPH

            // Check for required tenant (ITenantEntity)
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var baseType = entityType.BaseType;
                if (baseType != null && typeof(ITenantEntity).IsAssignableFrom(baseType.ClrType))
                {
                    // Skip derived types - the filter is already applied to the base type
                    continue;
                }

                var method = typeof(MultiTenantDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder, this });
            }
            // Check for optional tenant (IOptionalTenantEntity)
            else if (typeof(IOptionalTenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var baseType = entityType.BaseType;
                if (baseType != null && typeof(IOptionalTenantEntity).IsAssignableFrom(baseType.ClrType))
                {
                    // Skip derived types
                    continue;
                }

                var method = typeof(MultiTenantDbContext)
                    .GetMethod(nameof(ApplyOptionalTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder, this });
            }
        }
    }

    private static void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder, MultiTenantDbContext context)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => context.CurrentDealerId == null || e.DealerId == context.CurrentDealerId);
        modelBuilder.Entity<TEntity>()
            .HasIndex(e => e.DealerId)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_DealerId");
    }

    private static void ApplyOptionalTenantFilter<TEntity>(ModelBuilder modelBuilder, MultiTenantDbContext context)
        where TEntity : class, IOptionalTenantEntity
    {
        // Para entidades con DealerId opcional:
        // - Si no hay contexto de dealer, mostrar todas las entidades
        // - Si hay contexto de dealer, mostrar entidades del dealer O entidades globales (DealerId == null)
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => context.CurrentDealerId == null || e.DealerId == null || e.DealerId == context.CurrentDealerId);
        modelBuilder.Entity<TEntity>()
            .HasIndex(e => e.DealerId)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_DealerId");
    }

    public override int SaveChanges()
    {
        SetDealerIdOnNewEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetDealerIdOnNewEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetDealerIdOnNewEntities()
    {
        var dealerId = CurrentDealerId;
        if (!dealerId.HasValue) return;

        // Set DealerId for required tenant entities
        var entries = ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added && e.Entity.DealerId == Guid.Empty);
        foreach (var entry in entries)
        {
            entry.Entity.DealerId = dealerId.Value;
        }

        // Set DealerId for optional tenant entities (only if not already set)
        var optionalEntries = ChangeTracker.Entries<IOptionalTenantEntity>()
            .Where(e => e.State == EntityState.Added && e.Entity.DealerId == null);
        foreach (var entry in optionalEntries)
        {
            entry.Entity.DealerId = dealerId.Value;
        }
    }
}
