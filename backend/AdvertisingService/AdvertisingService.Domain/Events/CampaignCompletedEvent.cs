using CarDealer.Contracts.Abstractions;

namespace AdvertisingService.Domain.Events;

public class CampaignCompletedEvent : EventBase
{
    public override string EventType => "advertising.campaign.completed";
    public Guid CampaignId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid OwnerId { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public decimal TotalSpent { get; set; }
}
