using BankReconciliationService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankReconciliationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankStatementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BankStatementsController> _logger;

    public BankStatementsController(IMediator mediator, ILogger<BankStatementsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get bank statements with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankStatementDto>>> GetStatements(
        CancellationToken cancellationToken,
        [FromQuery] Guid? bankAccountId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        _logger.LogInformation("Getting bank statements with filters");
        
        // TODO: Implement GetBankStatementsQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<BankStatementDto>());
    }

    /// <summary>
    /// Get a specific bank statement by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BankStatementDto>> GetStatement(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting bank statement {StatementId}", id);
        
        // TODO: Implement GetBankStatementByIdQuery
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Get lines for a specific bank statement
    /// </summary>
    [HttpGet("{id:guid}/lines")]
    public async Task<ActionResult<IEnumerable<BankStatementLineDto>>> GetStatementLines(
        Guid id, 
        CancellationToken cancellationToken,
        [FromQuery] bool unreconciledOnly = false)
    {
        _logger.LogInformation("Getting lines for bank statement {StatementId}", id);
        
        // TODO: Implement GetBankStatementLinesQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<BankStatementLineDto>());
    }

    /// <summary>
    /// Import a bank statement from API
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<BankStatementDto>> ImportStatement(
        [FromBody] ImportBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Importing bank statement for account {AccountId}", request.BankAccountConfigId);
        
        // TODO: Implement ImportBankStatementCommand
        await Task.CompletedTask;
        return Accepted();
    }

    /// <summary>
    /// Delete a bank statement (only if not reconciled)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStatement(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting bank statement {StatementId}", id);
        
        // TODO: Implement DeleteBankStatementCommand
        await Task.CompletedTask;
        return NoContent();
    }
}
