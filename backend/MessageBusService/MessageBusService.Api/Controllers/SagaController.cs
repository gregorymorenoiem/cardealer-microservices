using MediatR;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Queries;
using MessageBusService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MessageBusService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SagaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SagaController> _logger;

    public SagaController(
        IMediator mediator,
        ILogger<SagaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Start a new saga execution
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(SagaResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> StartSaga([FromBody] StartSagaCommand command)
    {
        try
        {
            var saga = await _mediator.Send(command);

            return Ok(new SagaResponse
            {
                SagaId = saga.Id,
                Name = saga.Name,
                Status = saga.Status.ToString(),
                CorrelationId = saga.CorrelationId,
                CreatedAt = saga.CreatedAt,
                TotalSteps = saga.TotalSteps,
                CurrentStepIndex = saga.CurrentStepIndex,
                Message = "Saga started successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting saga");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get saga status and details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SagaDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSaga(Guid id)
    {
        try
        {
            var saga = await _mediator.Send(new GetSagaByIdQuery { SagaId = id });

            if (saga == null)
            {
                return NotFound(new { error = $"Saga {id} not found" });
            }

            return Ok(new SagaDetailResponse
            {
                SagaId = saga.Id,
                Name = saga.Name,
                Description = saga.Description,
                Type = saga.Type.ToString(),
                Status = saga.Status.ToString(),
                CorrelationId = saga.CorrelationId,
                CreatedAt = saga.CreatedAt,
                StartedAt = saga.StartedAt,
                CompletedAt = saga.CompletedAt,
                FailedAt = saga.FailedAt,
                ErrorMessage = saga.ErrorMessage,
                CurrentStepIndex = saga.CurrentStepIndex,
                TotalSteps = saga.TotalSteps,
                Steps = saga.Steps.Select(s => new SagaStepResponse
                {
                    StepId = s.Id,
                    Order = s.Order,
                    Name = s.Name,
                    ServiceName = s.ServiceName,
                    ActionType = s.ActionType,
                    Status = s.Status.ToString(),
                    StartedAt = s.StartedAt,
                    CompletedAt = s.CompletedAt,
                    FailedAt = s.FailedAt,
                    ErrorMessage = s.ErrorMessage,
                    RetryAttempts = s.RetryAttempts,
                    MaxRetries = s.MaxRetries
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saga {SagaId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Compensate a failed saga (rollback)
    /// </summary>
    [HttpPost("{id}/compensate")]
    [ProducesResponseType(typeof(SagaResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CompensateSaga(Guid id)
    {
        try
        {
            var saga = await _mediator.Send(new CompensateSagaCommand { SagaId = id });

            return Ok(new SagaResponse
            {
                SagaId = saga.Id,
                Name = saga.Name,
                Status = saga.Status.ToString(),
                CorrelationId = saga.CorrelationId,
                CreatedAt = saga.CreatedAt,
                TotalSteps = saga.TotalSteps,
                CurrentStepIndex = saga.CurrentStepIndex,
                Message = saga.Status == SagaStatus.Compensated
                    ? "Saga compensated successfully"
                    : $"Compensation failed: {saga.ErrorMessage}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compensating saga {SagaId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Abort a saga execution
    /// </summary>
    [HttpPost("{id}/abort")]
    [ProducesResponseType(typeof(SagaResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AbortSaga(Guid id, [FromBody] AbortSagaRequest request)
    {
        try
        {
            var saga = await _mediator.Send(new AbortSagaCommand
            {
                SagaId = id,
                Reason = request.Reason
            });

            return Ok(new SagaResponse
            {
                SagaId = saga.Id,
                Name = saga.Name,
                Status = saga.Status.ToString(),
                CorrelationId = saga.CorrelationId,
                CreatedAt = saga.CreatedAt,
                TotalSteps = saga.TotalSteps,
                CurrentStepIndex = saga.CurrentStepIndex,
                Message = $"Saga aborted: {request.Reason}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting saga {SagaId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retry a failed saga step
    /// </summary>
    [HttpPost("{sagaId}/steps/{stepId}/retry")]
    [ProducesResponseType(typeof(SagaResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RetryStep(Guid sagaId, Guid stepId)
    {
        try
        {
            var saga = await _mediator.Send(new RetrySagaStepCommand
            {
                SagaId = sagaId,
                StepId = stepId
            });

            return Ok(new SagaResponse
            {
                SagaId = saga.Id,
                Name = saga.Name,
                Status = saga.Status.ToString(),
                CorrelationId = saga.CorrelationId,
                CreatedAt = saga.CreatedAt,
                TotalSteps = saga.TotalSteps,
                CurrentStepIndex = saga.CurrentStepIndex,
                Message = "Step retry initiated"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying step {StepId} in saga {SagaId}", stepId, sagaId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all sagas by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(List<SagaResponse>), 200)]
    public async Task<IActionResult> GetSagasByStatus(string status)
    {
        try
        {
            if (!Enum.TryParse<SagaStatus>(status, true, out var sagaStatus))
            {
                return BadRequest(new { error = $"Invalid status: {status}" });
            }

            var sagas = await _mediator.Send(new GetSagasByStatusQuery { Status = sagaStatus });

            return Ok(sagas.Select(s => new SagaResponse
            {
                SagaId = s.Id,
                Name = s.Name,
                Status = s.Status.ToString(),
                CorrelationId = s.CorrelationId,
                CreatedAt = s.CreatedAt,
                TotalSteps = s.TotalSteps,
                CurrentStepIndex = s.CurrentStepIndex
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sagas by status {Status}", status);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// DTOs
public class SagaResponse
{
    public Guid SagaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentStepIndex { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SagaDetailResponse : SagaResponse
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SagaStepResponse> Steps { get; set; } = new();
}

public class SagaStepResponse
{
    public Guid StepId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryAttempts { get; set; }
    public int MaxRetries { get; set; }
}

public class AbortSagaRequest
{
    public string Reason { get; set; } = string.Empty;
}
