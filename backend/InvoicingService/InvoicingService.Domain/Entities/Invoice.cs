using CarDealer.Shared.MultiTenancy;

namespace InvoicingService.Domain.Entities;

public enum InvoiceStatus
{
    Draft,
    Pending,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Cancelled,
    Refunded
}

public enum InvoiceType
{
    Standard,
    Proforma,
    Credit,
    Debit
}

public class Invoice : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceType Type { get; private set; }
    public InvoiceStatus Status { get; private set; }

    // Customer info
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string? CustomerTaxId { get; private set; }
    public string? CustomerAddress { get; private set; }

    // Invoice details
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string Currency { get; private set; } = "MXN";
    public decimal Subtotal { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal BalanceDue => Total - PaidAmount;

    // CFDI (Mexico)
    public string? CfdiUuid { get; private set; }
    public string? CfdiXml { get; private set; }
    public string? CfdiPdf { get; private set; }
    public DateTime? CfdiStampedAt { get; private set; }

    // Related
    public Guid? QuoteId { get; private set; }
    public Guid? DealId { get; private set; }
    public string? Notes { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public ICollection<InvoiceItem> Items { get; private set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private Invoice() { }

    public Invoice(
        Guid dealerId,
        string invoiceNumber,
        InvoiceType type,
        Guid customerId,
        string customerName,
        string customerEmail,
        DateTime issueDate,
        DateTime dueDate,
        string currency,
        decimal taxRate,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        InvoiceNumber = invoiceNumber;
        Type = type;
        Status = InvoiceStatus.Draft;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
        TaxRate = taxRate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCustomerInfo(string name, string email, string? taxId, string? address)
    {
        CustomerName = name;
        CustomerEmail = email;
        CustomerTaxId = taxId;
        CustomerAddress = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDates(DateTime issueDate, DateTime dueDate)
    {
        IssueDate = issueDate;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToQuote(Guid quoteId)
    {
        QuoteId = quoteId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToDeal(Guid dealId)
    {
        DealId = dealId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(InvoiceItem item)
    {
        Items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Total);
        TaxAmount = (Subtotal - DiscountAmount) * (TaxRate / 100);
        Total = Subtotal - DiscountAmount + TaxAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Send()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be sent");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPending()
    {
        Status = InvoiceStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount)
    {
        PaidAmount += amount;

        if (PaidAmount >= Total)
            Status = InvoiceStatus.Paid;
        else if (PaidAmount > 0)
            Status = InvoiceStatus.PartiallyPaid;

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Sent || Status == InvoiceStatus.PartiallyPaid)
        {
            Status = InvoiceStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice");

        Status = InvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        if (Status != InvoiceStatus.Paid && Status != InvoiceStatus.PartiallyPaid)
            throw new InvalidOperationException("Only paid invoices can be refunded");

        Status = InvoiceStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCfdi(string uuid, string xml, string? pdf)
    {
        CfdiUuid = uuid;
        CfdiXml = xml;
        CfdiPdf = pdf;
        CfdiStampedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
