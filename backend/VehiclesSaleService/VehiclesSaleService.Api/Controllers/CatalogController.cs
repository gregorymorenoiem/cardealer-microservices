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
    // VIN DECODING
    // ========================================

    /// <summary>
    /// Decode a Vehicle Identification Number (VIN) to extract vehicle information.
    /// Uses NHTSA VPIC API for decoding.
    /// </summary>
    /// <remarks>
    /// Example VIN: 1HGCV1F32LA000001 (2020 Honda Accord)
    /// 
    /// Returns decoded information:
    /// - Make, Model, Year
    /// - Body Style, Vehicle Type
    /// - Engine Size, Fuel Type
    /// - Transmission, Drive Type
    /// - Manufacturing Location
    /// </remarks>
    [HttpGet("vin/{vin}/decode")]
    [ProducesResponseType(typeof(VinDecodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VinDecodeResponse>> DecodeVin(string vin)
    {
        // Validate VIN format (17 characters, alphanumeric except I, O, Q)
        if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
        {
            return BadRequest(new { message = "Invalid VIN format. VIN must be exactly 17 characters." });
        }

        vin = vin.ToUpperInvariant();

        // Check for invalid characters (I, O, Q are not used in VINs)
        if (vin.Any(c => c == 'I' || c == 'O' || c == 'Q'))
        {
            return BadRequest(new { message = "Invalid VIN. VINs cannot contain I, O, or Q characters." });
        }

        try
        {
            // Call NHTSA VPIC API
            using var httpClient = new HttpClient();
            var nhtsaUrl = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinvalues/{vin}?format=json";
            
            _logger.LogInformation("Decoding VIN: {VIN} via NHTSA API", vin);
            
            var response = await httpClient.GetAsync(nhtsaUrl);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var nhtsaResponse = System.Text.Json.JsonSerializer.Deserialize<NhtsaVinResponse>(content, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (nhtsaResponse?.Results == null || !nhtsaResponse.Results.Any())
            {
                return BadRequest(new { message = "Unable to decode VIN. No results returned from NHTSA." });
            }

            var result = nhtsaResponse.Results.First();

            // Check for error codes
            if (!string.IsNullOrEmpty(result.ErrorCode) && result.ErrorCode != "0")
            {
                _logger.LogWarning("VIN decode returned error: {ErrorCode} - {ErrorText}", 
                    result.ErrorCode, result.ErrorText);
            }

            // Parse year
            int.TryParse(result.ModelYear, out var year);
            
            // Parse engine info
            int.TryParse(result.EngineHP, out var horsepower);
            int.TryParse(result.EngineCylinders, out var cylinders);

            // Map fuel type
            var fuelType = MapFuelType(result.FuelTypePrimary);
            var transmission = MapTransmission(result.TransmissionStyle);
            var driveType = MapDriveType(result.DriveType);
            var bodyStyle = MapBodyStyle(result.BodyClass);
            var vehicleType = MapVehicleType(result.VehicleType);

            var vinResponse = new VinDecodeResponse
            {
                VIN = vin,
                IsValid = string.IsNullOrEmpty(result.ErrorCode) || result.ErrorCode == "0",
                
                // Basic Info
                Make = result.Make ?? string.Empty,
                Model = result.Model ?? string.Empty,
                Year = year,
                Trim = result.Trim,
                
                // Type & Body
                VehicleType = vehicleType,
                BodyStyle = bodyStyle,
                Doors = ParseInt(result.Doors),
                
                // Engine
                EngineSize = result.DisplacementL != null ? $"{result.DisplacementL}L" : result.EngineModel,
                Cylinders = cylinders > 0 ? cylinders : null,
                Horsepower = horsepower > 0 ? horsepower : null,
                FuelType = fuelType,
                
                // Transmission & Drive
                Transmission = transmission,
                DriveType = driveType,
                
                // Manufacturing
                PlantCity = result.PlantCity,
                PlantCountry = result.PlantCountry,
                Manufacturer = result.Manufacturer,
                
                // Additional
                Series = result.Series,
                GVWR = result.GVWR,
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorText,
                
                // Suggested data for form auto-fill
                SuggestedData = new VinSuggestedData
                {
                    Make = result.Make ?? string.Empty,
                    Model = result.Model ?? string.Empty,
                    Year = year,
                    Trim = result.Trim,
                    VehicleType = vehicleType,
                    BodyStyle = bodyStyle,
                    FuelType = fuelType,
                    Transmission = transmission,
                    DriveType = driveType,
                    EngineSize = result.DisplacementL != null ? $"{result.DisplacementL}L" : null,
                    Horsepower = horsepower > 0 ? horsepower : null,
                    Cylinders = cylinders > 0 ? cylinders : null
                }
            };

            _logger.LogInformation("VIN decoded successfully: {VIN} -> {Year} {Make} {Model}", 
                vin, year, result.Make, result.Model);

            return Ok(vinResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call NHTSA API for VIN: {VIN}", vin);
            return BadRequest(new { message = "Failed to decode VIN. NHTSA service unavailable." });
        }
    }

    // VIN Decode helper methods
    private static int? ParseInt(string? value)
    {
        if (int.TryParse(value, out var result))
            return result;
        return null;
    }

    private static string MapFuelType(string? nhtsaFuelType)
    {
        if (string.IsNullOrEmpty(nhtsaFuelType)) return "Gasoline";
        
        return nhtsaFuelType.ToLowerInvariant() switch
        {
            // Check plug-in hybrid first (before electric/hybrid checks)
            var f when f.Contains("plug") || f.Contains("phev") => "PlugInHybrid",
            var f when f.Contains("diesel") => "Diesel",
            var f when f.Contains("hybrid") => "Hybrid",
            var f when f.Contains("electric") => "Electric",
            var f when f.Contains("flex") => "FlexFuel",
            var f when f.Contains("hydrogen") => "Hydrogen",
            var f when f.Contains("natural gas") || f.Contains("cng") => "NaturalGas",
            _ => "Gasoline"
        };
    }

    private static string MapTransmission(string? nhtsaTransmission)
    {
        if (string.IsNullOrEmpty(nhtsaTransmission)) return "Automatic";
        
        return nhtsaTransmission.ToLowerInvariant() switch
        {
            var t when t.Contains("cvt") => "CVT",
            var t when t.Contains("dual") || t.Contains("dct") => "DualClutch",
            var t when t.Contains("automated") => "Automated",
            var t when t.Contains("manual") => "Manual",
            _ => "Automatic"
        };
    }

    private static string MapDriveType(string? nhtsaDriveType)
    {
        if (string.IsNullOrEmpty(nhtsaDriveType)) return "FWD";
        
        return nhtsaDriveType.ToLowerInvariant() switch
        {
            var d when d.Contains("4x4") || d.Contains("4wd") || d.Contains("four") => "FourWD",
            var d when d.Contains("awd") || d.Contains("all") => "AWD",
            var d when d.Contains("rwd") || d.Contains("rear") => "RWD",
            _ => "FWD"
        };
    }

    private static string MapBodyStyle(string? nhtsaBodyClass)
    {
        if (string.IsNullOrEmpty(nhtsaBodyClass)) return "Sedan";
        
        return nhtsaBodyClass.ToLowerInvariant() switch
        {
            var b when b.Contains("suv") || b.Contains("sport utility") => "SUV",
            var b when b.Contains("pickup") || b.Contains("truck") => "Pickup",
            var b when b.Contains("van") && b.Contains("mini") => "Minivan",
            var b when b.Contains("van") => "Van",
            var b when b.Contains("coupe") => "Coupe",
            var b when b.Contains("convertible") => "Convertible",
            var b when b.Contains("hatchback") => "Hatchback",
            var b when b.Contains("wagon") => "Wagon",
            var b when b.Contains("crossover") => "Crossover",
            _ => "Sedan"
        };
    }

    private static string MapVehicleType(string? nhtsaVehicleType)
    {
        if (string.IsNullOrEmpty(nhtsaVehicleType)) return "Car";
        
        return nhtsaVehicleType.ToLowerInvariant() switch
        {
            var v when v.Contains("truck") => "Truck",
            var v when v.Contains("suv") || v.Contains("multipurpose") => "SUV",
            var v when v.Contains("van") => "Van",
            var v when v.Contains("motorcycle") => "Motorcycle",
            var v when v.Contains("bus") => "Commercial",
            _ => "Car"
        };
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

// ========================================
// VIN DECODE DTOs
// ========================================

/// <summary>
/// Response from VIN decoding
/// </summary>
public record VinDecodeResponse
{
    public string VIN { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    
    // Basic Info
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? Trim { get; init; }
    
    // Type & Body
    public string VehicleType { get; init; } = "Car";
    public string BodyStyle { get; init; } = "Sedan";
    public int? Doors { get; init; }
    
    // Engine
    public string? EngineSize { get; init; }
    public int? Cylinders { get; init; }
    public int? Horsepower { get; init; }
    public string FuelType { get; init; } = "Gasoline";
    
    // Transmission & Drive
    public string Transmission { get; init; } = "Automatic";
    public string DriveType { get; init; } = "FWD";
    
    // Manufacturing
    public string? PlantCity { get; init; }
    public string? PlantCountry { get; init; }
    public string? Manufacturer { get; init; }
    
    // Additional
    public string? Series { get; init; }
    public string? GVWR { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    
    // Suggested data for form auto-fill
    public VinSuggestedData? SuggestedData { get; init; }
}

/// <summary>
/// Suggested values for form auto-fill based on VIN
/// </summary>
public record VinSuggestedData
{
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? Trim { get; init; }
    public string VehicleType { get; init; } = "Car";
    public string BodyStyle { get; init; } = "Sedan";
    public string FuelType { get; init; } = "Gasoline";
    public string Transmission { get; init; } = "Automatic";
    public string DriveType { get; init; } = "FWD";
    public string? EngineSize { get; init; }
    public int? Horsepower { get; init; }
    public int? Cylinders { get; init; }
}

/// <summary>
/// NHTSA VPIC API Response
/// </summary>
public class NhtsaVinResponse
{
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SearchCriteria { get; set; } = string.Empty;
    public List<NhtsaVinResult> Results { get; set; } = new();
}

/// <summary>
/// NHTSA VPIC Result item
/// </summary>
public class NhtsaVinResult
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? ModelYear { get; set; }
    public string? Trim { get; set; }
    public string? Series { get; set; }
    public string? VehicleType { get; set; }
    public string? BodyClass { get; set; }
    public string? Doors { get; set; }
    public string? DriveType { get; set; }
    public string? TransmissionStyle { get; set; }
    public string? FuelTypePrimary { get; set; }
    public string? DisplacementL { get; set; }
    public string? EngineModel { get; set; }
    public string? EngineCylinders { get; set; }
    public string? EngineHP { get; set; }
    public string? Manufacturer { get; set; }
    public string? PlantCity { get; set; }
    public string? PlantCountry { get; set; }
    public string? GVWR { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorText { get; set; }
}
