using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PostgresDbService.Api.Controllers;

/// <summary>
/// Generic controller for CRUD operations on any entity type
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GenericController : ControllerBase
{
    private readonly IGenericRepository _repository;
    private readonly ILogger&lt;GenericController&gt; _logger;

    public GenericController(IGenericRepository repository, ILogger&lt;GenericController&gt; logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// &lt;summary&gt;
    /// Get entity by ID
    /// &lt;/summary&gt;
    [HttpGet("{serviceName}/{entityType}/{entityId}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; GetById(
        [Required] string serviceName,
        [Required] string entityType,
        [Required] string entityId)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(serviceName, entityType, entityId);
            if (entity == null)
            {
                return NotFound($"Entity not found: {serviceName}/{entityType}/{entityId}");
            }

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity: {ServiceName}/{EntityType}/{EntityId}", 
                serviceName, entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get all entities by service and type
    /// &lt;/summary&gt;
    [HttpGet("{serviceName}/{entityType}")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetByService(
        [Required] string serviceName,
        [Required] string entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (pageSize &gt; 100)
                pageSize = 100;

            var entities = await _repository.GetByServiceAsync(serviceName, entityType, page, pageSize);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities: {ServiceName}/{EntityType}", serviceName, entityType);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Search entities by JSON property value
    /// &lt;/summary&gt;
    [HttpGet("{serviceName}/{entityType}/query")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; Query(
        [Required] string serviceName,
        [Required] string entityType,
        [Required][FromQuery] string jsonPath,
        [Required][FromQuery] string value)
    {
        try
        {
            var entities = await _repository.QueryAsync(serviceName, entityType, jsonPath, value);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying entities: {ServiceName}/{EntityType} with {JsonPath}={Value}", 
                serviceName, entityType, jsonPath, value);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Search entities with text search in IndexData
    /// &lt;/summary&gt;
    [HttpGet("{serviceName}/{entityType}/search")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; Search(
        [Required] string serviceName,
        [Required] string entityType,
        [Required][FromQuery] string searchTerm)
    {
        try
        {
            var entities = await _repository.SearchAsync(serviceName, entityType, searchTerm);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching entities: {ServiceName}/{EntityType} with term '{SearchTerm}'", 
                serviceName, entityType, searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Create a new entity
    /// &lt;/summary&gt;
    [HttpPost("{serviceName}/{entityType}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Create(
        [Required] string serviceName,
        [Required] string entityType,
        [FromBody] CreateEntityRequest request)
    {
        try
        {
            var entity = new GenericEntity
            {
                ServiceName = serviceName,
                EntityType = entityType,
                EntityId = request.EntityId ?? Guid.NewGuid().ToString(),
                DataJson = request.DataJson,
                IndexData = request.IndexData,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            var created = await _repository.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), 
                new { serviceName, entityType, entityId = created.EntityId }, 
                created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity: {ServiceName}/{EntityType}", serviceName, entityType);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Update an existing entity
    /// &lt;/summary&gt;
    [HttpPut("{serviceName}/{entityType}/{entityId}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Update(
        [Required] string serviceName,
        [Required] string entityType,
        [Required] string entityId,
        [FromBody] UpdateEntityRequest request)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(serviceName, entityType, entityId);
            if (entity == null)
            {
                return NotFound($"Entity not found: {serviceName}/{entityType}/{entityId}");
            }

            entity.DataJson = request.DataJson;
            entity.IndexData = request.IndexData;
            entity.UpdatedBy = User.Identity?.Name ?? "system";

            var updated = await _repository.UpdateAsync(entity);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity: {ServiceName}/{EntityType}/{EntityId}", 
                serviceName, entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Delete an entity (soft delete)
    /// &lt;/summary&gt;
    [HttpDelete("{serviceName}/{entityType}/{entityId}")]
    public async Task&lt;IActionResult&gt; Delete(
        [Required] string serviceName,
        [Required] string entityType,
        [Required] string entityId)
    {
        try
        {
            var success = await _repository.DeleteAsync(serviceName, entityType, entityId, 
                User.Identity?.Name ?? "system");
            
            if (!success)
            {
                return NotFound($"Entity not found: {serviceName}/{entityType}/{entityId}");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity: {ServiceName}/{EntityType}/{EntityId}", 
                serviceName, entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Bulk create entities
    /// &lt;/summary&gt;
    [HttpPost("{serviceName}/{entityType}/bulk")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; BulkCreate(
        [Required] string serviceName,
        [Required] string entityType,
        [FromBody] List&lt;CreateEntityRequest&gt; requests)
    {
        try
        {
            if (requests.Count &gt; 100)
            {
                return BadRequest("Maximum 100 entities allowed in bulk operation");
            }

            var entities = requests.Select(r =&gt; new GenericEntity
            {
                ServiceName = serviceName,
                EntityType = entityType,
                EntityId = r.EntityId ?? Guid.NewGuid().ToString(),
                DataJson = r.DataJson,
                IndexData = r.IndexData,
                CreatedBy = User.Identity?.Name ?? "system"
            }).ToList();

            var created = await _repository.BulkCreateAsync(entities);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating entities: {ServiceName}/{EntityType}", serviceName, entityType);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// &lt;summary&gt;
/// Request model for creating entities
/// &lt;/summary&gt;
public record CreateEntityRequest(
    string? EntityId,
    string DataJson,
    string? IndexData = null
);

/// &lt;summary&gt;
/// Request model for updating entities
/// &lt;/summary&gt;
public record UpdateEntityRequest(
    string DataJson,
    string? IndexData = null
);