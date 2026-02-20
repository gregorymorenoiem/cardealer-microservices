using CarDealer.Contracts.Abstractions;

namespace AdvertisingService.Domain.Events;

public class CampaignBudgetDepletedEvent : EventBase
{
    public override string EventType => "advertising.campaign.budget_depleted";
    public Guid CampaignId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid OwnerId { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal SpentBudget { get; set; }
    public int ViewsConsumed { get; set; }
}
