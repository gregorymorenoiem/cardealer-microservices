using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Video360Spins.Queries;

/// <summary>
/// Query to get 360° spin status by ID
/// </summary>
public record GetVideo360SpinStatusQuery(Guid SpinId) : IRequest<Video360SpinStatusResponse?>;

/// <summary>
/// Handler for GetVideo360SpinStatusQuery
/// </summary>
public class GetVideo360SpinStatusQueryHandler : IRequestHandler<GetVideo360SpinStatusQuery, Video360SpinStatusResponse?>
{
    private readonly IVideo360SpinRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<GetVideo360SpinStatusQueryHandler> _logger;

    public GetVideo360SpinStatusQueryHandler(
        IVideo360SpinRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<GetVideo360SpinStatusQueryHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<Video360SpinStatusResponse?> Handle(
        GetVideo360SpinStatusQuery request, 
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.SpinId, cancellationToken);
        
        if (entity == null)
        {
            _logger.LogWarning("Video360Spin not found: {SpinId}", request.SpinId);
            return null;
        }

        // If still processing, check Spyne API for updates
        if (entity.Status == TransformationStatus.Processing && !string.IsNullOrEmpty(entity.SpyneJobId))
        {
            try
            {
                _logger.LogDebug("Checking Spyne API status for job: {JobId}", entity.SpyneJobId);
                
                var spyneResult = await _spyneClient.GetVehicleMediaAsync(entity.SpyneJobId, cancellationToken);
                
                if (spyneResult != null)
                {
                    // Check if spin processing is complete
                    if (spyneResult.MediaData?.Spin?.SpinAiStatus == "DONE" ||
                        spyneResult.Status == "completed")
                    {
                        // Extract frame URLs from image data
                        var extractedFrames = spyneResult.MediaData?.Image?.ImageData?
                            .Where(i => !string.IsNullOrEmpty(i.OutputImage))
                            .Select(i => i.OutputImage!)
                            .ToList() ?? new List<string>();

                        entity.MarkAsCompleted(
                            spinViewerUrl: spyneResult.MediaData?.Spin?.EmbedUrl ?? "",
                            extractedFrameUrls: extractedFrames,
                            thumbnailUrl: extractedFrames.FirstOrDefault()
                        );
                        
                        await _repository.UpdateAsync(entity, cancellationToken);
                        
                        _logger.LogInformation(
                            "Video360Spin completed. SpinId: {SpinId}, Frames: {FrameCount}", 
                            entity.Id, 
                            extractedFrames.Count);
                    }
                    else if (spyneResult.Status == "failed" || 
                             spyneResult.MediaData?.Spin?.SpinAiStatus == "FAILED")
                    {
                        entity.MarkAsFailed(spyneResult.ErrorMessage ?? "Spyne processing failed");
                        await _repository.UpdateAsync(entity, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check Spyne status for job: {JobId}", entity.SpyneJobId);
                // Don't fail the request, just return current status
            }
        }

        return entity.ToStatusResponse();
    }
}

/// <summary>
/// Query to get 360° spin by vehicle ID
/// </summary>
public record GetVideo360SpinByVehicleQuery(Guid VehicleId) : IRequest<Video360SpinDto?>;

/// <summary>
/// Handler for GetVideo360SpinByVehicleQuery
/// </summary>
public class GetVideo360SpinByVehicleQueryHandler : IRequestHandler<GetVideo360SpinByVehicleQuery, Video360SpinDto?>
{
    private readonly IVideo360SpinRepository _repository;

    public GetVideo360SpinByVehicleQueryHandler(IVideo360SpinRepository repository)
    {
        _repository = repository;
    }

    public async Task<Video360SpinDto?> Handle(
        GetVideo360SpinByVehicleQuery request, 
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetLatestByVehicleIdAsync(request.VehicleId, cancellationToken);
        return entity?.ToDto();
    }
}

/// <summary>
/// Query to get all 360° spins for a vehicle
/// </summary>
public record GetVideo360SpinsByVehicleQuery(Guid VehicleId) : IRequest<List<Video360SpinDto>>;

/// <summary>
/// Handler for GetVideo360SpinsByVehicleQuery
/// </summary>
public class GetVideo360SpinsByVehicleQueryHandler : IRequestHandler<GetVideo360SpinsByVehicleQuery, List<Video360SpinDto>>
{
    private readonly IVideo360SpinRepository _repository;

    public GetVideo360SpinsByVehicleQueryHandler(IVideo360SpinRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Video360SpinDto>> Handle(
        GetVideo360SpinsByVehicleQuery request, 
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        return entities.Select(e => e.ToDto()).ToList();
    }
}

/// <summary>
/// Query to get all 360° spins for a dealer
/// </summary>
public record GetVideo360SpinsByDealerQuery(Guid DealerId) : IRequest<List<Video360SpinDto>>;

/// <summary>
/// Handler for GetVideo360SpinsByDealerQuery
/// </summary>
public class GetVideo360SpinsByDealerQueryHandler : IRequestHandler<GetVideo360SpinsByDealerQuery, List<Video360SpinDto>>
{
    private readonly IVideo360SpinRepository _repository;

    public GetVideo360SpinsByDealerQueryHandler(IVideo360SpinRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Video360SpinDto>> Handle(
        GetVideo360SpinsByDealerQuery request, 
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        return entities.Select(e => e.ToDto()).ToList();
    }
}
