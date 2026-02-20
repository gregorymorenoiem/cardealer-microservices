using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Features.Rotation.Commands.RefreshRotation;
using AdvertisingService.Application.Features.Rotation.Commands.UpdateRotationConfig;
using AdvertisingService.Application.Features.Rotation.Queries.GetHomepageRotation;
using AdvertisingService.Application.Features.Rotation.Queries.GetRotationConfig;
using AdvertisingService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingService.Api.Controllers;

[ApiController]
[Route("api/advertising/rotation")]
public class RotationController : ControllerBase
{
    private readonly IMediator _mediator;

    public RotationController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get current homepage rotation for a section (public, cached)</summary>
    [HttpGet("{section}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HomepageRotationDto>), 200)]
    public async Task<IActionResult> GetRotation(AdPlacementType section, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHomepageRotationQuery(section), ct);
        if (result == null)
            return Ok(ApiResponse<HomepageRotationDto>.Fail("No rotation available for this section"));
        return Ok(ApiResponse<HomepageRotationDto>.Ok(result));
    }

    /// <summary>Get rotation config for a section (admin only)</summary>
    [HttpGet("config/{section}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RotationConfigDto>), 200)]
    public async Task<IActionResult> GetConfig(AdPlacementType section, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRotationConfigQuery(section), ct);
        if (result == null) return NotFound(ApiResponse<RotationConfigDto>.Fail("Config not found"));
        return Ok(ApiResponse<RotationConfigDto>.Ok(result));
    }

    /// <summary>Update rotation config (admin only)</summary>
    [HttpPut("config")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RotationConfigDto>), 200)]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateRotationConfigCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<RotationConfigDto>.Ok(result));
    }

    /// <summary>Force refresh rotation cache (admin only)</summary>
    [HttpPost("refresh")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Refresh([FromQuery] AdPlacementType? section, CancellationToken ct)
    {
        await _mediator.Send(new RefreshRotationCommand(section), ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
