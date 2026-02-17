using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Infrastructure.Persistence;

namespace SpyneIntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PresetsController : ControllerBase
{
    private readonly SpyneDbContext _context;

    public PresetsController(SpyneDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all available background presets
    /// </summary>
    /// <remarks>
    /// Returns the list of background presets that can be used for image transformations,
    /// 360 spins, and video generation.
    /// </remarks>
    [HttpGet("backgrounds")]
    [ProducesResponseType(typeof(List<BackgroundPresetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBackgroundPresets()
    {
        var presets = await _context.BackgroundPresetConfigs
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new BackgroundPresetResponse
            {
                Preset = x.Preset,
                DisplayName = x.DisplayName,
                Description = x.Description,
                ThumbnailUrl = x.ThumbnailUrl,
                Category = x.Category
            })
            .ToListAsync();

        // If no presets in DB, return default enum values
        if (!presets.Any())
        {
            presets = Enum.GetValues<BackgroundPreset>()
                .Where(p => p != BackgroundPreset.Custom)
                .Select(p => new BackgroundPresetResponse
                {
                    Preset = p,
                    DisplayName = p.ToString(),
                    Description = GetDefaultDescription(p),
                    ThumbnailUrl = null,
                    Category = "Standard"
                })
                .ToList();
        }

        return Ok(presets);
    }

    /// <summary>
    /// Get available video styles
    /// </summary>
    [HttpGet("video-styles")]
    [ProducesResponseType(typeof(List<VideoStyleResponse>), StatusCodes.Status200OK)]
    public IActionResult GetVideoStyles()
    {
        var styles = Enum.GetValues<VideoStyle>()
            .Select(s => new VideoStyleResponse
            {
                Style = s,
                DisplayName = s.ToString(),
                Description = GetVideoStyleDescription(s)
            })
            .ToList();

        return Ok(styles);
    }

    /// <summary>
    /// Get available video output formats
    /// </summary>
    [HttpGet("video-formats")]
    [ProducesResponseType(typeof(List<VideoFormatResponse>), StatusCodes.Status200OK)]
    public IActionResult GetVideoFormats()
    {
        var formats = Enum.GetValues<VideoFormat>()
            .Select(f => new VideoFormatResponse
            {
                Format = f,
                DisplayName = f.ToString().Replace("_", " "),
                Resolution = GetResolution(f),
                FileType = f.ToString().Split('_')[0].ToUpper()
            })
            .ToList();

        return Ok(formats);
    }

    /// <summary>
    /// Get available transformation types
    /// </summary>
    [HttpGet("transformation-types")]
    [ProducesResponseType(typeof(List<TransformationTypeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetTransformationTypes()
    {
        var types = Enum.GetValues<TransformationType>()
            .Select(t => new TransformationTypeResponse
            {
                Type = t,
                DisplayName = GetTransformationDisplayName(t),
                Description = GetTransformationDescription(t)
            })
            .ToList();

        return Ok(types);
    }

    private static string GetDefaultDescription(BackgroundPreset preset) => preset switch
    {
        BackgroundPreset.Showroom => "Professional showroom ambiente con iluminación de estudio",
        BackgroundPreset.Outdoor => "Escenario exterior con luz natural",
        BackgroundPreset.Studio => "Fondo de estudio limpio y profesional",
        BackgroundPreset.White => "Fondo blanco puro para máxima claridad",
        BackgroundPreset.Urban => "Ambiente urbano moderno",
        BackgroundPreset.Luxury => "Ambiente premium de lujo",
        BackgroundPreset.Transparent => "Fondo transparente (PNG)",
        _ => "Fondo personalizado"
    };

    private static string GetVideoStyleDescription(VideoStyle style) => style switch
    {
        VideoStyle.Cinematic => "Transiciones suaves con efecto cinematográfico",
        VideoStyle.Dynamic => "Cortes rápidos y energéticos",
        VideoStyle.Showcase => "Presentación detallada del vehículo",
        VideoStyle.Social => "Optimizado para redes sociales (formato vertical disponible)",
        VideoStyle.Premium => "Producción de alta calidad con efectos premium",
        _ => "Estilo estándar"
    };

    private static string GetResolution(VideoFormat format) => format switch
    {
        VideoFormat.Mp4_720p or VideoFormat.Webm_720p => "1280x720",
        VideoFormat.Mp4_1080p or VideoFormat.Webm_1080p => "1920x1080",
        VideoFormat.Mp4_4K => "3840x2160",
        _ => "1920x1080"
    };

    private static string GetTransformationDisplayName(TransformationType type) => type switch
    {
        TransformationType.BackgroundRemoval => "Remover Fondo",
        TransformationType.BackgroundReplacement => "Reemplazar Fondo",
        TransformationType.Enhancement => "Mejora de Imagen",
        TransformationType.PlateMasking => "Enmascarar Placa",
        TransformationType.ShadowAdd => "Agregar Sombra",
        TransformationType.ColorCorrection => "Corrección de Color",
        _ => type.ToString()
    };

    private static string GetTransformationDescription(TransformationType type) => type switch
    {
        TransformationType.BackgroundRemoval => "Elimina el fondo de la imagen dejando solo el vehículo",
        TransformationType.BackgroundReplacement => "Reemplaza el fondo con un escenario profesional",
        TransformationType.Enhancement => "Mejora la calidad, iluminación y nitidez de la imagen",
        TransformationType.PlateMasking => "Oculta la placa del vehículo por privacidad",
        TransformationType.ShadowAdd => "Agrega sombras realistas al vehículo",
        TransformationType.ColorCorrection => "Corrige los colores para mayor fidelidad",
        _ => "Transformación de imagen"
    };
}

// Response DTOs
public record BackgroundPresetResponse
{
    public BackgroundPreset Preset { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? Category { get; init; }
}

public record VideoStyleResponse
{
    public VideoStyle Style { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record VideoFormatResponse
{
    public VideoFormat Format { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Resolution { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
}

public record TransformationTypeResponse
{
    public TransformationType Type { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
}
