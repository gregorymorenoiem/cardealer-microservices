using MediaService.Domain.Common;

namespace MediaService.Domain.Entities;

/// <summary>
/// Represents a variant of a media asset (thumbnails, different sizes, formats, etc.)
/// </summary>
public class MediaVariant : EntityBase
{
    public string MediaAssetId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty; // "thumb", "small", "medium", "large", "hls"
    public string StorageKey { get; private set; } = string.Empty;
    public string? CdnUrl { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public long SizeBytes { get; private set; }
    public string? Format { get; private set; }
    public int? Quality { get; private set; }
    public string? VideoProfile { get; private set; }
    public int? Bitrate { get; private set; }
    public TimeSpan? Duration { get; private set; }

    // Navigation property
    public virtual MediaAsset MediaAsset { get; private set; } = null!;

    // Private constructor for Entity Framework
    private MediaVariant() { }

    public MediaVariant(
        string mediaAssetId,
        string name,
        string storageKey,
        int? width = null,
        int? height = null,
        long sizeBytes = 0,
        string? format = null,
        int? quality = null,
        string? videoProfile = null,
        int? bitrate = null,
        TimeSpan? duration = null)
    {
        MediaAssetId = mediaAssetId ?? throw new ArgumentNullException(nameof(mediaAssetId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        StorageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
        Width = width;
        Height = height;
        SizeBytes = sizeBytes;
        Format = format;
        Quality = quality;
        VideoProfile = videoProfile;
        Bitrate = bitrate;
        Duration = duration;
    }

    public void SetCdnUrl(string cdnUrl)
    {
        CdnUrl = cdnUrl;
        MarkAsUpdated();
    }

    public void SetSize(long sizeBytes)
    {
        if (sizeBytes < 0) throw new ArgumentException("Size cannot be negative", nameof(sizeBytes));

        SizeBytes = sizeBytes;
        MarkAsUpdated();
    }

    public void SetDimensions(int width, int height)
    {
        if (width <= 0) throw new ArgumentException("Width must be greater than 0", nameof(width));
        if (height <= 0) throw new ArgumentException("Height must be greater than 0", nameof(height));

        Width = width;
        Height = height;
        MarkAsUpdated();
    }

    public void SetQuality(int quality)
    {
        if (quality < 0 || quality > 100)
            throw new ArgumentException("Quality must be between 0 and 100", nameof(quality));

        Quality = quality;
        MarkAsUpdated();
    }

    public void SetBitrate(int bitrate)
    {
        if (bitrate <= 0) throw new ArgumentException("Bitrate must be greater than 0", nameof(bitrate));

        Bitrate = bitrate;
        MarkAsUpdated();
    }

    public void SetDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be greater than zero", nameof(duration));

        Duration = duration;
        MarkAsUpdated();
    }

    public double GetAspectRatio()
    {
        if (!Width.HasValue || !Height.HasValue || Height.Value == 0)
            return 0;

        return (double)Width.Value / Height.Value;
    }

    public string GetResolution()
    {
        if (!Width.HasValue || !Height.HasValue)
            return "Unknown";

        return $"{Width}x{Height}";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(MediaAssetId) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(StorageKey) &&
               SizeBytes >= 0;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return MediaAssetId;
        yield return Name;
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}, Name={Name}, MediaAssetId={MediaAssetId}]";
    }
}