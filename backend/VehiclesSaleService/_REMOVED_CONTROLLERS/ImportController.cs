using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Persistence;

namespace VehiclesSaleService.Api.Controllers;

[ApiController]
[Route("api/vehicles/import")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        IVehicleRepository vehicleRepository,
        ApplicationDbContext context,
        ILogger<ImportController> logger)
    {
        _vehicleRepository = vehicleRepository;
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // POST api/vehicles/import — Upload CSV/XLSX file for bulk import
    // =========================================================================
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> ImportVehicles(
        IFormFile file,
        [FromForm] Guid dealerId,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se proporcionó un archivo válido." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".xlsx")
            return BadRequest(new { message = "Formato no soportado. Use CSV o XLSX." });

        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Usuario no autenticado." });

        _logger.LogInformation(
            "Import request from dealer {DealerId} with file {FileName} ({Size} bytes)",
            dealerId, file.FileName, file.Length);

        var importResult = new ImportResultDto
        {
            Id = Guid.NewGuid().ToString(),
            DealerId = dealerId.ToString(),
            Filename = file.FileName,
            Status = "processing",
            CreatedAt = DateTime.UtcNow.ToString("o"),
        };

        try
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                while (await reader.ReadLineAsync(ct) is { } line)
                    lines.Add(line);
            }

            if (lines.Count < 2)
                return BadRequest(new { message = "El archivo debe contener al menos una fila de encabezados y una de datos." });

            var headers = ParseCsvLine(lines[0])
                .Select(h => h.Trim().ToLowerInvariant())
                .ToArray();

            // Validate required columns
            var requiredColumns = new[] { "marca", "modelo", "año", "precio" };
            var missingColumns = requiredColumns
                .Where(rc => !headers.Contains(rc))
                .ToList();

            if (missingColumns.Count > 0)
                return BadRequest(new
                {
                    message = $"Faltan columnas requeridas: {string.Join(", ", missingColumns)}",
                    missingColumns
                });

            var errors = new List<ImportErrorDto>();
            var successCount = 0;

            for (int i = 1; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                try
                {
                    var values = ParseCsvLine(lines[i]);
                    var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                        row[headers[j]] = values[j].Trim();

                    var vehicle = MapRowToVehicle(row, dealerId, Guid.Parse(userId));
                    if (vehicle == null)
                    {
                        errors.Add(new ImportErrorDto { Row = i + 1, Field = "general", Message = "No se pudo mapear la fila." });
                        continue;
                    }

                    await _vehicleRepository.CreateAsync(vehicle);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportErrorDto
                    {
                        Row = i + 1,
                        Field = "general",
                        Message = ex.Message.Length > 200 ? ex.Message[..200] : ex.Message
                    });
                }
            }

            importResult.TotalRecords = lines.Count - 1;
            importResult.Successful = successCount;
            importResult.Failed = errors.Count;
            importResult.Errors = errors;
            importResult.Status = "completed";
            importResult.CompletedAt = DateTime.UtcNow.ToString("o");

            _logger.LogInformation(
                "Import completed for dealer {DealerId}: {Success}/{Total} successful",
                dealerId, successCount, importResult.TotalRecords);

            return Ok(importResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed for dealer {DealerId}", dealerId);
            importResult.Status = "failed";
            importResult.Errors = new List<ImportErrorDto>
            {
                new() { Row = 0, Field = "file", Message = ex.Message }
            };
            return StatusCode(500, importResult);
        }
    }

    // =========================================================================
    // GET api/vehicles/import/history/{dealerId}
    // =========================================================================
    [HttpGet("history/{dealerId:guid}")]
    public async Task<IActionResult> GetImportHistory(Guid dealerId, CancellationToken ct = default)
    {
        // Return vehicles grouped by creation batch (same day + same dealer)
        var recentVehicles = await _context.Vehicles
            .Where(v => v.DealerId == dealerId)
            .OrderByDescending(v => v.CreatedAt)
            .GroupBy(v => v.CreatedAt.Date)
            .Take(10)
            .Select(g => new ImportHistoryItemDto
            {
                Id = g.Key.ToString("yyyyMMdd"),
                Filename = $"Importación {g.Key:dd/MM/yyyy}",
                Date = g.Key.ToString("o"),
                TotalRecords = g.Count(),
                Successful = g.Count(v => v.Status != VehicleStatus.Draft),
                Failed = 0,
                Status = "completed",
            })
            .ToListAsync(ct);

        return Ok(recentVehicles);
    }

    // =========================================================================
    // GET api/vehicles/import/template?format=csv
    // =========================================================================
    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate([FromQuery] string format = "csv")
    {
        if (format.ToLowerInvariant() == "csv")
        {
            var csv = new StringBuilder();
            csv.AppendLine("marca,modelo,año,precio,kilometraje,transmision,combustible,color,version,puertas,cilindros,traccion,descripcion,vin,ciudad,provincia");
            csv.AppendLine("Toyota,Corolla,2023,1250000,15000,Automatica,Gasolina,Blanco,LE,4,,Delantera,Excelente condición,,Santo Domingo,Distrito Nacional");
            csv.AppendLine("Honda,Civic,2022,1100000,22000,Automatica,Gasolina,Negro,Sport,4,,Delantera,Un solo dueño,,Santiago,Santiago");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "plantilla-importacion.csv");
        }

        // For XLSX, return a simple CSV with xlsx extension (in production use EPPlus or similar)
        var csvContent = "marca,modelo,año,precio,kilometraje,transmision,combustible,color\nToyota,Corolla,2023,1250000,15000,Automatica,Gasolina,Blanco";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        return File(csvBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "plantilla-importacion.xlsx");
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(line[i]);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static Vehicle? MapRowToVehicle(
        Dictionary<string, string> row,
        Guid dealerId,
        Guid sellerId)
    {
        if (!row.TryGetValue("marca", out var make) || string.IsNullOrWhiteSpace(make))
            return null;
        if (!row.TryGetValue("modelo", out var model) || string.IsNullOrWhiteSpace(model))
            return null;
        if (!row.TryGetValue("año", out var yearStr) || !int.TryParse(yearStr, out var year))
            return null;
        if (!row.TryGetValue("precio", out var priceStr) || !decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            return null;

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            DealerId = dealerId,
            Make = make,
            Model = model,
            Year = year,
            Price = price,
            Currency = "DOP",
            Status = VehicleStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        if (row.TryGetValue("kilometraje", out var mileageStr) && int.TryParse(mileageStr, out var mileage))
            vehicle.Mileage = mileage;

        if (row.TryGetValue("transmision", out var transmission))
            vehicle.Transmission = ParseEnum<TransmissionType>(transmission);

        if (row.TryGetValue("combustible", out var fuel))
            vehicle.FuelType = ParseEnum<FuelType>(fuel);

        if (row.TryGetValue("color", out var color))
            vehicle.ExteriorColor = color;

        if (row.TryGetValue("version", out var version))
            vehicle.Trim = version;

        if (row.TryGetValue("puertas", out var doorsStr) && int.TryParse(doorsStr, out var doors))
            vehicle.Doors = doors;

        if (row.TryGetValue("descripcion", out var description))
            vehicle.Description = description;

        if (row.TryGetValue("vin", out var vin) && !string.IsNullOrWhiteSpace(vin))
            vehicle.VIN = vin;

        if (row.TryGetValue("ciudad", out var city))
            vehicle.City = city;

        if (row.TryGetValue("provincia", out var province))
            vehicle.State = province;

        return vehicle;
    }

    private static T ParseEnum<T>(string value) where T : struct
    {
        if (Enum.TryParse<T>(value.Replace(" ", ""), ignoreCase: true, out var result))
            return result;
        return default;
    }
}

// =========================================================================
// DTOs
// =========================================================================

public class ImportResultDto
{
    public string Id { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public string Status { get; set; } = "processing";
    public List<ImportErrorDto> Errors { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
    public string? CompletedAt { get; set; }
}

public class ImportErrorDto
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ImportHistoryItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public string Status { get; set; } = "completed";
}
