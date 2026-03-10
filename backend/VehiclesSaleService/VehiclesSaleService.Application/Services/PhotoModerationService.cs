using VehiclesSaleService.Domain.Entities;

namespace VehiclesSaleService.Application.Services;

/// <summary>
/// Rule-based photo moderation service that validates vehicle images
/// against quality and content criteria at publish time.
///
/// Checks (in order):
///   1. Resolution — minimum 400×300px
///   2. Watermark — competitor portal text (SuperCarros, OLX, Corotos, etc.)
///   3. Content type — rejects documents and person-only photos
///   4. Stock/generic — flags images likely from internet (based on URL patterns)
///
/// NOTE: Checks 2-4 currently use URL-based and metadata heuristics.
/// When a Vision AI service is integrated, these checks will be enhanced
/// with actual image analysis (OCR for watermarks, classification for content).
/// </summary>
public static class PhotoModerationService
{
    /// <summary>Minimum width in pixels</summary>
    private const int MinWidth = 400;
    /// <summary>Minimum height in pixels</summary>
    private const int MinHeight = 300;

    /// <summary>
    /// Known competitor portal watermark patterns (case-insensitive match against URL or caption).
    /// When Vision AI is integrated, these will also be checked via OCR on the actual image pixels.
    /// </summary>
    private static readonly string[] CompetitorWatermarks = new[]
    {
        "supercarros", "super carros", "olx", "corotos", "ecarros",
        "autocosmos", "encuentra24", "mercadolibre", "facebook.com",
        "fbcdn", "instagram", "carfax", "autotrader", "cargurus",
        "cars.com", "caranddriver"
    };

    /// <summary>
    /// URL patterns that indicate stock/internet images (not original dealer photos).
    /// </summary>
    private static readonly string[] StockImagePatterns = new[]
    {
        "stock", "generic", "placeholder", "default-car", "no-image",
        "shutterstock", "istock", "gettyimages", "unsplash", "pexels",
        "pixabay", "dreamstime", "123rf", "depositphotos",
        "wikimedia", "wikipedia"
    };

    /// <summary>
    /// Evaluates all images of a vehicle for moderation compliance.
    /// Returns per-image results with specific rejection reasons.
    /// </summary>
    public static PhotoModerationResult EvaluateAll(ICollection<VehicleImage> images)
    {
        var results = new List<ImageModerationResult>();
        var totalRejected = 0;

        foreach (var image in images)
        {
            var result = EvaluateSingle(image);
            results.Add(result);
            if (result.Status == ImageModerationStatus.Rejected)
                totalRejected++;
        }

        return new PhotoModerationResult
        {
            ImageResults = results,
            TotalImages = images.Count,
            RejectedCount = totalRejected,
            ApprovedCount = images.Count - totalRejected,
            HasRejections = totalRejected > 0,
            DealerMessage = totalRejected > 0
                ? $"⚠️ {totalRejected} de {images.Count} foto(s) no cumple(n) con los requisitos de calidad de OKLA. Por favor revisa los detalles y sube nuevas fotos."
                : null
        };
    }

    /// <summary>
    /// Evaluates a single image against all moderation criteria.
    /// </summary>
    public static ImageModerationResult EvaluateSingle(VehicleImage image)
    {
        var flags = new List<string>();
        var rejectionReasons = new List<string>();

        // ── Check 1: Resolution ─────────────────────────────────────────
        if (image.Width.HasValue && image.Height.HasValue)
        {
            if (image.Width.Value < MinWidth || image.Height.Value < MinHeight)
            {
                flags.Add("low_resolution");
                rejectionReasons.Add(
                    $"Resolución muy baja ({image.Width}×{image.Height}px). " +
                    $"Mínimo requerido: {MinWidth}×{MinHeight}px. " +
                    "Sube una foto con mayor resolución.");
            }
        }

        // ── Check 2: Competitor watermarks ──────────────────────────────
        var urlLower = (image.Url ?? string.Empty).ToLowerInvariant();
        var captionLower = (image.Caption ?? string.Empty).ToLowerInvariant();
        var combinedText = $"{urlLower} {captionLower}";

        foreach (var watermark in CompetitorWatermarks)
        {
            if (combinedText.Contains(watermark, StringComparison.OrdinalIgnoreCase))
            {
                flags.Add("watermark");
                rejectionReasons.Add(
                    $"Se detectó una marca de agua o referencia a otro portal (\"{watermark}\"). " +
                    "OKLA requiere fotos originales tomadas por el dealer. " +
                    "No se permiten fotos descargadas de otros portales.");
                break; // One watermark match is enough
            }
        }

        // ── Check 3: Stock/internet image detection ─────────────────────
        foreach (var pattern in StockImagePatterns)
        {
            if (urlLower.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                flags.Add("stock_photo");
                rejectionReasons.Add(
                    "La imagen parece ser una foto genérica o descargada de internet. " +
                    "OKLA requiere fotos reales del vehículo actual que estás vendiendo.");
                break;
            }
        }

        // ── Check 4: Document/person detection (by ImageType) ───────────
        if (image.ImageType == ImageType.Documents)
        {
            flags.Add("document");
            rejectionReasons.Add(
                "No se permiten fotos de documentos en la galería del vehículo. " +
                "Los documentos de identidad y registros se suben en la sección de verificación KYC.");
        }

        // Determine final status
        var isRejected = rejectionReasons.Count > 0;
        var status = isRejected ? ImageModerationStatus.Rejected : ImageModerationStatus.Approved;

        return new ImageModerationResult
        {
            ImageId = image.Id,
            Status = status,
            Flags = flags,
            RejectionReason = isRejected
                ? string.Join(" | ", rejectionReasons)
                : null,
            DealerMessage = isRejected
                ? $"❌ Foto rechazada: {string.Join(". ", rejectionReasons)}"
                : "✅ Foto aprobada"
        };
    }
}

// ── Result Records ──────────────────────────────────────────────

/// <summary>
/// Overall result of photo moderation for all images of a vehicle.
/// </summary>
public record PhotoModerationResult
{
    public List<ImageModerationResult> ImageResults { get; init; } = new();
    public int TotalImages { get; init; }
    public int RejectedCount { get; init; }
    public int ApprovedCount { get; init; }
    public bool HasRejections { get; init; }
    /// <summary>Summary message for the dealer if any photos were rejected</summary>
    public string? DealerMessage { get; init; }
}

/// <summary>
/// Per-image moderation result with specific rejection reasons.
/// </summary>
public record ImageModerationResult
{
    public Guid ImageId { get; init; }
    public ImageModerationStatus Status { get; init; }
    public List<string> Flags { get; init; } = new();
    /// <summary>Combined rejection reason (all checks that failed)</summary>
    public string? RejectionReason { get; init; }
    /// <summary>Human-readable message for the dealer</summary>
    public string? DealerMessage { get; init; }
}
