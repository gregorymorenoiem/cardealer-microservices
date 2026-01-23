using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;
using AuthService.Application.Features.ExternalAuth.Commands.ExternalAuthCallback;
using AuthService.Application.Features.ExternalAuth.Commands.ExternalLogin;
using AuthService.Application.Features.ExternalAuth.Commands.LinkExternalAccount;
using AuthService.Application.Features.ExternalAuth.Commands.UnlinkExternalAccount;
using AuthService.Application.Features.ExternalAuth.Queries.GetLinkedAccounts;
using AuthService.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class ExternalAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExternalAuthController> _logger;

    public ExternalAuthController(IMediator mediator, ILogger<ExternalAuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate with external provider using ID token
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<ActionResult<ApiResponse<ExternalAuthResponse>>> Authenticate(
        [FromBody] ExternalAuthRequest request)
    {
        try
        {
            _logger.LogInformation("External authentication attempt with provider: {Provider}", request.Provider);

            var command = new ExternalAuthCommand(request.Provider, request.IdToken);
            var result = await _mediator.Send(command);

            _logger.LogInformation("External authentication successful for user {UserId} with provider {Provider}",
                result.UserId, request.Provider);

            return Ok(ApiResponse<ExternalAuthResponse>.Ok(result, new Dictionary<string, object>
            {
                ["isNewUser"] = result.IsNewUser,
                ["provider"] = request.Provider
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External authentication failed for provider {Provider}", request.Provider);
            return BadRequest(ApiResponse<ExternalAuthResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Initiate external login flow and get authorization URL
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<ExternalLoginResponse>>> Login(
        [FromBody] ExternalLoginRequest request)
    {
        try
        {
            _logger.LogInformation("Initiating external login flow for provider: {Provider}", request.Provider);

            var command = new ExternalLoginCommand(request.Provider, request.RedirectUri);
            var result = await _mediator.Send(command);

            _logger.LogInformation("External login URL generated for provider {Provider}", request.Provider);

            return Ok(ApiResponse<ExternalLoginResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External login initiation failed for provider {Provider}", request.Provider);
            return BadRequest(ApiResponse<ExternalLoginResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Link external account to existing user (AUTH-EXT-005)
    /// Allows authenticated users to connect an OAuth provider to their account.
    /// User can only have one external provider linked at a time.
    /// </summary>
    /// <param name="request">Provider and ID token from OAuth flow</param>
    /// <returns>New tokens with updated claims</returns>
    [HttpPost("link-account")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ExternalAuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ExternalAuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ExternalAuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ExternalAuthResponse>>> LinkAccount(
        [FromBody] ExternalAuthRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ExternalAuthResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Linking external account for user {UserId} with provider {Provider}",
                userId, request.Provider);

            var command = new LinkExternalAccountCommand(userId, request.Provider, request.IdToken);
            var result = await _mediator.Send(command);

            _logger.LogInformation("External account linked successfully for user {UserId}, provider {Provider}",
                userId, request.Provider);

            return Ok(ApiResponse<ExternalAuthResponse>.Ok(result, new Dictionary<string, object>
            {
                ["isLinked"] = true,
                ["provider"] = request.Provider,
                ["linkedAt"] = DateTime.UtcNow
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link external account for user");
            return BadRequest(ApiResponse<ExternalAuthResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Unlink external account from user (AUTH-EXT-006)
    /// Security: Requires user to have a password set before unlinking
    /// </summary>
    /// <param name="request">The provider to unlink</param>
    /// <returns>Success response with unlink details</returns>
    [HttpDelete("unlink-account")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UnlinkExternalAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UnlinkExternalAccountResponse>>> UnlinkAccount(
        [FromBody] UnlinkExternalAccountRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UnlinkExternalAccountResponse>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Unlinking external account for user {UserId} with provider {Provider}",
                userId, request.Provider);

            var command = new UnlinkExternalAccountCommand(userId, request.Provider);
            var result = await _mediator.Send(command);

            _logger.LogInformation("External account unlinked successfully for user {UserId}, provider {Provider}",
                userId, request.Provider);

            return Ok(ApiResponse<UnlinkExternalAccountResponse>.Ok(result, new Dictionary<string, object>
            {
                ["provider"] = result.Provider,
                ["unlinkedAt"] = result.UnlinkedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink external account for user");
            return BadRequest(ApiResponse<UnlinkExternalAccountResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get linked external accounts for current user
    /// </summary>
    [HttpGet("linked-accounts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<LinkedAccountResponse>>>> GetLinkedAccounts()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<LinkedAccountResponse>>.Fail("User not authenticated"));
            }

            _logger.LogInformation("Getting linked accounts for user {UserId}", userId);

            var query = new GetLinkedAccountsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<List<LinkedAccountResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get linked accounts for user");
            return BadRequest(ApiResponse<List<LinkedAccountResponse>>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Handle external authentication callback
    /// </summary>
    [HttpPost("callback")]
    public async Task<ActionResult<ApiResponse<ExternalAuthResponse>>> Callback(
        [FromBody] ExternalAuthCallbackRequest request)
    {
        try
        {
            _logger.LogInformation("Processing external auth callback for provider: {Provider}", request.Provider);

            var command = new ExternalAuthCallbackCommand(
                request.Provider,
                request.Code,
                request.IdToken,
                request.RedirectUri,
                request.State);

            var result = await _mediator.Send(command);

            _logger.LogInformation("External auth callback processed successfully for user {UserId}",
                result.UserId);

            return Ok(ApiResponse<ExternalAuthResponse>.Ok(result, new Dictionary<string, object>
            {
                ["isNewUser"] = result.IsNewUser,
                ["provider"] = request.Provider
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External auth callback failed for provider {Provider}", request.Provider);
            return BadRequest(ApiResponse<ExternalAuthResponse>.Fail(ex.Message));
        }
    }
}
