using CarDealer.Shared.MultiTenancy;

namespace FinanceService.Domain.Entities;

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense,
    Bank,
    CashOnHand,
    AccountsReceivable,
    AccountsPayable,
    Inventory
}

public class Account : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AccountType Type { get; private set; }
    public string Currency { get; private set; } = "MXN";
    public decimal Balance { get; private set; }
    public decimal InitialBalance { get; private set; }

    public Guid? ParentAccountId { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystem { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Account? ParentAccount { get; private set; }
    public ICollection<Account> ChildAccounts { get; private set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    private Account() { }

    public Account(
        Guid dealerId,
        string code,
        string name,
        AccountType type,
        string currency,
        decimal initialBalance = 0,
        Guid? parentAccountId = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Code = code;
        Name = name;
        Type = type;
        Currency = currency;
        InitialBalance = initialBalance;
        Balance = initialBalance;
        ParentAccountId = parentAccountId;
        IsActive = true;
        IsSystem = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBalance(decimal amount)
    {
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBalance(decimal balance)
    {
        Balance = balance;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot deactivate a system account");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSystem()
    {
        IsSystem = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Account CreateBankAccount(Guid dealerId, string code, string name, string currency, decimal initialBalance = 0)
    {
        return new Account(dealerId, code, name, AccountType.Bank, currency, initialBalance);
    }

    public static Account CreateExpenseAccount(Guid dealerId, string code, string name, string currency)
    {
        return new Account(dealerId, code, name, AccountType.Expense, currency);
    }
}
