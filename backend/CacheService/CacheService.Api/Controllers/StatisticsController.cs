using Microsoft.AspNetCore.Mvc;
using MediatR;
using CacheService.Application.Queries;

namespace CacheService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var query = new GetStatisticsQuery();
        var result = await _mediator.Send(query);

        return Ok(new
        {
            totalHits = result.TotalHits,
            totalMisses = result.TotalMisses,
            totalSets = result.TotalSets,
            totalDeletes = result.TotalDeletes,
            totalKeys = result.TotalKeys,
            totalSizeInBytes = result.TotalSizeInBytes,
            hitRatio = result.GetHitRatio(),
            hitPercentage = result.GetHitPercentage(),
            lastResetAt = result.LastResetAt
        });
    }
}
