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

    // ── Resolution thresholds ──
    private const int MinWidth = 640;
    private const int MinHeight = 480;
    private const double MinAspectRatio = 0.5;
    private const double MaxAspectRatio = 3.0;

    // ── Blur/brightness heuristic thresholds ──
    // Low bytes-per-megapixel = uniform/blurred (JPEG compresses blurry images a LOT)
    private const double BlurBytesPerMpThreshold = 15_000;
    // Very low bytes-per-megapixel with large dimensions = possibly too dark/uniform
    private const double DarkBytesPerMpThreshold = 10_000;
    // Extremely high bytes-per-megapixel often indicates overexposure noise
    private const double BrightBytesPerMpThreshold = 500_000;

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

            // ── Resolution check ──
            var isTooSmall = imageInfo.Width < MinWidth || imageInfo.Height < MinHeight;
            if (isTooSmall)
            {
                score -= 30;
                warnings.Add($"Resolución baja ({imageInfo.Width}×{imageInfo.Height})");
                suggestions.Add("Usa una cámara con mayor resolución o acércate menos al vehículo");
            }

            // ── Aspect ratio check ──
            var aspectRatio = imageInfo.Height > 0 ? (double)imageInfo.Width / imageInfo.Height : 0;
            if (aspectRatio < MinAspectRatio || aspectRatio > MaxAspectRatio)
            {
                score -= 10;
                warnings.Add("Relación de aspecto inusual para fotos de vehículos");
                suggestions.Add("Usa formato horizontal (landscape) para mejores resultados");
            }

            // ── File size heuristic ──
            if (request.File.Length < 50_000) // < 50KB
            {
                score -= 20;
                warnings.Add("Archivo muy pequeño — posiblemente baja calidad");
                suggestions.Add("Usa la resolución máxima de tu cámara");
            }

            // ── Compute bytes per megapixel for blur/brightness heuristics ──
            var megapixels = (imageInfo.Width * imageInfo.Height) / 1_000_000.0;
            var bytesPerMegapixel = megapixels > 0.1 ? request.File.Length / megapixels : 0;

            // ── Too dark detection (heuristic — extreme compression = dark/uniform) ──
            // Very dark images compress to very few bytes because most pixels are near-black
            var isTooDark = megapixels >= 0.5 && bytesPerMegapixel < DarkBytesPerMpThreshold;
            if (isTooDark)
            {
                score -= 20;
                warnings.Add("Imagen posiblemente demasiado oscura");
                suggestions.Add("Toma las fotos con buena iluminación, preferiblemente con luz natural");
            }

            // ── Blur detection (heuristic — moderate compression = blur) ──
            // JPEG compression is effective on blurry images (low detail),
            // so a blurry image has low bytes-per-megapixel ratio (but above dark threshold)
            var isBlurry = megapixels >= 0.5 && bytesPerMegapixel < BlurBytesPerMpThreshold && !isTooDark;
            if (isBlurry)
            {
                score -= 25;
                warnings.Add("Imagen posiblemente borrosa (desenfocada)");
                suggestions.Add("Asegúrate de que la cámara esté enfocada y estable antes de tomar la foto");
            }

            // ── Too bright detection (heuristic) ──
            // Overexposed images contain noise that inflates file size disproportionately
            var isTooBright = megapixels >= 0.5 && bytesPerMegapixel > BrightBytesPerMpThreshold;
            if (isTooBright)
            {
                score -= 15;
                warnings.Add("Imagen posiblemente sobreexpuesta (demasiado clara)");
                suggestions.Add("Evita fotografiar directamente contra el sol o con flash demasiado fuerte");
            }

            // ── Uniform content detection (existing) ──
            if (bytesPerMegapixel < 20_000 && megapixels > 1 && !isBlurry && !isTooDark)
            {
                score -= 15;
                warnings.Add("Imagen con posible contenido uniforme o baja complejidad");
            }

            // ── Ideal resolution bonus ──
            if (imageInfo.Width >= 1920 && imageInfo.Height >= 1080)
            {
                score = Math.Min(100, score + 5);
            }

            // ── Good aspect ratio bonus (4:3 or 16:9) ──
            if (Math.Abs(aspectRatio - 1.33) < 0.1 || Math.Abs(aspectRatio - 1.78) < 0.1)
            {
                score = Math.Min(100, score + 5);
            }

            // ── EXIF orientation detection ──
            // If format info indicates orientation data, note it
            var hasExifOrientation = !string.IsNullOrEmpty(imageInfo.Format) &&
                                     imageInfo.Format.Contains("jpeg", StringComparison.OrdinalIgnoreCase);

            // ── Summary suggestions ──
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
                IsBlurry = isBlurry,
                IsTooDark = isTooDark,
                IsTooBright = isTooBright,
                IsTooSmall = isTooSmall,
                HasExifOrientation = hasExifOrientation,
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
