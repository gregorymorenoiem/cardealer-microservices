using DealerManagementService.Application.DTOs;
using DealerManagementService.Application.Features.Dealers.Commands;
using DealerManagementService.Application.Features.Dealers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealerManagementService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DealersController> _logger;

    public DealersController(IMediator mediator, ILogger<DealersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all dealers with pagination and filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DealerListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DealerListResponse>> GetDealers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? verificationStatus = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetDealersQuery(page, pageSize, status, verificationStatus, searchTerm);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get current dealer profile (from JWT)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> GetCurrentDealer()
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var query = new GetDealerByUserIdQuery(userId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "No dealer profile found for current user" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get dealer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> GetDealerById(Guid id)
    {
        var query = new GetDealerByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Dealer with ID {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get dealer by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> GetDealerByUserId(Guid userId)
    {
        var query = new GetDealerByUserIdQuery(userId);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Dealer for user {userId} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create new dealer account
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DealerDto>> CreateDealer([FromBody] CreateDealerRequest request)
    {
        try
        {
            var command = new CreateDealerCommand(request);
            var result = await _mediator.Send(command);
            
            return CreatedAtAction(
                nameof(GetDealerById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update dealer information
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> UpdateDealer(Guid id, [FromBody] UpdateDealerRequest request)
    {
        try
        {
            var command = new UpdateDealerCommand(id, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify dealer (admin only)
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifyDealer(
        Guid id,
        [FromBody] VerifyDealerRequest request)
    {
        try
        {
            // TODO: Get admin user ID from JWT claims
            var adminUserId = Guid.Parse(User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
            
            var command = new VerifyDealerCommand(
                id,
                request.Approved,
                request.RejectionReason,
                adminUserId);
            
            await _mediator.Send(command);
            
            return Ok(new { 
                message = request.Approved ? "Dealer verified successfully" : "Dealer rejected",
                dealerId = id 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get stats for a specific dealer
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDealerStats(Guid id)
    {
        // Verify dealer exists
        var query = new GetDealerByIdQuery(id);
        var dealer = await _mediator.Send(query);

        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {id} not found" });
        }

        // Return stats from dealer data (real aggregation can be added later)
        return Ok(new
        {
            totalListings = dealer.CurrentActiveListings,
            activeListings = dealer.CurrentActiveListings,
            totalViews = 0,
            viewsThisMonth = 0,
            viewsChange = 0.0,
            totalInquiries = 0,
            inquiriesThisMonth = 0,
            inquiriesChange = 0.0,
            pendingInquiries = 0,
            responseRate = 0.0,
            avgResponseTimeMinutes = 0.0,
            totalRevenue = 0.0,
            revenueThisMonth = 0.0,
            revenueChange = 0.0
        });
    }

    /// <summary>
    /// Get dealer statistics (admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStatistics()
    {
        // TODO: Implement statistics aggregation
        return Ok(new
        {
            totalDealers = 0,
            activeDealers = 0,
            pendingVerification = 0,
            byPlan = new
            {
                starter = 0,
                pro = 0,
                enterprise = 0
            }
        });
    }

    // ============================================
    // Sprint 7: Public Profile Endpoints
    // ============================================

    /// <summary>
    /// Get public dealer profile by slug
    /// </summary>
    [HttpGet("public/{slug}")]
    [ProducesResponseType(typeof(PublicDealerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicDealerProfileDto>> GetPublicProfile(string slug)
    {
        var query = new GetPublicProfileQuery(slug);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Dealer with slug '{slug}' not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get all trusted dealers
    /// </summary>
    [HttpGet("trusted")]
    [ProducesResponseType(typeof(List<PublicDealerProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublicDealerProfileDto>>> GetTrustedDealers()
    {
        var query = new GetTrustedDealersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update dealer public profile
    /// </summary>
    [HttpPut("{id:guid}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(PublicDealerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicDealerProfileDto>> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request)
    {
        try
        {
            var command = new UpdateDealerProfileCommand(id, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dealer profile {DealerId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get profile completion percentage and missing fields
    /// </summary>
    [HttpGet("{id:guid}/profile/completion")]
    [Authorize]
    [ProducesResponseType(typeof(ProfileCompletionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileCompletionDto>> GetProfileCompletion(Guid id)
    {
        var query = new GetProfileCompletionQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
