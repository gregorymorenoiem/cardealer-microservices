// =====================================================
// TaxComplianceService - Entities
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using TaxComplianceService.Domain.Enums;

namespace TaxComplianceService.Domain.Entities;

/// <summary>
/// Declaración tributaria según normativas DGII
/// </summary>
public class TaxDeclaration
{
    public Guid Id { get; set; }
    public Guid TaxpayerId { get; set; }
    public string Rnc { get; set; } = string.Empty;
    public DeclarationType DeclarationType { get; set; }
    public string Period { get; set; } = string.Empty; // YYYYMM format
    public decimal GrossAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public decimal NetPayable { get; set; }
    public DeclarationStatus Status { get; set; }
    public string? DgiiConfirmationNumber { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<TaxPayment> Payments { get; set; } = new List<TaxPayment>();
    public ICollection<Reporte606Item> Reporte606Items { get; set; } = new List<Reporte606Item>();
    public ICollection<Reporte607Item> Reporte607Items { get; set; } = new List<Reporte607Item>();
}

/// <summary>
/// Contribuyente registrado
/// </summary>
public class Taxpayer
{
    public Guid Id { get; set; }
    public string Rnc { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public TaxpayerType TaxpayerType { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<TaxDeclaration> Declarations { get; set; } = new List<TaxDeclaration>();
    public ICollection<NcfSequence> NcfSequences { get; set; } = new List<NcfSequence>();
}

/// <summary>
/// Pago de impuestos
/// </summary>
public class TaxPayment
{
    public Guid Id { get; set; }
    public Guid TaxDeclarationId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? BankReference { get; set; }
    public string? DgiiReceiptNumber { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public TaxDeclaration? TaxDeclaration { get; set; }
}

/// <summary>
/// Secuencia de NCF autorizada por DGII
/// </summary>
public class NcfSequence
{
    public Guid Id { get; set; }
    public Guid TaxpayerId { get; set; }
    public NcfType NcfType { get; set; }
    public string Serie { get; set; } = string.Empty;
    public long CurrentNumber { get; set; }
    public long StartNumber { get; set; }
    public long EndNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Taxpayer? Taxpayer { get; set; }
}

/// <summary>
/// Item del Reporte 606 (Compras)
/// </summary>
public class Reporte606Item
{
    public Guid Id { get; set; }
    public Guid TaxDeclarationId { get; set; }
    public string RncCedula { get; set; } = string.Empty;
    public int TipoIdentificacion { get; set; } // 1=RNC, 2=Cédula
    public string TipoBieneServicio { get; set; } = string.Empty;
    public string Ncf { get; set; } = string.Empty;
    public string? NcfModificado { get; set; }
    public DateTime FechaComprobante { get; set; }
    public DateTime? FechaPago { get; set; }
    public decimal MontoFacturado { get; set; }
    public decimal ItbisFacturado { get; set; }
    public decimal ItbisRetenido { get; set; }
    public decimal IsrRetenido { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public TaxDeclaration? TaxDeclaration { get; set; }
}

/// <summary>
/// Item del Reporte 607 (Ventas)
/// </summary>
public class Reporte607Item
{
    public Guid Id { get; set; }
    public Guid TaxDeclarationId { get; set; }
    public string RncCedula { get; set; } = string.Empty;
    public int TipoIdentificacion { get; set; }
    public string Ncf { get; set; } = string.Empty;
    public string? NcfModificado { get; set; }
    public string TipoIngreso { get; set; } = string.Empty;
    public DateTime FechaComprobante { get; set; }
    public DateTime? FechaRetencion { get; set; }
    public decimal MontoFacturado { get; set; }
    public decimal ItbisFacturado { get; set; }
    public decimal ItbisRetenidoPorTerceros { get; set; }
    public decimal ItbisPercibido { get; set; }
    public decimal IsrRetenidoPorTerceros { get; set; }
    public decimal IsrPercibido { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public TaxDeclaration? TaxDeclaration { get; set; }
}

/// <summary>
/// Retención de impuestos
/// </summary>
public class TaxWithholding
{
    public Guid Id { get; set; }
    public Guid TaxpayerId { get; set; }
    public string SupplierRnc { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public WithholdingType WithholdingType { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal WithholdingRate { get; set; }
    public decimal WithholdingAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Ncf { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
