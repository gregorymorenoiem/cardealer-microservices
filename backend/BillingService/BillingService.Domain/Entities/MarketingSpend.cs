namespace BillingService.Domain.Entities;

/// <summary>
/// Registro mensual de gasto en marketing por canal.
/// Se usa junto con AcquisitionTracking para calcular CAC = Gasto Total / Dealers Adquiridos.
/// 
/// KPI AUDIT: Sin tracking de gasto de marketing, es imposible calcular CAC real.
/// Este entity permite registrar gastos mensuales por canal para cálculos precisos.
/// </summary>
public class MarketingSpend
{
    public Guid Id { get; private set; }

    /// <summary>Año del gasto.</summary>
    public int Year { get; private set; }

    /// <summary>Mes del gasto (1-12).</summary>
    public int Month { get; private set; }

    /// <summary>Canal de marketing.</summary>
    public AcquisitionChannel Channel { get; private set; }

    /// <summary>ID de campaña específica (opcional).</summary>
    public string? CampaignId { get; private set; }

    /// <summary>Nombre de la campaña.</summary>
    public string? CampaignName { get; private set; }

    /// <summary>Gasto total del mes en este canal (USD).</summary>
    public decimal SpendUsd { get; private set; }

    /// <summary>Impresiones generadas.</summary>
    public long Impressions { get; private set; }

    /// <summary>Clicks generados.</summary>
    public long Clicks { get; private set; }

    /// <summary>Registros (signups) atribuidos a este canal/mes.</summary>
    public int Signups { get; private set; }

    /// <summary>Conversiones a plan pago atribuidas.</summary>
    public int PaidConversions { get; private set; }

    /// <summary>Notas sobre la campaña/gasto.</summary>
    public string? Notes { get; private set; }

    /// <summary>Quién registró este gasto.</summary>
    public string CreatedBy { get; private set; } = "admin";

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private MarketingSpend() { }

    public MarketingSpend(
        int year,
        int month,
        AcquisitionChannel channel,
        decimal spendUsd,
        string? campaignId = null,
        string? campaignName = null,
        long impressions = 0,
        long clicks = 0,
        int signups = 0,
        int paidConversions = 0,
        string? notes = null,
        string createdBy = "admin")
    {
        if (month < 1 || month > 12) throw new ArgumentException("Month must be between 1 and 12.");
        if (spendUsd < 0) throw new ArgumentException("Spend cannot be negative.");

        Id = Guid.NewGuid();
        Year = year;
        Month = month;
        Channel = channel;
        SpendUsd = spendUsd;
        CampaignId = campaignId;
        CampaignName = campaignName;
        Impressions = impressions;
        Clicks = clicks;
        Signups = signups;
        PaidConversions = paidConversions;
        Notes = notes;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza las métricas del gasto.
    /// </summary>
    public void UpdateMetrics(decimal spendUsd, long impressions, long clicks, int signups, int paidConversions)
    {
        SpendUsd = spendUsd;
        Impressions = impressions;
        Clicks = clicks;
        Signups = signups;
        PaidConversions = paidConversions;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula el Cost Per Click (CPC).
    /// </summary>
    public decimal GetCpc() => Clicks > 0 ? Math.Round(SpendUsd / Clicks, 2) : 0;

    /// <summary>
    /// Calcula el Cost Per Signup.
    /// </summary>
    public decimal GetCostPerSignup() => Signups > 0 ? Math.Round(SpendUsd / Signups, 2) : 0;

    /// <summary>
    /// Calcula el CAC (Cost Per Paid Conversion) para este canal/mes.
    /// </summary>
    public decimal GetCac() => PaidConversions > 0 ? Math.Round(SpendUsd / PaidConversions, 2) : 0;

    /// <summary>
    /// Calcula el Click-Through Rate (CTR).
    /// </summary>
    public decimal GetCtr() => Impressions > 0 ? Math.Round((decimal)Clicks / Impressions * 100, 2) : 0;
}
