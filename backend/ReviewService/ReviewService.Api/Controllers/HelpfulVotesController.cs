using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Application.DTOs;
using ReviewService.Application.Features.HelpfulVotes.Commands;
using ReviewService.Application.Features.HelpfulVotes.Queries;

namespace ReviewService.Api.Controllers;

/// <summary>
/// Controller para votos útiles de reviews (Sprint 15)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HelpfulVotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HelpfulVotesController> _logger;

    public HelpfulVotesController(IMediator mediator, ILogger<HelpfulVotesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Votar una review como útil o no útil
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="isHelpful">True si es útil, False si no</param>
    /// <returns>Voto creado o actualizado</returns>
    [HttpPost("{reviewId}/vote")]
    public async Task<ActionResult<ReviewHelpfulVoteDto>> VoteReviewHelpful(
        Guid reviewId, 
        [FromBody] VoteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var command = new VoteReviewHelpfulCommand(reviewId, userId, request.IsHelpful, userIpAddress);
            var result = await _mediator.Send(command);

            _logger.LogInformation("User {UserId} voted review {ReviewId} as {IsHelpful}", 
                userId, reviewId, request.IsHelpful ? "helpful" : "not helpful");

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting review {ReviewId}", reviewId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Obtener estadísticas de votos de una review
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <returns>Estadísticas de votos</returns>
    [HttpGet("{reviewId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewVoteStatsDto>> GetReviewVoteStats(Guid reviewId)
    {
        try
        {
            var query = new GetReviewVoteStatsQuery(reviewId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vote stats for review {ReviewId}", reviewId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }
}

/// <summary>
/// Request para votar una review
/// </summary>
public record VoteRequest(bool IsHelpful);