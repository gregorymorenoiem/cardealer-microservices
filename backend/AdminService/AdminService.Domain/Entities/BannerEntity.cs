namespace AdminService.Domain.Entities;

/// <summary>
/// Promotional banner displayed on various placements of the OKLA platform.
/// Persisted in-memory (stateless pod) — future: migrate to DB when ApplicationDbContext is ready.
/// </summary>
public class BannerEntity
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public string? Subtitle { get; private set; }
    public string Image { get; private set; }
    public string? MobileImage { get; private set; }
    public string Link { get; private set; }
    public string? CtaText { get; private set; }

    /// <summary>
    /// Where the banner is rendered:
    /// "search_leaderboard" — full-width strip between vehicles on /vehiculos
    /// "homepage-hero"      — hero section of the homepage
    /// "homepage-secondary" — secondary row homepage
    /// "sidebar"            — right sidebar
    /// </summary>
    public string Placement { get; private set; }

    public string Status { get; private set; }   // "active" | "scheduled" | "inactive"
    public string StartDate { get; private set; }
    public string EndDate { get; private set; }
    public int Views { get; private set; }
    public int Clicks { get; private set; }
    public int DisplayOrder { get; private set; }
    public string CreatedAt { get; private set; }
    public string UpdatedAt { get; private set; }

    private BannerEntity() { }

    public static BannerEntity Create(
        string title,
        string image,
        string link,
        string placement,
        string status,
        string startDate,
        string endDate,
        string? subtitle = null,
        string? mobileImage = null,
        string? ctaText = null,
        int displayOrder = 0)
    {
        return new BannerEntity
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Title = title,
            Subtitle = subtitle,
            Image = image,
            MobileImage = mobileImage,
            Link = link,
            CtaText = ctaText,
            Placement = placement,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Views = 0,
            Clicks = 0,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        };
    }

    public void Update(
        string title,
        string image,
        string link,
        string placement,
        string status,
        string startDate,
        string endDate,
        string? subtitle = null,
        string? mobileImage = null,
        string? ctaText = null,
        int displayOrder = 0)
    {
        Title = title;
        Subtitle = subtitle;
        Image = image;
        MobileImage = mobileImage;
        Link = link;
        CtaText = ctaText;
        Placement = placement;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public void RecordView() { Views++; UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); }
    public void RecordClick() { Clicks++; UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); }
}
