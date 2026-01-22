// =====================================================
// ComplianceReportingService - Enums
// Reportes Consolidados de Cumplimiento RD
// =====================================================

namespace ComplianceReportingService.Domain.Enums;

/// <summary>
/// Tipo de reporte
/// </summary>
public enum ReportType
{
    Daily,          // Diario
    Weekly,         // Semanal
    Monthly,        // Mensual
    Quarterly,      // Trimestral
    Annual,         // Anual
    OnDemand        // Bajo demanda
}

/// <summary>
/// Organismo regulador
/// </summary>
public enum RegulatoryBody
{
    DGII,                   // Impuestos Internos
    UAF,                    // Análisis Financiero (AML)
    ProConsumidor,          // Protección al consumidor
    INTRANT,                // Tránsito y transporte
    OGTIC,                  // Firma digital
    SuperintendenciaBancos, // Regulador bancario
    BancoCentral            // Banco Central
}

/// <summary>
/// Estado del reporte
/// </summary>
public enum ReportStatus
{
    Draft,          // Borrador
    Generated,      // Generado
    Validated,      // Validado
    Submitted,      // Enviado
    Accepted,       // Aceptado
    Rejected,       // Rechazado
    Error           // Error en generación
}
