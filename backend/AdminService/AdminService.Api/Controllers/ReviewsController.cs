using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Api.Controllers;

/// <summary>
/// Admin controller for managing reviews — proxies to ReviewService
/// </summary>
[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewServiceClient _reviewServiceClient;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IReviewServiceClient reviewServiceClient,
        ILogger<ReviewsController> logger)
    {
        _reviewServiceClient = reviewServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// List all reviews with pagination and filters
    /// GET /api/admin/reviews?page=1&pageSize=20&search=...&status=...
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminReviewListResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _reviewServiceClient.GetAdminReviewsAsync(page, pageSize, search, status, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin reviews");
            return StatusCode(500, new { message = "Error al obtener reviews" });
        }
    }

    /// <summary>
    /// Get reported/flagged reviews
    /// GET /api/admin/reviews/reported
    /// </summary>
    [HttpGet("reported")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetReportedReviews(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _reviewServiceClient.GetFlaggedReviewsAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reported reviews");
            return StatusCode(500, new { message = "Error al obtener reviews reportadas" });
        }
    }

    /// <summary>
    /// Get global review statistics
    /// GET /api/admin/reviews/stats
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminReviewStatsResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetReviewStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _reviewServiceClient.GetAdminReviewStatsAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching review stats");
            return StatusCode(500, new { message = "Error al obtener estadísticas de reviews" });
        }
    }

    /// <summary>
    /// Approve a review
    /// POST /api/admin/reviews/{id}/approve
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ApproveReview([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _reviewServiceClient.ApproveReviewAsync(id, cancellationToken);
            return Ok(new { message = "Review aprobada exitosamente" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "ReviewService error approving review {ReviewId}", id);
            return BadRequest(new { message = "No se pudo aprobar la review" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", id);
            return StatusCode(500, new { message = "Error al aprobar la review" });
        }
    }

    /// <summary>
    /// Reject a review with optional reason
    /// POST /api/admin/reviews/{id}/reject
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> RejectReview(
        [FromRoute] Guid id,
        [FromBody] RejectReviewRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _reviewServiceClient.RejectReviewAsync(id, request?.Reason, cancellationToken);
            return Ok(new { message = "Review rechazada exitosamente" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "ReviewService error rejecting review {ReviewId}", id);
            return BadRequest(new { message = "No se pudo rechazar la review" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting review {ReviewId}", id);
            return StatusCode(500, new { message = "Error al rechazar la review" });
        }
    }

    /// <summary>
    /// Delete a review (admin — no ownership check)
    /// DELETE /api/admin/reviews/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteReview([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _reviewServiceClient.DeleteReviewAsync(id, cancellationToken);
            return Ok(new { message = "Review eliminada exitosamente" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "ReviewService error deleting review {ReviewId}", id);
            return BadRequest(new { message = "No se pudo eliminar la review" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            return StatusCode(500, new { message = "Error al eliminar la review" });
        }
    }
}

/// <summary>
/// Request body for reject action
/// </summary>
public record RejectReviewRequest(string? Reason);
