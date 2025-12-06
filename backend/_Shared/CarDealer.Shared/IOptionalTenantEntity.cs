namespace CarDealer.Shared.MultiTenancy
{
    /// <summary>
    /// Interface for entities that may optionally belong to a tenant.
    /// When DealerId is null, the entity is considered global/system-wide.
    /// When DealerId has a value, the entity belongs to a specific dealer.
    /// </summary>
    public interface IOptionalTenantEntity
    {
        Guid? DealerId { get; set; }
    }
}
