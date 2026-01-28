using Vehicle360ProcessingService.Domain.Entities;

namespace Vehicle360ProcessingService.Application.DTOs;

// ============================================================================
// Request DTOs
// ============================================================================

/// <summary>
/// Request para iniciar el procesamiento 360 de un vehículo
/// </summary>
public class StartProcessingRequest
{
    public Guid VehicleId { get; set; }
    public int FrameCount { get; set; } = 6;
    public ProcessingOptionsDto? Options { get; set; }
}

public class ProcessingOptionsDto
{
    public int OutputWidth { get; set; } = 1920;
    public int OutputHeight { get; set; } = 1080;
    public string OutputFormat { get; set; } = "png";
    public int JpegQuality { get; set; } = 90;
    public bool SmartFrameSelection { get; set; } = true;
    public bool AutoCorrectExposure { get; set; } = true;
    public bool GenerateThumbnails { get; set; } = true;
    public string BackgroundColor { get; set; } = "transparent";
}

// ============================================================================
// Response DTOs
// ============================================================================

/// <summary>
/// Respuesta al iniciar el procesamiento
/// </summary>
public class StartProcessingResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? QueuePosition { get; set; }
    public int? EstimatedWaitSeconds { get; set; }
}

/// <summary>
/// Estado del job de procesamiento
/// </summary>
public class JobStatusResponse
{
    public Guid JobId { get; set; }
    public Guid VehicleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
    public bool IsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CurrentStep { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingDurationMs { get; set; }
}

/// <summary>
/// Resultado completo del procesamiento
/// </summary>
public class ProcessingResultResponse
{
    public Guid JobId { get; set; }
    public Guid VehicleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalFrames { get; set; }
    public List<ProcessedFrameDto> Frames { get; set; } = new();
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingDurationMs { get; set; }
}

public class ProcessedFrameDto
{
    public int Index { get; set; }
    public int Angle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OriginalImageUrl { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool HasTransparentBackground { get; set; }
}

/// <summary>
/// Respuesta para el visor 360 del frontend
/// </summary>
public class Vehicle360ViewerResponse
{
    public Guid VehicleId { get; set; }
    public Guid? JobId { get; set; }
    public bool IsReady { get; set; }
    public int TotalFrames { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<ViewerFrameDto> Frames { get; set; } = new();
    public ViewerConfigDto Config { get; set; } = new();
}

public class ViewerFrameDto
{
    public int Index { get; set; }
    public int Angle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}

public class ViewerConfigDto
{
    public bool AutoRotate { get; set; } = true;
    public int AutoRotateSpeed { get; set; } = 5000; // ms per rotation
    public bool AllowDrag { get; set; } = true;
    public bool ShowThumbnails { get; set; } = true;
    public bool HasTransparentBackground { get; set; } = true;
}

// ============================================================================
// Job Response (full details)
// ============================================================================

public class Vehicle360JobResponse
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int FrameCount { get; set; }
    public string? OriginalVideoUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public ProcessingOptionsDto? Options { get; set; }
    public List<ProcessedFrameDto> Frames { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingDurationMs { get; set; }
    public string? CorrelationId { get; set; }
    
    public static Vehicle360JobResponse FromEntity(Vehicle360Job job)
    {
        return new Vehicle360JobResponse
        {
            Id = job.Id,
            VehicleId = job.VehicleId,
            UserId = job.UserId,
            Status = job.Status.ToString(),
            Progress = job.Progress,
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            FrameCount = job.FrameCount,
            OriginalVideoUrl = job.OriginalVideoUrl,
            OriginalFileName = job.OriginalFileName,
            Options = job.Options != null ? new ProcessingOptionsDto
            {
                OutputWidth = job.Options.OutputWidth,
                OutputHeight = job.Options.OutputHeight,
                OutputFormat = job.Options.OutputFormat,
                JpegQuality = job.Options.JpegQuality,
                SmartFrameSelection = job.Options.SmartFrameSelection,
                AutoCorrectExposure = job.Options.AutoCorrectExposure,
                GenerateThumbnails = job.Options.GenerateThumbnails,
                BackgroundColor = job.Options.BackgroundColor
            } : null,
            Frames = job.ProcessedFrames.Select(f => new ProcessedFrameDto
            {
                Index = f.SequenceNumber - 1,
                Angle = f.AngleDegrees,
                Name = f.ViewName,
                OriginalImageUrl = f.OriginalImageUrl,
                ImageUrl = f.ProcessedImageUrl ?? f.OriginalImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                HasTransparentBackground = !string.IsNullOrEmpty(f.ProcessedImageUrl)
            }).ToList(),
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs,
            CorrelationId = job.CorrelationId
        };
    }
}
