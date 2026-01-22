// =====================================================
// DataPipelineService - Controllers
// Procesamiento de Datos y ETL
// =====================================================

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataPipelineService.Application.Commands;
using DataPipelineService.Application.Queries;
using DataPipelineService.Application.DTOs;
using DataPipelineService.Domain.Enums;

namespace DataPipelineService.Api.Controllers;

// ==================== Pipelines ====================
[ApiController]
[Route("api/pipelines")]
[Authorize]
public class PipelinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PipelinesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPipelinesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPipelineByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _mediator.Send(new GetPipelineByNameQuery(name));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _mediator.Send(new GetActivePipelinesQuery());
        return Ok(result);
    }

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduled()
    {
        var result = await _mediator.Send(new GetScheduledPipelinesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePipelineDto dto)
    {
        var result = await _mediator.Send(new CreatePipelineCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePipelineDto dto)
    {
        var result = await _mediator.Send(new UpdatePipelineCommand(id, dto));
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePipelineCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivatePipelineCommand(id));
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _mediator.Send(new DeactivatePipelineCommand(id));
        return result ? Ok() : NotFound();
    }
}

// ==================== Steps ====================
[ApiController]
[Route("api/pipeline-steps")]
[Authorize]
public class PipelineStepsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PipelineStepsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("pipeline/{pipelineId:guid}")]
    public async Task<IActionResult> GetByPipeline(Guid pipelineId)
    {
        var result = await _mediator.Send(new GetStepsByPipelineQuery(pipelineId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateStepDto dto)
    {
        var result = await _mediator.Send(new AddStepCommand(dto));
        return Created("", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateStepDto dto)
    {
        var result = await _mediator.Send(new UpdateStepCommand(id, dto));
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteStepCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("pipeline/{pipelineId:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid pipelineId, [FromBody] Dictionary<Guid, int> newOrder)
    {
        var result = await _mediator.Send(new ReorderStepsCommand(pipelineId, newOrder));
        return result ? Ok() : NotFound();
    }
}

// ==================== Runs ====================
[ApiController]
[Route("api/pipeline-runs")]
[Authorize]
public class PipelineRunsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PipelineRunsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRunByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("pipeline/{pipelineId:guid}")]
    public async Task<IActionResult> GetByPipeline(Guid pipelineId, [FromQuery] int limit = 50)
    {
        var result = await _mediator.Send(new GetRunsByPipelineQuery(pipelineId, limit));
        return Ok(result);
    }

    [HttpGet("running")]
    public async Task<IActionResult> GetRunning()
    {
        var result = await _mediator.Send(new GetRunningPipelinesQuery());
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(RunStatus status)
    {
        var result = await _mediator.Send(new GetRunsByStatusQuery(status));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartRunDto dto)
    {
        var result = await _mediator.Send(new StartPipelineRunCommand(dto));
        return Created("", result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelPipelineRunCommand(id));
        return result ? Ok() : NotFound();
    }
}

// ==================== Connectors ====================
[ApiController]
[Route("api/connectors")]
[Authorize]
public class ConnectorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConnectorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllConnectorsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetConnectorByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(ConnectorType type)
    {
        var result = await _mediator.Send(new GetConnectorsByTypeQuery(type));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConnectorDto dto)
    {
        var result = await _mediator.Send(new CreateConnectorCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateConnectorDto dto)
    {
        var result = await _mediator.Send(new UpdateConnectorCommand(id, dto));
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteConnectorCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/test")]
    public async Task<IActionResult> Test(Guid id)
    {
        var result = await _mediator.Send(new TestConnectorCommand(id));
        return Ok(result);
    }
}

// ==================== Transformations ====================
[ApiController]
[Route("api/transformations")]
[Authorize]
public class TransformationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransformationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTransformationsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTransformationByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _mediator.Send(new GetActiveTransformationsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransformationDto dto)
    {
        var result = await _mediator.Send(new CreateTransformationCommand(dto));
        return Created("", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTransformationDto dto)
    {
        var result = await _mediator.Send(new UpdateTransformationCommand(id, dto));
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTransformationCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/execute")]
    public async Task<IActionResult> Execute(Guid id)
    {
        var result = await _mediator.Send(new ExecuteTransformationCommand(id));
        return result ? Ok() : BadRequest();
    }
}

// ==================== Statistics ====================
[ApiController]
[Route("api/pipeline-statistics")]
public class PipelineStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PipelineStatisticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetPipelineStatisticsQuery());
        return Ok(result);
    }
}
