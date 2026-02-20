namespace AdvertisingService.Domain.Entities;

public class AdClick
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid? ImpressionId { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTime RecordedAt { get; private set; }

    public AdCampaign Campaign { get; private set; } = null!;

    private AdClick() { }

    public static AdClick Create(Guid campaignId, Guid? impressionId, Guid? userId)
    {
        return new AdClick
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            ImpressionId = impressionId,
            UserId = userId,
            RecordedAt = DateTime.UtcNow
        };
    }
}
