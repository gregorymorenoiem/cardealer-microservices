namespace MediaService.Shared.Settings;

/// <summary>
/// Configuration settings for image processing
/// </summary>
public class ImageProcessingSettings
{
    /// <summary>
    /// Whether to enable automatic image processing
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum width for processed images
    /// </summary>
    public int MaxWidth { get; set; } = 3840;

    /// <summary>
    /// Maximum height for processed images
    /// </summary>
    public int MaxHeight { get; set; } = 2160;

    /// <summary>
    /// Default JPEG quality (1-100)
    /// </summary>
    public int DefaultQuality { get; set; } = 85;

    /// <summary>
    /// Whether to preserve EXIF metadata
    /// </summary>
    public bool PreserveMetadata { get; set; } = false;

    /// <summary>
    /// Whether to generate image variants
    /// </summary>
    public bool GenerateVariants { get; set; } = true;

    /// <summary>
    /// Image variants to generate
    /// </summary>
    public List<ImageVariant> Variants { get; set; } = new()
    {
        new ImageVariant { Name = "thumb", MaxWidth = 200, MaxHeight = 200, Quality = 80 },
        new ImageVariant { Name = "small", MaxWidth = 400, MaxHeight = 400, Quality = 80 },
        new ImageVariant { Name = "medium", MaxWidth = 800, MaxHeight = 800, Quality = 85 },
        new ImageVariant { Name = "large", MaxWidth = 1200, MaxHeight = 1200, Quality = 90 }
    };

    /// <summary>
    /// Supported input formats
    /// </summary>
    public string[] SupportedFormats { get; set; } = new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    /// <summary>
    /// Output format (null preserves original format)
    /// </summary>
    public string? OutputFormat { get; set; } = null;

    /// <summary>
    /// Whether to enable image optimization
    /// </summary>
    public bool EnableOptimization { get; set; } = true;

    /// <summary>
    /// Maximum processing time in seconds
    /// </summary>
    public int MaxProcessingTimeSeconds { get; set; } = 30;
}

/// <summary>
/// Image variant configuration
/// </summary>
public class ImageVariant
{
    /// <summary>
    /// Variant name (thumb, small, medium, large, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum width for this variant
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// Maximum height for this variant
    /// </summary>
    public int MaxHeight { get; set; }

    /// <summary>
    /// JPEG quality for this variant (1-100)
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Whether to maintain aspect ratio
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>
    /// Resize mode (Crop, Max, Pad, etc.)
    /// </summary>
    public string ResizeMode { get; set; } = "Max";
}