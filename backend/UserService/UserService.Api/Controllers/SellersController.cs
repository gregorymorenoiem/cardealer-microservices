using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Sellers.CreateSellerProfile;
using UserService.Application.UseCases.Sellers.GetSellerProfile;
using UserService.Application.UseCases.Sellers.UpdateSellerProfile;
using UserService.Application.UseCases.Sellers.VerifySellerProfile;
using UserService.Application.UseCases.Sellers.GetSellerStats;
using UserService.Application.UseCases.Sellers.ConvertBuyerToSeller;
using System.Security.Claims;

namespace UserService.Api.Controllers
{
    /// <summary>
    /// Controller for managing seller profiles (individual sellers)
    /// </summary>
    [ApiController]
    [Route("api/sellers")]
    public class SellersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public SellersController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        /// <summary>
        /// Convert a buyer account to a seller account.
        /// Rejects Dealer/DealerEmployee accounts with CONVERSION_NOT_ALLOWED.
        /// Supports Idempotency-Key header for safe retries.
        /// </summary>
        [HttpPost("convert")]
        [Authorize]
        [ProducesResponseType(typeof(SellerConversionResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(SellerConversionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConvertBuyerToSeller(
            [FromBody] ConvertBuyerToSellerRequest request)
        {
            // Feature flag check
            var featureEnabled = _configuration.GetValue<bool>("Features:SellerConversion", true);
            if (!featureEnabled)
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "https://okla.com/errors/feature-disabled",
                    Title = "Feature Disabled",
                    Status = 400,
                    Detail = "La conversión a vendedor no está disponible en este momento.",
                    Extensions = { ["errorCode"] = "FEATURE_DISABLED" }
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Type = "https://okla.com/errors/unauthorized",
                    Title = "Unauthorized",
                    Status = 401,
                    Detail = "No se pudo identificar al usuario autenticado."
                });
            }

            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.FirstOrDefault();

            try
            {
                var command = new ConvertBuyerToSellerCommand(
                    UserId: userId,
                    Request: request,
                    IdempotencyKey: idempotencyKey,
                    IpAddress: ipAddress,
                    UserAgent: userAgent);

                var result = await _mediator.Send(command);

                // If conversion was idempotent (existing), return 200
                if (result.Source == "existing" || result.ConversionId == Guid.Empty)
                {
                    return Ok(result);
                }

                // If pending KYC verification, return 202
                if (result.PendingVerification)
                {
                    return StatusCode(StatusCodes.Status202Accepted, result);
                }

                // New conversion — return 201
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CONVERSION_NOT_ALLOWED")
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "https://okla.com/errors/conversion-not-allowed",
                    Title = "Conversion Not Allowed",
                    Status = 400,
                    Detail = "Los compradores no pueden convertirse en Dealers. Para registrar una empresa, utiliza el flujo 'Registrar Dealer'.",
                    Extensions =
                    {
                        ["errorCode"] = "CONVERSION_NOT_ALLOWED"
                    }
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
        /// Create a new seller profile
        /// </summary>
        /// <param name="request">Seller profile creation data</param>
        /// <returns>The created seller profile</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SellerProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SellerProfileDto>> CreateSellerProfile([FromBody] CreateSellerProfileRequest request)
        {
            try
            {
                var command = new CreateSellerProfileCommand(request);
                var result = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetSellerProfile),
                    new { sellerId = result.Id },
                    result
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a seller profile by ID
        /// </summary>
        /// <param name="sellerId">The seller profile ID</param>
        /// <returns>The seller profile details</returns>
        [HttpGet("{sellerId:guid}")]
        [ProducesResponseType(typeof(SellerProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SellerProfileDto>> GetSellerProfile(Guid sellerId)
        {
            var query = new GetSellerProfileQuery(sellerId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { error = "Seller profile not found" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get seller profile by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The seller profile details</returns>
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(SellerProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SellerProfileDto>> GetSellerProfileByUser(Guid userId)
        {
            var query = new GetSellerProfileByUserQuery(userId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { error = "Seller profile not found" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Update a seller profile
        /// </summary>
        /// <param name="sellerId">The seller profile ID</param>
        /// <param name="request">Updated seller profile data</param>
        /// <returns>The updated seller profile</returns>
        [HttpPut("{sellerId:guid}")]
        [ProducesResponseType(typeof(SellerProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SellerProfileDto>> UpdateSellerProfile(
            Guid sellerId,
            [FromBody] UpdateSellerProfileRequest request)
        {
            try
            {
                var command = new UpdateSellerProfileCommand(sellerId, request);
                var result = await _mediator.Send(command);

                if (result == null)
                {
                    return NotFound(new { error = "Seller profile not found" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verify a seller profile (admin only)
        /// </summary>
        /// <param name="sellerId">The seller profile ID</param>
        /// <param name="request">Verification data</param>
        /// <returns>Success status</returns>
        [HttpPost("{sellerId:guid}/verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> VerifySellerProfile(
            Guid sellerId,
            [FromBody] VerifySellerProfileRequest request)
        {
            try
            {
                var command = new VerifySellerProfileCommand(sellerId, request);
                var result = await _mediator.Send(command);

                return Ok(new { message = request.IsVerified ? "Seller verified successfully" : "Seller verification revoked" });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new { error = ex.Message });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get seller statistics
        /// </summary>
        /// <param name="sellerId">The seller profile ID</param>
        /// <returns>Seller statistics</returns>
        [HttpGet("{sellerId:guid}/stats")]
        [ProducesResponseType(typeof(SellerStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SellerStatsDto>> GetSellerStats(Guid sellerId)
        {
            var query = new GetSellerStatsQuery(sellerId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { error = "Seller profile not found" });
            }

            return Ok(result);
        }
    }
}
