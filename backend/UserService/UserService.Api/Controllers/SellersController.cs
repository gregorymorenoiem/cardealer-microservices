using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Sellers.CreateSellerProfile;
using UserService.Application.UseCases.Sellers.GetSellerProfile;
using UserService.Application.UseCases.Sellers.UpdateSellerProfile;
using UserService.Application.UseCases.Sellers.VerifySellerProfile;
using UserService.Application.UseCases.Sellers.GetSellerStats;

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

        public SellersController(IMediator mediator)
        {
            _mediator = mediator;
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
