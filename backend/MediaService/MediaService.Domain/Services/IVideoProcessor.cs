namespace MediaService.Domain.Interfaces.Services;

/// <summary>
/// Interface for video processing operations
/// </summary>
public interface IVideoProcessor
{
    /// <summary>
    /// Processes a video and creates variants/transcodes
    /// </summary>
    Task<VideoProcessingResult> ProcessVideoAsync(Stream videoStream, string originalFileName, VideoProcessingConfig config);

    /// <summary>
    /// Gets video information without processing
    /// </summary>
    Task<VideoInfo> GetVideoInfoAsync(Stream videoStream);

    /// <summary>
    /// Validates if a video can be processed
    /// </summary>
    Task<bool> ValidateVideoAsync(Stream videoStream, string contentType);

    /// <summary>
    /// Generates thumbnails from a video
    /// </summary>
    Task<List<Stream>> GenerateThumbnailsAsync(Stream videoStream, int count, int? width = null, int? height = null);

    /// <summary>
    /// Extracts audio from a video
    /// </summary>
    Task<Stream> ExtractAudioAsync(Stream videoStream, string audioFormat);
}

/// <summary>
/// Video processing result
/// </summary>
public class VideoProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public VideoInfo OriginalVideoInfo { get; set; } = new();
    public List<ProcessedVideoVariant> Variants { get; set; } = new();
    public List<ProcessedThumbnail> Thumbnails { get; set; } = new();
    public Stream? AudioTrack { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}

/// <summary>
/// Processed video variant
/// </summary>
public class ProcessedVideoVariant
{
    public string Name { get; set; } = string.Empty;
    public Stream VideoStream { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string VideoCodec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int Bitrate { get; set; }
    public string StorageKey { get; set; } = string.Empty;
}

/// <summary>
/// Processed thumbnail
/// </summary>
public class ProcessedThumbnail
{
    public int Index { get; set; }
    public TimeSpan Timestamp { get; set; }
    public Stream ImageStream { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string StorageKey { get; set; } = string.Empty;
}

/// <summary>
/// Video information
/// </summary>
public class VideoInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public string VideoCodec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int Bitrate { get; set; }
    public double FrameRate { get; set; }
    public long FileSize { get; set; }
}

/// <summary>
/// Video processing configuration
/// </summary>
public class VideoProcessingConfig
{
    public bool GenerateHls { get; set; } = true;
    public int HlsSegmentSeconds { get; set; } = 4;
    public List<VideoBitrateProfile> BitrateLadder { get; set; } = new();
    public bool GenerateThumbnails { get; set; } = true;
    public int ThumbnailCount { get; set; } = 10;
    public int ThumbnailIntervalSeconds { get; set; } = 10;
    public bool ExtractAudio { get; set; } = true;
    public string OutputFormat { get; set; } = "h264";
    public string AudioFormat { get; set; } = "aac";
}

/// <summary>
/// Video bitrate profile
/// </summary>
public class VideoBitrateProfile
{
    public string Name { get; set; } = string.Empty;
    public int Height { get; set; }
    public string VideoBitrate { get; set; } = string.Empty;
    public string AudioBitrate { get; set; } = string.Empty;
    public string VideoCodec { get; set; } = "libx264";
    public string AudioCodec { get; set; } = "aac";
    public string Profile { get; set; } = "main";
    public string Level { get; set; } = "3.1";
}