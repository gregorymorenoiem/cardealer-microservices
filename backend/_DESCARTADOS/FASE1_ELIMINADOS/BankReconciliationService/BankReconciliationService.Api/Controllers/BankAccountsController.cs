using BankReconciliationService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankReconciliationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BankAccountsController> _logger;

    public BankAccountsController(IMediator mediator, ILogger<BankAccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all bank account configurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccountConfigDto>>> GetAccounts(
        CancellationToken cancellationToken,
        [FromQuery] bool activeOnly = false)
    {
        _logger.LogInformation("Getting bank account configurations (activeOnly: {ActiveOnly})", activeOnly);
        
        // TODO: Implement GetBankAccountsQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<BankAccountConfigDto>());
    }

    /// <summary>
    /// Get a specific bank account by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BankAccountConfigDto>> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting bank account {AccountId}", id);
        
        // TODO: Implement GetBankAccountByIdQuery
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Create a new bank account configuration
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<BankAccountConfigDto>> CreateAccount(
        [FromBody] CreateBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating bank account configuration for {BankName}", request.BankName);
        
        // TODO: Implement CreateBankAccountCommand
        await Task.CompletedTask;
        var result = new BankAccountConfigDto(
            Guid.NewGuid(),
            request.BankCode,
            request.BankName,
            request.AccountNumber,
            request.AccountName,
            request.AccountType,
            request.Currency,
            true,
            request.UseApiIntegration,
            request.ApiSyncEnabled,
            null,
            request.EnableAutoReconciliation
        );
        return CreatedAtAction(nameof(GetAccount), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a bank account configuration
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<BankAccountConfigDto>> UpdateAccount(
        Guid id,
        [FromBody] UpdateBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating bank account {AccountId}", id);
        
        // TODO: Implement UpdateBankAccountCommand
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Delete a bank account configuration
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting bank account {AccountId}", id);
        
        // TODO: Implement DeleteBankAccountCommand
        await Task.CompletedTask;
        return NoContent();
    }

    /// <summary>
    /// Test API connection for a bank account
    /// </summary>
    [HttpPost("{id:guid}/test-connection")]
    public async Task<ActionResult<ApiConnectionTestResult>> TestConnection(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Testing API connection for bank account {AccountId}", id);
        
        // TODO: Implement TestBankApiConnectionCommand
        await Task.CompletedTask;
        return Ok(new ApiConnectionTestResult(true, "Connection successful", DateTime.UtcNow, 150));
    }

    /// <summary>
    /// Get supported banks
    /// </summary>
    [HttpGet("supported-banks")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<SupportedBankDto>> GetSupportedBanks()
    {
        _logger.LogInformation("Getting supported banks list");
        
        var banks = new[]
        {
            new SupportedBankDto("BPD", "Banco Popular Dominicano", true, "OAuth2", new[] { "statements", "transactions", "balances" }),
            new SupportedBankDto("BANRESERVAS", "Banco de Reservas", true, "ApiKey", new[] { "statements", "transactions" }),
            new SupportedBankDto("BHD", "Banco BHD León", true, "OAuth2 OpenBanking", new[] { "statements", "transactions", "balances", "payments" }),
            new SupportedBankDto("SCOTIABANK", "Scotiabank RD", true, "Certificate", new[] { "statements", "transactions" })
        };
        
        return Ok(banks);
    }
}
