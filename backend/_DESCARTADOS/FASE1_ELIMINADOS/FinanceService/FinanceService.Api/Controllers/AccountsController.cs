using Microsoft.AspNetCore.Mvc;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;

namespace FinanceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;

    public AccountsController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        return Ok(accounts.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account == null)
            return NotFound();

        return Ok(MapToDto(account));
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<AccountDto>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByCodeAsync(code, cancellationToken);
        if (account == null)
            return NotFound();

        return Ok(MapToDto(account));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetByType(AccountType type, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByTypeAsync(type, cancellationToken);
        return Ok(accounts.Select(MapToDto));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetActiveAccounts(CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetActiveAccountsAsync(cancellationToken);
        return Ok(accounts.Select(MapToDto));
    }

    [HttpGet("{parentId:guid}/children")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetChildAccounts(Guid parentId, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetChildAccountsAsync(parentId, cancellationToken);
        return Ok(accounts.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (await _accountRepository.CodeExistsAsync(request.Code, cancellationToken))
            return Conflict($"Account with code '{request.Code}' already exists");

        if (!Enum.TryParse<AccountType>(request.Type, true, out var accountType))
            return BadRequest($"Invalid account type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var account = new Account(
            dealerId,
            request.Code,
            request.Name,
            accountType,
            request.Currency,
            request.InitialBalance,
            request.ParentAccountId
        );

        await _accountRepository.AddAsync(account, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, MapToDto(account));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account == null)
            return NotFound();

        account.Update(request.Name, request.Description);
        await _accountRepository.UpdateAsync(account, cancellationToken);
        return Ok(MapToDto(account));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<AccountDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account == null)
            return NotFound();

        account.Activate();
        await _accountRepository.UpdateAsync(account, cancellationToken);
        return Ok(MapToDto(account));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<AccountDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account == null)
            return NotFound();

        account.Deactivate();
        await _accountRepository.UpdateAsync(account, cancellationToken);
        return Ok(MapToDto(account));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account == null)
            return NotFound();

        if (account.IsSystem)
            return BadRequest("Cannot delete system accounts");

        await _accountRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentDealerId()
    {
        // In production, this would come from the JWT claims
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private static AccountDto MapToDto(Account account) => new(
        account.Id,
        account.Code,
        account.Name,
        account.Description,
        account.Type.ToString(),
        account.Currency,
        account.Balance,
        account.InitialBalance,
        account.ParentAccountId,
        account.IsActive,
        account.IsSystem,
        account.CreatedAt,
        account.UpdatedAt
    );
}
