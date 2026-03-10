using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Imagen de vehículo
/// </summary>
public class VehicleImage : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid VehicleId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public ImageType ImageType { get; set; } = ImageType.Exterior;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    // ========================================
    // MODERACIÓN AUTOMÁTICA DE FOTOS
    // ========================================
    /// <summary>
    /// Moderation status: Pending (default), Approved, Rejected.
    /// Images rejected by AI moderation are flagged with a specific reason.
    /// </summary>
    public ImageModerationStatus ModerationStatus { get; set; } = ImageModerationStatus.Pending;
    /// <summary>Specific rejection reason shown to the dealer (e.g., "Marca de agua de SuperCarros detectada")</summary>
    public string? ModerationRejectionReason { get; set; }
    /// <summary>Comma-separated flags (e.g., "watermark,low_resolution,stock_photo")</summary>
    public string? ModerationFlags { get; set; }
    /// <summary>When the moderation was last performed</summary>
    public DateTime? ModeratedAt { get; set; }
    /// <summary>Who/what moderated: "PhotoModerationService", "admin:userId", etc.</summary>
    public string? ModeratedBy { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vehicle? Vehicle { get; set; }
}

/// <summary>
/// Per-image moderation status
/// </summary>
public enum ImageModerationStatus
{
    /// <summary>Not yet moderated</summary>
    Pending = 0,
    /// <summary>Passed all moderation checks</summary>
    Approved = 1,
    /// <summary>Failed moderation — reason in ModerationRejectionReason</summary>
    Rejected = 2
}
