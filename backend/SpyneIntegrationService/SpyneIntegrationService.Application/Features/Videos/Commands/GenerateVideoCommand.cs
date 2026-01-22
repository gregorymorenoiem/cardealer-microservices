using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Videos.Commands;

/// <summary>
/// Command to generate video tour
/// </summary>
public record GenerateVideoCommand : IRequest<GenerateVideoResponse>
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public VideoStyle Style { get; init; } = VideoStyle.Cinematic;
    public VideoFormat OutputFormat { get; init; } = VideoFormat.Mp4_1080p;
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
    public bool IncludeMusic { get; init; } = true;
    public string? MusicTrackId { get; init; }
    public int? DurationSeconds { get; init; }
}
