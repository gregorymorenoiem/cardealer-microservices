// =====================================================
// TaxComplianceService - Enums
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

namespace TaxComplianceService.Domain.Enums;

/// <summary>
/// Tipo de declaración tributaria según normativas DGII
/// </summary>
public enum DeclarationType
{
    /// <summary>ITBIS - Impuesto sobre Transferencias de Bienes Industrializados y Servicios (18%)</summary>
    ITBIS,
    
    /// <summary>ISR - Impuesto Sobre la Renta</summary>
    ISR,
    
    /// <summary>Reporte 606 - Compras de Bienes y Servicios</summary>
    Reporte606,
    
    /// <summary>Reporte 607 - Ventas de Bienes y Servicios</summary>
    Reporte607,
    
    /// <summary>Reporte 608 - Comprobantes Anulados</summary>
    Reporte608,
    
    /// <summary>Reporte 609 - Pagos al Exterior</summary>
    Reporte609,
    
    /// <summary>IR-17 - Retenciones del 27%</summary>
    IR17,
    
    /// <summary>IT-1 - ITBIS Mensual</summary>
    IT1,
    
    /// <summary>IR-1 - Retenciones ISR Mensual</summary>
    IR1
}

/// <summary>
/// Estado de la declaración tributaria
/// </summary>
public enum DeclarationStatus
{
    /// <summary>Borrador - En preparación</summary>
    Draft,
    
    /// <summary>Pendiente de envío</summary>
    Pending,
    
    /// <summary>Enviada a DGII</summary>
    Submitted,
    
    /// <summary>Aceptada por DGII</summary>
    Accepted,
    
    /// <summary>Rechazada por DGII</summary>
    Rejected,
    
    /// <summary>En revisión por DGII</summary>
    UnderReview,
    
    /// <summary>Requiere corrección</summary>
    RequiresCorrection,
    
    /// <summary>Pagada</summary>
    Paid
}

/// <summary>
/// Tipo de contribuyente según RNC
/// </summary>
public enum TaxpayerType
{
    /// <summary>Persona Física (RNC 9 dígitos)</summary>
    Individual,
    
    /// <summary>Persona Jurídica (RNC 11 dígitos)</summary>
    Company,
    
    /// <summary>Pequeño Contribuyente</summary>
    SmallBusiness,
    
    /// <summary>Gran Contribuyente</summary>
    LargeContributor,
    
    /// <summary>Zona Franca</summary>
    FreeZone,
    
    /// <summary>Entidad Sin Fines de Lucro</summary>
    NonProfit
}

/// <summary>
/// Tipo de NCF (Número de Comprobante Fiscal)
/// </summary>
public enum NcfType
{
    /// <summary>B01 - Facturas con Crédito Fiscal</summary>
    B01,
    
    /// <summary>B02 - Facturas para Consumidor Final</summary>
    B02,
    
    /// <summary>B03 - Nota de Débito</summary>
    B03,
    
    /// <summary>B04 - Nota de Crédito</summary>
    B04,
    
    /// <summary>B11 - Comprobantes Especiales</summary>
    B11,
    
    /// <summary>B13 - Comprobantes de Gastos Menores</summary>
    B13,
    
    /// <summary>B14 - Regímenes Especiales</summary>
    B14,
    
    /// <summary>B15 - Comprobantes Gubernamentales</summary>
    B15,
    
    /// <summary>B16 - Exportación</summary>
    B16,
    
    /// <summary>E31 - e-CF Factura de Crédito Fiscal</summary>
    E31,
    
    /// <summary>E32 - e-CF Factura de Consumidor</summary>
    E32
}

/// <summary>
/// Estado del pago tributario
/// </summary>
public enum PaymentStatus
{
    /// <summary>Pendiente de pago</summary>
    Pending,
    
    /// <summary>Procesando</summary>
    Processing,
    
    /// <summary>Pagado</summary>
    Paid,
    
    /// <summary>Fallido</summary>
    Failed,
    
    /// <summary>Cancelado</summary>
    Cancelled,
    
    /// <summary>Reembolsado</summary>
    Refunded
}

/// <summary>
/// Tipo de retención
/// </summary>
public enum WithholdingType
{
    /// <summary>Retención ISR 10%</summary>
    ISR10,
    
    /// <summary>Retención ISR 27%</summary>
    ISR27,
    
    /// <summary>Retención ITBIS 30%</summary>
    ITBIS30,
    
    /// <summary>Retención ITBIS 100%</summary>
    ITBIS100,
    
    /// <summary>Retención por Pagos al Exterior</summary>
    ForeignPayment
}
