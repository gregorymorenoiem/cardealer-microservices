using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Queries;
using SearchService.Domain.Entities;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatsController> _logger;

    public StatsController(IMediator mediator, ILogger<StatsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene metadatos y estadísticas de un índice
    /// </summary>
    [HttpGet("{indexName}")]
    [ProducesResponseType(typeof(IndexMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndexMetadata(string indexName)
    {
        try
        {
            var metadata = await _mediator.Send(new GetIndexMetadataQuery(indexName));

            if (metadata == null)
            {
                return NotFound(new { error = $"Index '{indexName}' not found" });
            }

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for index {IndexName}", indexName);
            return StatusCode(500, new { error = "An error occurred while retrieving index metadata" });
        }
    }

    /// <summary>
    /// Lista todos los índices con sus estadísticas básicas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> ListAllIndices()
    {
        try
        {
            var indices = await _mediator.Send(new ListIndicesQuery());
            
            return Ok(indices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing indices");
            return StatusCode(500, new { error = "An error occurred while listing indices" });
        }
    }
}
