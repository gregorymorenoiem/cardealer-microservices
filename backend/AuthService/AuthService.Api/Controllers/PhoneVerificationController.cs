using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.PhoneVerification;
using AuthService.Application.Features.TwoFactor.Commands.SendPhoneVerification;
using AuthService.Application.Features.TwoFactor.Commands.VerifyPhoneNumber;
using AuthService.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("AuthPolicy")]
public class PhoneVerificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PhoneVerificationController> _logger;

    public PhoneVerificationController(IMediator mediator, ILogger<PhoneVerificationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Send phone verification code via SMS
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<SendPhoneVerificationResponse>>> SendVerification(
        [FromBody] SendPhoneVerificationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<SendPhoneVerificationResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Sending phone verification code to user {UserId} for phone {PhoneNumber}",
                userId!, request.PhoneNumber);

            var command = new SendPhoneVerificationCommand(userId!, request.PhoneNumber);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Phone verification code sent successfully to user {UserId}", userId);

            return Ok(ApiResponse<SendPhoneVerificationResponse>.Ok(result, new Dictionary<string, object>
            {
                ["expiresAt"] = result.ExpiresAt!,
                ["phoneNumber"] = request.PhoneNumber
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send phone verification code for user");
            return BadRequest(ApiResponse<SendPhoneVerificationResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Verify phone number with received code
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<VerifyPhoneNumberResponse>>> Verify(
        [FromBody] VerifyPhoneRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<VerifyPhoneNumberResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Verifying phone number for user {UserId} with code", userId!);

            var command = new VerifyPhoneNumberCommand(userId!, request.PhoneNumber, request.VerificationCode);
            var result = await _mediator.Send(command);

            if (result.Success && result.IsVerified)
            {
                _logger.LogInformation("Phone number verified successfully for user {UserId}", userId);
                return Ok(ApiResponse<VerifyPhoneNumberResponse>.Ok(result, new Dictionary<string, object>
                {
                    ["isVerified"] = true,
                    ["phoneNumber"] = request.PhoneNumber
                }));
            }
            else
            {
                _logger.LogWarning("Phone number verification failed for user {UserId}", userId);
                return BadRequest(ApiResponse<VerifyPhoneNumberResponse>.Fail(result.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify phone number for user");
            return BadRequest(ApiResponse<VerifyPhoneNumberResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Resend phone verification code
    /// </summary>
    [HttpPost("resend")]
    public async Task<ActionResult<ApiResponse<SendPhoneVerificationResponse>>> ResendVerification(
        [FromBody] ResendPhoneVerificationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<SendPhoneVerificationResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Resending phone verification code to user {UserId} for phone {PhoneNumber}",
                userId!, request.PhoneNumber);

            var command = new SendPhoneVerificationCommand(userId!, request.PhoneNumber);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Phone verification code resent successfully to user {UserId}", userId);

            return Ok(ApiResponse<SendPhoneVerificationResponse>.Ok(result, new Dictionary<string, object>
            {
                ["expiresAt"] = result.ExpiresAt!,
                ["phoneNumber"] = request.PhoneNumber,
                ["isResend"] = true
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend phone verification code for user");
            return BadRequest(ApiResponse<SendPhoneVerificationResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Check if phone number is verified for current user
    /// </summary>
    [HttpGet("status")]
    public Task<ActionResult<ApiResponse<PhoneVerificationStatusResponse>>> GetVerificationStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Task.FromResult<ActionResult<ApiResponse<PhoneVerificationStatusResponse>>>(
                    Unauthorized(ApiResponse<PhoneVerificationStatusResponse>.Fail("User not authenticated")));
            }

            _logger.LogInformation("Getting phone verification status for user {UserId}", userId!);

            // Note: You'll need to create GetPhoneVerificationStatusQuery and its handler
            // var query = new GetPhoneVerificationStatusQuery(userId);
            // var result = await _mediator.Send(query);

            // Placeholder response
            var result = new PhoneVerificationStatusResponse(false, null, DateTime.UtcNow);

            return Task.FromResult<ActionResult<ApiResponse<PhoneVerificationStatusResponse>>>(
                Ok(ApiResponse<PhoneVerificationStatusResponse>.Ok(result)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get phone verification status for user");
            return Task.FromResult<ActionResult<ApiResponse<PhoneVerificationStatusResponse>>>(
                BadRequest(ApiResponse<PhoneVerificationStatusResponse>.Fail(ex.Message)));
        }
    }

    /// <summary>
    /// Update phone number and send verification
    /// </summary>
    [HttpPut("update")]
    public async Task<ActionResult<ApiResponse<SendPhoneVerificationResponse>>> UpdatePhoneNumber(
        [FromBody] UpdatePhoneNumberRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<SendPhoneVerificationResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Updating phone number for user {UserId} to {NewPhoneNumber}",
                userId!, request.NewPhoneNumber);

            // Note: You'll need to create UpdatePhoneNumberCommand and its handler
            // var command = new UpdatePhoneNumberCommand(userId, request.NewPhoneNumber);
            // var result = await _mediator.Send(command);

            // For now, using the send verification command
            var sendCommand = new SendPhoneVerificationCommand(userId!, request.NewPhoneNumber);
            var result = await _mediator.Send(sendCommand);

            _logger.LogInformation("Phone number updated and verification sent for user {UserId}", userId!);

            return Ok(ApiResponse<SendPhoneVerificationResponse>.Ok(result, new Dictionary<string, object>
            {
                ["expiresAt"] = result.ExpiresAt!,
                ["phoneNumber"] = request.NewPhoneNumber,
                ["isUpdate"] = true
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update phone number for user");
            return BadRequest(ApiResponse<SendPhoneVerificationResponse>.Fail(ex.Message));
        }
    }
}
