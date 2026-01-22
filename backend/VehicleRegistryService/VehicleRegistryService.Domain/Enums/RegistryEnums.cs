// =====================================================
// VehicleRegistryService - Enums
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

namespace VehicleRegistryService.Domain.Enums;

/// <summary>
/// Estado del registro vehicular
/// </summary>
public enum RegistrationStatus
{
    Active,         // Vigente
    Expired,        // Expirado (necesita renovación)
    Suspended,      // Suspendido por infracción
    Cancelled,      // Cancelado (vehículo de baja)
    Pending         // Pendiente de aprobación
}

/// <summary>
/// Tipo de vehículo según INTRANT
/// </summary>
public enum VehicleType
{
    Private,        // Particular
    Public,         // Público (taxi, concho)
    Commercial,     // Comercial (camión, furgoneta)
    Government,     // Gubernamental
    Diplomatic,     // Diplomático
    Rental,         // Alquiler
    Exempt          // Exonerado
}

/// <summary>
/// Estado de transferencia de propiedad
/// </summary>
public enum TransferStatus
{
    Pending,        // Pendiente de documentos
    InProcess,      // En proceso de trámite
    Completed,      // Completada
    Rejected,       // Rechazada
    Cancelled       // Cancelada
}

/// <summary>
/// Estado del gravamen
/// </summary>
public enum LienStatus
{
    Active,         // Vigente
    Released,       // Liberado
    Defaulted       // En mora
}
