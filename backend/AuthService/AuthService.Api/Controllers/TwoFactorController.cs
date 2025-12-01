using AuthService.Application.DTOs.TwoFactor;
using AuthService.Application.Features.TwoFactor.Commands.Disable2FA;
using AuthService.Application.Features.TwoFactor.Commands.Enable2FA;
using AuthService.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;
using AuthService.Application.Features.TwoFactor.Commands.TwoFactorLogin;
using AuthService.Application.Features.TwoFactor.Commands.Verify2FA;
using AuthService.Application.Features.TwoFactor.Commands.VerifyRecoveryCode;
using AuthService.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TwoFactorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TwoFactorController> _logger;

    public TwoFactorController(IMediator mediator, ILogger<TwoFactorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Enable two-factor authentication for the current user
    /// </summary>
    [HttpPost("enable")]
    public async Task<ActionResult<ApiResponse<Enable2FAResponse>>> Enable2FA([FromBody] Enable2FARequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<Enable2FAResponse>.Fail("User not authenticated"));
            }

            var command = new Enable2FACommand(userId, request.Type);
            var result = await _mediator.Send(command);

            _logger.LogInformation("2FA enabled successfully for user {UserId} with type {Type}", userId, request.Type);
            return Ok(ApiResponse<Enable2FAResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA for user");
            return BadRequest(ApiResponse<Enable2FAResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Verify two-factor authentication setup
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<Verify2FAResponse>>> Verify2FA([FromBody] Verify2FARequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<Verify2FAResponse>.Fail("User not authenticated"));
            }

            var command = new Verify2FACommand(userId, request.Code, request.Type);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("2FA verified successfully for user {UserId}", userId);
                return Ok(ApiResponse<Verify2FAResponse>.Ok(result));
            }
            else
            {
                return BadRequest(ApiResponse<Verify2FAResponse>.Fail(result.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA for user");
            return BadRequest(ApiResponse<Verify2FAResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Disable two-factor authentication for the current user
    /// </summary>
    [HttpPost("disable")]
    public async Task<ActionResult<ApiResponse<Verify2FAResponse>>> Disable2FA([FromBody] Disable2FARequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<Verify2FAResponse>.Fail("User not authenticated"));
            }

            var command = new Disable2FACommand(userId, request.Password);
            var result = await _mediator.Send(command);

            _logger.LogInformation("2FA disabled successfully for user {UserId}", userId);
            return Ok(ApiResponse<Verify2FAResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA for user");
            return BadRequest(ApiResponse<Verify2FAResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Generate new recovery codes for two-factor authentication
    /// </summary>
    [HttpPost("generate-recovery-codes")]
    public async Task<ActionResult<ApiResponse<GenerateRecoveryCodesResponse>>> GenerateRecoveryCodes([FromBody] GenerateRecoveryCodesRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<GenerateRecoveryCodesResponse>.Fail("User not authenticated"));
            }

            var command = new GenerateRecoveryCodesCommand(userId, request.Password);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Recovery codes generated successfully for user {UserId}", userId);
            return Ok(ApiResponse<GenerateRecoveryCodesResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery codes for user");
            return BadRequest(ApiResponse<GenerateRecoveryCodesResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Verify a recovery code for two-factor authentication
    /// </summary>
    [HttpPost("verify-recovery-code")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Verify2FAResponse>>> VerifyRecoveryCode([FromBody] VerifyRecoveryCodeRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<Verify2FAResponse>.Fail("User not authenticated"));
            }

            var command = new VerifyRecoveryCodeCommand(userId, request.Code);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("Recovery code verified successfully for user {UserId}", userId);
                return Ok(ApiResponse<Verify2FAResponse>.Ok(result));
            }
            else
            {
                return BadRequest(ApiResponse<Verify2FAResponse>.Fail(result.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying recovery code for user");
            return BadRequest(ApiResponse<Verify2FAResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Complete two-factor authentication login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TwoFactorLoginResponse>>> TwoFactorLogin([FromBody] TwoFactorLoginRequest request)
    {
        try
        {
            var command = new TwoFactorLoginCommand(request.TempToken, request.TwoFactorCode);
            var result = await _mediator.Send(command);

            _logger.LogInformation("2FA login completed successfully for user {UserId}", result.UserId);
            return Ok(ApiResponse<TwoFactorLoginResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA login");
            return BadRequest(ApiResponse<TwoFactorLoginResponse>.Fail(ex.Message));
        }
    }
}
