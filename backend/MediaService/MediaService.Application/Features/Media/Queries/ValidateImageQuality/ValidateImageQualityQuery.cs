using MediaService.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MediaService.Application.Features.Media.Queries.ValidateImageQuality;

public class ValidateImageQualityQuery : IRequest<ApiResponse<ImageQualityResult>>
{
    public IFormFile File { get; set; } = null!;
}

public class ImageQualityResult
{
    public double OverallScore { get; set; }
    public bool IsBlurry { get; set; }
    public bool IsTooDark { get; set; }
    public bool IsTooBright { get; set; }
    public bool IsTooSmall { get; set; }
    public bool HasExifOrientation { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double AspectRatio { get; set; }
    public string[] Warnings { get; set; } = Array.Empty<string>();
    public string[] Suggestions { get; set; } = Array.Empty<string>();
}
