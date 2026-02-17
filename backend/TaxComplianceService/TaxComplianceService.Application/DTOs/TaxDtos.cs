// =====================================================
// TaxComplianceService - DTOs
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

namespace TaxComplianceService.Application.DTOs;

public record TaxpayerDto(
    Guid Id,
    string Rnc,
    string BusinessName,
    string? TradeName,
    string TaxpayerType,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt
);

public record CreateTaxpayerDto(
    string Rnc,
    string BusinessName,
    string? TradeName,
    string TaxpayerType,
    string? Email,
    string? Phone,
    string? Address
);

public record TaxDeclarationDto(
    Guid Id,
    Guid TaxpayerId,
    string Rnc,
    string DeclarationType,
    string Period,
    decimal GrossAmount,
    decimal TaxableAmount,
    decimal TaxAmount,
    decimal WithholdingAmount,
    decimal NetPayable,
    string Status,
    string? DgiiConfirmationNumber,
    DateTime? SubmittedAt,
    DateTime DueDate,
    DateTime CreatedAt
);

public record CreateTaxDeclarationDto(
    Guid TaxpayerId,
    string DeclarationType,
    string Period,
    decimal GrossAmount,
    decimal TaxableAmount,
    decimal TaxAmount,
    decimal WithholdingAmount
);

public record TaxPaymentDto(
    Guid Id,
    Guid TaxDeclarationId,
    decimal Amount,
    string Status,
    string? BankReference,
    string? DgiiReceiptNumber,
    DateTime? PaidAt,
    DateTime CreatedAt
);

public record CreateTaxPaymentDto(
    Guid TaxDeclarationId,
    decimal Amount,
    string? BankReference
);

public record NcfSequenceDto(
    Guid Id,
    Guid TaxpayerId,
    string NcfType,
    string Serie,
    long CurrentNumber,
    long StartNumber,
    long EndNumber,
    DateTime ExpirationDate,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateNcfSequenceDto(
    Guid TaxpayerId,
    string NcfType,
    string Serie,
    long StartNumber,
    long EndNumber,
    DateTime ExpirationDate
);

public record Reporte606ItemDto(
    Guid Id,
    string RncCedula,
    string IdentificationType,
    string NcfNumber,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal ItbisAmount,
    decimal ItbisRetenido,
    string? PurchaseType
);

public record Reporte607ItemDto(
    Guid Id,
    string RncCedula,
    string IdentificationType,
    string NcfNumber,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal ItbisAmount,
    string? SaleType
);

public record TaxStatisticsDto(
    int TotalTaxpayers,
    int TotalDeclarations,
    int PendingDeclarations,
    int OverdueDeclarations,
    decimal TotalTaxCollected,
    decimal PendingPayments,
    DateTime GeneratedAt
);

public record DgiiSubmissionResultDto(
    bool Success,
    string? ConfirmationNumber,
    string? ErrorMessage,
    DateTime SubmittedAt
);
