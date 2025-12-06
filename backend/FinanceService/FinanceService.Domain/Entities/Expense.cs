using CarDealer.Shared.MultiTenancy;

namespace FinanceService.Domain.Entities;

public enum ExpenseStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Paid,
    Cancelled
}

public enum ExpenseCategory
{
    Utilities,
    Rent,
    Salaries,
    Marketing,
    Supplies,
    Maintenance,
    Insurance,
    Taxes,
    Travel,
    Entertainment,
    Professional,
    Equipment,
    VehicleCosts,
    Other
}

public class Expense : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string ExpenseNumber { get; private set; } = string.Empty;

    public ExpenseCategory Category { get; private set; }
    public ExpenseStatus Status { get; private set; }

    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MXN";

    public DateTime ExpenseDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }

    public string? Vendor { get; private set; }
    public string? VendorTaxId { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? ReceiptUrl { get; private set; }

    public Guid? AccountId { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    // Navigation
    public Account? Account { get; private set; }

    private Expense() { }

    public Expense(
        Guid dealerId,
        string expenseNumber,
        ExpenseCategory category,
        string description,
        decimal amount,
        string currency,
        DateTime expenseDate,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        ExpenseNumber = expenseNumber;
        Category = category;
        Status = ExpenseStatus.Draft;
        Description = description;
        Amount = amount;
        Currency = currency;
        ExpenseDate = expenseDate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string description, decimal amount, ExpenseCategory category)
    {
        if (Status != ExpenseStatus.Draft && Status != ExpenseStatus.Rejected)
            throw new InvalidOperationException("Only draft or rejected expenses can be updated");

        Description = description;
        Amount = amount;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVendor(string vendor, string? taxId, string? invoiceNumber)
    {
        Vendor = vendor;
        VendorTaxId = taxId;
        InvoiceNumber = invoiceNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReceipt(string receiptUrl)
    {
        ReceiptUrl = receiptUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToAccount(Guid accountId)
    {
        AccountId = accountId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit()
    {
        if (Status != ExpenseStatus.Draft && Status != ExpenseStatus.Rejected)
            throw new InvalidOperationException("Only draft or rejected expenses can be submitted");

        Status = ExpenseStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Only submitted expenses can be approved");

        Status = ExpenseStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? reason)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Only submitted expenses can be rejected");

        Status = ExpenseStatus.Rejected;
        Notes = reason ?? Notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime paidDate)
    {
        if (Status != ExpenseStatus.Approved)
            throw new InvalidOperationException("Only approved expenses can be marked as paid");

        Status = ExpenseStatus.Paid;
        PaidDate = paidDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ExpenseStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid expense");

        Status = ExpenseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
