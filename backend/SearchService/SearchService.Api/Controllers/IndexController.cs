using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Commands;
using System.Text.Json;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndexController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexController> _logger;

    public IndexController(IMediator mediator, ILogger<IndexController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Indexa un nuevo documento
    /// </summary>
    [HttpPost("{indexName}/document")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IndexDocument(string indexName, [FromBody] JsonElement document)
    {
        try
        {
            var documentId = Guid.NewGuid().ToString();
            
            var id = await _mediator.Send(new IndexDocumentCommand(indexName, documentId, document));
            
            return CreatedAtAction(
                nameof(SearchController.GetDocument), 
                "Search", 
                new { indexName, documentId = id }, 
                new { id, indexName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Indexa un documento con ID específico
    /// </summary>
    [HttpPut("{indexName}/document/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IndexDocumentWithId(string indexName, string documentId, [FromBody] JsonElement document)
    {
        try
        {
            var id = await _mediator.Send(new IndexDocumentCommand(indexName, documentId, document));
            
            return Ok(new { id, indexName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {DocumentId}", documentId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un documento existente
    /// </summary>
    [HttpPatch("{indexName}/document/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDocument(string indexName, string documentId, [FromBody] JsonElement document)
    {
        try
        {
            var success = await _mediator.Send(new UpdateDocumentCommand(indexName, documentId, document));
            
            if (!success)
            {
                return NotFound(new { error = $"Document '{documentId}' not found" });
            }

            return Ok(new { success, documentId, indexName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", documentId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un documento
    /// </summary>
    [HttpDelete("{indexName}/document/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(string indexName, string documentId)
    {
        try
        {
            var success = await _mediator.Send(new DeleteDocumentCommand(indexName, documentId));
            
            if (!success)
            {
                return NotFound(new { error = $"Document '{documentId}' not found" });
            }

            return Ok(new { success, documentId, indexName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Indexa múltiples documentos en batch
    /// </summary>
    [HttpPost("{indexName}/bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkIndex(string indexName, [FromBody] List<BulkIndexRequest> documents)
    {
        try
        {
            var documentsToIndex = documents.Select(d => 
                (d.Id ?? Guid.NewGuid().ToString(), (object)d.Document)
            ).ToList();

            var (successful, failed) = await _mediator.Send(new BulkIndexCommand(indexName, documentsToIndex));
            
            return Ok(new 
            { 
                indexName, 
                successful, 
                failed, 
                total = successful + failed 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk indexing");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo índice
    /// </summary>
    [HttpPost("{indexName}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIndex(string indexName, [FromBody] CreateIndexRequest? request = null)
    {
        try
        {
            var success = await _mediator.Send(new CreateIndexCommand(
                indexName, 
                request?.Mappings, 
                request?.Settings));
            
            if (!success)
            {
                return BadRequest(new { error = "Failed to create index" });
            }

            return CreatedAtAction(
                nameof(StatsController.GetIndexMetadata), 
                "Stats", 
                new { indexName }, 
                new { indexName, created = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", indexName);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un índice
    /// </summary>
    [HttpDelete("{indexName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIndex(string indexName)
    {
        try
        {
            var success = await _mediator.Send(new DeleteIndexCommand(indexName));
            
            if (!success)
            {
                return NotFound(new { error = $"Index '{indexName}' not found" });
            }

            return Ok(new { indexName, deleted = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class BulkIndexRequest
{
    public string? Id { get; set; }
    public JsonElement Document { get; set; }
}

public class CreateIndexRequest
{
    public Dictionary<string, object>? Mappings { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}
