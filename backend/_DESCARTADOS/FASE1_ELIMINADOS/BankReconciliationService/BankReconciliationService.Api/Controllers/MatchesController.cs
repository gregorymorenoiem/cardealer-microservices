using BankReconciliationService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankReconciliationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MatchesController> _logger;

    public MatchesController(IMediator mediator, ILogger<MatchesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get matches for a reconciliation
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReconciliationMatchDto>>> GetMatches(
        [FromQuery] Guid reconciliationId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting matches for reconciliation {ReconciliationId}", reconciliationId);
        
        // TODO: Implement GetMatchesQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<ReconciliationMatchDto>());
    }

    /// <summary>
    /// Get match suggestions for a bank statement line
    /// </summary>
    [HttpGet("suggestions/{bankLineId:guid}")]
    public async Task<ActionResult<IEnumerable<MatchSuggestionDto>>> GetSuggestions(
        Guid bankLineId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting match suggestions for bank line {BankLineId}", bankLineId);
        
        // TODO: Implement GetMatchSuggestionsQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<MatchSuggestionDto>());
    }

    /// <summary>
    /// Create a manual match
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<ReconciliationMatchDto>> CreateMatch(
        [FromBody] CreateManualMatchRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating manual match between bank line {BankLineId} and internal tx {InternalTxId}", 
            request.BankStatementLineId, request.InternalTransactionId);
        
        // TODO: Implement CreateManualMatchCommand
        await Task.CompletedTask;
        return Accepted();
    }

    /// <summary>
    /// Create multiple matches in bulk
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<IEnumerable<ReconciliationMatchDto>>> CreateBulkMatches(
        [FromBody] CreateBulkMatchesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating {Count} manual matches", request.Matches.Count);
        
        // TODO: Implement CreateBulkMatchesCommand
        await Task.CompletedTask;
        return Accepted();
    }

    /// <summary>
    /// Delete (undo) a match
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<IActionResult> DeleteMatch(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting match {MatchId}", id);
        
        // TODO: Implement DeleteMatchCommand
        await Task.CompletedTask;
        return NoContent();
    }
}
