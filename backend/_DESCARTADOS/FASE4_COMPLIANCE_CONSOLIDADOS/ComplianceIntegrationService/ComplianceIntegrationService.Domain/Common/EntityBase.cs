// =====================================================
// C12: ComplianceIntegrationService - Domain Layer
// EntityBase para herencia de entidades
// =====================================================

namespace ComplianceIntegrationService.Domain.Common;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Proporciona propiedades comunes de auditoría y soft delete.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que creó el registro
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que realizó la última modificación
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Indica si el registro está activo (soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;
}
