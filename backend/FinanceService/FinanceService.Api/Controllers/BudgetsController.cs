using Microsoft.AspNetCore.Mvc;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;

namespace FinanceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetRepository _budgetRepository;

    public BudgetsController(IBudgetRepository budgetRepository)
    {
        _budgetRepository = budgetRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAll(CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepository.GetAllAsync(cancellationToken);
        return Ok(budgets.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        return Ok(MapToDto(budget));
    }

    [HttpGet("year/{year:int}")]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetByYear(int year, CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepository.GetByYearAsync(year, cancellationToken);
        return Ok(budgets.Select(MapToDto));
    }

    [HttpGet("period/{period}")]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetByPeriod(BudgetPeriod period, CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepository.GetByPeriodAsync(period, cancellationToken);
        return Ok(budgets.Select(MapToDto));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetActiveBudgets(CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepository.GetActiveBudgetsAsync(cancellationToken);
        return Ok(budgets.Select(MapToDto));
    }

    [HttpGet("current/{name}")]
    public async Task<ActionResult<BudgetDto>> GetCurrentBudget(string name, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetCurrentBudgetAsync(name, cancellationToken);
        if (budget == null)
            return NotFound();

        return Ok(MapToDto(budget));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BudgetPeriod>(request.Period, true, out var period))
            return BadRequest($"Invalid budget period: {request.Period}");

        if (await _budgetRepository.NameExistsForYearAsync(request.Name, request.Year, cancellationToken))
            return Conflict($"Budget '{request.Name}' already exists for year {request.Year}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var budget = new Budget(
            dealerId,
            request.Name,
            period,
            request.Year,
            request.TotalBudget,
            request.StartDate,
            request.EndDate,
            userId,
            request.Month,
            request.Quarter
        );

        // Add categories if provided
        if (request.Categories != null)
        {
            foreach (var cat in request.Categories)
            {
                if (Enum.TryParse<ExpenseCategory>(cat.Category, true, out var expenseCategory))
                {
                    var category = new BudgetCategory(budget.Id, expenseCategory, cat.AllocatedAmount);
                    budget.AddCategory(category);
                }
            }
        }

        await _budgetRepository.AddAsync(budget, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, MapToDto(budget));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        budget.Update(request.Name, request.Description, request.TotalBudget);
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return Ok(MapToDto(budget));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<BudgetDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        budget.Activate();
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return Ok(MapToDto(budget));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<BudgetDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        budget.Deactivate();
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return Ok(MapToDto(budget));
    }

    [HttpPut("{id:guid}/adjust")]
    public async Task<ActionResult<BudgetDto>> AdjustAmount(Guid id, [FromQuery] decimal amount, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        budget.AdjustSpending(amount);
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return Ok(MapToDto(budget));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
        if (budget == null)
            return NotFound();

        await _budgetRepository.DeleteAsync(id, cancellationToken);
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

    private static BudgetDto MapToDto(Budget budget) => new(
        budget.Id,
        budget.Name,
        budget.Description,
        budget.Period.ToString(),
        budget.Year,
        budget.Month,
        budget.Quarter,
        budget.TotalBudget,
        budget.TotalSpent,
        budget.Remaining,
        budget.TotalBudget > 0 ? (budget.TotalSpent / budget.TotalBudget) * 100 : 0,
        budget.IsActive,
        budget.StartDate,
        budget.EndDate,
        budget.CreatedAt,
        budget.Categories.Select(c => new BudgetCategoryDto(
            c.Id,
            c.Category.ToString(),
            c.AllocatedAmount,
            c.SpentAmount,
            c.Remaining,
            c.IsOverBudget
        ))
    );
}
