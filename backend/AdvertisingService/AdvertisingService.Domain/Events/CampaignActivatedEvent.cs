using CarDealer.Contracts.Abstractions;

namespace AdvertisingService.Domain.Events;

public class CampaignActivatedEvent : EventBase
{
    public override string EventType => "advertising.campaign.activated";
    public Guid CampaignId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid BillingReferenceId { get; set; }
}
