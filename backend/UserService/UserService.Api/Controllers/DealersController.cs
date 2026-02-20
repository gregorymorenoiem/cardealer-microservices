using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Dealers.CreateDealer;
using UserService.Application.UseCases.Dealers.GetDealer;
using UserService.Application.UseCases.Dealers.UpdateDealer;
using System.Security.Claims;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller for dealer (company) registration and management.
/// Dealers must be approved by an admin before they can list vehicles.
/// </summary>
[ApiController]
[Route("api/dealers")]
public class DealersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public DealersController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new dealer (company). Status will be Pending until admin approves.
    /// Requires authentication. The authenticated user becomes the dealer owner.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterDealer([FromBody] CreateDealerRequest request)
    {
        // Feature flag check
        var featureEnabled = _configuration.GetValue<bool>("Features:DealerRegistration", true);
        if (!featureEnabled)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://okla.com/errors/feature-disabled",
                Title = "Feature Disabled",
                Status = 400,
                Detail = "El registro de dealers no está disponible en este momento."
            });
        }

        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Type = "https://okla.com/errors/unauthorized",
                Title = "Unauthorized",
                Status = 401,
                Detail = "No se pudo identificar al usuario autenticado."
            });
        }

        // Override OwnerUserId with authenticated user
        request.OwnerUserId = userId.Value;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.FirstOrDefault();

        try
        {
            var command = new CreateDealerCommand(
                Request: request,
                IpAddress: ipAddress,
                UserAgent: userAgent);

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetDealer), new { dealerId = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message == "ALREADY_DEALER")
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://okla.com/errors/already-dealer",
                Title = "Already a Dealer",
                Status = 400,
                Detail = "Ya tienes una cuenta de dealer registrada.",
                Extensions = { ["errorCode"] = "ALREADY_DEALER" }
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://okla.com/errors/not-found",
                Title = "User Not Found",
                Status = 404,
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Get dealer by ID. Returns full dealer details for verified dealers.
    /// </summary>
    [HttpGet("{dealerId:guid}")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealer(Guid dealerId)
    {
        try
        {
            var query = new GetDealerQuery(dealerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://okla.com/errors/not-found",
                Title = "Dealer Not Found",
                Status = 404,
                Detail = $"Dealer {dealerId} no fue encontrado."
            });
        }
    }

    /// <summary>
    /// Get dealer by owner user ID. Used to check if current user already has a dealer account.
    /// </summary>
    [HttpGet("owner/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealerByOwner(Guid userId)
    {
        var query = new GetDealerByOwnerQuery(userId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://okla.com/errors/not-found",
                Title = "Dealer Not Found",
                Status = 404,
                Detail = "El usuario no tiene una cuenta de dealer."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the dealer profile for the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyDealer()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetDealerByOwnerQuery(userId.Value);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://okla.com/errors/not-found",
                Title = "Dealer Not Found",
                Status = 404,
                Detail = "No tienes una cuenta de dealer registrada."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Update dealer profile. Only the owner can update their dealer.
    /// </summary>
    [HttpPut("{dealerId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateDealer(Guid dealerId, [FromBody] UpdateDealerRequest request)
    {
        try
        {
            var command = new UpdateDealerCommand(dealerId, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://okla.com/errors/not-found",
                Title = "Dealer Not Found",
                Status = 404,
                Detail = ex.Message
            });
        }
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
