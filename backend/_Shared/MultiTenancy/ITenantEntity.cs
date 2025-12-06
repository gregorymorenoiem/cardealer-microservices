namespace CarDealer.Shared.MultiTenancy;

public interface ITenantEntity
{
    Guid DealerId { get; set; }
}
