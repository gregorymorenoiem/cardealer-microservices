using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Queries;
using Video360Service.Application.Interfaces;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Application.Features.Handlers;

/// <summary>
/// Handler para obtener un job por ID
/// </summary>
public class GetVideo360JobByIdQueryHandler : IRequestHandler<GetVideo360JobByIdQuery, Video360JobResponse?>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly ILogger<GetVideo360JobByIdQueryHandler> _logger;

    public GetVideo360JobByIdQueryHandler(
        IVideo360JobRepository jobRepository,
        ILogger<GetVideo360JobByIdQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<Video360JobResponse?> Handle(GetVideo360JobByIdQuery query, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdWithFramesAsync(query.JobId, cancellationToken);
        
        if (job == null)
        {
            _logger.LogDebug("Job {JobId} not found", query.JobId);
            return null;
        }
        
        // Verificar ownership si hay UserId
        if (query.UserId.HasValue && job.UserId != query.UserId)
        {
            _logger.LogWarning("User {UserId} not authorized to view job {JobId}", query.UserId, query.JobId);
            return null;
        }
        
        return MapToResponse(job);
    }
    
    private static Video360JobResponse MapToResponse(Video360Job job)
    {
        return new Video360JobResponse
        {
            JobId = job.Id,
            VehicleId = job.VehicleId,
            Status = job.Status,
            Provider = job.Provider,
            SourceVideoUrl = job.SourceVideoUrl,
            VideoDurationSeconds = job.VideoDurationSeconds,
            VideoResolution = job.VideoResolution,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                FrameId = f.Id,
                Index = f.FrameIndex,
                AngleDegrees = f.AngleDegrees,
                AngleLabel = f.AngleLabel ?? ExtractedFrame.GetAngleLabelByIndex(f.FrameIndex),
                TimestampSeconds = f.TimestampSeconds,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                FileSizeBytes = f.FileSizeBytes,
                ContentType = f.ContentType,
                Width = f.Width,
                Height = f.Height
            }).OrderBy(f => f.Index).ToList(),
            ProcessingTimeMs = job.ProcessingTimeMs,
            CostUsd = job.CostUsd,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            Metadata = job.MetadataJson != null 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(job.MetadataJson) 
                : null
        };
    }
}

/// <summary>
/// Handler para listar jobs
/// </summary>
public class GetVideo360JobsQueryHandler : IRequestHandler<GetVideo360JobsQuery, Video360JobListResponse>
{
    private readonly IVideo360JobRepository _jobRepository;

    public GetVideo360JobsQueryHandler(IVideo360JobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Video360JobListResponse> Handle(GetVideo360JobsQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<Video360Job> jobs;
        int totalCount;
        
        if (query.VehicleId.HasValue)
        {
            jobs = await _jobRepository.GetByVehicleIdAsync(query.VehicleId.Value, cancellationToken);
            totalCount = jobs.Count();
        }
        else if (query.Status.HasValue)
        {
            jobs = await _jobRepository.GetByStatusAsync(query.Status.Value, query.PageSize, cancellationToken);
            totalCount = await _jobRepository.GetCountByStatusAsync(query.Status.Value, cancellationToken);
        }
        else if (query.UserId.HasValue)
        {
            jobs = await _jobRepository.GetByUserIdAsync(query.UserId.Value, query.Page, query.PageSize, cancellationToken);
            totalCount = await _jobRepository.GetTotalCountAsync(query.UserId, cancellationToken);
        }
        else
        {
            jobs = await _jobRepository.GetByUserIdAsync(Guid.Empty, query.Page, query.PageSize, cancellationToken);
            totalCount = await _jobRepository.GetTotalCountAsync(null, cancellationToken);
        }
        
        return new Video360JobListResponse
        {
            Items = jobs.Select(j => new Video360JobResponse
            {
                JobId = j.Id,
                VehicleId = j.VehicleId,
                Status = j.Status,
                Provider = j.Provider,
                SourceVideoUrl = j.SourceVideoUrl,
                VideoDurationSeconds = j.VideoDurationSeconds,
                VideoResolution = j.VideoResolution,
                Frames = j.ExtractedFrames.Select(f => new ExtractedFrameResponse
                {
                    FrameId = f.Id,
                    Index = f.FrameIndex,
                    AngleDegrees = f.AngleDegrees,
                    AngleLabel = f.AngleLabel ?? ExtractedFrame.GetAngleLabelByIndex(f.FrameIndex),
                    TimestampSeconds = f.TimestampSeconds,
                    ImageUrl = f.ImageUrl,
                    ThumbnailUrl = f.ThumbnailUrl,
                    FileSizeBytes = f.FileSizeBytes,
                    ContentType = f.ContentType,
                    Width = f.Width,
                    Height = f.Height
                }).OrderBy(f => f.Index).ToList(),
                ProcessingTimeMs = j.ProcessingTimeMs,
                CostUsd = j.CostUsd,
                ErrorMessage = j.ErrorMessage,
                ErrorCode = j.ErrorCode,
                CreatedAt = j.CreatedAt,
                CompletedAt = j.CompletedAt
            }),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}

/// <summary>
/// Handler para obtener vista 360 de un vehículo
/// </summary>
public class GetVehicle360ViewQueryHandler : IRequestHandler<GetVehicle360ViewQuery, Vehicle360ViewResponse?>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly ILogger<GetVehicle360ViewQueryHandler> _logger;

    public GetVehicle360ViewQueryHandler(
        IVideo360JobRepository jobRepository,
        ILogger<GetVehicle360ViewQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<Vehicle360ViewResponse?> Handle(GetVehicle360ViewQuery query, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetByVehicleIdAsync(query.VehicleId, cancellationToken);
        
        // Obtener el job completado más reciente
        var completedJob = jobs
            .Where(j => j.Status == Domain.Enums.ProcessingStatus.Completed)
            .OrderByDescending(j => j.CompletedAt)
            .FirstOrDefault();
        
        if (completedJob == null)
        {
            _logger.LogDebug("No completed 360 view found for vehicle {VehicleId}", query.VehicleId);
            
            // Retornar el último job en proceso si existe
            var latestJob = jobs.OrderByDescending(j => j.CreatedAt).FirstOrDefault();
            if (latestJob != null)
            {
                return new Vehicle360ViewResponse
                {
                    VehicleId = query.VehicleId,
                    JobId = latestJob.Id,
                    Status = latestJob.Status,
                    Frames = [],
                    CreatedAt = latestJob.CreatedAt
                };
            }
            
            return null;
        }
        
        return new Vehicle360ViewResponse
        {
            VehicleId = query.VehicleId,
            JobId = completedJob.Id,
            Status = completedJob.Status,
            Frames = completedJob.ExtractedFrames
                .OrderBy(f => f.FrameIndex)
                .Select(f => new Frame360Data
                {
                    Index = f.FrameIndex,
                    Angle = f.AngleDegrees,
                    Label = f.AngleLabel ?? ExtractedFrame.GetAngleLabelByIndex(f.FrameIndex),
                    Url = f.ImageUrl,
                    Thumbnail = f.ThumbnailUrl
                }).ToList(),
            CreatedAt = completedJob.CreatedAt,
            ExpiresAt = completedJob.ExpiresAt
        };
    }
}

/// <summary>
/// Handler para obtener información de proveedores
/// </summary>
public class GetProvidersInfoQueryHandler : IRequestHandler<GetProvidersInfoQuery, IEnumerable<ProviderInfoResponse>>
{
    private readonly IVideo360ProviderFactory _providerFactory;
    private readonly IProviderConfigurationRepository _configRepository;

    public GetProvidersInfoQueryHandler(
        IVideo360ProviderFactory providerFactory,
        IProviderConfigurationRepository configRepository)
    {
        _providerFactory = providerFactory;
        _configRepository = configRepository;
    }

    public async Task<IEnumerable<ProviderInfoResponse>> Handle(GetProvidersInfoQuery query, CancellationToken cancellationToken)
    {
        var providers = query.OnlyAvailable 
            ? await _providerFactory.GetAvailableProvidersAsync(cancellationToken)
            : _providerFactory.GetAllProviders();
        
        var configs = await _configRepository.GetAllAsync(cancellationToken);
        var configDict = configs.ToDictionary(c => c.Provider);
        
        var result = new List<ProviderInfoResponse>();
        
        foreach (var provider in providers)
        {
            var isAvailable = await provider.IsAvailableAsync(cancellationToken);
            var accountInfo = isAvailable ? await provider.GetAccountInfoAsync(cancellationToken) : null;
            
            configDict.TryGetValue(provider.ProviderType, out var config);
            
            result.Add(new ProviderInfoResponse
            {
                Provider = provider.ProviderType,
                Name = provider.ProviderName,
                IsEnabled = config?.IsEnabled ?? true,
                IsAvailable = isAvailable,
                CostPerVideoUsd = provider.CostPerVideoUsd,
                RemainingCredits = accountInfo?.RemainingCredits,
                DailyLimit = config?.DailyLimit ?? 0,
                DailyUsageCount = config?.DailyUsageCount ?? 0,
                SupportedFormats = config?.SupportedFormats.Split(',') ?? ["mp4", "webm", "mov"],
                MaxVideoSizeMb = config?.MaxVideoSizeMb ?? 100,
                MaxVideoDurationSeconds = config?.MaxVideoDurationSeconds ?? 120
            });
        }
        
        return result.OrderBy(p => p.CostPerVideoUsd);
    }
}
