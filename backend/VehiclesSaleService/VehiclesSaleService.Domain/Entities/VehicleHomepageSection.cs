namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Tabla de relación muchos-a-muchos entre Vehicle y HomepageSectionConfig.
/// Permite que un vehículo aparezca en múltiples secciones del homepage.
/// </summary>
public class VehicleHomepageSection
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID del vehículo
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// ID de la sección del homepage
    /// </summary>
    public Guid HomepageSectionConfigId { get; set; }

    /// <summary>
    /// Orden del vehículo dentro de la sección (menor = primero)
    /// Permite ordenar manualmente los vehículos en cada sección
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Si este vehículo está fijado (pinned) en la sección
    /// Los vehículos fijados siempre aparecen primero
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Fecha desde la cual el vehículo aparece en esta sección
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Fecha hasta la cual el vehículo aparece en esta sección
    /// Permite programar la visibilidad
    /// </summary>
    public DateTime? EndDate { get; set; }

    // ========================================
    // METADATOS
    // ========================================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public Vehicle Vehicle { get; set; } = null!;
    public HomepageSectionConfig HomepageSectionConfig { get; set; } = null!;
}
