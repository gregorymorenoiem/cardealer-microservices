namespace AdvertisingService.Domain.Entities;

public class AdImpression
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public string? SessionId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? IpHash { get; private set; }
    public int Section { get; private set; }
    public int Position { get; private set; }
    public DateTime RecordedAt { get; private set; }

    public AdCampaign Campaign { get; private set; } = null!;

    private AdImpression() { }

    public static AdImpression Create(
        Guid campaignId,
        string? sessionId,
        Guid? userId,
        string? ipHash,
        int section,
        int position)
    {
        return new AdImpression
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            SessionId = sessionId,
            UserId = userId,
            IpHash = ipHash,
            Section = section,
            Position = position,
            RecordedAt = DateTime.UtcNow
        };
    }
}
