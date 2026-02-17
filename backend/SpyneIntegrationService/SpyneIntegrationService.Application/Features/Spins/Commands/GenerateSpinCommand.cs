using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Spins.Commands;

/// <summary>
/// Command to generate a 360° spin
/// </summary>
public record GenerateSpinCommand : IRequest<GenerateSpinResponse>
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
}
