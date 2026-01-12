using CRMService.Application.DTOs;
using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Api.Controllers;

[ApiController]
[Route("api/crm/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IDealRepository _dealRepository;
    private readonly IPipelineRepository _pipelineRepository;
    private readonly ILogger<DealsController> _logger;

    public DealsController(
        IDealRepository dealRepository,
        IPipelineRepository pipelineRepository,
        ILogger<DealsController> logger)
    {
        _dealRepository = dealRepository;
        _pipelineRepository = pipelineRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var deals = await _dealRepository.GetAllAsync(cancellationToken);
        return Ok(deals.Select(DealDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DealDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _dealRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (deal == null)
            return NotFound();

        return Ok(DealDto.FromEntity(deal));
    }

    [HttpGet("pipeline/{pipelineId:guid}")]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetByPipelineId(
        Guid pipelineId,
        CancellationToken cancellationToken = default)
    {
        var deals = await _dealRepository.GetByPipelineAsync(pipelineId, cancellationToken);
        return Ok(deals.Select(DealDto.FromEntity));
    }

    [HttpGet("stage/{stageId:guid}")]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetByStageId(
        Guid stageId,
        CancellationToken cancellationToken = default)
    {
        var deals = await _dealRepository.GetByStageAsync(stageId, cancellationToken);
        return Ok(deals.Select(DealDto.FromEntity));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetByStatus(
        string status,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DealStatus>(status, out var dealStatus))
            return BadRequest("Invalid status");

        var deals = await _dealRepository.GetByStatusAsync(dealStatus, cancellationToken);
        return Ok(deals.Select(DealDto.FromEntity));
    }

    [HttpGet("closing-soon/{days:int}")]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetClosingSoon(
        int days,
        CancellationToken cancellationToken = default)
    {
        var deals = await _dealRepository.GetClosingSoonAsync(days, cancellationToken);
        return Ok(deals.Select(DealDto.FromEntity));
    }

    [HttpPost]
    public async Task<ActionResult<DealDto>> Create(
        [FromBody] CreateDealRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        // Get default pipeline if not specified
        var pipelineId = request.PipelineId;
        var stageId = request.StageId;

        if (!pipelineId.HasValue || !stageId.HasValue)
        {
            var defaultPipeline = await _pipelineRepository.GetDefaultAsync(cancellationToken);
            if (defaultPipeline != null)
            {
                pipelineId ??= defaultPipeline.Id;
                stageId ??= defaultPipeline.Stages.OrderBy(s => s.Order).FirstOrDefault()?.Id;
            }
        }

        if (!pipelineId.HasValue || !stageId.HasValue)
            return BadRequest("Pipeline and Stage are required");

        var deal = new Deal(
            dealerId,
            request.Title,
            request.Value,
            pipelineId.Value,
            stageId.Value,
            request.LeadId,
            request.ContactId
        );

        await _dealRepository.AddAsync(deal, cancellationToken);

        _logger.LogInformation("Deal {DealId} created for dealer {DealerId}", deal.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = deal.Id }, DealDto.FromEntity(deal));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DealDto>> Update(
        Guid id,
        [FromBody] UpdateDealRequest request,
        CancellationToken cancellationToken = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, cancellationToken);
        if (deal == null)
            return NotFound();

        deal.UpdateDetails(request.Title, request.Description, request.Value, request.Currency);

        if (request.Probability.HasValue)
        {
            deal.UpdateProbability(request.Probability.Value);
        }

        await _dealRepository.UpdateAsync(deal, cancellationToken);

        _logger.LogInformation("Deal {DealId} updated", id);

        return Ok(DealDto.FromEntity(deal));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<DealDto>> Close(
        Guid id,
        [FromBody] CloseDealRequest request,
        CancellationToken cancellationToken = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, cancellationToken);
        if (deal == null)
            return NotFound();

        if (request.IsWon)
            deal.MarkAsWon(request.Notes);
        else
            deal.MarkAsLost(request.LostReason ?? "Not specified");

        await _dealRepository.UpdateAsync(deal, cancellationToken);

        _logger.LogInformation("Deal {DealId} closed as {Result}", id, request.IsWon ? "Won" : "Lost");

        return Ok(DealDto.FromEntity(deal));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, cancellationToken);
        if (deal == null)
            return NotFound();

        await _dealRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Deal {DealId} deleted", id);

        return NoContent();
    }
}
