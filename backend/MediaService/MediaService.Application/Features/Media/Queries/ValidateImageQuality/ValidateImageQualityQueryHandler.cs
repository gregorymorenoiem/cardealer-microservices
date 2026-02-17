using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Queries.ValidateImageQuality;

public class ValidateImageQualityQueryHandler
    : IRequestHandler<ValidateImageQualityQuery, ApiResponse<ImageQualityResult>>
{
    private readonly IImageProcessor _imageProcessor;
    private readonly ILogger<ValidateImageQualityQueryHandler> _logger;

    private const int MinWidth = 640;
    private const int MinHeight = 480;
    private const double MinAspectRatio = 0.5;
    private const double MaxAspectRatio = 3.0;

    public ValidateImageQualityQueryHandler(
        IImageProcessor imageProcessor,
        ILogger<ValidateImageQualityQueryHandler> logger)
    {
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    public async Task<ApiResponse<ImageQualityResult>> Handle(
        ValidateImageQualityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = request.File.OpenReadStream();
            var imageInfo = await _imageProcessor.GetImageInfoAsync(stream);

            var warnings = new List<string>();
            var suggestions = new List<string>();
            var score = 100.0;

            // Resolution check
            var isTooSmall = imageInfo.Width < MinWidth || imageInfo.Height < MinHeight;
            if (isTooSmall)
            {
                score -= 30;
                warnings.Add($"Resolución baja ({imageInfo.Width}×{imageInfo.Height})");
                suggestions.Add("Usa una cámara con mayor resolución o acércate menos al vehículo");
            }

            // Aspect ratio check
            var aspectRatio = imageInfo.Height > 0 ? (double)imageInfo.Width / imageInfo.Height : 0;
            if (aspectRatio < MinAspectRatio || aspectRatio > MaxAspectRatio)
            {
                score -= 10;
                warnings.Add("Relación de aspecto inusual para fotos de vehículos");
                suggestions.Add("Usa formato horizontal (landscape) para mejores resultados");
            }

            // File size heuristic — very small files are likely low quality
            if (request.File.Length < 50_000) // < 50KB
            {
                score -= 20;
                warnings.Add("Archivo muy pequeño — posiblemente baja calidad");
                suggestions.Add("Usa la resolución máxima de tu cámara");
            }

            // Very large dimension without proportional file size = likely blank/uniform
            var megapixels = (imageInfo.Width * imageInfo.Height) / 1_000_000.0;
            var bytesPerMegapixel = request.File.Length / Math.Max(megapixels, 0.1);
            if (bytesPerMegapixel < 20_000 && megapixels > 1)
            {
                score -= 15;
                warnings.Add("Imagen con posible contenido uniforme o baja complejidad");
            }

            // Ideal resolution bonus
            if (imageInfo.Width >= 1920 && imageInfo.Height >= 1080)
            {
                score = Math.Min(100, score + 5);
            }

            // Good aspect ratio bonus (4:3 or 16:9 — common for vehicle photos)
            if (Math.Abs(aspectRatio - 1.33) < 0.1 || Math.Abs(aspectRatio - 1.78) < 0.1)
            {
                score = Math.Min(100, score + 5);
            }

            // Add general suggestions
            if (score >= 80 && suggestions.Count == 0)
            {
                suggestions.Add("¡Excelente calidad de imagen!");
            }
            else if (score < 50)
            {
                suggestions.Add("Considera tomar la foto nuevamente con mejor iluminación y mayor resolución");
            }

            var result = new ImageQualityResult
            {
                OverallScore = Math.Max(0, Math.Min(100, score)),
                IsBlurry = false, // Basic heuristic — full blur detection requires more processing
                IsTooDark = false,
                IsTooBright = false,
                IsTooSmall = isTooSmall,
                HasExifOrientation = false,
                Width = imageInfo.Width,
                Height = imageInfo.Height,
                AspectRatio = Math.Round(aspectRatio, 2),
                Warnings = warnings.ToArray(),
                Suggestions = suggestions.ToArray()
            };

            return ApiResponse<ImageQualityResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image quality");
            return ApiResponse<ImageQualityResult>.Fail("Error al validar la calidad de la imagen");
        }
    }
}
