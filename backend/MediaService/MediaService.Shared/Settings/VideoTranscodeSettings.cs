namespace MediaService.Shared.Settings;

/// <summary>
/// Configuration settings for video transcoding
/// </summary>
public class VideoTranscodeSettings
{
    /// <summary>
    /// Whether to enable video transcoding
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to generate HLS streams
    /// </summary>
    public bool GenerateHls { get; set; } = true;

    /// <summary>
    /// HLS segment duration in seconds
    /// </summary>
    public int HlsSegmentSeconds { get; set; } = 4;

    /// <summary>
    /// Bitrate ladder for multiple quality streams
    /// </summary>
    public List<VideoBitrateProfile> BitrateLadder { get; set; } = new()
    {
        new VideoBitrateProfile { Height = 360, VideoBitrate = "800k", AudioBitrate = "96k" },
        new VideoBitrateProfile { Height = 720, VideoBitrate = "2500k", AudioBitrate = "128k" },
        new VideoBitrateProfile { Height = 1080, VideoBitrate = "5000k", AudioBitrate = "192k" }
    };

    /// <summary>
    /// Supported input formats
    /// </summary>
    public string[] SupportedFormats { get; set; } = new[]
    {
        "video/mp4",
        "video/avi",
        "video/mov",
        "video/mkv",
        "video/webm"
    };

    /// <summary>
    /// Output format
    /// </summary>
    public string OutputFormat { get; set; } = "h264";

    /// <summary>
    /// Maximum processing time in minutes
    /// </summary>
    public int MaxProcessingTimeMinutes { get; set; } = 60;

    /// <summary>
    /// Whether to generate thumbnails
    /// </summary>
    public bool GenerateThumbnails { get; set; } = true;

    /// <summary>
    /// Thumbnail interval in seconds
    /// </summary>
    public int ThumbnailIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Number of thumbnails to generate
    /// </summary>
    public int ThumbnailCount { get; set; } = 10;

    /// <summary>
    /// Whether to extract audio tracks
    /// </summary>
    public bool ExtractAudio { get; set; } = true;

    /// <summary>
    /// Audio output format
    /// </summary>
    public string AudioFormat { get; set; } = "aac";
}

/// <summary>
/// Video bitrate profile for multiple quality streams
/// </summary>
public class VideoBitrateProfile
{
    /// <summary>
    /// Video height for this profile
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Video bitrate (e.g., "800k", "1.5M")
    /// </summary>
    public string VideoBitrate { get; set; } = string.Empty;

    /// <summary>
    /// Audio bitrate (e.g., "96k", "128k")
    /// </summary>
    public string AudioBitrate { get; set; } = string.Empty;

    /// <summary>
    /// Video codec
    /// </summary>
    public string VideoCodec { get; set; } = "libx264";

    /// <summary>
    /// Audio codec
    /// </summary>
    public string AudioCodec { get; set; } = "aac";

    /// <summary>
    /// Profile name (baseline, main, high)
    /// </summary>
    public string Profile { get; set; } = "main";

    /// <summary>
    /// Level for H.264 encoding
    /// </summary>
    public string Level { get; set; } = "3.1";
}