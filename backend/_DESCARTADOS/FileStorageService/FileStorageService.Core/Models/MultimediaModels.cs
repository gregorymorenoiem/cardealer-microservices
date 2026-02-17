namespace FileStorageService.Core.Models;

/// <summary>
/// Configuration for video variant generation
/// </summary>
public class VideoVariantConfig
{
    /// <summary>
    /// Variant name (e.g., "720p", "1080p", "mobile")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Output width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Output height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Video bitrate in kbps
    /// </summary>
    public int VideoBitrate { get; set; }

    /// <summary>
    /// Audio bitrate in kbps
    /// </summary>
    public int AudioBitrate { get; set; }

    /// <summary>
    /// Video codec (h264, h265, vp9, av1)
    /// </summary>
    public string VideoCodec { get; set; } = "h264";

    /// <summary>
    /// Audio codec (aac, opus, mp3)
    /// </summary>
    public string AudioCodec { get; set; } = "aac";

    /// <summary>
    /// Output format (mp4, webm, mkv)
    /// </summary>
    public string Format { get; set; } = "mp4";

    /// <summary>
    /// Encoding preset (ultrafast, fast, medium, slow, veryslow)
    /// </summary>
    public string Preset { get; set; } = "medium";

    /// <summary>
    /// Constant Rate Factor (18-28, lower = better quality)
    /// </summary>
    public int CRF { get; set; } = 23;

    /// <summary>
    /// Frame rate (null to keep original)
    /// </summary>
    public int? FrameRate { get; set; }

    /// <summary>
    /// Key frame interval in seconds
    /// </summary>
    public int? KeyframeInterval { get; set; }

    /// <summary>
    /// Whether to include audio
    /// </summary>
    public bool IncludeAudio { get; set; } = true;
}

/// <summary>
/// Result of video processing operation
/// </summary>
public class VideoProcessingResult
{
    /// <summary>
    /// Success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Output file path or identifier
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Processing duration in seconds
    /// </summary>
    public double ProcessingDuration { get; set; }

    /// <summary>
    /// Output file size in bytes
    /// </summary>
    public long OutputSize { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// FFmpeg command executed
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// FFmpeg output log
    /// </summary>
    public string? Log { get; set; }
}

/// <summary>
/// Configuration for audio processing
/// </summary>
public class AudioProcessingConfig
{
    /// <summary>
    /// Output format (mp3, aac, opus, flac, wav)
    /// </summary>
    public string Format { get; set; } = "mp3";

    /// <summary>
    /// Bitrate in kbps (for lossy formats)
    /// </summary>
    public int? Bitrate { get; set; } = 192;

    /// <summary>
    /// Sample rate in Hz (44100, 48000, etc.)
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary>
    /// Number of audio channels (1=mono, 2=stereo)
    /// </summary>
    public int? Channels { get; set; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public string? Codec { get; set; }

    /// <summary>
    /// Whether to normalize audio volume
    /// </summary>
    public bool Normalize { get; set; }

    /// <summary>
    /// Target normalization level in LUFS
    /// </summary>
    public double NormalizationLevel { get; set; } = -16.0;

    /// <summary>
    /// Fade in duration in seconds
    /// </summary>
    public double FadeIn { get; set; }

    /// <summary>
    /// Fade out duration in seconds
    /// </summary>
    public double FadeOut { get; set; }
}

/// <summary>
/// Thumbnail generation options
/// </summary>
public class ThumbnailOptions
{
    /// <summary>
    /// Width in pixels
    /// </summary>
    public int Width { get; set; } = 320;

    /// <summary>
    /// Height in pixels
    /// </summary>
    public int Height { get; set; } = 180;

    /// <summary>
    /// Output format (jpg, png, webp)
    /// </summary>
    public string Format { get; set; } = "jpg";

    /// <summary>
    /// Quality (1-100)
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Timestamp in seconds (-1 for automatic selection)
    /// </summary>
    public double Timestamp { get; set; } = -1;

    /// <summary>
    /// Number of thumbnails to generate (for contact sheet)
    /// </summary>
    public int Count { get; set; } = 1;
}

/// <summary>
/// FFmpeg processing options
/// </summary>
public class FFmpegOptions
{
    /// <summary>
    /// Path to FFmpeg executable
    /// </summary>
    public string FFmpegPath { get; set; } = "ffmpeg";

    /// <summary>
    /// Path to FFprobe executable
    /// </summary>
    public string FFprobePath { get; set; } = "ffprobe";

    /// <summary>
    /// Maximum processing time in seconds (0 = no limit)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Working directory for temporary files
    /// </summary>
    public string WorkingDirectory { get; set; } = Path.GetTempPath();

    /// <summary>
    /// Whether to use hardware acceleration
    /// </summary>
    public bool UseHardwareAcceleration { get; set; } = false;

    /// <summary>
    /// Hardware acceleration method (cuda, qsv, vaapi, etc.)
    /// </summary>
    public string? HardwareAccelerationMethod { get; set; }

    /// <summary>
    /// Additional FFmpeg arguments
    /// </summary>
    public string? AdditionalArguments { get; set; }

    /// <summary>
    /// Thread count (0 = auto)
    /// </summary>
    public int Threads { get; set; } = 0;
}
