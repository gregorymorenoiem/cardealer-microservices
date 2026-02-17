using CarDealer.Shared.MultiTenancy;

namespace PostgresDbService.Domain.Entities;

/// <summary>
/// Base entity for all database entities across microservices
/// This service acts as a centralized database access point
/// </summary>
public abstract class BaseDbEntity : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty; // Which microservice this entity belongs to
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Generic database table to store any microservice data
/// Uses JSONB for flexible schema support
/// </summary>
public class GenericEntity : BaseDbEntity
{
    public string EntityType { get; set; } = string.Empty; // e.g., "User", "Vehicle", "ContactRequest"
    public string EntityId { get; set; } = string.Empty;   // Original entity ID from microservice
    public string DataJson { get; set; } = "{}";           // JSONB data
    public string? IndexData { get; set; }                 // Additional indexable fields as JSON
    public string Status { get; set; } = "Active";         // Active, Inactive, Deleted
    public int Version { get; set; } = 1;                  // For optimistic concurrency
}