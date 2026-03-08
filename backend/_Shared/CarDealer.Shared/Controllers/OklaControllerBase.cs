using Microsoft.AspNetCore.Mvc;

namespace CarDealer.Shared.Controllers;

/// <summary>
/// Base controller for all OKLA microservice controllers.
/// Provides standardized response helpers ensuring consistent
/// response shapes across all services.
/// 
/// Usage:
///   [ApiController]
///   [Route("api/[controller]")]
///   public class VehiclesController : OklaControllerBase { ... }
/// </summary>
[ApiController]
public abstract class OklaControllerBase : ControllerBase
{
    /// <summary>
    /// Returns 200 OK with standard response wrapper.
    /// Use for successful GET, PUT, PATCH operations.
    /// </summary>
    protected IActionResult OkResponse<T>(T data, string? message = null)
        => Ok(new StandardResponse<T> { Success = true, Data = data, Message = message });

    /// <summary>
    /// Returns 201 Created with standard response wrapper.
    /// Use for successful POST creation operations.
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data, string? message = null)
        => StatusCode(201, new StandardResponse<T> { Success = true, Data = data, Message = message });

    /// <summary>
    /// Returns 201 Created with standard response wrapper and Location header via CreatedAtAction.
    /// Use when the new resource has a GET endpoint to link to.
    /// </summary>
    protected IActionResult CreatedAtResponse<T>(string actionName, object routeValues, T data, string? message = null)
        => CreatedAtAction(actionName, routeValues, new StandardResponse<T> { Success = true, Data = data, Message = message });

    /// <summary>
    /// Returns 204 No Content with no body.
    /// Use for successful DELETE operations.
    /// </summary>
    protected new IActionResult NoContent()
        => StatusCode(204);

    /// <summary>
    /// Returns 400 Bad Request with consistent error shape.
    /// Use for validation failures and malformed requests.
    /// </summary>
    protected IActionResult BadRequestResponse(string message)
        => BadRequest(new StandardResponse<object> { Success = false, Message = message });

    /// <summary>
    /// Returns 404 Not Found with consistent error shape.
    /// Use when a requested resource doesn't exist.
    /// </summary>
    protected IActionResult NotFoundResponse(string message)
        => NotFound(new StandardResponse<object> { Success = false, Message = message });

    /// <summary>
    /// Returns 403 Forbidden with consistent error shape.
    /// Use when the user is authenticated but lacks permissions.
    /// </summary>
    protected IActionResult ForbiddenResponse(string message)
        => StatusCode(403, new StandardResponse<object> { Success = false, Message = message });

    /// <summary>
    /// Returns 409 Conflict with consistent error shape.
    /// Use for duplicate resource creation attempts.
    /// </summary>
    protected IActionResult ConflictResponse(string message)
        => Conflict(new StandardResponse<object> { Success = false, Message = message });

    /// <summary>
    /// Returns 402 Payment Required with consistent error shape.
    /// Use when a paid feature is accessed without subscription.
    /// </summary>
    protected IActionResult PaymentRequiredResponse(string message)
        => StatusCode(402, new StandardResponse<object> { Success = false, Message = message });

    /// <summary>
    /// Returns 500 Internal Server Error with consistent error shape.
    /// Use sparingly — prefer letting global error handling catch exceptions.
    /// </summary>
    protected IActionResult InternalErrorResponse(string message)
        => StatusCode(500, new StandardResponse<object> { Success = false, Message = message });
}

/// <summary>
/// Lightweight standard response shape for OklaControllerBase.
/// Compatible with the ApiResponse&lt;T&gt; contract used by services.
/// </summary>
public class StandardResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
