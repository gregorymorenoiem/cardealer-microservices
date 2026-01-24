using AuthService.Application.Features.Auth.Commands.Login;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.Features.Auth.Commands.ForgotPassword;
using AuthService.Application.Features.Auth.Commands.ResetPassword;
using AuthService.Application.Features.Auth.Commands.RefreshToken;
using AuthService.Application.Features.Auth.Commands.Logout;
using AuthService.Application.Features.Auth.Commands.VerifyEmail;
using AuthService.Application.Features.Auth.Commands.ResendVerification;
using AuthService.Application.Features.Auth.Commands.RequestPasswordSetup;
using AuthService.Application.Features.Auth.Commands.ValidatePasswordSetupToken;
using AuthService.Application.Features.Auth.Commands.SetPasswordForOAuthUser;
using AuthService.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthService.Application.DTOs.Auth;
using CarDealer.Shared.Audit.Attributes;
using CarDealer.Shared.Audit.Models;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    [Audit("AUTH_REGISTER", "Register", ResourceType = "User", Severity = AuditSeverity.Warning)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.UserName, request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RegisterResponse>.Ok(result));
    }

    [HttpPost("login")]
    [Audit("AUTH_LOGIN", "Login", ResourceType = "User", Severity = AuditSeverity.Warning)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("forgot-password")]
    [Audit("AUTH_FORGOT_PASSWORD", "ForgotPassword", ResourceType = "User", Severity = AuditSeverity.Info)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ForgotPasswordResponse>.Ok(result));
    }

    [HttpPost("reset-password")]
    [Audit("AUTH_RESET_PASSWORD", "ResetPassword", ResourceType = "User", Severity = AuditSeverity.Warning)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // DEBUG: Log incoming request details
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        logger.LogInformation("ResetPassword request received - Token: {TokenLength} chars, NewPassword: {PasswordLength} chars, ConfirmPassword: {ConfirmLength} chars",
            request.Token?.Length ?? 0,
            request.NewPassword?.Length ?? 0,
            request.ConfirmPassword?.Length ?? 0);
        
        if (string.IsNullOrEmpty(request.Token))
        {
            logger.LogWarning("ResetPassword failed: Token is null or empty");
            return BadRequest(ApiResponse.Fail("Token is required"));
        }
        if (string.IsNullOrEmpty(request.NewPassword))
        {
            logger.LogWarning("ResetPassword failed: NewPassword is null or empty");
            return BadRequest(ApiResponse.Fail("New password is required"));
        }
        
        var command = new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ResetPasswordResponse>.Ok(result));
    }

    [HttpPost("verify-email")]
    [Audit("AUTH_VERIFY_EMAIL", "VerifyEmail", ResourceType = "User", Severity = AuditSeverity.Info)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var command = new VerifyEmailCommand(request.Token);
        await _mediator.Send(command);
        return Ok(ApiResponse.Ok());
    }

    [HttpPost("resend-verification")]
    [Audit("AUTH_RESEND_VERIFICATION", "ResendVerification", ResourceType = "User", Severity = AuditSeverity.Info)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var command = new ResendVerificationCommand(request.Email);
        await _mediator.Send(command);
        return Ok(ApiResponse.Ok());
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]  // RefreshToken NO debe requerir autorización (ese es su propósito)
    [Audit("AUTH_REFRESH_TOKEN", "RefreshToken", ResourceType = "Token", Severity = AuditSeverity.Debug)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RefreshTokenResponse>.Ok(result));
    }

    /// <summary>
    /// Cierra la sesión del usuario revocando su refresh token
    /// </summary>
    /// <param name="request">Request con el refreshToken a revocar</param>
    /// <returns>Confirmación del logout</returns>
    /// <response code="200">Logout exitoso, refreshToken revocado</response>
    /// <response code="400">RefreshToken no proporcionado</response>
    /// <response code="401">Token de acceso inválido o expirado</response>
    [HttpPost("logout")]
    [Authorize]
    [Audit("AUTH_LOGOUT", "Logout", ResourceType = "User", Severity = AuditSeverity.Info)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        await _mediator.Send(command);
        return Ok(ApiResponse.Ok());
    }

    /// <summary>
    /// AUTH-SEC-005: Request verification code for login from a revoked device.
    /// This is called when a login attempt is detected from a device that was previously revoked.
    /// </summary>
    [HttpPost("revoked-device/request-code")]
    [AllowAnonymous]
    [Audit("AUTH_REVOKED_DEVICE_CODE_REQUEST", "RequestRevokedDeviceCode", ResourceType = "Device", Severity = AuditSeverity.Warning)]
    [ProducesResponseType(typeof(ApiResponse<RevokedDeviceLoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestRevokedDeviceCode([FromBody] RevokedDeviceCodeRequest request)
    {
        var command = new Application.Features.Auth.Commands.VerifyRevokedDeviceLogin.RequestRevokedDeviceLoginCommand(
            request.UserId,
            request.Email,
            request.DeviceFingerprint,
            GetIpAddress(),
            Request.Headers.UserAgent.ToString(),
            GetBrowserFromUserAgent(Request.Headers.UserAgent.ToString()),
            GetOSFromUserAgent(Request.Headers.UserAgent.ToString())
        );
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<Application.Features.Auth.Commands.VerifyRevokedDeviceLogin.RequestRevokedDeviceLoginResponse>.Ok(result));
    }

    /// <summary>
    /// AUTH-SEC-005: Verify code and complete login from a revoked device.
    /// </summary>
    [HttpPost("revoked-device/verify")]
    [AllowAnonymous]
    [Audit("AUTH_REVOKED_DEVICE_VERIFY", "VerifyRevokedDevice", ResourceType = "Device", Severity = AuditSeverity.Warning)]
    [ProducesResponseType(typeof(ApiResponse<RevokedDeviceVerifyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyRevokedDevice([FromBody] RevokedDeviceVerifyRequest request)
    {
        var command = new Application.Features.Auth.Commands.VerifyRevokedDeviceLogin.VerifyRevokedDeviceLoginCommand(
            request.VerificationToken,
            request.Code,
            GetIpAddress()
        );
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<Application.Features.Auth.Commands.VerifyRevokedDeviceLogin.VerifyRevokedDeviceLoginResponse>.Ok(result));
    }

    private string? GetIpAddress()
    {
        var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.Split(',').FirstOrDefault()?.Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Request password setup for OAuth users (AUTH-PWD-001)
    /// Sends an email with a secure link to set a password.
    /// </summary>
    [HttpPost("password/setup-request")]
    [Authorize]
    [Audit("AUTH_PASSWORD_SETUP_REQUEST", "PasswordSetupRequest", ResourceType = "User", Severity = AuditSeverity.Info)]
    [ProducesResponseType(typeof(ApiResponse<RequestPasswordSetupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordSetup()
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        logger.LogInformation("Password setup request for user {UserId}", userId);

        var command = new RequestPasswordSetupCommand(userId, email, ipAddress, userAgent);
        var result = await _mediator.Send(command);
        
        return Ok(ApiResponse<RequestPasswordSetupResponse>.Ok(result));
    }

    /// <summary>
    /// Validate password setup token (AUTH-PWD-001)
    /// Called when user clicks the link in the email to verify it's still valid.
    /// </summary>
    [HttpGet("password/setup-validate")]
    [Audit("AUTH_PASSWORD_SETUP_VALIDATE", "PasswordSetupValidate", ResourceType = "User", Severity = AuditSeverity.Info)]
    [ProducesResponseType(typeof(ApiResponse<ValidatePasswordSetupTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidatePasswordSetupToken([FromQuery] string token)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(ApiResponse.Fail("Token is required"));
        }

        logger.LogInformation("Validating password setup token");

        var command = new ValidatePasswordSetupTokenCommand(token);
        var result = await _mediator.Send(command);
        
        return Ok(ApiResponse<ValidatePasswordSetupTokenResponse>.Ok(result));
    }

    /// <summary>
    /// Complete password setup for OAuth user (AUTH-PWD-001)
    /// Sets the password and invalidates the token.
    /// </summary>
    [HttpPost("password/setup-complete")]
    [Audit("AUTH_PASSWORD_SETUP_COMPLETE", "PasswordSetupComplete", ResourceType = "User", Severity = AuditSeverity.Warning)]
    [ProducesResponseType(typeof(ApiResponse<SetPasswordForOAuthUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompletePasswordSetup([FromBody] SetPasswordRequest request)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(ApiResponse.Fail("Token is required"));
        }
        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return BadRequest(ApiResponse.Fail("Password is required"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        logger.LogInformation("Completing password setup");

        var command = new SetPasswordForOAuthUserCommand(
            request.Token, 
            request.NewPassword, 
            request.ConfirmPassword, 
            ipAddress, 
            userAgent);
        var result = await _mediator.Send(command);
        
        return Ok(ApiResponse<SetPasswordForOAuthUserResponse>.Ok(result));
    }

    private static string GetBrowserFromUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        if (userAgent.Contains("Edg/")) return "Microsoft Edge";
        if (userAgent.Contains("Chrome/") && !userAgent.Contains("Chromium")) return "Chrome";
        if (userAgent.Contains("Firefox/")) return "Firefox";
        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
        return "Unknown Browser";
    }

    private static string GetOSFromUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        if (userAgent.Contains("Windows NT 10")) return "Windows 10/11";
        if (userAgent.Contains("Windows NT")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Linux") && !userAgent.Contains("Android")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        return "Unknown OS";
    }
}

// DTOs for revoked device endpoints
public record RevokedDeviceCodeRequest(
    string UserId,
    string Email,
    string DeviceFingerprint
);

public record RevokedDeviceLoginResponse(
    bool RequiresVerification,
    string Message,
    string? VerificationToken = null,
    DateTime? CodeExpiresAt = null
);

public record RevokedDeviceVerifyRequest(
    string VerificationToken,
    string Code
);

public record RevokedDeviceVerifyResponse(
    bool Success,
    string Message,
    bool DeviceCleared = false,
    int? RemainingAttempts = null
);

// DTO for password setup (AUTH-PWD-001)
public record SetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword
);
