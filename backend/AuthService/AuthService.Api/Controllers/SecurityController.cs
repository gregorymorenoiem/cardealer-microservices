using AuthService.Application.DTOs.Security;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService.Domain.Entities;
using MediatR;

namespace AuthService.Api.Controllers;

/// <summary>
/// Controller for managing user security settings
/// Proceso: AUTH-LOG-001, AUTH-PWD, SEC-2FA
/// </summary>
[ApiController]
[Route("api/auth/security")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly ILoginHistoryRepository _loginHistoryRepository;
    private readonly IUserRepository _userRepository;

    public SecurityController(
        ILogger<SecurityController> logger,
        IUserSessionRepository sessionRepository,
        ILoginHistoryRepository loginHistoryRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _sessionRepository = sessionRepository;
        _loginHistoryRepository = loginHistoryRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get security settings for the current user
    /// Incluye: 2FA status, sesiones activas, historial de logins
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SecuritySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SecuritySettingsDto>> GetSecuritySettings(
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? headerUserId;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("User not authenticated"));
        }

        _logger.LogInformation("Getting security settings for user {UserId}", userId);

        try
        {
            // Obtener usuario para información de 2FA
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("User not found"));
            }

            // Obtener sesiones activas
            var activeSessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
            var currentSessionId = GetCurrentSessionId();

            var sessionDtos = activeSessions.Select(s => new ActiveSessionDto(
                Id: s.Id.ToString(),
                Device: s.DeviceInfo,
                Browser: s.Browser,
                Location: s.Location ?? GetLocationString(s.City, s.Country),
                IpAddress: s.IpAddress,
                LastActive: s.LastActiveAt.ToString("o"),
                IsCurrent: s.Id.ToString() == currentSessionId
            )).ToList();

            // Obtener historial de logins (últimos 10)
            var loginHistory = await _loginHistoryRepository.GetByUserIdAsync(userId, 10, cancellationToken);
            
            var loginDtos = loginHistory.Select(l => new LoginHistoryDto(
                Id: l.Id.ToString(),
                Device: l.DeviceInfo,
                Browser: l.Browser,
                Location: l.Location ?? GetLocationString(l.City, l.Country),
                IpAddress: l.IpAddress,
                LoginTime: l.LoginTime.ToString("o"),
                Success: l.Success
            )).ToList();

            // Obtener fecha de último cambio de contraseña
            var lastPasswordChange = user.UpdatedAt?.ToString("o") ?? user.CreatedAt.ToString("o");

            var settings = new SecuritySettingsDto(
                TwoFactorEnabled: user.IsTwoFactorEnabled,
                TwoFactorType: user.TwoFactorAuth?.PrimaryMethod.ToString(),
                LastPasswordChange: lastPasswordChange,
                ActiveSessions: sessionDtos,
                RecentLogins: loginDtos
            );

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security settings for user {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Change user password
    /// Proceso: AUTH-PWD-001
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<ChangePasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ChangePasswordResponse>.Fail("User not authenticated"));
            }

            // Validar que las contraseñas coincidan
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Passwords do not match"));
            }

            // Validar fortaleza de contraseña
            if (request.NewPassword.Length < 8)
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Password must be at least 8 characters"));
            }

            if (!request.NewPassword.Any(char.IsUpper))
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Password must contain at least one uppercase letter"));
            }

            if (!request.NewPassword.Any(char.IsDigit))
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Password must contain at least one number"));
            }

            // Obtener usuario
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<ChangePasswordResponse>.Fail("User not found"));
            }

            // Verificar contraseña actual
            var passwordValid = await _userRepository.VerifyPasswordAsync(user, request.CurrentPassword);
            if (!passwordValid)
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Current password is incorrect"));
            }

            // Cambiar contraseña
            await _userRepository.ChangePasswordAsync(user, request.NewPassword, cancellationToken);

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);

            // Registrar en historial
            var loginHistory = new LoginHistory(
                userId: userId,
                deviceInfo: GetDeviceFromUserAgent(),
                browser: GetBrowserFromUserAgent(),
                operatingSystem: GetOsFromUserAgent(),
                ipAddress: GetClientIpAddress(),
                success: true,
                method: LoginMethod.Password
            );
            await _loginHistoryRepository.AddAsync(loginHistory, cancellationToken);

            return Ok(ApiResponse<ChangePasswordResponse>.Ok(new ChangePasswordResponse(
                Success: true,
                Message: "Password changed successfully"
            )));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return BadRequest(ApiResponse<ChangePasswordResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get active sessions for the current user
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(List<ActiveSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ActiveSessionDto>>> GetActiveSessions(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Getting active sessions for user {UserId}", userId);

        var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
        var currentSessionId = GetCurrentSessionId();

        var sessionDtos = sessions.Select(s => new ActiveSessionDto(
            Id: s.Id.ToString(),
            Device: s.DeviceInfo,
            Browser: s.Browser,
            Location: s.Location ?? GetLocationString(s.City, s.Country),
            IpAddress: s.IpAddress,
            LastActive: s.LastActiveAt.ToString("o"),
            IsCurrent: s.Id.ToString() == currentSessionId
        )).ToList();

        return Ok(sessionDtos);
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeSession(string sessionId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            return BadRequest(new { Message = "Invalid session ID format" });
        }

        var session = await _sessionRepository.GetByIdAsync(sessionGuid, cancellationToken);
        if (session == null)
        {
            return NotFound(new { Message = "Session not found" });
        }

        // Verificar que la sesión pertenece al usuario
        if (session.UserId != userId)
        {
            return Forbid();
        }

        await _sessionRepository.RevokeSessionAsync(sessionGuid, "User revoked session", cancellationToken);
        
        _logger.LogInformation("Session {SessionId} revoked for user {UserId}", sessionId, userId);

        return Ok(new { Message = "Session revoked successfully" });
    }

    /// <summary>
    /// Revoke all sessions except the current one
    /// </summary>
    [HttpPost("sessions/revoke-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentSessionId = GetCurrentSessionId();
        Guid? exceptSessionId = null;
        
        if (!string.IsNullOrEmpty(currentSessionId) && Guid.TryParse(currentSessionId, out var guid))
        {
            exceptSessionId = guid;
        }

        await _sessionRepository.RevokeAllUserSessionsAsync(userId, exceptSessionId, "User revoked all sessions", cancellationToken);
        
        _logger.LogInformation("All sessions revoked for user {UserId} except current", userId);

        return Ok(new { Message = "All other sessions revoked successfully" });
    }

    /// <summary>
    /// Get login history for the current user
    /// </summary>
    [HttpGet("login-history")]
    [ProducesResponseType(typeof(List<LoginHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<LoginHistoryDto>>> GetLoginHistory(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Getting login history for user {UserId}", userId);

        var history = await _loginHistoryRepository.GetByUserIdAsync(userId, Math.Min(limit, 100), cancellationToken);

        var historyDtos = history.Select(l => new LoginHistoryDto(
            Id: l.Id.ToString(),
            Device: l.DeviceInfo,
            Browser: l.Browser,
            Location: l.Location ?? GetLocationString(l.City, l.Country),
            IpAddress: l.IpAddress,
            LoginTime: l.LoginTime.ToString("o"),
            Success: l.Success
        )).ToList();

        return Ok(historyDtos);
    }

    #region Helper Methods

    private string? GetCurrentSessionId()
    {
        return User.FindFirst("SessionId")?.Value;
    }

    private static string GetLocationString(string? city, string? country)
    {
        if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
            return $"{city}, {country}";
        if (!string.IsNullOrEmpty(country))
            return country;
        if (!string.IsNullOrEmpty(city))
            return city;
        return "Unknown location";
    }

    private string GetDeviceFromUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            return "Mobile iOS";
        if (userAgent.Contains("Android"))
            return "Mobile Android";
        if (userAgent.Contains("Windows"))
            return "Desktop Windows";
        if (userAgent.Contains("Mac"))
            return "Desktop Mac";
        if (userAgent.Contains("Linux"))
            return "Desktop Linux";
        
        return "Unknown Device";
    }

    private string GetBrowserFromUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        
        if (userAgent.Contains("Edg/"))
            return "Edge";
        if (userAgent.Contains("Chrome/"))
            return "Chrome";
        if (userAgent.Contains("Firefox/"))
            return "Firefox";
        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome"))
            return "Safari";
        
        return "Unknown Browser";
    }

    private string GetOsFromUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        
        if (userAgent.Contains("Windows NT 10"))
            return "Windows 10/11";
        if (userAgent.Contains("Windows"))
            return "Windows";
        if (userAgent.Contains("Mac OS X"))
            return "macOS";
        if (userAgent.Contains("Linux"))
            return "Linux";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            return "iOS";
        if (userAgent.Contains("Android"))
            return "Android";
        
        return "Unknown OS";
    }

    private string GetClientIpAddress()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',').First().Trim();
        }
        
        return ip ?? "Unknown";
    }

    #endregion
}
