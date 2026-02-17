namespace CarDealer.Shared.Persistence;

/// <summary>
/// Interface for entities that track creation and modification timestamps.
/// Implemented automatically by <see cref="AuditableEntityInterceptor"/>.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
