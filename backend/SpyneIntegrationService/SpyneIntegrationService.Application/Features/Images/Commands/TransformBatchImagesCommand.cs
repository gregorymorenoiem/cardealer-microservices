using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Images.Commands;

/// <summary>
/// Command to transform multiple images in batch
/// </summary>
public record TransformBatchImagesCommand : IRequest<TransformBatchResponse>
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    
    /// <summary>Alias for ImageUrls</summary>
    public List<string> SourceImageUrls 
    { 
        get => ImageUrls; 
        init => ImageUrls = value; 
    }
    
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Showroom;
    
    /// <summary>Type of transformation</summary>
    public TransformationType TransformationType { get; init; } = TransformationType.Background;
    
    public string? CustomBackgroundId { get; init; }
    public bool MaskLicensePlate { get; init; } = true;
    public bool EnhanceQuality { get; init; } = true;
}
