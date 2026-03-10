namespace BillingService.Domain.Entities;

/// <summary>
/// Canal de adquisición del dealer.
/// Permite calcular CAC (Customer Acquisition Cost) por canal.
/// </summary>
public enum AcquisitionChannel
{
    /// <summary>Búsqueda orgánica (Google, Bing).</summary>
    OrganicSearch = 1,

    /// <summary>Campaña de Google Ads (SEM).</summary>
    GoogleAds = 2,

    /// <summary>Meta Ads (Facebook/Instagram).</summary>
    MetaAds = 3,

    /// <summary>TikTok Ads.</summary>
    TikTokAds = 4,

    /// <summary>Referido por otro dealer o usuario.</summary>
    Referral = 5,

    /// <summary>Tráfico directo.</summary>
    Direct = 6,

    /// <summary>Email marketing.</summary>
    EmailMarketing = 7,

    /// <summary>WhatsApp marketing.</summary>
    WhatsAppMarketing = 8,

    /// <summary>Redes sociales orgánicas.</summary>
    OrganicSocial = 9,

    /// <summary>Evento presencial (ferias, expos).</summary>
    Event = 10,

    /// <summary>Equipo de ventas (outbound).</summary>
    SalesTeam = 11,

    /// <summary>Partner/alianza estratégica.</summary>
    Partnership = 12,

    /// <summary>Otro canal no categorizado.</summary>
    Other = 99,
}

/// <summary>
/// Tracking de adquisición de dealers para cálculo de CAC (Customer Acquisition Cost).
/// Registra la fuente, costo y conversión de cada dealer adquirido.
/// 
/// KPI AUDIT: CAC era un gap crítico — no existía tracking de costos de adquisición
/// por canal, impidiendo medir la eficiencia de marketing y comparar CAC vs LTV.
/// </summary>
public class AcquisitionTracking
{
    public Guid Id { get; private set; }

    /// <summary>ID del dealer adquirido.</summary>
    public Guid DealerId { get; private set; }

    /// <summary>Canal de adquisición principal.</summary>
    public AcquisitionChannel Channel { get; private set; }

    /// <summary>Sub-canal o campaña específica (e.g., "google-ads-dr-dealers-q1-2026").</summary>
    public string? CampaignId { get; private set; }

    /// <summary>Nombre de la campaña para display.</summary>
    public string? CampaignName { get; private set; }

    /// <summary>UTM Source capturado al registrarse.</summary>
    public string? UtmSource { get; private set; }

    /// <summary>UTM Medium capturado al registrarse.</summary>
    public string? UtmMedium { get; private set; }

    /// <summary>UTM Campaign capturado al registrarse.</summary>
    public string? UtmCampaign { get; private set; }

    /// <summary>UTM Content capturado al registrarse.</summary>
    public string? UtmContent { get; private set; }

    /// <summary>UTM Term capturado al registrarse.</summary>
    public string? UtmTerm { get; private set; }

    /// <summary>Costo de adquisición individual de este dealer (USD).</summary>
    public decimal AcquisitionCostUsd { get; private set; }

    /// <summary>Fecha de registro del dealer.</summary>
    public DateTime RegisteredAt { get; private set; }

    /// <summary>¿El dealer se convirtió a plan pago?</summary>
    public bool ConvertedToPaid { get; private set; }

    /// <summary>Fecha en que convirtió a plan pago (si aplica).</summary>
    public DateTime? ConvertedAt { get; private set; }

    /// <summary>Plan al que convirtió.</summary>
    public SubscriptionPlan? ConvertedToPlan { get; private set; }

    /// <summary>Días desde registro hasta conversión a pago.</summary>
    public int? DaysToConversion { get; private set; }

    /// <summary>ID del referido (si el canal es Referral).</summary>
    public Guid? ReferredByDealerId { get; private set; }

    /// <summary>Código de referido utilizado.</summary>
    public string? ReferralCode { get; private set; }

    /// <summary>Landing page donde el dealer llegó primero.</summary>
    public string? LandingPage { get; private set; }

    /// <summary>País del dealer.</summary>
    public string Country { get; private set; } = "DO";

    /// <summary>Notas adicionales.</summary>
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private AcquisitionTracking() { }

    public AcquisitionTracking(
        Guid dealerId,
        AcquisitionChannel channel,
        DateTime registeredAt,
        string? campaignId = null,
        string? campaignName = null,
        string? utmSource = null,
        string? utmMedium = null,
        string? utmCampaign = null,
        string? utmContent = null,
        string? utmTerm = null,
        decimal acquisitionCostUsd = 0,
        Guid? referredByDealerId = null,
        string? referralCode = null,
        string? landingPage = null,
        string? country = "DO")
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Channel = channel;
        RegisteredAt = registeredAt;
        CampaignId = campaignId;
        CampaignName = campaignName;
        UtmSource = utmSource;
        UtmMedium = utmMedium;
        UtmCampaign = utmCampaign;
        UtmContent = utmContent;
        UtmTerm = utmTerm;
        AcquisitionCostUsd = acquisitionCostUsd;
        ReferredByDealerId = referredByDealerId;
        ReferralCode = referralCode;
        LandingPage = landingPage;
        Country = country ?? "DO";
        ConvertedToPaid = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca al dealer como convertido a plan pago.
    /// </summary>
    public void MarkConverted(SubscriptionPlan plan)
    {
        ConvertedToPaid = true;
        ConvertedAt = DateTime.UtcNow;
        ConvertedToPlan = plan;
        DaysToConversion = (int)(ConvertedAt.Value - RegisteredAt).TotalDays;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el costo de adquisición (e.g., cuando se reconcilia con datos de ads).
    /// </summary>
    public void UpdateAcquisitionCost(decimal costUsd)
    {
        if (costUsd < 0)
            throw new ArgumentException("Acquisition cost cannot be negative.");

        AcquisitionCostUsd = costUsd;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la información de campaña.
    /// </summary>
    public void UpdateCampaignInfo(string? campaignId, string? campaignName)
    {
        CampaignId = campaignId;
        CampaignName = campaignName;
        UpdatedAt = DateTime.UtcNow;
    }
}
