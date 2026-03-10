using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportsService.Application.DTOs;
using ReportsService.Domain.Entities;
using ReportsService.Domain.Interfaces;

namespace ReportsService.Api.Controllers;

/// <summary>
/// Content moderation reports — user-submitted reports flagging
/// vehicles, users, messages, or dealers for admin review.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContentReportsController : ControllerBase
{
    private readonly IContentReportRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ContentReportsController> _logger;

    /// <summary>Auto-suspend threshold: listing is suspended after this many reports</summary>
    private const int AutoSuspendThreshold = 3;

    /// <summary>
    /// Category → human-readable reason mapping for the audit log.
    /// </summary>
    private static readonly Dictionary<ContentReportReason, string> CategoryLabels = new()
    {
        [ContentReportReason.PrecioIncorrecto] = "Precio incorrecto",
        [ContentReportReason.VehiculoNoDisponible] = "Vehículo no disponible",
        [ContentReportReason.FotosNoCorresponden] = "Fotos no corresponden al vehículo",
        [ContentReportReason.PosibleFraude] = "Posible fraude",
        [ContentReportReason.Otra] = "Otra razón"
    };

    public ContentReportsController(
        IContentReportRepository repository,
        IHttpClientFactory httpClientFactory,
        ILogger<ContentReportsController> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated content reports with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedContentReportResponse>> GetReports(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        ContentReportType? parsedType = null;
        ContentReportStatus? parsedStatus = null;
        ContentReportPriority? parsedPriority = null;

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<ContentReportType>(type, true, out var t))
            parsedType = t;

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContentReportStatus>(status, true, out var s))
            parsedStatus = s;

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<ContentReportPriority>(priority, true, out var p))
            parsedPriority = p;

        var (items, total) = await _repository.GetPaginatedAsync(
            parsedType, parsedStatus, parsedPriority, search, page, pageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var response = new PaginatedContentReportResponse(
            Items: items.Select(MapToDto).ToList(),
            Total: total,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages);

        return Ok(response);
    }

    /// <summary>
    /// Get content report statistics.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ContentReportStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatsAsync(cancellationToken);

        return Ok(new ContentReportStatsDto(
            stats.Total,
            stats.Pending,
            stats.Investigating,
            stats.Resolved,
            stats.Dismissed,
            stats.HighPriority));
    }

    /// <summary>
    /// Get a single content report by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentReportDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            return NotFound();

        return Ok(MapToDto(report));
    }

    /// <summary>
    /// 🔓 ANONYMOUS — Report a vehicle listing without requiring registration.
    /// Buyers can flag a vehicle from the listing page directly.
    /// Categories: PrecioIncorrecto, VehiculoNoDisponible, FotosNoCorresponden, PosibleFraude, Otra.
    /// After ≥3 reports, the listing is auto-suspended via VehiclesSaleService.
    /// </summary>
    [HttpPost("vehicle/anonymous")]
    [AllowAnonymous]
    public async Task<ActionResult<ContentReportDto>> CreateAnonymousVehicleReport(
        [FromBody] CreateAnonymousVehicleReportRequest request,
        CancellationToken cancellationToken)
    {
        // ── Validate category ──
        if (!Enum.TryParse<ContentReportReason>(request.ReportCategory, true, out var category))
            return BadRequest(new
            {
                Error = "Categoría inválida.",
                ValidCategories = new[] { "PrecioIncorrecto", "VehiculoNoDisponible", "FotosNoCorresponden", "PosibleFraude", "Otra" }
            });

        if (string.IsNullOrWhiteSpace(request.VehicleId))
            return BadRequest(new { Error = "El ID del vehículo es requerido." });

        // ── Abuse prevention: IP-based dedup ──
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var existingByIp = await _repository.FindByTargetAndIpAsync(
            request.VehicleId, ipAddress, cancellationToken);

        if (existingByIp != null)
        {
            existingByIp.IncrementReportCount();
            await _repository.UpdateAsync(existingByIp, cancellationToken);

            _logger.LogInformation(
                "Duplicate anonymous report from IP {Ip} for vehicle {VehicleId} — count now {Count}",
                ipAddress, request.VehicleId, existingByIp.ReportCount);

            // Check auto-suspend even on duplicates
            await CheckAndAutoSuspendAsync(request.VehicleId, cancellationToken);

            return Ok(MapToDto(existingByIp));
        }

        // ── Build reason text from category ──
        var reasonText = CategoryLabels.GetValueOrDefault(category, "Reporte de usuario");
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? reasonText
            : $"{reasonText}: {request.Description}";

        // Priority: Fraude = High, rest = Medium
        var priority = category == ContentReportReason.PosibleFraude
            ? ContentReportPriority.High
            : ContentReportPriority.Medium;

        var report = new ContentReport(
            type: ContentReportType.Vehicle,
            targetId: request.VehicleId,
            targetTitle: request.VehicleTitle,
            reason: reasonText,
            description: description,
            reportedById: Guid.Empty, // anonymous
            reportedByEmail: request.ContactEmail ?? string.Empty,
            priority: priority,
            reportCategory: category,
            reporterIpAddress: ipAddress);

        var created = await _repository.AddAsync(report, cancellationToken);

        _logger.LogInformation(
            "Anonymous vehicle report {ReportId} created for {VehicleId} — category: {Category}, IP: {Ip}",
            created.Id, request.VehicleId, category, ipAddress);

        // ── Auto-suspend check ──
        await CheckAndAutoSuspendAsync(request.VehicleId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Create a new content report (authenticated user flags content).
    /// If the same user already reported this target, increment the count.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContentReportDto>> Create(
        [FromBody] CreateContentReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ContentReportType>(request.Type, true, out var reportType))
            return BadRequest("Invalid report type. Valid values: Vehicle, User, Message, Dealer");

        // Parse optional category
        ContentReportReason? category = null;
        if (!string.IsNullOrEmpty(request.ReportCategory) &&
            Enum.TryParse<ContentReportReason>(request.ReportCategory, true, out var parsedCat))
            category = parsedCat;

        // Check for existing report from same user on same target
        if (Guid.TryParse(request.ReportedById, out var reportedById))
        {
            var existing = await _repository.FindByTargetAndReporterAsync(
                request.TargetId, reportedById, cancellationToken);

            if (existing != null)
            {
                existing.IncrementReportCount();
                await _repository.UpdateAsync(existing, cancellationToken);

                // Check auto-suspend for vehicle reports
                if (reportType == ContentReportType.Vehicle)
                    await CheckAndAutoSuspendAsync(request.TargetId, cancellationToken);

                return Ok(MapToDto(existing));
            }
        }
        else
        {
            reportedById = Guid.Empty;
        }

        var priority = ContentReportPriority.Medium;
        if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<ContentReportPriority>(request.Priority, true, out var p))
            priority = p;

        var report = new ContentReport(
            reportType,
            request.TargetId,
            request.TargetTitle,
            request.Reason,
            request.Description,
            reportedById,
            request.ReportedByEmail,
            priority,
            category);

        var created = await _repository.AddAsync(report, cancellationToken);
        _logger.LogInformation(
            "Content report {ReportId} created for {Type} target {TargetId} by {ReportedByEmail}",
            created.Id, reportType, request.TargetId, request.ReportedByEmail);

        // Check auto-suspend for vehicle reports
        if (reportType == ContentReportType.Vehicle)
            await CheckAndAutoSuspendAsync(request.TargetId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Update content report status (admin action).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ContentReportDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateContentReportStatusRequest request,
        CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            return NotFound();

        if (!Enum.TryParse<ContentReportStatus>(request.Status, true, out var newStatus))
            return BadRequest("Invalid status. Valid values: Pending, Investigating, Resolved, Dismissed");

        report.SetStatus(newStatus, request.Resolution, request.ResolvedById);
        await _repository.UpdateAsync(report, cancellationToken);

        _logger.LogInformation(
            "Content report {ReportId} status updated to {Status}",
            id, newStatus);

        return Ok(MapToDto(report));
    }

    /// <summary>
    /// Delete a content report.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _repository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Content report {ReportId} deleted", id);

        return NoContent();
    }

    /// <summary>
    /// Get total active report count for a specific vehicle (internal endpoint).
    /// Used by VehiclesSaleService / AdminService to check suspension status.
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/count")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetVehicleReportCount(
        string vehicleId, CancellationToken cancellationToken)
    {
        var count = await _repository.CountActiveByTargetIdAsync(vehicleId, cancellationToken);
        return Ok(new { vehicleId, activeReportCount = count, autoSuspendThreshold = AutoSuspendThreshold });
    }

    // ── Auto-suspend logic ──────────────────────────────────────────────────

    /// <summary>
    /// Checks if a vehicle has ≥3 active reports and calls VehiclesSaleService
    /// to suspend it. This ensures the listing is hidden within minutes.
    /// </summary>
    private async Task CheckAndAutoSuspendAsync(string vehicleId, CancellationToken cancellationToken)
    {
        try
        {
            var activeCount = await _repository.CountActiveByTargetIdAsync(vehicleId, cancellationToken);

            if (activeCount < AutoSuspendThreshold)
                return;

            _logger.LogWarning(
                "Vehicle {VehicleId} has {Count} active reports — auto-suspending listing",
                vehicleId, activeCount);

            // Call VehiclesSaleService internal endpoint to suspend
            var client = _httpClientFactory.CreateClient("VehiclesSaleService");
            var response = await client.PatchAsync(
                $"/api/vehicles/{vehicleId}/suspend",
                new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        reason = $"Auto-suspendido: {activeCount} reportes de compradores (umbral: {AutoSuspendThreshold})",
                        reportCount = activeCount
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Vehicle {VehicleId} successfully auto-suspended after {Count} reports",
                    vehicleId, activeCount);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to auto-suspend vehicle {VehicleId}: {StatusCode}",
                    vehicleId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // Auto-suspend failure should NOT block the report creation
            _logger.LogError(ex,
                "Error during auto-suspend check for vehicle {VehicleId}",
                vehicleId);
        }
    }

    // ── Mapping ─────────────────────────────────────────────────────────────

    private static ContentReportDto MapToDto(ContentReport report) => new(
        report.Id,
        report.Type.ToString().ToLower(),
        report.TargetId,
        report.TargetTitle,
        report.Reason,
        report.Description,
        report.ReportedById.ToString(),
        report.ReportedByEmail,
        report.Status.ToString().ToLower(),
        report.Priority.ToString().ToLower(),
        report.CreatedAt,
        report.ResolvedAt,
        report.ResolvedById,
        report.Resolution,
        report.ReportCount,
        report.ReportCategory?.ToString());
}
