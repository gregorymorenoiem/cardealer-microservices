namespace CarDealer.Shared.MultiTenancy;

public interface ITenantContext
{
    Guid? CurrentDealerId { get; }
    bool HasDealerContext { get; }
}
