using Microsoft.AspNetCore.Mvc;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;

namespace FinanceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public TransactionsController(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        return Ok(await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpGet("account/{accountId:guid}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByAccount(Guid accountId, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId, cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetPending(CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetPendingTransactionsAsync(cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByStatus(TransactionStatus status, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByType(TransactionType type, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByTypeAsync(type, cancellationToken);
        return Ok(await MapToDtosAsync(transactions, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            return BadRequest("Invalid source account");

        if (request.TargetAccountId.HasValue)
        {
            var targetAccount = await _accountRepository.GetByIdAsync(request.TargetAccountId.Value, cancellationToken);
            if (targetAccount == null)
                return BadRequest("Invalid target account");
        }

        if (!Enum.TryParse<TransactionType>(request.Type, true, out var transactionType))
            return BadRequest($"Invalid transaction type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();
        var transactionNumber = GenerateTransactionNumber();

        var transaction = new Transaction(
            dealerId,
            transactionNumber,
            transactionType,
            request.AccountId,
            request.Amount,
            request.Currency,
            request.Description,
            request.TransactionDate,
            userId
        );

        if (request.TargetAccountId.HasValue && transactionType == TransactionType.Transfer)
        {
            transaction.SetTargetAccount(request.TargetAccountId.Value);
        }

        if (!string.IsNullOrEmpty(request.Reference))
        {
            transaction.SetReference(request.Reference);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            transaction.SetCategory(request.Category);
        }

        if (request.ExchangeRate.HasValue)
        {
            transaction.SetExchangeRate(request.ExchangeRate.Value);
        }

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionDto>> Transfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
    {
        var sourceAccount = await _accountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken);
        if (sourceAccount == null)
            return BadRequest("Invalid source account");

        var targetAccount = await _accountRepository.GetByIdAsync(request.TargetAccountId, cancellationToken);
        if (targetAccount == null)
            return BadRequest("Invalid target account");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();
        var transactionNumber = GenerateTransactionNumber();

        var transaction = new Transaction(
            dealerId,
            transactionNumber,
            TransactionType.Transfer,
            request.SourceAccountId,
            request.Amount,
            request.Currency,
            request.Description,
            request.TransactionDate,
            userId
        );

        transaction.SetTargetAccount(request.TargetAccountId);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<TransactionDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        transaction.Post();
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        return Ok(await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<TransactionDto>> Post(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        transaction.Post();
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        return Ok(await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<TransactionDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        transaction.Void();
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        return Ok(await MapToDtoAsync(transaction, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        if (transaction.Status == TransactionStatus.Posted)
            return BadRequest("Cannot delete posted transactions");

        await _transactionRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("account/{accountId:guid}/balance")]
    public async Task<ActionResult<decimal>> GetAccountBalance(Guid accountId, CancellationToken cancellationToken)
    {
        if (!await _accountRepository.ExistsAsync(accountId, cancellationToken))
            return NotFound("Account not found");

        var balance = await _transactionRepository.GetAccountBalanceAsync(accountId, cancellationToken);
        return Ok(balance);
    }

    private Guid GetCurrentDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static string GenerateTransactionNumber()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private async Task<IEnumerable<TransactionDto>> MapToDtosAsync(IEnumerable<Transaction> transactions, CancellationToken cancellationToken)
    {
        var result = new List<TransactionDto>();
        foreach (var t in transactions)
        {
            result.Add(await MapToDtoAsync(t, cancellationToken));
        }
        return result;
    }

    private async Task<TransactionDto> MapToDtoAsync(Transaction t, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(t.AccountId, cancellationToken);
        Account? targetAccount = null;
        if (t.TargetAccountId.HasValue)
        {
            targetAccount = await _accountRepository.GetByIdAsync(t.TargetAccountId.Value, cancellationToken);
        }

        return new TransactionDto(
            t.Id,
            t.TransactionNumber,
            t.Type.ToString(),
            t.Status.ToString(),
            t.AccountId,
            account?.Name,
            t.TargetAccountId,
            targetAccount?.Name,
            t.Amount,
            t.Currency,
            t.ExchangeRate,
            t.Description,
            t.Reference,
            t.Category,
            t.TransactionDate,
            t.PostedDate,
            t.ReconciledDate,
            t.InvoiceId,
            t.PaymentId,
            t.ExpenseId,
            t.CreatedAt
        );
    }
}
