using System.ComponentModel.DataAnnotations;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// Shared base entity for all domain entities across OKLA microservices.
/// Provides consistent Id, audit timestamps, soft delete, and concurrency control.
/// </summary>
public abstract class EntityBase : IAuditableEntity, ISoftDeletableEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    // ========================================
    // AUDIT FIELDS (IAuditableEntity)
    // ========================================

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    // ========================================
    // SOFT DELETE (ISoftDeletableEntity)
    // ========================================

    /// <summary>
    /// Whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was soft-deleted (UTC).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted this entity.
    /// </summary>
    public string? DeletedBy { get; set; }

    // ========================================
    // CONCURRENCY CONTROL
    // ========================================

    /// <summary>
    /// Optimistic concurrency token. Updated automatically on every save.
    /// </summary>
    [ConcurrencyCheck]
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Marks the entity as updated (refreshes ConcurrencyStamp).
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Marks the entity as soft-deleted.
    /// </summary>
    public void MarkAsDeleted(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}
