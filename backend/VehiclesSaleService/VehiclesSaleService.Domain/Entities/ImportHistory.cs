namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Tracks each bulk CSV/Excel import performed by a dealer.
/// Stores filename, counts, and timestamp for the import history UI.
/// </summary>
public class ImportHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SellerId { get; set; }
    public Guid DealerId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorsSummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
