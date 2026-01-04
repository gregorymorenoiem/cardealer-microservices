using Microsoft.AspNetCore.Mvc;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;

namespace VehiclesSaleService.Api.Controllers;

/// <summary>
/// Controller para el catálogo maestro de vehículos.
/// Proporciona datos para auto-completar formularios de publicación.
/// 
/// Flujo de uso:
/// 1. GET /catalog/makes - Lista de marcas
/// 2. GET /catalog/makes/{makeSlug}/models - Modelos de una marca
/// 3. GET /catalog/models/{modelId}/years - Años disponibles para un modelo
/// 4. GET /catalog/models/{modelId}/years/{year}/trims - Trims con specs completas
/// 5. El frontend auto-llena el formulario con las specs del trim seleccionado
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly IVehicleCatalogRepository _catalogRepository;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(
        IVehicleCatalogRepository catalogRepository,
        ILogger<CatalogController> logger)
    {
        _catalogRepository = catalogRepository;
        _logger = logger;
    }

    // ========================================
    // MAKES (Marcas)
    // ========================================

    /// <summary>
    /// Obtiene todas las marcas de vehículos activas.
    /// </summary>
    /// <remarks>
    /// Ejemplo: GET /api/catalog/makes
    /// Retorna: Toyota, Honda, Ford, Chevrolet, etc.
    /// Las marcas populares aparecen primero.
    /// </remarks>
    [HttpGet("makes")]
    [ProducesResponseType(typeof(IEnumerable<MakeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MakeDto>>> GetAllMakes()
    {
        var makes = await _catalogRepository.GetAllMakesAsync();
        var dtos = makes.Select(m => new MakeDto
        {
            Id = m.Id,
            Name = m.Name,
            Slug = m.Slug,
            LogoUrl = m.LogoUrl,
            Country = m.Country,
            IsPopular = m.IsPopular
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene las marcas más populares (para mostrar primero en UI).
    /// </summary>
    [HttpGet("makes/popular")]
    [ProducesResponseType(typeof(IEnumerable<MakeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MakeDto>>> GetPopularMakes([FromQuery] int take = 20)
    {
        var makes = await _catalogRepository.GetPopularMakesAsync(take);
        var dtos = makes.Select(m => new MakeDto
        {
            Id = m.Id,
            Name = m.Name,
            Slug = m.Slug,
            LogoUrl = m.LogoUrl,
            Country = m.Country,
            IsPopular = m.IsPopular
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Busca marcas por nombre (para autocomplete).
    /// </summary>
    [HttpGet("makes/search")]
    [ProducesResponseType(typeof(IEnumerable<MakeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MakeDto>>> SearchMakes([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Enumerable.Empty<MakeDto>());

        var makes = await _catalogRepository.SearchMakesAsync(q);
        var dtos = makes.Select(m => new MakeDto
        {
            Id = m.Id,
            Name = m.Name,
            Slug = m.Slug,
            LogoUrl = m.LogoUrl,
            Country = m.Country,
            IsPopular = m.IsPopular
        });
        return Ok(dtos);
    }

    // ========================================
    // MODELS (Modelos)
    // ========================================

    /// <summary>
    /// Obtiene todos los modelos de una marca.
    /// </summary>
    /// <remarks>
    /// Ejemplo: GET /api/catalog/makes/toyota/models
    /// Retorna: Camry, Corolla, RAV4, Tacoma, etc.
    /// </remarks>
    [HttpGet("makes/{makeSlug}/models")]
    [ProducesResponseType(typeof(IEnumerable<ModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ModelDto>>> GetModelsByMake(string makeSlug)
    {
        var make = await _catalogRepository.GetMakeBySlugAsync(makeSlug);
        if (make == null)
            return NotFound(new { message = $"Make '{makeSlug}' not found" });

        var models = await _catalogRepository.GetModelsByMakeSlugAsync(makeSlug);
        var dtos = models.Select(m => new ModelDto
        {
            Id = m.Id,
            MakeId = m.MakeId,
            MakeName = make.Name,
            Name = m.Name,
            Slug = m.Slug,
            VehicleType = m.VehicleType.ToString(),
            BodyStyle = m.DefaultBodyStyle?.ToString(),
            StartYear = m.StartYear,
            EndYear = m.EndYear,
            IsPopular = m.IsPopular
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Busca modelos dentro de una marca (para autocomplete).
    /// </summary>
    [HttpGet("makes/{makeId:guid}/models/search")]
    [ProducesResponseType(typeof(IEnumerable<ModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ModelDto>>> SearchModels(Guid makeId, [FromQuery] string q)
    {
        var models = await _catalogRepository.SearchModelsAsync(makeId, q);
        var dtos = models.Select(m => new ModelDto
        {
            Id = m.Id,
            MakeId = m.MakeId,
            MakeName = m.Make?.Name ?? "",
            Name = m.Name,
            Slug = m.Slug,
            VehicleType = m.VehicleType.ToString(),
            BodyStyle = m.DefaultBodyStyle?.ToString(),
            StartYear = m.StartYear,
            EndYear = m.EndYear,
            IsPopular = m.IsPopular
        });
        return Ok(dtos);
    }

    // ========================================
    // YEARS (Años disponibles)
    // ========================================

    /// <summary>
    /// Obtiene los años disponibles para un modelo.
    /// </summary>
    /// <remarks>
    /// Ejemplo: GET /api/catalog/models/{modelId}/years
    /// Retorna: [2024, 2023, 2022, 2021, 2020, ...]
    /// </remarks>
    [HttpGet("models/{modelId:guid}/years")]
    [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<int>>> GetAvailableYears(Guid modelId)
    {
        var model = await _catalogRepository.GetModelByIdAsync(modelId);
        if (model == null)
            return NotFound(new { message = "Model not found" });

        var years = await _catalogRepository.GetAvailableYearsAsync(modelId);
        return Ok(years);
    }

    // ========================================
    // TRIMS (Versiones) - Auto-fill specs
    // ========================================

    /// <summary>
    /// Obtiene todos los trims/versiones disponibles para un modelo y año.
    /// Este endpoint retorna las especificaciones completas de cada trim.
    /// </summary>
    /// <remarks>
    /// Ejemplo: GET /api/catalog/models/{modelId}/years/2024/trims
    /// Retorna lista de trims con specs para auto-llenar el formulario:
    /// - LE: 2.5L, 203hp, FWD, 28/39 MPG, $28,400
    /// - SE: 2.5L, 203hp, FWD, 28/39 MPG, $29,495
    /// - XLE: 2.5L, 203hp, FWD, 28/39 MPG, $31,170
    /// - TRD: 3.5L V6, 301hp, FWD, 22/33 MPG, $36,095
    /// - Hybrid LE: 2.5L Hybrid, 225hp, AWD, 51/53 MPG, $29,845
    /// </remarks>
    [HttpGet("models/{modelId:guid}/years/{year:int}/trims")]
    [ProducesResponseType(typeof(IEnumerable<TrimDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TrimDto>>> GetTrimsByModelAndYear(Guid modelId, int year)
    {
        var model = await _catalogRepository.GetModelByIdAsync(modelId);
        if (model == null)
            return NotFound(new { message = "Model not found" });

        var trims = await _catalogRepository.GetTrimsByModelAndYearAsync(modelId, year);
        var dtos = trims.Select(t => new TrimDto
        {
            Id = t.Id,
            ModelId = t.ModelId,
            MakeName = t.Model?.Make?.Name ?? "",
            ModelName = t.Model?.Name ?? "",
            Name = t.Name,
            Slug = t.Slug,
            Year = t.Year,

            // Specs para auto-fill
            EngineSize = t.EngineSize,
            Horsepower = t.Horsepower,
            Torque = t.Torque,
            FuelType = t.FuelType?.ToString(),
            Transmission = t.Transmission?.ToString(),
            DriveType = t.DriveType?.ToString(),

            // Fuel economy
            MpgCity = t.MpgCity,
            MpgHighway = t.MpgHighway,
            MpgCombined = t.MpgCombined,

            // Precio base de referencia
            BaseMSRP = t.BaseMSRP
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene las especificaciones completas de un trim específico.
    /// Usado para auto-llenar TODOS los campos del formulario de publicación.
    /// </summary>
    [HttpGet("trims/{trimId:guid}")]
    [ProducesResponseType(typeof(TrimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrimDto>> GetTrimById(Guid trimId)
    {
        var trim = await _catalogRepository.GetTrimByIdAsync(trimId);
        if (trim == null)
            return NotFound(new { message = "Trim not found" });

        return Ok(new TrimDto
        {
            Id = trim.Id,
            ModelId = trim.ModelId,
            MakeName = trim.Model?.Make?.Name ?? "",
            ModelName = trim.Model?.Name ?? "",
            Name = trim.Name,
            Slug = trim.Slug,
            Year = trim.Year,
            EngineSize = trim.EngineSize,
            Horsepower = trim.Horsepower,
            Torque = trim.Torque,
            FuelType = trim.FuelType?.ToString(),
            Transmission = trim.Transmission?.ToString(),
            DriveType = trim.DriveType?.ToString(),
            MpgCity = trim.MpgCity,
            MpgHighway = trim.MpgHighway,
            MpgCombined = trim.MpgCombined,
            BaseMSRP = trim.BaseMSRP
        });
    }

    // ========================================
    // STATS & HEALTH
    // ========================================

    /// <summary>
    /// Obtiene estadísticas del catálogo.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CatalogStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogStats>> GetStats()
    {
        var stats = await _catalogRepository.GetStatsAsync();
        return Ok(stats);
    }

    // ========================================
    // SEED / IMPORT
    // ========================================

    /// <summary>
    /// Importa catálogo de vehículos masivamente.
    /// Este endpoint recibe datos de la API NHTSA procesados y los inserta en la BD.
    /// Solo agrega datos nuevos (upsert - no duplica).
    /// </summary>
    /// <remarks>
    /// Usar con el script: scripts/seed-vehicle-catalog.ts
    /// 
    /// Ejemplo payload:
    /// {
    ///   "makes": [{
    ///     "name": "Toyota",
    ///     "slug": "toyota",
    ///     "country": "Japan",
    ///     "isPopular": true,
    ///     "models": [{
    ///       "name": "Camry",
    ///       "vehicleType": "Car",
    ///       "bodyStyle": "Sedan",
    ///       "trims": [{
    ///         "name": "LE",
    ///         "year": 2024,
    ///         "baseMSRP": 28400,
    ///         "horsepower": 203,
    ///         ...
    ///       }]
    ///     }]
    ///   }]
    /// }
    /// </remarks>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeedResult>> SeedCatalog([FromBody] SeedCatalogRequest request)
    {
        if (request.Makes == null || !request.Makes.Any())
        {
            return BadRequest(new { message = "No makes provided" });
        }

        _logger.LogInformation("Starting catalog seed with {Count} makes", request.Makes.Count);

        int makesCount = 0;
        int modelsCount = 0;
        int trimsCount = 0;

        foreach (var makeDto in request.Makes)
        {
            // Upsert Make
            var make = new VehicleMake
            {
                Name = makeDto.Name,
                Slug = makeDto.Slug ?? CreateSlug(makeDto.Name),
                Country = makeDto.Country,
                IsPopular = makeDto.IsPopular,
                IsActive = true
            };

            var savedMake = await _catalogRepository.UpsertMakeAsync(make);
            makesCount++;

            foreach (var modelDto in makeDto.Models ?? Enumerable.Empty<SeedModelDto>())
            {
                // Parse VehicleType
                if (!Enum.TryParse<VehicleType>(modelDto.VehicleType, true, out var vehicleType))
                {
                    vehicleType = VehicleType.Car;
                }

                // Parse BodyStyle
                BodyStyle? bodyStyle = null;
                if (!string.IsNullOrEmpty(modelDto.BodyStyle) &&
                    Enum.TryParse<BodyStyle>(modelDto.BodyStyle, true, out var bs))
                {
                    bodyStyle = bs;
                }

                var model = new VehicleModel
                {
                    MakeId = savedMake.Id,
                    Name = modelDto.Name,
                    Slug = modelDto.Slug ?? CreateSlug(modelDto.Name),
                    VehicleType = vehicleType,
                    DefaultBodyStyle = bodyStyle,
                    StartYear = modelDto.StartYear,
                    EndYear = modelDto.EndYear,
                    IsPopular = modelDto.IsPopular,
                    IsActive = true
                };

                var savedModel = await _catalogRepository.UpsertModelAsync(model);
                modelsCount++;

                foreach (var trimDto in modelDto.Trims ?? Enumerable.Empty<SeedTrimDto>())
                {
                    // Parse enums
                    FuelType? fuelType = null;
                    if (!string.IsNullOrEmpty(trimDto.FuelType) &&
                        Enum.TryParse<FuelType>(trimDto.FuelType, true, out var ft))
                    {
                        fuelType = ft;
                    }

                    TransmissionType? transmission = null;
                    if (!string.IsNullOrEmpty(trimDto.Transmission) &&
                        Enum.TryParse<TransmissionType>(trimDto.Transmission, true, out var tt))
                    {
                        transmission = tt;
                    }

                    VehiclesSaleService.Domain.Entities.DriveType? driveType = null;
                    if (!string.IsNullOrEmpty(trimDto.DriveType) &&
                        Enum.TryParse<VehiclesSaleService.Domain.Entities.DriveType>(trimDto.DriveType, true, out var dt))
                    {
                        driveType = dt;
                    }

                    var trim = new VehicleTrim
                    {
                        ModelId = savedModel.Id,
                        Name = trimDto.Name,
                        Slug = trimDto.Slug ?? CreateSlug($"{modelDto.Name}-{trimDto.Name}-{trimDto.Year}"),
                        Year = trimDto.Year,
                        BaseMSRP = trimDto.BaseMSRP,
                        EngineSize = trimDto.EngineSize,
                        Horsepower = trimDto.Horsepower,
                        Torque = trimDto.Torque,
                        FuelType = fuelType,
                        Transmission = transmission,
                        DriveType = driveType,
                        MpgCity = trimDto.MpgCity,
                        MpgHighway = trimDto.MpgHighway,
                        MpgCombined = trimDto.MpgCombined,
                        IsActive = true
                    };

                    await _catalogRepository.UpsertTrimAsync(trim);
                    trimsCount++;
                }
            }
        }

        _logger.LogInformation("Catalog seed completed: {Makes} makes, {Models} models, {Trims} trims",
            makesCount, modelsCount, trimsCount);

        return Ok(new SeedResult
        {
            Success = true,
            MakesCount = makesCount,
            ModelsCount = modelsCount,
            TrimsCount = trimsCount,
            Message = $"Successfully imported {makesCount} makes, {modelsCount} models, {trimsCount} trims"
        });
    }

    private static string CreateSlug(string text)
    {
        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace(".", "")
            .Replace(",", "");
    }
}

// ========================================
// SEED DTOs
// ========================================

public record SeedCatalogRequest
{
    public List<SeedMakeDto>? Makes { get; init; }
}

public record SeedMakeDto
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Country { get; init; }
    public bool IsPopular { get; init; }
    public List<SeedModelDto>? Models { get; init; }
}

public record SeedModelDto
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string VehicleType { get; init; } = "Car";
    public string? BodyStyle { get; init; }
    public int? StartYear { get; init; }
    public int? EndYear { get; init; }
    public bool IsPopular { get; init; }
    public List<SeedTrimDto>? Trims { get; init; }
}

public record SeedTrimDto
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public int Year { get; init; }
    public decimal? BaseMSRP { get; init; }
    public string? EngineSize { get; init; }
    public int? Horsepower { get; init; }
    public int? Torque { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? DriveType { get; init; }
    public int? MpgCity { get; init; }
    public int? MpgHighway { get; init; }
    public int? MpgCombined { get; init; }
}

public record SeedResult
{
    public bool Success { get; init; }
    public int MakesCount { get; init; }
    public int ModelsCount { get; init; }
    public int TrimsCount { get; init; }
    public string Message { get; init; } = string.Empty;
}

// ========================================
// DTOs
// ========================================

public record MakeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Country { get; init; }
    public bool IsPopular { get; init; }
}

public record ModelDto
{
    public Guid Id { get; init; }
    public Guid MakeId { get; init; }
    public string MakeName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public string? BodyStyle { get; init; }
    public int? StartYear { get; init; }
    public int? EndYear { get; init; }
    public bool IsPopular { get; init; }
}

public record TrimDto
{
    public Guid Id { get; init; }
    public Guid ModelId { get; init; }
    public string MakeName { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int Year { get; init; }

    // Engine & Performance - Auto-fill fields
    public string? EngineSize { get; init; }
    public int? Horsepower { get; init; }
    public int? Torque { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? DriveType { get; init; }

    // Fuel Economy - Auto-fill fields
    public int? MpgCity { get; init; }
    public int? MpgHighway { get; init; }
    public int? MpgCombined { get; init; }

    // Pricing reference
    public decimal? BaseMSRP { get; init; }
}
