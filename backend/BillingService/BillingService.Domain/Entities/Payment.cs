using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Refunded,
    PartiallyRefunded,
    Disputed
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer,
    Cash,
    Check,
    Other
}

public class Payment : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public Guid? SubscriptionId { get; private set; }
    public Guid? InvoiceId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";

    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }

    public string? StripePaymentIntentId { get; private set; }
    public string? StripeChargeId { get; private set; }

    public string? Description { get; private set; }
    public string? ReceiptUrl { get; private set; }

    public string? FailureReason { get; private set; }
    public string? RefundReason { get; private set; }
    public decimal RefundedAmount { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // ✅ AUDIT FIX: Concurrency control
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    // ✅ AUDIT FIX: Navigation properties for FK relationships
    public Subscription? Subscription { get; private set; }
    public Invoice? Invoice { get; private set; }

    private Payment() { }

    public Payment(
        Guid dealerId,
        decimal amount,
        PaymentMethod method,
        string? description = null,
        Guid? subscriptionId = null,
        Guid? invoiceId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Amount = amount;
        Method = method;
        Description = description;
        SubscriptionId = subscriptionId;
        InvoiceId = invoiceId;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetStripeInfo(string paymentIntentId, string? chargeId = null, string? receiptUrl = null)
    {
        StripePaymentIntentId = paymentIntentId;
        StripeChargeId = chargeId;
        ReceiptUrl = receiptUrl;
    }

    public void MarkProcessing()
    {
        Status = PaymentStatus.Processing;
    }

    public void MarkSucceeded(string? receiptUrl = null)
    {
        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(receiptUrl))
            ReceiptUrl = receiptUrl;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void Refund(decimal amount, string reason)
    {
        if (amount > Amount - RefundedAmount)
            throw new ArgumentException("Refund amount exceeds available balance");

        RefundedAmount += amount;
        RefundReason = reason;
        RefundedAt = DateTime.UtcNow;

        Status = RefundedAmount >= Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;
    }

    public void MarkDisputed()
    {
        Status = PaymentStatus.Disputed;
    }
}
