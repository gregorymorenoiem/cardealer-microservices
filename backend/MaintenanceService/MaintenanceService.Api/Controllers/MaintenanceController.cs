using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MaintenanceService.Domain.Entities;
using MaintenanceService.Domain.Interfaces;

namespace MaintenanceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceRepository _repository;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        IMaintenanceRepository repository,
        ILogger<MaintenanceController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Verifica si el sistema está en modo mantenimiento
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<MaintenanceStatusResponse>> GetStatus()
    {
        var isActive = await _repository.IsMaintenanceModeActiveAsync();
        var activeWindow = await _repository.GetActiveAsync();

        return Ok(new MaintenanceStatusResponse
        {
            IsMaintenanceMode = isActive,
            MaintenanceWindow = activeWindow != null ? MapToDto(activeWindow) : null
        });
    }

    /// <summary>
    /// Obtiene todas las ventanas de mantenimiento
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MaintenanceWindowDto>>> GetAll()
    {
        var windows = await _repository.GetAllAsync();
        return Ok(windows.Select(MapToDto));
    }

    /// <summary>
    /// Obtiene ventanas de mantenimiento próximas
    /// </summary>
    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MaintenanceWindowDto>>> GetUpcoming(
        [FromQuery] int days = 7)
    {
        var windows = await _repository.GetUpcomingAsync(days);
        return Ok(windows.Select(MapToDto));
    }

    /// <summary>
    /// Obtiene una ventana de mantenimiento específica
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> GetById(Guid id)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        return Ok(MapToDto(window));
    }

    /// <summary>
    /// Crea una nueva ventana de mantenimiento
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> Create(
        [FromBody] CreateMaintenanceWindowRequest request)
    {
        try
        {
            var window = new MaintenanceWindow(
                request.Title,
                request.Description,
                request.Type,
                request.ScheduledStart,
                request.ScheduledEnd,
                User.Identity?.Name ?? "System",
                request.NotifyUsers,
                request.NotifyMinutesBefore,
                request.AffectedServices
            );

            await _repository.CreateAsync(window);
            _logger.LogInformation(
                "Maintenance window created: {Id} - {Title} ({Start} to {End})",
                window.Id, window.Title, window.ScheduledStart, window.ScheduledEnd);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = window.Id }, 
                MapToDto(window));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Inicia una ventana de mantenimiento
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> Start(Guid id)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        try
        {
            window.Start();
            await _repository.UpdateAsync(window);

            _logger.LogWarning(
                "Maintenance mode STARTED: {Id} - {Title}",
                window.Id, window.Title);

            return Ok(MapToDto(window));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Completa una ventana de mantenimiento
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> Complete(Guid id)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        try
        {
            window.Complete();
            await _repository.UpdateAsync(window);

            _logger.LogInformation(
                "Maintenance mode COMPLETED: {Id} - {Title}",
                window.Id, window.Title);

            return Ok(MapToDto(window));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una ventana de mantenimiento
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> Cancel(
        Guid id,
        [FromBody] CancelMaintenanceRequest request)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        try
        {
            window.Cancel(request.Reason);
            await _repository.UpdateAsync(window);

            _logger.LogInformation(
                "Maintenance window CANCELLED: {Id} - {Title}. Reason: {Reason}",
                window.Id, window.Title, request.Reason);

            return Ok(MapToDto(window));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el horario de una ventana de mantenimiento
    /// </summary>
    [HttpPut("{id:guid}/schedule")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> UpdateSchedule(
        Guid id,
        [FromBody] UpdateScheduleRequest request)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        try
        {
            window.UpdateSchedule(request.NewStart, request.NewEnd);
            await _repository.UpdateAsync(window);

            return Ok(MapToDto(window));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza las notas de una ventana de mantenimiento
    /// </summary>
    [HttpPut("{id:guid}/notes")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MaintenanceWindowDto>> UpdateNotes(
        Guid id,
        [FromBody] UpdateNotesRequest request)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        window.UpdateNotes(request.Notes);
        await _repository.UpdateAsync(window);

        return Ok(MapToDto(window));
    }

    /// <summary>
    /// Elimina una ventana de mantenimiento
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var window = await _repository.GetByIdAsync(id);
        if (window == null)
            return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }

    private static MaintenanceWindowDto MapToDto(MaintenanceWindow window)
    {
        return new MaintenanceWindowDto
        {
            Id = window.Id,
            Title = window.Title,
            Description = window.Description,
            Type = window.Type.ToString(),
            Status = window.Status.ToString(),
            ScheduledStart = window.ScheduledStart,
            ScheduledEnd = window.ScheduledEnd,
            ActualStart = window.ActualStart,
            ActualEnd = window.ActualEnd,
            CreatedBy = window.CreatedBy,
            CreatedAt = window.CreatedAt,
            UpdatedAt = window.UpdatedAt,
            Notes = window.Notes,
            NotifyUsers = window.NotifyUsers,
            NotifyMinutesBefore = window.NotifyMinutesBefore,
            AffectedServices = window.AffectedServices,
            IsActive = window.IsActive(),
            IsUpcoming = window.IsUpcoming()
        };
    }
}

#region DTOs

public record MaintenanceStatusResponse
{
    public bool IsMaintenanceMode { get; init; }
    public MaintenanceWindowDto? MaintenanceWindow { get; init; }
}

public record MaintenanceWindowDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public DateTime? ActualStart { get; init; }
    public DateTime? ActualEnd { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? Notes { get; init; }
    public bool NotifyUsers { get; init; }
    public int NotifyMinutesBefore { get; init; }
    public List<string> AffectedServices { get; init; } = new();
    public bool IsActive { get; init; }
    public bool IsUpcoming { get; init; }
}

public record CreateMaintenanceWindowRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public MaintenanceType Type { get; init; }
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public bool NotifyUsers { get; init; } = true;
    public int NotifyMinutesBefore { get; init; } = 30;
    public List<string>? AffectedServices { get; init; }
}

public record CancelMaintenanceRequest
{
    public string Reason { get; init; } = string.Empty;
}

public record UpdateScheduleRequest
{
    public DateTime NewStart { get; init; }
    public DateTime NewEnd { get; init; }
}

public record UpdateNotesRequest
{
    public string Notes { get; init; } = string.Empty;
}

#endregion
