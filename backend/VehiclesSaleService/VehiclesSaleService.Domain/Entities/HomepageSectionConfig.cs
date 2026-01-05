namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Configuración de una sección del homepage.
/// Define qué secciones existen, cuántos items mostrar, y su orden.
/// </summary>
public class HomepageSectionConfig
{
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre visible de la sección (ej: "Carousel Principal", "Sedanes", "SUVs")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único para código (ej: "carousel", "sedanes", "suvs")
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la sección
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Orden de visualización en el homepage (menor = primero)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Cantidad máxima de vehículos a mostrar en esta sección
    /// </summary>
    public int MaxItems { get; set; } = 10;

    /// <summary>
    /// Si la sección está activa y visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Icono de la sección (nombre de icono o URL)
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color de acento para la sección (ej: "blue", "amber", "emerald")
    /// </summary>
    public string AccentColor { get; set; } = "blue";

    /// <summary>
    /// URL de destino del botón "Ver todos"
    /// </summary>
    public string? ViewAllHref { get; set; }

    /// <summary>
    /// Tipo de layout para la sección: "carousel", "grid", "list"
    /// </summary>
    public SectionLayoutType LayoutType { get; set; } = SectionLayoutType.Carousel;

    /// <summary>
    /// Subtítulo de la sección
    /// </summary>
    public string? Subtitle { get; set; }

    // ========================================
    // METADATOS
    // ========================================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ========================================
    // NAVEGACIÓN
    // ========================================
    /// <summary>
    /// Vehículos asignados a esta sección
    /// </summary>
    public ICollection<VehicleHomepageSection> VehicleSections { get; set; } = new List<VehicleHomepageSection>();
}

/// <summary>
/// Tipo de layout para las secciones del homepage
/// </summary>
public enum SectionLayoutType
{
    /// <summary>Carousel horizontal con navegación</summary>
    Carousel = 0,

    /// <summary>Grid de tarjetas</summary>
    Grid = 1,

    /// <summary>Lista vertical</summary>
    List = 2,

    /// <summary>Hero carousel (sección principal grande)</summary>
    Hero = 3
}
