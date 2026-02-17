namespace InvoicingService.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }

    public string Description { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public string? ProductName { get; private set; }
    public Guid? ProductId { get; private set; }

    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = "pcs";
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount => (Quantity * UnitPrice) * (DiscountPercent / 100);
    public decimal Total => (Quantity * UnitPrice) - DiscountAmount;

    public int SortOrder { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }

    private InvoiceItem() { }

    public InvoiceItem(
        Guid invoiceId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        int sortOrder,
        string? productCode = null,
        string? productName = null,
        Guid? productId = null,
        decimal discountPercent = 0)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        SortOrder = sortOrder;
        ProductCode = productCode;
        ProductName = productName;
        ProductId = productId;
        DiscountPercent = discountPercent;
    }

    public void Update(string description, decimal quantity, string unit, decimal unitPrice, decimal discountPercent)
    {
        Description = description;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
    }

    public void LinkToProduct(Guid productId, string productCode, string productName)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
    }

    public void SetSortOrder(int order)
    {
        SortOrder = order;
    }
}
