using CarDealer.Shared.MultiTenancy;

namespace InvoicingService.Domain.Entities;

public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    BankTransfer,
    Check,
    PayPal,
    Stripe,
    Other
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    Cancelled
}

public class Payment : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string PaymentNumber { get; private set; } = string.Empty;

    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MXN";
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }

    public DateTime PaymentDate { get; private set; }
    public string? Reference { get; private set; }
    public string? TransactionId { get; private set; }
    public string? Notes { get; private set; }

    // Refund info
    public decimal RefundedAmount { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? RefundReason { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }

    private Payment() { }

    public Payment(
        Guid dealerId,
        string paymentNumber,
        Guid invoiceId,
        Guid customerId,
        decimal amount,
        string currency,
        PaymentMethod method,
        DateTime paymentDate,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        PaymentNumber = paymentNumber;
        InvoiceId = invoiceId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency;
        Method = method;
        PaymentDate = paymentDate;
        Status = PaymentStatus.Pending;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetReference(string reference)
    {
        Reference = reference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTransactionId(string transactionId)
    {
        TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as processing");

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string? transactionId = null)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Only pending or processing payments can be completed");

        Status = PaymentStatus.Completed;
        if (!string.IsNullOrEmpty(transactionId))
            TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string? reason = null)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot fail a completed or refunded payment");

        Status = PaymentStatus.Failed;
        Notes = reason ?? Notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot cancel a completed or refunded payment");

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund(decimal amount, string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        if (amount > Amount - RefundedAmount)
            throw new InvalidOperationException("Refund amount exceeds available amount");

        RefundedAmount += amount;
        RefundReason = reason;
        RefundedAt = DateTime.UtcNow;

        if (RefundedAmount >= Amount)
            Status = PaymentStatus.Refunded;

        UpdatedAt = DateTime.UtcNow;
    }
}
