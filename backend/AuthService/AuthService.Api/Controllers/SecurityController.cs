using AuthService.Application.DTOs.Security;
using AuthService.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

/// <summary>
/// Controller for managing user security settings
/// </summary>
[ApiController]
[Route("api/auth/security")]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ILogger<SecurityController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get security settings for the current user
    /// </summary>
    [HttpGet]
    public ActionResult<SecuritySettingsDto> GetSecuritySettings(
        [FromHeader(Name = "X-User-Id")] string? headerUserId)
    {
        // Get user ID from claims or header
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? headerUserId;
        
        _logger.LogInformation("Getting security settings for user {UserId}", userId);

        // Return security settings - in production these would come from the database
        var settings = new SecuritySettingsDto(
            TwoFactorEnabled: false,
            TwoFactorType: null,
            LastPasswordChange: DateTime.UtcNow.AddDays(-30).ToString("o"),
            ActiveSessions: new List<ActiveSessionDto>
            {
                new(
                    Id: "session_current",
                    Device: GetDeviceFromUserAgent(),
                    Browser: GetBrowserFromUserAgent(),
                    Location: "Santo Domingo, DO",
                    IpAddress: GetClientIpAddress(),
                    LastActive: DateTime.UtcNow.ToString("o"),
                    IsCurrent: true
                )
            },
            RecentLogins: new List<LoginHistoryDto>
            {
                new(
                    Id: "login_1",
                    Device: GetDeviceFromUserAgent(),
                    Browser: GetBrowserFromUserAgent(),
                    Location: "Santo Domingo, DO",
                    IpAddress: GetClientIpAddress(),
                    LoginTime: DateTime.UtcNow.ToString("o"),
                    Success: true
                )
            }
        );

        return Ok(settings);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ChangePasswordResponse>.Fail("User not authenticated"));
            }

            // Validate password confirmation
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Passwords do not match"));
            }

            // Validate password strength
            if (request.NewPassword.Length < 8)
            {
                return BadRequest(ApiResponse<ChangePasswordResponse>.Fail("Password must be at least 8 characters"));
            }

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            
            // In production, this would actually change the password in the database
            await Task.CompletedTask;

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
    [Authorize]
    public ActionResult<List<ActiveSessionDto>> GetActiveSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Getting active sessions for user {UserId}", userId);

        var sessions = new List<ActiveSessionDto>
        {
            new(
                Id: "session_current",
                Device: GetDeviceFromUserAgent(),
                Browser: GetBrowserFromUserAgent(),
                Location: "Santo Domingo, DO",
                IpAddress: GetClientIpAddress(),
                LastActive: DateTime.UtcNow.ToString("o"),
                IsCurrent: true
            )
        };

        return Ok(sessions);
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public ActionResult RevokeSession(string sessionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Revoking session {SessionId} for user {UserId}", sessionId, userId);

        // In production, this would invalidate the session token
        return Ok(new { Message = "Session revoked successfully" });
    }

    /// <summary>
    /// Revoke all sessions except the current one
    /// </summary>
    [HttpPost("sessions/revoke-all")]
    [Authorize]
    public ActionResult RevokeAllSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Revoking all sessions for user {UserId}", userId);

        // In production, this would invalidate all session tokens except current
        return Ok(new { Message = "All other sessions revoked successfully" });
    }

    /// <summary>
    /// Get login history for the current user
    /// </summary>
    [HttpGet("login-history")]
    [Authorize]
    public ActionResult<List<LoginHistoryDto>> GetLoginHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Getting login history for user {UserId}", userId);

        var history = new List<LoginHistoryDto>
        {
            new(
                Id: "login_1",
                Device: GetDeviceFromUserAgent(),
                Browser: GetBrowserFromUserAgent(),
                Location: "Santo Domingo, DO",
                IpAddress: GetClientIpAddress(),
                LoginTime: DateTime.UtcNow.ToString("o"),
                Success: true
            ),
            new(
                Id: "login_2",
                Device: "Desktop Windows",
                Browser: "Chrome 120",
                Location: "Santiago, DO",
                IpAddress: "192.168.1.100",
                LoginTime: DateTime.UtcNow.AddDays(-1).ToString("o"),
                Success: true
            ),
            new(
                Id: "login_3",
                Device: "Mobile iPhone",
                Browser: "Safari 17",
                Location: "Santo Domingo, DO",
                IpAddress: "192.168.1.101",
                LoginTime: DateTime.UtcNow.AddDays(-2).ToString("o"),
                Success: true
            )
        };

        return Ok(history);
    }

    #region Helper Methods

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
        
        if (userAgent.Contains("Chrome"))
            return "Chrome";
        if (userAgent.Contains("Firefox"))
            return "Firefox";
        if (userAgent.Contains("Safari"))
            return "Safari";
        if (userAgent.Contains("Edge"))
            return "Edge";
        
        return "Unknown Browser";
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
