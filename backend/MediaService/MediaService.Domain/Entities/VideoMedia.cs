namespace MediaService.Domain.Entities;

/// <summary>
/// Represents a video media asset
/// </summary>
public class VideoMedia : MediaAsset
{
    public TimeSpan? Duration { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string? VideoCodec { get; private set; }
    public string? AudioCodec { get; private set; }
    public double? FrameRate { get; private set; }
    public int? Bitrate { get; private set; }
    public string StorageKeyInput { get; private set; } = string.Empty;
    public string StorageKeyHls { get; private set; } = string.Empty;
    public bool HasHlsStream { get; private set; }

    public VideoMedia(
        string ownerId,
        string? context,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string storageKey)
        : base(ownerId, context, Enums.MediaType.Video, originalFileName, contentType, sizeBytes, storageKey)
    {
        StorageKeyInput = storageKey;
    }

    public void SetDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be greater than zero", nameof(duration));

        Duration = duration;
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

    public void SetCodecs(string videoCodec, string audioCodec)
    {
        VideoCodec = videoCodec ?? throw new ArgumentNullException(nameof(videoCodec));
        AudioCodec = audioCodec ?? throw new ArgumentNullException(nameof(audioCodec));
        MarkAsUpdated();
    }

    public void SetFrameRate(double frameRate)
    {
        if (frameRate <= 0) throw new ArgumentException("Frame rate must be greater than 0", nameof(frameRate));

        FrameRate = frameRate;
        MarkAsUpdated();
    }

    public void SetBitrate(int bitrate)
    {
        if (bitrate <= 0) throw new ArgumentException("Bitrate must be greater than 0", nameof(bitrate));

        Bitrate = bitrate;
        MarkAsUpdated();
    }

    public void SetHlsStream(string hlsStorageKey)
    {
        StorageKeyHls = hlsStorageKey ?? throw new ArgumentNullException(nameof(hlsStorageKey));
        HasHlsStream = true;
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
        return base.IsValid() &&
               !string.IsNullOrWhiteSpace(StorageKeyInput);
    }
}