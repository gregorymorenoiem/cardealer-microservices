using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

public enum InvoiceStatus
{
    Draft,
    Issued,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Cancelled,
    Voided
}

public class Invoice : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid? SubscriptionId { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string Currency { get; private set; } = "USD";

    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }

    public string? StripeInvoiceId { get; private set; }
    public string? PdfUrl { get; private set; }

    public string? Notes { get; private set; }
    public string? LineItems { get; private set; } // JSON

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // ✅ AUDIT FIX: Concurrency control
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    // ✅ AUDIT FIX: Navigation property for FK relationship
    public Subscription? Subscription { get; private set; }

    private Invoice() { }

    public Invoice(
        Guid dealerId,
        string invoiceNumber,
        decimal subtotal,
        decimal taxAmount,
        DateTime dueDate,
        Guid? subscriptionId = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));

        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative", nameof(subtotal));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        InvoiceNumber = invoiceNumber;
        SubscriptionId = subscriptionId;
        Subtotal = subtotal;
        TaxAmount = taxAmount;
        TotalAmount = subtotal + taxAmount;
        DueDate = dueDate;
        IssueDate = DateTime.UtcNow;
        Status = InvoiceStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetStripeInfo(string invoiceId, string? pdfUrl = null)
    {
        StripeInvoiceId = invoiceId;
        PdfUrl = pdfUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLineItems(string lineItems)
    {
        LineItems = lineItems;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Issue()
    {
        Status = InvoiceStatus.Issued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSent()
    {
        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive");

        PaidAmount += amount;

        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            PaidDate = DateTime.UtcNow;
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void MarkOverdue()
    {
        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Void()
    {
        Status = InvoiceStatus.Voided;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetBalanceDue() => TotalAmount - PaidAmount;
}
