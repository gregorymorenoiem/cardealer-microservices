using MediaService.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MediaService.Application.Features.Media.Commands.UploadVehicleImage;

/// <summary>
/// Command to upload a single vehicle image with optional compression and metadata.
/// </summary>
public class UploadVehicleImageCommand : IRequest<ApiResponse<VehicleImageUploadResponse>>
{
    public IFormFile File { get; set; } = null!;
    public string OwnerId { get; set; } = string.Empty;
    public Guid DealerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string? ImageType { get; set; } // Exterior, Interior, Engine, etc.
    public int? SortOrder { get; set; }
    public bool? IsPrimary { get; set; }
    public bool Compress { get; set; } = true;
}
