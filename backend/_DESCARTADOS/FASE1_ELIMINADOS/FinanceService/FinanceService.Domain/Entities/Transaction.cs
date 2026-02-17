using CarDealer.Shared.MultiTenancy;

namespace FinanceService.Domain.Entities;

public enum TransactionType
{
    Debit,
    Credit,
    Transfer,
    Adjustment
}

public enum TransactionStatus
{
    Pending,
    Posted,
    Void,
    Reconciled
}

public class Transaction : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string TransactionNumber { get; private set; } = string.Empty;

    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }

    public Guid AccountId { get; private set; }
    public Guid? TargetAccountId { get; private set; } // For transfers

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MXN";
    public decimal? ExchangeRate { get; private set; }

    public string Description { get; private set; } = string.Empty;
    public string? Reference { get; private set; }
    public string? Category { get; private set; }

    public DateTime TransactionDate { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public DateTime? ReconciledDate { get; private set; }

    // Related entities
    public Guid? InvoiceId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Guid? ExpenseId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public Account? Account { get; private set; }
    public Account? TargetAccount { get; private set; }

    private Transaction() { }

    public Transaction(
        Guid dealerId,
        string transactionNumber,
        TransactionType type,
        Guid accountId,
        decimal amount,
        string currency,
        string description,
        DateTime transactionDate,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        TransactionNumber = transactionNumber;
        Type = type;
        Status = TransactionStatus.Pending;
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        Description = description;
        TransactionDate = transactionDate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetTargetAccount(Guid targetAccountId)
    {
        if (Type != TransactionType.Transfer)
            throw new InvalidOperationException("Target account only applies to transfers");

        TargetAccountId = targetAccountId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReference(string reference)
    {
        Reference = reference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCategory(string category)
    {
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExchangeRate(decimal rate)
    {
        ExchangeRate = rate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToInvoice(Guid invoiceId)
    {
        InvoiceId = invoiceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToPayment(Guid paymentId)
    {
        PaymentId = paymentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToExpense(Guid expenseId)
    {
        ExpenseId = expenseId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Post()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be posted");

        Status = TransactionStatus.Posted;
        PostedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Void()
    {
        if (Status == TransactionStatus.Reconciled)
            throw new InvalidOperationException("Cannot void a reconciled transaction");

        Status = TransactionStatus.Void;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reconcile()
    {
        if (Status != TransactionStatus.Posted)
            throw new InvalidOperationException("Only posted transactions can be reconciled");

        Status = TransactionStatus.Reconciled;
        ReconciledDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
