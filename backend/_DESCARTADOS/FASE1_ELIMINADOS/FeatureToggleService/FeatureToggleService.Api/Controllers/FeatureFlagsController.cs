using FeatureToggleService.Application.Commands;
using FeatureToggleService.Application.Queries;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(IMediator mediator, ILogger<FeatureFlagsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Obtiene todos los feature flags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllFeatureFlagsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un feature flag por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeatureFlag>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagByIdQuery(id), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un feature flag por clave
    /// </summary>
    [HttpGet("key/{key}")]
    public async Task<ActionResult<FeatureFlag>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagByKeyQuery(key), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with key '{key}' not found" });
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo feature flag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FeatureFlag>> Create([FromBody] CreateFeatureFlagCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating feature flag: {Key}", command.Key);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza un feature flag
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeatureFlag>> Update(Guid id, [FromBody] UpdateFeatureFlagCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID mismatch" });

        _logger.LogInformation("Updating feature flag: {Id}", id);
        var result = await _mediator.Send(command, cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Elimina un feature flag
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting feature flag: {Id}", id);
        var result = await _mediator.Send(new DeleteFeatureFlagCommand(id), cancellationToken);
        if (!result)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return NoContent();
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Habilita un feature flag
    /// </summary>
    [HttpPost("{id:guid}/enable")]
    public async Task<ActionResult<FeatureFlag>> Enable(Guid id, [FromQuery] string? modifiedBy, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enabling feature flag: {Id}", id);
        var result = await _mediator.Send(new EnableFeatureFlagCommand(id, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Deshabilita un feature flag
    /// </summary>
    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<FeatureFlag>> Disable(Guid id, [FromQuery] string? modifiedBy, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disabling feature flag: {Id}", id);
        var result = await _mediator.Send(new DisableFeatureFlagCommand(id, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Archiva un feature flag
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<FeatureFlag>> Archive(Guid id, [FromQuery] string? modifiedBy, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Archiving feature flag: {Id}", id);
        var result = await _mediator.Send(new ArchiveFeatureFlagCommand(id, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Restaura un feature flag archivado
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<FeatureFlag>> Restore(Guid id, [FromQuery] string? modifiedBy, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Restoring feature flag: {Id}", id);
        var result = await _mediator.Send(new RestoreFeatureFlagCommand(id, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Activa el kill switch (deshabilitación de emergencia)
    /// </summary>
    [HttpPost("{id:guid}/kill-switch")]
    public async Task<ActionResult<FeatureFlag>> TriggerKillSwitch(Guid id, [FromQuery] string? triggeredBy, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Kill switch triggered for feature flag: {Id} by {User}. Reason: {Reason}", id, triggeredBy, reason);
        var result = await _mediator.Send(new TriggerKillSwitchCommand(id, triggeredBy ?? "system", reason ?? "Emergency shutdown"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    #endregion

    #region Filtering

    /// <summary>
    /// Obtiene feature flags por entorno
    /// </summary>
    [HttpGet("environment/{environment}")]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetByEnvironment(Environment environment, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagsByEnvironmentQuery(environment), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene feature flags por estado
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetByStatus(FlagStatus status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagsByStatusQuery(status), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene feature flags por etiqueta
    /// </summary>
    [HttpGet("tag/{tag}")]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetByTag(string tag, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagsByTagQuery(tag), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los feature flags activos
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActiveFeatureFlagsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene feature flags expirados
    /// </summary>
    [HttpGet("expired")]
    public async Task<ActionResult<IEnumerable<FeatureFlag>>> GetExpired(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExpiredFeatureFlagsQuery(), cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Evalúa un feature flag para un contexto dado
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<bool>> Evaluate([FromBody] EvaluateFeatureFlagQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new { FlagKey = query.FlagKey, IsEnabled = result });
    }

    /// <summary>
    /// Evalúa múltiples feature flags para un contexto dado
    /// </summary>
    [HttpPost("evaluate-multiple")]
    public async Task<ActionResult<Dictionary<string, bool>>> EvaluateMultiple([FromBody] EvaluateMultipleFeatureFlagsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region History & Stats

    /// <summary>
    /// Obtiene el historial de cambios de un feature flag
    /// </summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IEnumerable<FeatureFlagHistory>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagHistoryQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de los feature flags
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<FeatureFlagStats>> GetStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagStatsQuery(), cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Rollout Management

    /// <summary>
    /// Configura el porcentaje de rollout
    /// </summary>
    [HttpPost("{id:guid}/rollout-percentage")]
    public async Task<ActionResult<FeatureFlag>> SetRolloutPercentage(
        Guid id,
        [FromQuery] int percentage,
        [FromQuery] string? modifiedBy,
        CancellationToken cancellationToken)
    {
        if (percentage < 0 || percentage > 100)
            return BadRequest(new { Message = "Percentage must be between 0 and 100" });

        _logger.LogInformation("Setting rollout percentage for feature flag {Id} to {Percentage}%", id, percentage);
        var result = await _mediator.Send(new SetRolloutPercentageCommand(id, percentage, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Agrega usuarios a la lista de targets
    /// </summary>
    [HttpPost("{id:guid}/target-users")]
    public async Task<ActionResult<FeatureFlag>> AddTargetUsers(
        Guid id,
        [FromBody] List<string> userIds,
        [FromQuery] string? modifiedBy,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding {Count} target users to feature flag {Id}", userIds.Count, id);
        var result = await _mediator.Send(new AddTargetUsersCommand(id, userIds, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>
    /// Remueve usuarios de la lista de targets
    /// </summary>
    [HttpDelete("{id:guid}/target-users")]
    public async Task<ActionResult<FeatureFlag>> RemoveTargetUsers(
        Guid id,
        [FromBody] List<string> userIds,
        [FromQuery] string? modifiedBy,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing {Count} target users from feature flag {Id}", userIds.Count, id);
        var result = await _mediator.Send(new RemoveTargetUsersCommand(id, userIds, modifiedBy ?? "system"), cancellationToken);
        if (result == null)
            return NotFound(new { Message = $"Feature flag with ID {id} not found" });
        return Ok(result);
    }

    #endregion
}
