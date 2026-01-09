using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReviewService.Application.Features.Reviews.Commands;
using ReviewService.Application.Features.Reviews.Queries;
using ReviewService.Application.DTOs;
using System.Security.Claims;

namespace ReviewService.Api.Controllers;

/// &lt;summary&gt;
/// API para gestión de reviews de vendedores/dealers
/// &lt;/summary&gt;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger&lt;ReviewsController&gt; _logger;

    public ReviewsController(IMediator mediator, ILogger&lt;ReviewsController&gt; logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// &lt;summary&gt;
    /// Obtener reviews de un vendedor con paginación y filtros
    /// &lt;/summary&gt;
    /// &lt;param name="sellerId"&gt;ID del vendedor&lt;/param&gt;
    /// &lt;param name="page"&gt;Número de página&lt;/param&gt;
    /// &lt;param name="pageSize"&gt;Tamaño de página&lt;/param&gt;
    /// &lt;param name="rating"&gt;Filtrar por rating específico&lt;/param&gt;
    /// &lt;param name="onlyVerified"&gt;Solo compras verificadas&lt;/param&gt;
    /// &lt;returns&gt;Lista paginada de reviews&lt;/returns&gt;
    [HttpGet("seller/{sellerId:guid}")]
    [ProducesResponseType(typeof(PagedReviewsDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task&lt;ActionResult&lt;PagedReviewsDto&gt;&gt; GetSellerReviews(
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

    /// &lt;summary&gt;
    /// Obtener estadísticas de reviews de un vendedor
    /// &lt;/summary&gt;
    /// &lt;param name="sellerId"&gt;ID del vendedor&lt;/param&gt;
    /// &lt;returns&gt;Estadísticas agregadas&lt;/returns&gt;
    [HttpGet("seller/{sellerId:guid}/summary")]
    [ProducesResponseType(typeof(ReviewSummaryDto), 200)]
    [ProducesResponseType(404)]
    public async Task&lt;ActionResult&lt;ReviewSummaryDto&gt;&gt; GetReviewSummary([FromRoute] Guid sellerId)
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

    /// &lt;summary&gt;
    /// Obtener una review específica por ID
    /// &lt;/summary&gt;
    /// &lt;param name="reviewId"&gt;ID de la review&lt;/param&gt;
    /// &lt;returns&gt;Datos de la review&lt;/returns&gt;
    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(404)]
    public async Task&lt;ActionResult&lt;ReviewDto&gt;&gt; GetReview([FromRoute] Guid reviewId)
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

    /// &lt;summary&gt;
    /// Crear una nueva review (requiere autenticación)
    /// &lt;/summary&gt;
    /// &lt;param name="dto"&gt;Datos de la review&lt;/param&gt;
    /// &lt;returns&gt;Review creada&lt;/returns&gt;
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task&lt;ActionResult&lt;ReviewDto&gt;&gt; CreateReview([FromBody] CreateReviewDto dto)
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

    /// &lt;summary&gt;
    /// Actualizar una review existente (solo el autor)
    /// &lt;/summary&gt;
    /// &lt;param name="reviewId"&gt;ID de la review&lt;/param&gt;
    /// &lt;param name="dto"&gt;Nuevos datos&lt;/param&gt;
    /// &lt;returns&gt;Review actualizada&lt;/returns&gt;
    [HttpPut("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task&lt;ActionResult&lt;ReviewDto&gt;&gt; UpdateReview([FromRoute] Guid reviewId, [FromBody] UpdateReviewDto dto)
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

    /// &lt;summary&gt;
    /// Eliminar una review (solo el autor)
    /// &lt;/summary&gt;
    /// &lt;param name="reviewId"&gt;ID de la review&lt;/param&gt;
    /// &lt;returns&gt;Confirmación&lt;/returns&gt;
    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task&lt;ActionResult&gt; DeleteReview([FromRoute] Guid reviewId)
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

    /// &lt;summary&gt;
    /// Moderar una review (solo admins)
    /// &lt;/summary&gt;
    /// &lt;param name="reviewId"&gt;ID de la review&lt;/param&gt;
    /// &lt;param name="isApproved"&gt;Aprobar o rechazar&lt;/param&gt;
    /// &lt;param name="rejectionReason"&gt;Razón de rechazo (opcional)&lt;/param&gt;
    /// &lt;returns&gt;Confirmación&lt;/returns&gt;
    [HttpPost("{reviewId:guid}/moderate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task&lt;ActionResult&gt; ModerateReview(
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