namespace FileStorageService.Core.Models;

/// <summary>
/// Extracted file metadata
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// File identifier
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Content type (MIME)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File extension
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// SHA-256 content hash
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Image-specific metadata
    /// </summary>
    public ImageMetadata? Image { get; set; }

    /// <summary>
    /// Video-specific metadata
    /// </summary>
    public VideoMetadata? Video { get; set; }

    /// <summary>
    /// Audio-specific metadata
    /// </summary>
    public AudioMetadata? Audio { get; set; }

    /// <summary>
    /// Document-specific metadata
    /// </summary>
    public DocumentMetadata? Document { get; set; }

    /// <summary>
    /// EXIF data for images
    /// </summary>
    public ExifData? Exif { get; set; }

    /// <summary>
    /// Additional custom properties
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Extraction timestamp
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Extraction duration in milliseconds
    /// </summary>
    public long ExtractionDurationMs { get; set; }

    /// <summary>
    /// Errors during extraction
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Image-specific metadata
/// </summary>
public class ImageMetadata
{
    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Image format (JPEG, PNG, etc.)
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Bits per pixel
    /// </summary>
    public int BitsPerPixel { get; set; }

    /// <summary>
    /// Color space
    /// </summary>
    public string? ColorSpace { get; set; }

    /// <summary>
    /// Has alpha channel
    /// </summary>
    public bool HasAlpha { get; set; }

    /// <summary>
    /// Is animated (GIF)
    /// </summary>
    public bool IsAnimated { get; set; }

    /// <summary>
    /// Number of frames (for animated images)
    /// </summary>
    public int? FrameCount { get; set; }

    /// <summary>
    /// Horizontal resolution (DPI)
    /// </summary>
    public double? HorizontalResolution { get; set; }

    /// <summary>
    /// Vertical resolution (DPI)
    /// </summary>
    public double? VerticalResolution { get; set; }

    /// <summary>
    /// Aspect ratio
    /// </summary>
    public double AspectRatio => Height > 0 ? (double)Width / Height : 0;

    /// <summary>
    /// Get dimensions as string
    /// </summary>
    public string Dimensions => $"{Width}x{Height}";

    /// <summary>
    /// Total pixels
    /// </summary>
    public long TotalPixels => (long)Width * Height;
}

/// <summary>
/// Video-specific metadata
/// </summary>
public class VideoMetadata
{
    /// <summary>
    /// Video width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Video height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// Frame rate (FPS)
    /// </summary>
    public double FrameRate { get; set; }

    /// <summary>
    /// Video codec
    /// </summary>
    public string? VideoCodec { get; set; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Video bitrate in kbps
    /// </summary>
    public int? VideoBitrateKbps { get; set; }

    /// <summary>
    /// Audio bitrate in kbps
    /// </summary>
    public int? AudioBitrateKbps { get; set; }

    /// <summary>
    /// Has audio track
    /// </summary>
    public bool HasAudio { get; set; }

    /// <summary>
    /// Container format
    /// </summary>
    public string? ContainerFormat { get; set; }

    /// <summary>
    /// Aspect ratio
    /// </summary>
    public double AspectRatio => Height > 0 ? (double)Width / Height : 0;

    /// <summary>
    /// Get formatted duration
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var ts = TimeSpan.FromSeconds(DurationSeconds);
            return ts.Hours > 0
                ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}

/// <summary>
/// Audio-specific metadata
/// </summary>
public class AudioMetadata
{
    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public string? Codec { get; set; }

    /// <summary>
    /// Bitrate in kbps
    /// </summary>
    public int BitrateKbps { get; set; }

    /// <summary>
    /// Sample rate in Hz
    /// </summary>
    public int SampleRateHz { get; set; }

    /// <summary>
    /// Number of channels
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Bits per sample
    /// </summary>
    public int? BitsPerSample { get; set; }

    /// <summary>
    /// Artist name
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// Album name
    /// </summary>
    public string? Album { get; set; }

    /// <summary>
    /// Track title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Track number
    /// </summary>
    public int? TrackNumber { get; set; }

    /// <summary>
    /// Year
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Genre
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Get formatted duration
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var ts = TimeSpan.FromSeconds(DurationSeconds);
            return ts.Hours > 0
                ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}

/// <summary>
/// Document-specific metadata
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// Document title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Document author
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Document subject
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Keywords
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Creator application
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// Producer application
    /// </summary>
    public string? Producer { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Modification date
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Number of pages
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Word count
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    /// Character count
    /// </summary>
    public int? CharacterCount { get; set; }

    /// <summary>
    /// Is encrypted/password protected
    /// </summary>
    public bool IsEncrypted { get; set; }
}

/// <summary>
/// EXIF data from images
/// </summary>
public class ExifData
{
    /// <summary>
    /// Camera make
    /// </summary>
    public string? Make { get; set; }

    /// <summary>
    /// Camera model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Software used
    /// </summary>
    public string? Software { get; set; }

    /// <summary>
    /// Date taken
    /// </summary>
    public DateTime? DateTaken { get; set; }

    /// <summary>
    /// Exposure time
    /// </summary>
    public string? ExposureTime { get; set; }

    /// <summary>
    /// F-number (aperture)
    /// </summary>
    public double? FNumber { get; set; }

    /// <summary>
    /// ISO speed
    /// </summary>
    public int? IsoSpeed { get; set; }

    /// <summary>
    /// Focal length in mm
    /// </summary>
    public double? FocalLength { get; set; }

    /// <summary>
    /// Flash mode
    /// </summary>
    public string? Flash { get; set; }

    /// <summary>
    /// Orientation
    /// </summary>
    public int? Orientation { get; set; }

    /// <summary>
    /// GPS latitude
    /// </summary>
    public double? GpsLatitude { get; set; }

    /// <summary>
    /// GPS longitude
    /// </summary>
    public double? GpsLongitude { get; set; }

    /// <summary>
    /// GPS altitude in meters
    /// </summary>
    public double? GpsAltitude { get; set; }

    /// <summary>
    /// White balance mode
    /// </summary>
    public string? WhiteBalance { get; set; }

    /// <summary>
    /// Metering mode
    /// </summary>
    public string? MeteringMode { get; set; }

    /// <summary>
    /// Lens model
    /// </summary>
    public string? LensModel { get; set; }

    /// <summary>
    /// Has GPS coordinates
    /// </summary>
    public bool HasGps => GpsLatitude.HasValue && GpsLongitude.HasValue;

    /// <summary>
    /// Get GPS coordinates as string
    /// </summary>
    public string? GpsCoordinates => HasGps
        ? $"{GpsLatitude:F6}, {GpsLongitude:F6}"
        : null;

    /// <summary>
    /// Additional EXIF tags
    /// </summary>
    public Dictionary<string, string> AdditionalTags { get; set; } = new();
}
