using Microsoft.AspNetCore.Mvc;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;

namespace FinanceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpensesController(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetAllAsync(cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        return Ok(MapToDto(expense));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByStatus(ExpenseStatus status, CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByCategory(ExpenseCategory category, CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(category, cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("pending-approvals")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetPendingApprovalsAsync(cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("vendor/{vendor}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByVendor(string vendor, CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetByVendorAsync(vendor, cancellationToken);
        return Ok(expenses.Select(MapToDto));
    }

    [HttpGet("totals")]
    public async Task<ActionResult<decimal>> GetTotalByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var total = await _expenseRepository.GetTotalByPeriodAsync(startDate, endDate, cancellationToken);
        return Ok(total);
    }

    [HttpGet("totals-by-category")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetTotalsByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var totals = await _expenseRepository.GetTotalsByCategoryAsync(startDate, endDate, cancellationToken);
        return Ok(totals.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ExpenseCategory>(request.Category, true, out var category))
            return BadRequest($"Invalid expense category: {request.Category}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();
        var expenseNumber = GenerateExpenseNumber();

        var expense = new Expense(
            dealerId,
            expenseNumber,
            category,
            request.Description,
            request.Amount,
            request.Currency,
            request.ExpenseDate,
            userId
        );

        if (request.DueDate.HasValue)
        {
            expense.SetDueDate(request.DueDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Vendor))
        {
            expense.SetVendor(request.Vendor, request.VendorTaxId, request.InvoiceNumber);
        }

        if (!string.IsNullOrEmpty(request.ReceiptUrl))
        {
            expense.SetReceipt(request.ReceiptUrl);
        }

        if (request.AccountId.HasValue)
        {
            expense.LinkToAccount(request.AccountId.Value);
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            expense.SetNotes(request.Notes);
        }

        await _expenseRepository.AddAsync(expense, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, MapToDto(expense));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        if (expense.Status != ExpenseStatus.Draft && expense.Status != ExpenseStatus.Rejected)
            return BadRequest("Only draft or rejected expenses can be updated");

        // Parse category if provided
        ExpenseCategory? newCategory = null;
        if (!string.IsNullOrEmpty(request.Category))
        {
            if (!Enum.TryParse<ExpenseCategory>(request.Category, true, out var parsedCategory))
                return BadRequest($"Invalid expense category: {request.Category}");
            newCategory = parsedCategory;
        }

        // Update fields using entity methods
        if (!string.IsNullOrEmpty(request.Description) && request.Amount.HasValue && newCategory.HasValue)
        {
            expense.Update(request.Description, request.Amount.Value, newCategory.Value);
        }

        if (!string.IsNullOrEmpty(request.Vendor))
        {
            expense.SetVendor(request.Vendor, request.VendorTaxId, request.InvoiceNumber);
        }

        if (!string.IsNullOrEmpty(request.ReceiptUrl))
        {
            expense.SetReceipt(request.ReceiptUrl);
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            expense.SetNotes(request.Notes);
        }

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        return Ok(MapToDto(expense));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ExpenseDto>> Submit(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        expense.Submit();
        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        return Ok(MapToDto(expense));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ExpenseDto>> Approve(Guid id, [FromBody] ApproveExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        var approverId = GetCurrentUserId();

        if (request.Approve)
        {
            expense.Approve(approverId);
        }
        else
        {
            expense.Reject(request.RejectionReason);
        }

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        return Ok(MapToDto(expense));
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<ActionResult<ExpenseDto>> MarkAsPaid(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        expense.MarkAsPaid(DateTime.UtcNow);
        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        return Ok(MapToDto(expense));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
            return NotFound();

        if (expense.Status == ExpenseStatus.Paid)
            return BadRequest("Cannot delete paid expenses");

        await _expenseRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
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

    private static string GenerateExpenseNumber()
    {
        return $"EXP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private static ExpenseDto MapToDto(Expense expense) => new(
        expense.Id,
        expense.ExpenseNumber,
        expense.Category.ToString(),
        expense.Status.ToString(),
        expense.Description,
        expense.Amount,
        expense.Currency,
        expense.ExpenseDate,
        expense.DueDate,
        expense.PaidDate,
        expense.Vendor,
        expense.VendorTaxId,
        expense.InvoiceNumber,
        expense.ReceiptUrl,
        expense.AccountId,
        expense.Notes,
        expense.CreatedAt,
        expense.CreatedBy,
        expense.ApprovedBy,
        expense.ApprovedAt
    );
}
