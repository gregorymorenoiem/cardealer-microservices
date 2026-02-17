namespace FinanceService.Application.DTOs;

public record BudgetDto(
    Guid Id,
    string Name,
    string? Description,
    string Period,
    int Year,
    int? Month,
    int? Quarter,
    decimal TotalBudget,
    decimal TotalSpent,
    decimal Remaining,
    decimal PercentUsed,
    bool IsActive,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    IEnumerable<BudgetCategoryDto> Categories
);

public record BudgetCategoryDto(
    Guid Id,
    string Category,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal Remaining,
    bool IsOverBudget
);

public record CreateBudgetRequest(
    string Name,
    string? Description,
    string Period,
    int Year,
    int? Month,
    int? Quarter,
    decimal TotalBudget,
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<CreateBudgetCategoryRequest>? Categories = null
);

public record CreateBudgetCategoryRequest(
    string Category,
    decimal AllocatedAmount
);

public record UpdateBudgetRequest(
    string Name,
    string? Description,
    decimal TotalBudget
);
