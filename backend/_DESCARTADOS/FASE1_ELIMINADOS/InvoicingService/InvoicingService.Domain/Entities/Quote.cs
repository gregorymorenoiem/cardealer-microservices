using CarDealer.Shared.MultiTenancy;

namespace InvoicingService.Domain.Entities;

public enum QuoteStatus
{
    Draft,
    Sent,
    Viewed,
    Accepted,
    Rejected,
    Expired,
    Converted
}

public class Quote : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string QuoteNumber { get; private set; } = string.Empty;
    public QuoteStatus Status { get; private set; }

    // Customer info
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string? CustomerPhone { get; private set; }

    // Quote details
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public string Currency { get; private set; } = "MXN";

    public decimal Subtotal { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total { get; private set; }

    // Related
    public Guid? DealId { get; private set; }
    public Guid? LeadId { get; private set; }
    public string? Terms { get; private set; }
    public string? Notes { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? ViewedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Navigation
    public ICollection<QuoteItem> Items { get; private set; } = new List<QuoteItem>();

    private Quote() { }

    public Quote(
        Guid dealerId,
        string quoteNumber,
        Guid customerId,
        string customerName,
        string customerEmail,
        string title,
        DateTime issueDate,
        DateTime validUntil,
        string currency,
        decimal taxRate,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        QuoteNumber = quoteNumber;
        Status = QuoteStatus.Draft;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        Title = title;
        IssueDate = issueDate;
        ValidUntil = validUntil;
        Currency = currency;
        TaxRate = taxRate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, string? terms, string? notes)
    {
        Title = title;
        Description = description;
        Terms = terms;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCustomerInfo(string name, string email, string? phone)
    {
        CustomerName = name;
        CustomerEmail = email;
        CustomerPhone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDates(DateTime issueDate, DateTime validUntil)
    {
        IssueDate = issueDate;
        ValidUntil = validUntil;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToDeal(Guid dealId)
    {
        DealId = dealId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToLead(Guid leadId)
    {
        LeadId = leadId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(QuoteItem item)
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
        if (Status != QuoteStatus.Draft)
            throw new InvalidOperationException("Only draft quotes can be sent");

        Status = QuoteStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsViewed()
    {
        if (ViewedAt == null)
        {
            ViewedAt = DateTime.UtcNow;
            Status = QuoteStatus.Viewed;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Accept()
    {
        if (Status == QuoteStatus.Rejected || Status == QuoteStatus.Expired)
            throw new InvalidOperationException("Cannot accept a rejected or expired quote");

        Status = QuoteStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? reason)
    {
        if (Status == QuoteStatus.Accepted || Status == QuoteStatus.Converted)
            throw new InvalidOperationException("Cannot reject an accepted or converted quote");

        Status = QuoteStatus.Rejected;
        RejectionReason = reason;
        RejectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status == QuoteStatus.Draft || Status == QuoteStatus.Sent || Status == QuoteStatus.Viewed)
        {
            Status = QuoteStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsConverted()
    {
        if (Status != QuoteStatus.Accepted)
            throw new InvalidOperationException("Only accepted quotes can be converted to invoices");

        Status = QuoteStatus.Converted;
        UpdatedAt = DateTime.UtcNow;
    }

    public Invoice ConvertToInvoice(string invoiceNumber, DateTime issueDate, DateTime dueDate, Guid createdBy)
    {
        MarkAsConverted();

        var invoice = new Invoice(
            DealerId,
            invoiceNumber,
            InvoiceType.Standard,
            CustomerId,
            CustomerName,
            CustomerEmail,
            issueDate,
            dueDate,
            Currency,
            TaxRate,
            createdBy);

        invoice.LinkToQuote(Id);

        if (DealId.HasValue)
            invoice.LinkToDeal(DealId.Value);

        if (!string.IsNullOrEmpty(Notes))
            invoice.SetNotes(Notes);

        return invoice;
    }
}
