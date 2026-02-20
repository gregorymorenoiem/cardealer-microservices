using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Dealers.GetDealer;
using UserService.Application.UseCases.Dealers.VerifyDealer;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Api.Controllers;

/// <summary>
/// Admin controller for managing dealer registrations.
/// Provides endpoints to list pending dealers, approve, and reject them.
/// </summary>
[ApiController]
[Route("api/admin/dealers")]
[Authorize(Roles = "Admin,PlatformEmployee")]
public class AdminDealersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDealerRepository _dealerRepository;

    public AdminDealersController(IMediator mediator, IDealerRepository dealerRepository)
    {
        _mediator = mediator;
        _dealerRepository = dealerRepository;
    }

    /// <summary>
    /// Get all dealer registration requests. Supports filtering by verification status.
    /// </summary>
    /// <param name="status">Filter by status: Pending, UnderReview, Verified, Rejected, Suspended</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size (default 20, max 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDealersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDealerRequests(
        [FromQuery] DealerVerificationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var allDealers = await _dealerRepository.GetAllAsync();

        // Filter by status if provided
        var filteredDealers = status.HasValue
            ? allDealers.Where(d => d.VerificationStatus == status.Value)
            : allDealers;

        var totalCount = filteredDealers.Count();
        var dealers = filteredDealers
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DealerSummaryDto
            {
                Id = d.Id,
                BusinessName = d.BusinessName,
                DealerType = d.DealerType,
                LogoUrl = d.LogoUrl,
                City = d.City,
                State = d.State,
                VerificationStatus = d.VerificationStatus,
                AverageRating = d.AverageRating,
                TotalReviews = d.TotalReviews,
                ActiveListings = d.ActiveListings
            })
            .ToList();

        return Ok(new PaginatedDealersResponse
        {
            Dealers = dealers,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get pending dealer registrations awaiting approval.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PaginatedDealersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingDealers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await GetDealerRequests(DealerVerificationStatus.Pending, page, pageSize);
    }

    /// <summary>
    /// Get a specific dealer's full details (admin view includes all fields).
    /// </summary>
    [HttpGet("{dealerId:guid}")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealerDetails(Guid dealerId)
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
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Approve a dealer registration. Sets VerificationStatus to Verified and activates the dealer.
    /// Publishes DealerCreatedEvent.
    /// </summary>
    [HttpPost("{dealerId:guid}/approve")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveDealer(Guid dealerId, [FromBody] AdminDealerActionRequest? request = null)
    {
        var adminUserId = GetAdminUserId();

        try
        {
            var command = new VerifyDealerCommand(dealerId, new VerifyDealerRequest
            {
                IsVerified = true,
                VerifiedByUserId = adminUserId,
                Notes = request?.Notes
            });

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

    /// <summary>
    /// Reject a dealer registration. Sets VerificationStatus to Rejected.
    /// A rejection reason (Notes) should be provided.
    /// </summary>
    [HttpPost("{dealerId:guid}/reject")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectDealer(Guid dealerId, [FromBody] AdminDealerActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://okla.com/errors/validation",
                Title = "Validation Error",
                Status = 400,
                Detail = "Se requiere una razón para rechazar el registro del dealer."
            });
        }

        var adminUserId = GetAdminUserId();

        try
        {
            var command = new VerifyDealerCommand(dealerId, new VerifyDealerRequest
            {
                IsVerified = false,
                VerifiedByUserId = adminUserId,
                Notes = request.Notes
            });

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

    private Guid GetAdminUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

/// <summary>
/// Request body for admin dealer actions (approve/reject).
/// </summary>
public class AdminDealerActionRequest
{
    /// <summary>
    /// Admin notes or rejection reason.
    /// </summary>
    public string? Notes { get; set; }
}
