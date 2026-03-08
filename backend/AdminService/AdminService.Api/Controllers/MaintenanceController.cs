using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for platform maintenance mode management (consolidated from MaintenanceService).
/// TODO: Implement full CQRS handlers with MediatR when MaintenanceService logic is migrated.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MaintenanceController : ControllerBase
{
    private readonly ILogger<MaintenanceController> _logger;

    // TODO: Replace with persistent storage (database or Redis) when fully implemented
    private static MaintenanceStatus _currentStatus = new(
        IsEnabled: false,
        Message: null,
        StartedAt: null,
        EstimatedEndAt: null,
        StartedBy: null
    );

    public MaintenanceController(ILogger<MaintenanceController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get current maintenance mode status
    /// </summary>
    /// <returns>Current maintenance status</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MaintenanceStatus), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Getting maintenance status. IsEnabled={IsEnabled}", _currentStatus.IsEnabled);

        return Ok(_currentStatus);
    }

    /// <summary>
    /// Update maintenance mode status (enable/disable with details)
    /// </summary>
    /// <param name="request">New maintenance status</param>
    /// <returns>Updated maintenance status</returns>
    [HttpPost("status")]
    [ProducesResponseType(typeof(MaintenanceStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SetStatus([FromBody] SetMaintenanceStatusRequest request)
    {
        _logger.LogInformation("Setting maintenance status: IsEnabled={IsEnabled}, Message={Message}",
            request.IsEnabled, request.Message);

        // TODO: Replace with MediatR command and persist to database
        _currentStatus = new MaintenanceStatus(
            IsEnabled: request.IsEnabled,
            Message: request.Message,
            StartedAt: request.IsEnabled ? DateTime.UtcNow : null,
            EstimatedEndAt: request.EstimatedEndAt,
            StartedBy: User.Identity?.Name ?? "system"
        );

        return Ok(_currentStatus);
    }

    /// <summary>
    /// Toggle maintenance mode on/off
    /// </summary>
    /// <returns>Updated maintenance status</returns>
    [HttpPost("toggle")]
    [ProducesResponseType(typeof(MaintenanceStatus), StatusCodes.Status200OK)]
    public IActionResult Toggle()
    {
        var newState = !_currentStatus.IsEnabled;
        _logger.LogInformation("Toggling maintenance mode: {OldState} -> {NewState}",
            _currentStatus.IsEnabled, newState);

        // TODO: Replace with MediatR command and persist to database
        _currentStatus = new MaintenanceStatus(
            IsEnabled: newState,
            Message: newState ? "Platform is under maintenance" : null,
            StartedAt: newState ? DateTime.UtcNow : null,
            EstimatedEndAt: null,
            StartedBy: newState ? (User.Identity?.Name ?? "system") : null
        );

        return Ok(_currentStatus);
    }
}

/// <summary>
/// Current maintenance mode status
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record MaintenanceStatus(
    bool IsEnabled,
    string? Message,
    DateTime? StartedAt,
    DateTime? EstimatedEndAt,
    string? StartedBy
);

/// <summary>
/// Request to set maintenance mode status
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record SetMaintenanceStatusRequest(
    bool IsEnabled,
    string? Message = null,
    DateTime? EstimatedEndAt = null
);
