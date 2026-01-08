using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AlertService.Domain.Entities;
using AlertService.Domain.Interfaces;
using System.Security.Claims;

namespace AlertService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PriceAlertsController : ControllerBase
{
    private readonly IPriceAlertRepository _repository;
    private readonly ILogger<PriceAlertsController> _logger;

    public PriceAlertsController(
        IPriceAlertRepository repository,
        ILogger<PriceAlertsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las alertas de precio del usuario
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PriceAlertDto>>> GetMyAlerts()
    {
        var userId = GetCurrentUserId();
        var alerts = await _repository.GetByUserIdAsync(userId);

        return Ok(alerts.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Obtiene una alerta específica
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PriceAlertDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        return Ok(MapToDto(alert));
    }

    /// <summary>
    /// Crea una nueva alerta de precio
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PriceAlertDto>> Create([FromBody] CreatePriceAlertRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Check if alert already exists
            if (await _repository.ExistsAsync(userId, request.VehicleId))
            {
                return BadRequest(new { error = "Ya existe una alerta para este vehículo" });
            }

            var alert = new PriceAlert(
                userId,
                request.VehicleId,
                request.TargetPrice,
                request.Condition);

            await _repository.CreateAsync(alert);

            _logger.LogInformation(
                "User {UserId} created price alert {AlertId} for vehicle {VehicleId}",
                userId, alert.Id, request.VehicleId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = alert.Id },
                MapToDto(alert));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el precio objetivo de una alerta
    /// </summary>
    [HttpPut("{id:guid}/target-price")]
    public async Task<ActionResult<PriceAlertDto>> UpdateTargetPrice(
        Guid id,
        [FromBody] UpdateTargetPriceRequest request)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        try
        {
            alert.UpdateTargetPrice(request.TargetPrice);
            await _repository.UpdateAsync(alert);

            return Ok(MapToDto(alert));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Activa una alerta desactivada
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<PriceAlertDto>> Activate(Guid id)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        alert.Activate();
        await _repository.UpdateAsync(alert);

        return Ok(MapToDto(alert));
    }

    /// <summary>
    /// Desactiva una alerta
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<PriceAlertDto>> Deactivate(Guid id)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        alert.Deactivate();
        await _repository.UpdateAsync(alert);

        return Ok(MapToDto(alert));
    }

    /// <summary>
    /// Resetea una alerta disparada
    /// </summary>
    [HttpPost("{id:guid}/reset")]
    public async Task<ActionResult<PriceAlertDto>> Reset(Guid id)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        alert.Reset();
        await _repository.UpdateAsync(alert);

        return Ok(MapToDto(alert));
    }

    /// <summary>
    /// Elimina una alerta
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var alert = await _repository.GetByIdAsync(id);

        if (alert == null)
            return NotFound();

        if (alert.UserId != userId)
            return Forbid();

        await _repository.DeleteAsync(id);

        return NoContent();
    }

    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userId;
    }

    private static PriceAlertDto MapToDto(PriceAlert alert)
    {
        return new PriceAlertDto
        {
            Id = alert.Id,
            VehicleId = alert.VehicleId,
            TargetPrice = alert.TargetPrice,
            Condition = alert.Condition.ToString(),
            IsActive = alert.IsActive,
            IsTriggered = alert.IsTriggered,
            TriggeredAt = alert.TriggeredAt,
            CreatedAt = alert.CreatedAt,
            UpdatedAt = alert.UpdatedAt
        };
    }
}

#region DTOs

public record CreatePriceAlertRequest
{
    public Guid VehicleId { get; init; }
    public decimal TargetPrice { get; init; }
    public AlertCondition Condition { get; init; }
}

public record UpdateTargetPriceRequest
{
    public decimal TargetPrice { get; init; }
}

public record PriceAlertDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public decimal TargetPrice { get; init; }
    public string Condition { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsTriggered { get; init; }
    public DateTime? TriggeredAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

#endregion
