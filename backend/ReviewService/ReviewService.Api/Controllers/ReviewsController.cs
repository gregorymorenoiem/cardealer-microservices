using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReviewService.Application.Features.Reviews.Commands;
using ReviewService.Application.Features.Reviews.Queries;
using ReviewService.Application.DTOs;
using System.Security.Claims;

namespace ReviewService.Api.Controllers;

/// <summary>
/// API para gestión de reviews de vendedores/dealers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener reviews de un vendedor con paginación y filtros
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="rating">Filtrar por rating específico</param>
    /// <param name="onlyVerified">Solo compras verificadas</param>
    /// <returns>Lista paginada de reviews</returns>
    [HttpGet("seller/{sellerId:guid}")]
    [ProducesResponseType(typeof(PagedReviewsDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PagedReviewsDto>> GetSellerReviews(
        [FromRoute] Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? rating = null,
        [FromQuery] bool? onlyVerified = null)
    {
        try
        {
            var filters = new ReviewFiltersDto
            {
                Rating = rating,
                OnlyVerified = onlyVerified
            };

            var query = new GetSellerReviewsQuery
            {
                SellerId = sellerId,
                Page = page,
                PageSize = pageSize,
                Filters = filters,
                OnlyApproved = true
            };

            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for seller {SellerId}", sellerId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener estadísticas de reviews de un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <returns>Estadísticas agregadas</returns>
    [HttpGet("seller/{sellerId:guid}/summary")]
    [ProducesResponseType(typeof(ReviewSummaryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReviewSummaryDto>> GetReviewSummary([FromRoute] Guid sellerId)
    {
        try
        {
            var query = new GetReviewSummaryQuery { SellerId = sellerId };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review summary for seller {SellerId}", sellerId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener una review específica por ID
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <returns>Datos de la review</returns>
    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReviewDto>> GetReview([FromRoute] Guid reviewId)
    {
        try
        {
            var query = new GetReviewByIdQuery { ReviewId = reviewId };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crear una nueva review (requiere autenticación)
    /// </summary>
    /// <param name="dto">Datos de la review</param>
    /// <returns>Review creada</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
    {
        try
        {
            // Obtener información del usuario autenticado
            var buyerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var buyerNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(buyerIdClaim) || !Guid.TryParse(buyerIdClaim, out var buyerId))
            {
                return Unauthorized("Token inválido");
            }

            var command = new CreateReviewCommand
            {
                BuyerId = buyerId,
                SellerId = dto.SellerId,
                VehicleId = dto.VehicleId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Title = dto.Title,
                Content = dto.Content,
                BuyerName = buyerNameClaim ?? "Usuario Anónimo",
                BuyerPhotoUrl = null // Se puede obtener del UserService si está disponible
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetReview), new { reviewId = result.Value!.Id }, result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar una review existente (solo el autor)
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="dto">Nuevos datos</param>
    /// <returns>Review actualizada</returns>
    [HttpPut("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ReviewDto>> UpdateReview([FromRoute] Guid reviewId, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var buyerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(buyerIdClaim) || !Guid.TryParse(buyerIdClaim, out var buyerId))
            {
                return Unauthorized("Token inválido");
            }

            var command = new UpdateReviewCommand
            {
                ReviewId = reviewId,
                BuyerId = buyerId,
                Rating = dto.Rating,
                Title = dto.Title,
                Content = dto.Content
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            // Si el error contiene "forbidden" o "unauthorized", retornar 403
            if (result.Error.Contains("no autorizado") || result.Error.Contains("forbidden"))
            {
                return Forbid(result.Error);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar una review (solo el autor)
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <returns>Confirmación</returns>
    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> DeleteReview([FromRoute] Guid reviewId)
    {
        try
        {
            var buyerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(buyerIdClaim) || !Guid.TryParse(buyerIdClaim, out var buyerId))
            {
                return Unauthorized("Token inválido");
            }

            var command = new DeleteReviewCommand
            {
                ReviewId = reviewId,
                BuyerId = buyerId
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { message = "Review eliminada exitosamente" });
            }

            if (result.Error.Contains("no autorizado") || result.Error.Contains("forbidden"))
            {
                return Forbid(result.Error);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Moderar una review (solo admins)
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="isApproved">Aprobar o rechazar</param>
    /// <param name="rejectionReason">Razón de rechazo (opcional)</param>
    /// <returns>Confirmación</returns>
    [HttpPost("{reviewId:guid}/moderate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> ModerateReview(
        [FromRoute] Guid reviewId,
        [FromQuery] bool isApproved,
        [FromQuery] string? rejectionReason = null)
    {
        try
        {
            var moderatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(moderatorIdClaim) || !Guid.TryParse(moderatorIdClaim, out var moderatorId))
            {
                return Unauthorized("Token inválido");
            }

            var command = new ModerateReviewCommand
            {
                ReviewId = reviewId,
                ModeratorId = moderatorId,
                IsApproved = isApproved,
                RejectionReason = rejectionReason
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var status = isApproved ? "aprobada" : "rechazada";
                return Ok(new { message = $"Review {status} exitosamente" });
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating review {ReviewId}", reviewId);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}