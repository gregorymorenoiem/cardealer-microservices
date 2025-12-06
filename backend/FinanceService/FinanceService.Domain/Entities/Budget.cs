using CarDealer.Shared.MultiTenancy;

namespace FinanceService.Domain.Entities;

public enum BudgetPeriod
{
    Monthly,
    Quarterly,
    Yearly
}

public class Budget : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public BudgetPeriod Period { get; private set; }
    public int Year { get; private set; }
    public int? Month { get; private set; }
    public int? Quarter { get; private set; }

    public decimal TotalBudget { get; private set; }
    public decimal TotalSpent { get; private set; }
    public decimal Remaining => TotalBudget - TotalSpent;
    public decimal PercentUsed => TotalBudget > 0 ? (TotalSpent / TotalBudget) * 100 : 0;

    public bool IsActive { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public ICollection<BudgetCategory> Categories { get; private set; } = new List<BudgetCategory>();

    private Budget() { }

    public Budget(
        Guid dealerId,
        string name,
        BudgetPeriod period,
        int year,
        decimal totalBudget,
        DateTime startDate,
        DateTime endDate,
        Guid createdBy,
        int? month = null,
        int? quarter = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Period = period;
        Year = year;
        Month = month;
        Quarter = quarter;
        TotalBudget = totalBudget;
        TotalSpent = 0;
        IsActive = true;
        StartDate = startDate;
        EndDate = endDate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, decimal totalBudget)
    {
        Name = name;
        Description = description;
        TotalBudget = totalBudget;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSpending(decimal amount)
    {
        TotalSpent += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustSpending(decimal adjustment)
    {
        TotalSpent += adjustment;
        if (TotalSpent < 0) TotalSpent = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCategory(BudgetCategory category)
    {
        Categories.Add(category);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverBudget => TotalSpent > TotalBudget;

    public bool IsNearLimit(decimal thresholdPercent = 90)
    {
        return PercentUsed >= thresholdPercent && !IsOverBudget;
    }
}

public class BudgetCategory
{
    public Guid Id { get; private set; }
    public Guid BudgetId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public decimal AllocatedAmount { get; private set; }
    public decimal SpentAmount { get; private set; }
    public decimal Remaining => AllocatedAmount - SpentAmount;

    // Navigation
    public Budget? Budget { get; private set; }

    private BudgetCategory() { }

    public BudgetCategory(Guid budgetId, ExpenseCategory category, decimal allocatedAmount)
    {
        Id = Guid.NewGuid();
        BudgetId = budgetId;
        Category = category;
        AllocatedAmount = allocatedAmount;
        SpentAmount = 0;
    }

    public void UpdateAllocation(decimal amount)
    {
        AllocatedAmount = amount;
    }

    public void RecordSpending(decimal amount)
    {
        SpentAmount += amount;
    }

    public bool IsOverBudget => SpentAmount > AllocatedAmount;
}
