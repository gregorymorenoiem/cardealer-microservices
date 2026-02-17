using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Spins.Queries;

public class GetSpinStatusQueryHandler : IRequestHandler<GetSpinStatusQuery, SpinGenerationDto?>
{
    private readonly ISpinGenerationRepository _repository;

    public GetSpinStatusQueryHandler(ISpinGenerationRepository repository)
    {
        _repository = repository;
    }

    public async Task<SpinGenerationDto?> Handle(GetSpinStatusQuery request, CancellationToken cancellationToken)
    {
        var spin = await _repository.GetByIdAsync(request.SpinId, cancellationToken);
        
        if (spin == null)
            return null;

        return MapToDto(spin);
    }

    private static SpinGenerationDto MapToDto(SpinGeneration s) => new()
    {
        Id = s.Id,
        VehicleId = s.VehicleId,
        DealerId = s.DealerId,
        SpyneJobId = s.SpyneJobId,
        SourceImageUrls = s.SourceImageUrls,
        SpinViewerUrl = s.SpinViewerUrl,
        SpinEmbedCode = s.SpinEmbedCode,
        FallbackImageUrl = s.FallbackImageUrl,
        BackgroundPreset = s.BackgroundPreset,
        RequiredImageCount = s.RequiredImageCount,
        ReceivedImageCount = s.ReceivedImageCount,
        Status = s.Status,
        ErrorMessage = s.ErrorMessage,
        ProcessingTimeMs = s.ProcessingTimeMs,
        CreatedAt = s.CreatedAt,
        CompletedAt = s.CompletedAt
    };
}
