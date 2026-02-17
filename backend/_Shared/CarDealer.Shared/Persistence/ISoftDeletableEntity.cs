namespace CarDealer.Shared.Persistence;

/// <summary>
/// Interface for entities that support soft delete.
/// Automatically handled by <see cref="AuditableEntityInterceptor"/>
/// and enforced by <see cref="SoftDeleteQueryFilterExtensions"/>.
/// </summary>
public interface ISoftDeletableEntity
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
