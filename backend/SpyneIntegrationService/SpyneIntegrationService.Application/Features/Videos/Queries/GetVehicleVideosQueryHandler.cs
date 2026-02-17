using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Videos.Queries;

public class GetVehicleVideosQueryHandler : IRequestHandler<GetVehicleVideosQuery, List<VideoGenerationDto>>
{
    private readonly IVideoGenerationRepository _repository;

    public GetVehicleVideosQueryHandler(IVideoGenerationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VideoGenerationDto>> Handle(GetVehicleVideosQuery request, CancellationToken cancellationToken)
    {
        var videos = await _repository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        
        return videos.Select(MapToDto).ToList();
    }

    private static VideoGenerationDto MapToDto(VideoGeneration v) => new()
    {
        Id = v.Id,
        VehicleId = v.VehicleId,
        DealerId = v.DealerId,
        SpyneJobId = v.SpyneJobId,
        VideoUrl = v.VideoUrl,
        ThumbnailUrl = v.ThumbnailUrl,
        DurationSeconds = v.DurationSeconds,
        Style = v.Style,
        OutputFormat = v.OutputFormat,
        BackgroundPreset = v.BackgroundPreset,
        Status = v.Status,
        ErrorMessage = v.ErrorMessage,
        ProcessingTimeMs = v.ProcessingTimeMs,
        FileSizeBytes = v.FileSizeBytes,
        CreditsCost = v.CreditsCost,
        CreatedAt = v.CreatedAt,
        CompletedAt = v.CompletedAt
    };
}
