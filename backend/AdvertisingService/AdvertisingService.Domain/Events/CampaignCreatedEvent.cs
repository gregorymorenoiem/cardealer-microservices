using CarDealer.Contracts.Abstractions;

namespace AdvertisingService.Domain.Events;

public class CampaignCreatedEvent : EventBase
{
    public override string EventType => "advertising.campaign.created";
    public Guid CampaignId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public string PlacementType { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
}
