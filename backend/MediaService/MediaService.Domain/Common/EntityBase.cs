using MediatR;

namespace MediaService.Domain.Common;

/// <summary>
/// Base class for all entities in the domain
/// </summary>
public abstract class EntityBase : IEquatable<EntityBase>
{
    private readonly List<INotification> _domainEvents = new();

    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Domain events raised by this entity
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity
    /// </summary>
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// Removes a domain event from the entity
    /// </summary>
    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    /// <summary>
    /// Clears all domain events from the entity
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the entity as updated and sets the UpdatedAt timestamp
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the creation timestamp (used for importing existing data)
    /// </summary>
    protected void SetCreatedAt(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EntityBase)obj);
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity
    /// </summary>
    public bool Equals(EntityBase? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <summary>
    /// Serves as the default hash function
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(EntityBase? left, EntityBase? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(EntityBase? left, EntityBase? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Gets the components that are used for equality comparison
    /// </summary>
    protected virtual IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }

    /// <summary>
    /// Returns a string that represents the current entity
    /// </summary>
    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }

    /// <summary>
    /// Validates the entity state
    /// </summary>
    public virtual bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) && CreatedAt <= DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a shallow copy of the entity
    /// </summary>
    public virtual EntityBase ShallowCopy()
    {
        return (EntityBase)MemberwiseClone();
    }
}

/// <summary>
/// Marker interface for aggregate roots
/// </summary>
public interface IAggregateRoot { }
