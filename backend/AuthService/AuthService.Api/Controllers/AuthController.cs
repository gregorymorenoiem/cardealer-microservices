using AuthService.Application.Features.Auth.Commands.Login;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.Features.Auth.Commands.ForgotPassword;
using AuthService.Application.Features.Auth.Commands.ResetPassword;
using AuthService.Application.Features.Auth.Commands.RefreshToken;
using AuthService.Application.Features.Auth.Commands.Logout;
using AuthService.Application.Features.Auth.Commands.VerifyEmail;
using AuthService.Application.Features.Auth.Commands.ResendVerification;
using AuthService.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthService.Application.DTOs.Auth;
using CarDealer.Shared.Audit.Attributes;
using CarDealer.Shared.Audit.Models;

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
}
