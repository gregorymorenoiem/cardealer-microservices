using Video360Service.Domain.Entities;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Interface para el procesador de videos 360
/// </summary>
public interface IVideo360Processor
{
    /// <summary>
    /// Procesa un video y extrae los frames
    /// </summary>
    Task<Video360ProcessingResult> ProcessVideoAsync(
        string videoPath,
        ProcessingOptions options,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene información del video sin procesarlo
    /// </summary>
    Task<VideoInfo> GetVideoInfoAsync(
        string videoPath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Valida que el video sea apto para procesamiento
    /// </summary>
    Task<VideoValidationResult> ValidateVideoAsync(
        string videoPath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado del procesamiento de video
/// </summary>
public class Video360ProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ExtractedFrameResult> Frames { get; set; } = new();
    public long ProcessingTimeMs { get; set; }
    public VideoInfo? VideoInfo { get; set; }
}

/// <summary>
/// Resultado de un frame extraído
/// </summary>
public class ExtractedFrameResult
{
    public int SequenceNumber { get; set; }
    public string ViewName { get; set; } = string.Empty;
    public int AngleDegrees { get; set; }
    public string LocalFilePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public int SourceFrameNumber { get; set; }
    public double TimestampSeconds { get; set; }
    public int? QualityScore { get; set; }
}

/// <summary>
/// Información del video
/// </summary>
public class VideoInfo
{
    public double DurationSeconds { get; set; }
    public int TotalFrames { get; set; }
    public double Fps { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Codec { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Format { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de validación de video
/// </summary>
public class VideoValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public VideoInfo? VideoInfo { get; set; }
}
