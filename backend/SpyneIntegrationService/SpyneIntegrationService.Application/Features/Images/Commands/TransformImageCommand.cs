using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Images.Commands;

/// <summary>
/// Command to transform a single image using Spyne AI
/// </summary>
public record TransformImageCommand : IRequest<ImageTransformationDto>
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string OriginalImageUrl { get; init; } = string.Empty;
    
    /// <summary>Alias for OriginalImageUrl</summary>
    public string SourceImageUrl 
    { 
        get => OriginalImageUrl; 
        init => OriginalImageUrl = value; 
    }
    
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Showroom;
    
    /// <summary>Type of transformation</summary>
    public TransformationType TransformationType { get; init; } = TransformationType.Background;
    
    public string? CustomBackgroundId { get; init; }
    public bool MaskLicensePlate { get; init; } = true;
    public bool EnhanceQuality { get; init; } = true;
}
