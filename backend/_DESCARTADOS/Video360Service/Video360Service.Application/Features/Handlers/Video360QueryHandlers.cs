using MediatR;
using Microsoft.Extensions.Logging;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Queries;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Application.Features.Handlers;

/// <summary>
/// Handler para obtener un trabajo por ID
/// </summary>
public class GetVideo360JobHandler : IRequestHandler<GetVideo360JobQuery, Video360JobResponse?>
{
    private readonly IVideo360JobRepository _jobRepository;

    public GetVideo360JobHandler(IVideo360JobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Video360JobResponse?> Handle(GetVideo360JobQuery request, CancellationToken cancellationToken)
    {
        var job = request.IncludeFrames
            ? await _jobRepository.GetByIdWithFramesAsync(request.JobId, cancellationToken)
            : await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job == null) return null;

        var queuePosition = job.Status == Video360JobStatus.Queued
            ? await _jobRepository.GetQueuePositionAsync(job.Id, cancellationToken)
            : (int?)null;

        return MapToResponse(job, queuePosition);
    }

    private static Video360JobResponse MapToResponse(Video360Job job, int? queuePosition = null)
    {
        return new Video360JobResponse
        {
            Id = job.Id,
            VehicleId = job.VehicleId,
            UserId = job.UserId,
            VideoUrl = job.VideoUrl,
            OriginalFileName = job.OriginalFileName,
            FileSizeBytes = job.FileSizeBytes,
            DurationSeconds = job.DurationSeconds,
            FramesToExtract = job.FramesToExtract,
            Status = job.Status,
            Progress = job.Progress,
            ErrorMessage = job.ErrorMessage,
            QueuePosition = queuePosition,
            ProcessingStartedAt = job.ProcessingStartedAt,
            ProcessingCompletedAt = job.ProcessingCompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs,
            CreatedAt = job.CreatedAt,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                Id = f.Id,
                SequenceNumber = f.SequenceNumber,
                AngleDegrees = f.AngleDegrees,
                ViewName = f.ViewName,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                Format = f.Format,
                QualityScore = f.QualityScore,
                IsPrimary = f.IsPrimary,
                TimestampSeconds = f.TimestampSeconds
            }).OrderBy(f => f.SequenceNumber).ToList()
        };
    }
}

/// <summary>
/// Handler para obtener el estado de un trabajo
/// </summary>
public class GetJobStatusHandler : IRequestHandler<GetJobStatusQuery, JobStatusResponse?>
{
    private readonly IVideo360JobRepository _jobRepository;

    public GetJobStatusHandler(IVideo360JobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobStatusResponse?> Handle(GetJobStatusQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null) return null;

        var queuePosition = job.Status == Video360JobStatus.Queued
            ? await _jobRepository.GetQueuePositionAsync(job.Id, cancellationToken)
            : (int?)null;

        return new JobStatusResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Progress = job.Progress,
            QueuePosition = queuePosition,
            ErrorMessage = job.ErrorMessage
        };
    }
}

/// <summary>
/// Handler para obtener trabajos de un vehículo
/// </summary>
public class GetJobsByVehicleHandler : IRequestHandler<GetJobsByVehicleQuery, IEnumerable<Video360JobResponse>>
{
    private readonly IVideo360JobRepository _jobRepository;

    public GetJobsByVehicleHandler(IVideo360JobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IEnumerable<Video360JobResponse>> Handle(GetJobsByVehicleQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        return jobs.Select(MapToResponse);
    }

    private static Video360JobResponse MapToResponse(Video360Job job)
    {
        return new Video360JobResponse
        {
            Id = job.Id,
            VehicleId = job.VehicleId,
            UserId = job.UserId,
            VideoUrl = job.VideoUrl,
            OriginalFileName = job.OriginalFileName,
            FileSizeBytes = job.FileSizeBytes,
            DurationSeconds = job.DurationSeconds,
            FramesToExtract = job.FramesToExtract,
            Status = job.Status,
            Progress = job.Progress,
            ErrorMessage = job.ErrorMessage,
            ProcessingStartedAt = job.ProcessingStartedAt,
            ProcessingCompletedAt = job.ProcessingCompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs,
            CreatedAt = job.CreatedAt,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                Id = f.Id,
                SequenceNumber = f.SequenceNumber,
                AngleDegrees = f.AngleDegrees,
                ViewName = f.ViewName,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                Format = f.Format,
                QualityScore = f.QualityScore,
                IsPrimary = f.IsPrimary,
                TimestampSeconds = f.TimestampSeconds
            }).OrderBy(f => f.SequenceNumber).ToList()
        };
    }
}

/// <summary>
/// Handler para obtener los datos del viewer 360
/// </summary>
public class GetVehicle360ViewerHandler : IRequestHandler<GetVehicle360ViewerQuery, Vehicle360ViewerResponse?>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly ILogger<GetVehicle360ViewerHandler> _logger;

    public GetVehicle360ViewerHandler(
        IVideo360JobRepository jobRepository,
        ILogger<GetVehicle360ViewerHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<Vehicle360ViewerResponse?> Handle(GetVehicle360ViewerQuery request, CancellationToken cancellationToken)
    {
        // Buscar el último trabajo completado para este vehículo
        var jobs = await _jobRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        var completedJob = jobs
            .Where(j => j.Status == Video360JobStatus.Completed)
            .OrderByDescending(j => j.ProcessingCompletedAt)
            .FirstOrDefault();

        if (completedJob == null)
        {
            _logger.LogDebug("No se encontró trabajo 360 completado para vehículo {VehicleId}", request.VehicleId);
            return null;
        }

        // Cargar frames si no están cargados
        if (!completedJob.ExtractedFrames.Any())
        {
            completedJob = await _jobRepository.GetByIdWithFramesAsync(completedJob.Id, cancellationToken);
        }

        var primaryFrame = completedJob!.ExtractedFrames.FirstOrDefault(f => f.IsPrimary);

        return new Vehicle360ViewerResponse
        {
            VehicleId = request.VehicleId,
            JobId = completedJob.Id,
            PrimaryImageUrl = primaryFrame?.ImageUrl,
            ProcessedAt = completedJob.ProcessingCompletedAt ?? completedJob.CreatedAt,
            Frames = completedJob.ExtractedFrames
                .OrderBy(f => f.SequenceNumber)
                .Select(f => new ViewerFrameResponse
                {
                    Index = f.SequenceNumber - 1,
                    Angle = f.AngleDegrees,
                    Name = f.ViewName,
                    ImageUrl = f.ImageUrl ?? string.Empty,
                    ThumbnailUrl = f.ThumbnailUrl
                }).ToList()
        };
    }
}
