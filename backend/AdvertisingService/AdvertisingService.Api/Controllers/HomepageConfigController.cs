using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateBrand;
using AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateCategory;
using AdvertisingService.Application.Features.HomepageConfig.Queries.GetBrands;
using AdvertisingService.Application.Features.HomepageConfig.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingService.Api.Controllers;

[ApiController]
[Route("api/advertising/homepage")]
public class HomepageConfigController : ControllerBase
{
    private readonly IMediator _mediator;

    public HomepageConfigController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get categories for homepage (public)</summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryImageConfigDto>>), 200)]
    public async Task<IActionResult> GetCategories([FromQuery] bool includeHidden = false, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(includeHidden), ct);
        return Ok(ApiResponse<List<CategoryImageConfigDto>>.Ok(result));
    }

    /// <summary>Update category config (admin only)</summary>
    [HttpPut("categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryImageConfigDto>), 200)]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<CategoryImageConfigDto>.Ok(result));
    }

    /// <summary>Get brands for homepage (public)</summary>
    [HttpGet("brands")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<BrandConfigDto>>), 200)]
    public async Task<IActionResult> GetBrands([FromQuery] bool includeHidden = false, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetBrandsQuery(includeHidden), ct);
        return Ok(ApiResponse<List<BrandConfigDto>>.Ok(result));
    }

    /// <summary>Update brand config (admin only)</summary>
    [HttpPut("brands")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<BrandConfigDto>), 200)]
    public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<BrandConfigDto>.Ok(result));
    }
}
