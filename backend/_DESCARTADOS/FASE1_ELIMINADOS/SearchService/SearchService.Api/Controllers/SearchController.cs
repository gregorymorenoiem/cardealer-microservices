using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Queries;
using SearchService.Domain.ValueObjects;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IMediator mediator, ILogger<SearchController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta una búsqueda en Elasticsearch
    /// </summary>
    [HttpPost("query")]
    [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResult>> Search([FromBody] SearchQuery query)
    {
        try
        {
            if (!query.IsValid())
            {
                return BadRequest(new { error = "Invalid search query parameters" });
            }

            var result = await _mediator.Send(new ExecuteSearchQuery(query));
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search");
            return StatusCode(500, new { error = "An error occurred while executing the search" });
        }
    }

    /// <summary>
    /// Obtiene un documento específico por ID
    /// </summary>
    [HttpGet("{indexName}/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(string indexName, string documentId)
    {
        try
        {
            var document = await _mediator.Send(new GetDocumentQuery(indexName, documentId));

            if (document == null)
            {
                return NotFound(new { error = $"Document '{documentId}' not found in index '{indexName}'" });
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId} from {IndexName}", documentId, indexName);
            return StatusCode(500, new { error = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Lista los índices disponibles
    /// </summary>
    [HttpGet("indices")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> ListIndices()
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
