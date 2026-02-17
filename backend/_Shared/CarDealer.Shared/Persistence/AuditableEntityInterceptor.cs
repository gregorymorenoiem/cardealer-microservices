using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// EF Core SaveChanges interceptor that automatically handles:
/// - Audit timestamps (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
/// - Soft delete (IsDeleted, DeletedAt, DeletedBy) instead of hard delete
/// - Concurrency stamp refresh on every modification
/// 
/// Register via: options.AddInterceptors(new AuditableEntityInterceptor(logger, httpContextAccessor));
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditableEntityInterceptor>? _logger;
    private readonly Func<string?>? _currentUserProvider;

    /// <summary>
    /// Creates a new AuditableEntityInterceptor.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="currentUserProvider">Optional function to resolve the current user ID (e.g., from HttpContext).</param>
    public AuditableEntityInterceptor(
        ILogger<AuditableEntityInterceptor>? logger = null,
        Func<string?>? currentUserProvider = null)
    {
        _logger = logger;
        _currentUserProvider = currentUserProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditInfo(DbContext? context)
    {
        if (context is null) return;

        var currentUser = _currentUserProvider?.Invoke();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    HandleAdded(entry, currentUser, utcNow);
                    break;

                case EntityState.Modified:
                    HandleModified(entry, currentUser, utcNow);
                    break;

                case EntityState.Deleted:
                    HandleDeleted(entry, currentUser, utcNow);
                    break;
            }
        }
    }

    private void HandleAdded(EntityEntry entry, string? currentUser, DateTime utcNow)
    {
        if (entry.Entity is IAuditableEntity auditable)
        {
            if (auditable.CreatedAt == default)
                auditable.CreatedAt = utcNow;

            if (string.IsNullOrEmpty(auditable.CreatedBy))
                auditable.CreatedBy = currentUser;
        }

        // Ensure new ConcurrencyStamp for entities that have one
        if (entry.Entity is EntityBase entityBase)
        {
            entityBase.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
    }

    private void HandleModified(EntityEntry entry, string? currentUser, DateTime utcNow)
    {
        if (entry.Entity is IAuditableEntity auditable)
        {
            auditable.UpdatedAt = utcNow;
            auditable.UpdatedBy = currentUser;
        }

        // Refresh ConcurrencyStamp
        if (entry.Entity is EntityBase entityBase)
        {
            entityBase.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
    }

    private void HandleDeleted(EntityEntry entry, string? currentUser, DateTime utcNow)
    {
        // Convert hard deletes to soft deletes for ISoftDeletableEntity
        if (entry.Entity is ISoftDeletableEntity softDeletable)
        {
            // Prevent actual deletion — change to modification
            entry.State = EntityState.Modified;

            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = utcNow;
            softDeletable.DeletedBy = currentUser;

            // Also update audit fields
            if (entry.Entity is IAuditableEntity auditable)
            {
                auditable.UpdatedAt = utcNow;
                auditable.UpdatedBy = currentUser;
            }

            // Refresh ConcurrencyStamp
            if (entry.Entity is EntityBase entityBase)
            {
                entityBase.ConcurrencyStamp = Guid.NewGuid().ToString();
            }

            _logger?.LogInformation(
                "Soft-deleted {EntityType} with Id {EntityId} by user {UserId}",
                entry.Entity.GetType().Name,
                GetEntityId(entry),
                currentUser ?? "system");
        }
        // If not ISoftDeletableEntity, allow hard delete (EF default behavior)
    }

    private static string? GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue?.ToString();
    }
}
