using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.UseCases.Content;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for platform content management endpoints (/admin/contenido page).
/// All endpoints require Admin/SuperAdmin role.
/// </summary>
[ApiController]
[Route("api/admin/content")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ContentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContentController> _logger;

    public ContentController(IMediator mediator, ILogger<ContentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Get content overview (banners + pages + blog)</summary>
    [HttpGet]
    public async Task<IActionResult> GetContentOverview()
    {
        _logger.LogInformation("Getting content overview");
        var result = await _mediator.Send(new GetContentOverviewQuery());
        return Ok(result);
    }

    /// <summary>Get all banners (admin — includes inactive)</summary>
    [HttpGet("banners")]
    public async Task<IActionResult> GetBanners()
    {
        var result = await _mediator.Send(new GetBannersQuery());
        return Ok(result);
    }

    /// <summary>Create a new banner</summary>
    [HttpPost("banners")]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerRequest data)
    {
        var result = await _mediator.Send(new CreateBannerCommand(data));
        _logger.LogInformation("Banner created: {BannerId}", result.Id);
        return Ok(result);
    }

    /// <summary>Update an existing banner</summary>
    [HttpPut("banners/{id}")]
    public async Task<IActionResult> UpdateBanner(string id, [FromBody] UpdateBannerRequest data)
    {
        var result = await _mediator.Send(new UpdateBannerCommand(id, data));
        if (result is null) return NotFound(new { message = $"Banner '{id}' no encontrado" });
        return Ok(result);
    }

    /// <summary>Delete a banner</summary>
    [HttpDelete("banners/{id}")]
    public async Task<IActionResult> DeleteBanner(string id)
    {
        await _mediator.Send(new DeleteBannerCommand(id));
        return Ok(new { message = "Banner eliminado exitosamente" });
    }

    /// <summary>Record a banner click (analytics)</summary>
    [HttpPost("banners/{id}/click")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordBannerClick(string id)
    {
        await _mediator.Send(new RecordBannerClickCommand(id));
        return Ok();
    }

    /// <summary>Get static pages</summary>
    [HttpGet("pages")]
    public async Task<IActionResult> GetPages()
    {
        var result = await _mediator.Send(new GetStaticPagesQuery());
        return Ok(result);
    }

    /// <summary>Get blog posts</summary>
    [HttpGet("blog")]
    public async Task<IActionResult> GetBlog()
    {
        var result = await _mediator.Send(new GetBlogPostsQuery());
        return Ok(result);
    }
}

