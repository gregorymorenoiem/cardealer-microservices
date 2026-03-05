using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Entities;

/// <summary>
/// Producto del catálogo publicitario OKLA — Margen 99%
/// Cada producto representa un tipo de visibilidad que el dealer puede comprar.
/// </summary>
public class AdvertisingProduct
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Slug único del producto (e.g., "listing-destacado", "top-3-busquedas")
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre visible del producto
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoría del producto
    /// </summary>
    public AdProductCategory Category { get; set; }
    
    /// <summary>
    /// Precio por día (USD). Null si no se vende por día.
    /// </summary>
    public decimal? PricePerDay { get; set; }
    
    /// <summary>
    /// Precio por semana (USD). Null si no se vende por semana.
    /// </summary>
    public decimal? PricePerWeek { get; set; }
    
    /// <summary>
    /// Precio por mes (USD). Null si no se vende por mes.
    /// </summary>
    public decimal? PricePerMonth { get; set; }
    
    /// <summary>
    /// Precio en OKLA Coins por día. Null si no aplica.
    /// </summary>
    public int? CoinsPricePerDay { get; set; }
    
    /// <summary>
    /// Precio en OKLA Coins por semana.
    /// </summary>
    public int? CoinsPricePerWeek { get; set; }
    
    /// <summary>
    /// Precio en OKLA Coins por mes.
    /// </summary>
    public int? CoinsPricePerMonth { get; set; }
    
    /// <summary>
    /// Costo operativo estimado para OKLA (USD)
    /// </summary>
    public decimal EstimatedCost { get; set; }
    
    /// <summary>
    /// Máximo de instancias simultáneas (e.g., max 3 banners homepage)
    /// </summary>
    public int? MaxSimultaneous { get; set; }
    
    /// <summary>
    /// Si aplica a un vehículo individual o al dealer completo
    /// </summary>
    public AdProductScope Scope { get; set; }
    
    /// <summary>
    /// Tipo de placement que genera al comprar
    /// </summary>
    public AdPlacementType? PlacementType { get; set; }
    
    /// <summary>
    /// Indica si es un bundle (paquete combinado)
    /// </summary>
    public bool IsBundle { get; set; }
    
    /// <summary>
    /// Para bundles: ahorro respecto a compra individual (USD)
    /// </summary>
    public decimal? BundleSavings { get; set; }
    
    /// <summary>
    /// Orden de display en el catálogo
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Si el producto está activo para la venta
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Categoría de producto publicitario
/// </summary>
public enum AdProductCategory
{
    Visibility = 0,         // Productos de visibilidad (destacado, top, etc.)
    Display = 1,            // Display ads (banners, showcase)
    DirectMarketing = 2,    // Marketing directo (email alerts, etc.)
    Bundle = 3              // Paquetes combinados
}

/// <summary>
/// Alcance del producto publicitario
/// </summary>
public enum AdProductScope
{
    PerVehicle = 0,         // Aplica a un vehículo específico
    PerDealer = 1,          // Aplica al dealer completo
    PerCampaign = 2         // Aplica a una campaña
}
