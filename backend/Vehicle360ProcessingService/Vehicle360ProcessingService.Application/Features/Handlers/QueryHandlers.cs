using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Vehicle360ProcessingService.Application.DTOs;
using Vehicle360ProcessingService.Application.Features.Queries;
using Vehicle360ProcessingService.Domain.Entities;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Application.Features.Handlers;

/// <summary>
/// Handler para obtener un job por ID
/// </summary>
public class GetVehicle360JobHandler : IRequestHandler<GetVehicle360JobQuery, Vehicle360JobResponse?>
{
    private readonly IVehicle360JobRepository _repository;

    public GetVehicle360JobHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<Vehicle360JobResponse?> Handle(
        GetVehicle360JobQuery request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        return job != null ? Vehicle360JobResponse.FromEntity(job) : null;
    }
}

/// <summary>
/// Handler para obtener el estado de un job
/// </summary>
public class GetJobStatusHandler : IRequestHandler<GetJobStatusQuery, JobStatusResponse?>
{
    private readonly IVehicle360JobRepository _repository;

    public GetJobStatusHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<JobStatusResponse?> Handle(
        GetJobStatusQuery request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null) return null;

        return new JobStatusResponse
        {
            JobId = job.Id,
            VehicleId = job.VehicleId,
            Status = job.Status.ToString(),
            Progress = job.Progress,
            IsComplete = job.IsComplete,
            IsFailed = job.IsFailed,
            ErrorMessage = job.ErrorMessage,
            CurrentStep = GetCurrentStep(job.Status),
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs
        };
    }

    private static string GetCurrentStep(Vehicle360JobStatus status) => status switch
    {
        Vehicle360JobStatus.Pending => "Waiting to start",
        Vehicle360JobStatus.Queued => "In queue",
        Vehicle360JobStatus.Processing => "Starting processing",
        Vehicle360JobStatus.UploadingVideo => "Uploading video",
        Vehicle360JobStatus.VideoUploaded => "Video uploaded",
        Vehicle360JobStatus.ExtractingFrames => "Extracting frames from video",
        Vehicle360JobStatus.FramesExtracted => "Frames extracted",
        Vehicle360JobStatus.RemovingBackgrounds => "Removing backgrounds",
        Vehicle360JobStatus.UploadingResults => "Uploading final images",
        Vehicle360JobStatus.Completed => "Completed",
        Vehicle360JobStatus.Failed => "Failed",
        Vehicle360JobStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
}

/// <summary>
/// Handler para obtener el resultado del procesamiento
/// </summary>
public class GetProcessingResultHandler : IRequestHandler<GetProcessingResultQuery, ProcessingResultResponse?>
{
    private readonly IVehicle360JobRepository _repository;

    public GetProcessingResultHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProcessingResultResponse?> Handle(
        GetProcessingResultQuery request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null) return null;

        var frames = job.ProcessedFrames
            .OrderBy(f => f.SequenceNumber)
            .Select(f => new ProcessedFrameDto
            {
                Index = f.SequenceNumber - 1,
                Angle = f.AngleDegrees,
                Name = f.ViewName,
                OriginalImageUrl = f.OriginalImageUrl,
                ImageUrl = f.ProcessedImageUrl ?? f.OriginalImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                HasTransparentBackground = !string.IsNullOrEmpty(f.ProcessedImageUrl)
            }).ToList();

        return new ProcessingResultResponse
        {
            JobId = job.Id,
            VehicleId = job.VehicleId,
            Status = job.Status.ToString(),
            TotalFrames = frames.Count,
            Frames = frames,
            PrimaryImageUrl = frames.FirstOrDefault()?.ImageUrl,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs
        };
    }
}

/// <summary>
/// Handler para obtener los datos del visor 360
/// </summary>
public class GetVehicle360ViewerHandler : IRequestHandler<GetVehicle360ViewerQuery, Vehicle360ViewerResponse?>
{
    private readonly IVehicle360JobRepository _repository;

    public GetVehicle360ViewerHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<Vehicle360ViewerResponse?> Handle(
        GetVehicle360ViewerQuery request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetLatestCompletedByVehicleIdAsync(request.VehicleId, cancellationToken);
        
        if (job == null)
        {
            return new Vehicle360ViewerResponse
            {
                VehicleId = request.VehicleId,
                IsReady = false,
                TotalFrames = 0,
                Frames = new List<ViewerFrameDto>()
            };
        }

        var frames = job.ProcessedFrames
            .OrderBy(f => f.SequenceNumber)
            .Select(f => new ViewerFrameDto
            {
                Index = f.SequenceNumber - 1,
                Angle = f.AngleDegrees,
                Name = f.ViewName,
                ImageUrl = f.ProcessedImageUrl ?? f.OriginalImageUrl,
                ThumbnailUrl = f.ThumbnailUrl
            }).ToList();

        return new Vehicle360ViewerResponse
        {
            VehicleId = request.VehicleId,
            JobId = job.Id,
            IsReady = true,
            TotalFrames = frames.Count,
            PrimaryImageUrl = frames.FirstOrDefault()?.ImageUrl,
            Frames = frames,
            Config = new ViewerConfigDto
            {
                AutoRotate = true,
                AutoRotateSpeed = 5000,
                AllowDrag = true,
                ShowThumbnails = true,
                HasTransparentBackground = frames.Any(f => 
                    f.ImageUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            }
        };
    }
}

/// <summary>
/// Handler para obtener jobs de un vehículo
/// </summary>
public class GetJobsByVehicleHandler : IRequestHandler<GetJobsByVehicleQuery, List<Vehicle360JobResponse>>
{
    private readonly IVehicle360JobRepository _repository;

    public GetJobsByVehicleHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Vehicle360JobResponse>> Handle(
        GetJobsByVehicleQuery request, 
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        return jobs.Select(Vehicle360JobResponse.FromEntity).ToList();
    }
}

/// <summary>
/// Handler para obtener jobs de un usuario
/// </summary>
public class GetUserJobsHandler : IRequestHandler<GetUserJobsQuery, List<Vehicle360JobResponse>>
{
    private readonly IVehicle360JobRepository _repository;

    public GetUserJobsHandler(IVehicle360JobRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Vehicle360JobResponse>> Handle(
        GetUserJobsQuery request, 
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetByUserIdAsync(
            request.UserId, 
            request.Page, 
            request.PageSize, 
            cancellationToken);
        return jobs.Select(Vehicle360JobResponse.FromEntity).ToList();
    }
}

/// <summary>
/// Handler para verificar la salud de los servicios
/// </summary>
public class CheckServicesHealthHandler : IRequestHandler<CheckServicesHealthQuery, ServicesHealthResponse>
{
    private readonly IMediaServiceClient _mediaClient;
    private readonly IVideo360ServiceClient _video360Client;
    private readonly IBackgroundRemovalClient _bgRemovalClient;
    private readonly ILogger<CheckServicesHealthHandler> _logger;

    public CheckServicesHealthHandler(
        IMediaServiceClient mediaClient,
        IVideo360ServiceClient video360Client,
        IBackgroundRemovalClient bgRemovalClient,
        ILogger<CheckServicesHealthHandler> logger)
    {
        _mediaClient = mediaClient;
        _video360Client = video360Client;
        _bgRemovalClient = bgRemovalClient;
        _logger = logger;
    }

    public async Task<ServicesHealthResponse> Handle(
        CheckServicesHealthQuery request, 
        CancellationToken cancellationToken)
    {
        var services = new Dictionary<string, ServiceHealthStatus>();

        // Check MediaService
        services["MediaService"] = await CheckServiceAsync(
            "MediaService",
            () => _mediaClient.IsHealthyAsync(cancellationToken));

        // Check Video360Service
        services["Video360Service"] = await CheckServiceAsync(
            "Video360Service",
            () => _video360Client.IsHealthyAsync(cancellationToken));

        // Check BackgroundRemovalService
        services["BackgroundRemovalService"] = await CheckServiceAsync(
            "BackgroundRemovalService",
            () => _bgRemovalClient.IsHealthyAsync(cancellationToken));

        return new ServicesHealthResponse
        {
            AllHealthy = services.Values.All(s => s.IsHealthy),
            Services = services
        };
    }

    private async Task<ServiceHealthStatus> CheckServiceAsync(
        string name, 
        Func<Task<bool>> healthCheck)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var isHealthy = await healthCheck();
            sw.Stop();
            return new ServiceHealthStatus
            {
                Name = name,
                IsHealthy = isHealthy,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Health check failed for {Service}", name);
            return new ServiceHealthStatus
            {
                Name = name,
                IsHealthy = false,
                LastError = ex.Message,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow
            };
        }
    }
}
