using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for video processing operations using FFmpeg
/// </summary>
public interface IVideoProcessingService
{
    /// <summary>
    /// Generates thumbnails from video at specific timestamps
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="timestamps">Timestamps in seconds</param>
    /// <param name="width">Thumbnail width</param>
    /// <param name="height">Thumbnail height</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of timestamp to thumbnail stream</returns>
    Task<Dictionary<double, Stream>> GenerateVideoThumbnailsAsync(
        Stream videoStream,
        IEnumerable<double> timestamps,
        int width = 320,
        int height = 180,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts detailed video metadata using FFprobe
    /// </summary>
    /// <param name="videoStream">Video stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete video metadata</returns>
    Task<VideoMetadata> ExtractVideoMetadataAsync(
        Stream videoStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes video to different format/codec
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="outputFormat">Output format (mp4, webm, etc.)</param>
    /// <param name="videoCodec">Video codec (h264, vp9, etc.)</param>
    /// <param name="audioCodec">Audio codec (aac, opus, etc.)</param>
    /// <param name="preset">Encoding preset (ultrafast, fast, medium, slow)</param>
    /// <param name="crf">Constant Rate Factor (18-28, lower = better quality)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transcoded video stream</returns>
    Task<Stream> TranscodeVideoAsync(
        Stream videoStream,
        string outputFormat = "mp4",
        string videoCodec = "h264",
        string audioCodec = "aac",
        string preset = "medium",
        int crf = 23,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple video variants (different resolutions/bitrates)
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="variants">Variant configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated video variants with streams</returns>
    Task<IEnumerable<(VideoVariantConfig Config, Stream Stream)>> GenerateVideoVariantsAsync(
        Stream videoStream,
        IEnumerable<VideoVariantConfig> variants,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts audio track from video
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="outputFormat">Audio format (mp3, aac, opus)</param>
    /// <param name="bitrate">Audio bitrate in kbps</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audio stream</returns>
    Task<Stream> ExtractAudioFromVideoAsync(
        Stream videoStream,
        string outputFormat = "mp3",
        int bitrate = 192,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates video preview/trailer by extracting segments
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="segments">List of (start, duration) in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Preview video stream</returns>
    Task<Stream> CreateVideoPreviewAsync(
        Stream videoStream,
        IEnumerable<(double Start, double Duration)> segments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies watermark to video
    /// </summary>
    /// <param name="videoStream">Source video stream</param>
    /// <param name="watermarkImageStream">Watermark image stream</param>
    /// <param name="position">Position (top-left, top-right, bottom-left, bottom-right, center)</param>
    /// <param name="opacity">Opacity (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Watermarked video stream</returns>
    Task<Stream> ApplyWatermarkAsync(
        Stream videoStream,
        Stream watermarkImageStream,
        string position = "bottom-right",
        double opacity = 0.7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if stream is a valid video file
    /// </summary>
    /// <param name="videoStream">Video stream</param>
    /// <returns>True if valid</returns>
    Task<bool> ValidateVideoAsync(Stream videoStream);

    /// <summary>
    /// Gets video duration in seconds
    /// </summary>
    /// <param name="videoStream">Video stream</param>
    /// <returns>Duration in seconds</returns>
    Task<double> GetVideoDurationAsync(Stream videoStream);

    /// <summary>
    /// Checks if FFmpeg is available
    /// </summary>
    /// <returns>True if FFmpeg is installed and accessible</returns>
    Task<bool> IsFFmpegAvailableAsync();
}
