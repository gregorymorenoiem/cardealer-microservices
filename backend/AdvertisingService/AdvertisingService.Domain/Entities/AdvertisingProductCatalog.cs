using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Entities;

/// <summary>
/// Catálogo estático de productos publicitarios OKLA.
/// Todos los productos con margen ~99% (costo operativo prácticamente cero).
/// </summary>
public static class AdvertisingProductCatalog
{
    // ═══════════════════════════════════════════════════════════════
    // Deterministic IDs for seeding/testing
    // ═══════════════════════════════════════════════════════════════
    public static readonly Guid ListingDestacadoId   = new("ad000000-0000-0000-0000-000000000001");
    public static readonly Guid Top3BusquedasId      = new("ad000000-0000-0000-0000-000000000002");
    public static readonly Guid OfertaDelDiaId       = new("ad000000-0000-0000-0000-000000000003");
    public static readonly Guid BannerHomepageId     = new("ad000000-0000-0000-0000-000000000004");
    public static readonly Guid DealerShowcaseId     = new("ad000000-0000-0000-0000-000000000005");
    public static readonly Guid PackAlertasEmailId   = new("ad000000-0000-0000-0000-000000000006");
    public static readonly Guid PaqueteVisibilidadId = new("ad000000-0000-0000-0000-000000000007");

    /// <summary>
    /// Returns the full product catalog (7 products)
    /// </summary>
    public static List<AdvertisingProduct> GetAll() => new()
    {
        // ── 1. Listing Destacado (por vehículo) ──
        new AdvertisingProduct
        {
            Id = ListingDestacadoId,
            Slug = "listing-destacado",
            Name = "Listing Destacado",
            Description = "Badge dorado, prioridad en resultados, icono especial. Costo OKLA: $0.",
            Category = AdProductCategory.Visibility,
            PricePerDay = 0.50m,
            PricePerWeek = 2.50m,
            PricePerMonth = 6.00m,
            CoinsPricePerDay = 50,
            CoinsPricePerWeek = 250,
            CoinsPricePerMonth = 600,
            EstimatedCost = 0m,
            Scope = AdProductScope.PerVehicle,
            PlacementType = AdPlacementType.FeaturedSpot,
            IsBundle = false,
            DisplayOrder = 1,
            IsActive = true
        },

        // ── 2. Posición Top 3 en búsquedas ──
        new AdvertisingProduct
        {
            Id = Top3BusquedasId,
            Slug = "top-3-busquedas",
            Name = "Posición Top 3 en búsquedas",
            Description = "Aparece entre los 3 primeros resultados para búsquedas del modelo. Costo OKLA: $0.",
            Category = AdProductCategory.Visibility,
            PricePerDay = 1.50m,
            PricePerWeek = 7.00m,
            PricePerMonth = 20.00m,
            CoinsPricePerDay = 150,
            CoinsPricePerWeek = 700,
            CoinsPricePerMonth = 2000,
            EstimatedCost = 0m,
            Scope = AdProductScope.PerVehicle,
            PlacementType = AdPlacementType.PremiumSpot,
            IsBundle = false,
            DisplayOrder = 2,
            IsActive = true
        },

        // ── 3. Oferta del Día (homepage + email blast) ──
        new AdvertisingProduct
        {
            Id = OfertaDelDiaId,
            Slug = "oferta-del-dia",
            Name = "Oferta del Día",
            Description = "Vehículo en sección Oferta del Día. Envío a subscribers de alertas por modelo. Costo OKLA: ~$0.10.",
            Category = AdProductCategory.Display,
            PricePerDay = 15.00m,
            PricePerWeek = null,
            PricePerMonth = null,
            CoinsPricePerDay = 1500,
            EstimatedCost = 0.10m,
            Scope = AdProductScope.PerVehicle,
            PlacementType = AdPlacementType.PremiumSpot,
            IsBundle = false,
            DisplayOrder = 3,
            IsActive = true
        },

        // ── 4. Banner Homepage (max. 3 simultáneos) ──
        new AdvertisingProduct
        {
            Id = BannerHomepageId,
            Slug = "banner-homepage",
            Name = "Banner Homepage",
            Description = "Banner 728x90 en homepage. Máximo 3 dealers simultáneos con rotación equitativa.",
            Category = AdProductCategory.Display,
            PricePerDay = null,
            PricePerWeek = null,
            PricePerMonth = 120.00m,
            CoinsPricePerMonth = 12000,
            EstimatedCost = 0m,
            MaxSimultaneous = 3,
            Scope = AdProductScope.PerDealer,
            PlacementType = AdPlacementType.PremiumSpot,
            IsBundle = false,
            DisplayOrder = 4,
            IsActive = true
        },

        // ── 5. Dealer Showcase (directorio destacado) ──
        new AdvertisingProduct
        {
            Id = DealerShowcaseId,
            Slug = "dealer-showcase",
            Name = "Dealer Showcase",
            Description = "El dealer aparece primero en el directorio de dealers de OKLA.",
            Category = AdProductCategory.Display,
            PricePerDay = null,
            PricePerWeek = null,
            PricePerMonth = 50.00m,
            CoinsPricePerMonth = 5000,
            EstimatedCost = 0m,
            Scope = AdProductScope.PerDealer,
            IsBundle = false,
            DisplayOrder = 5,
            IsActive = true
        },

        // ── 6. Pack Alertas Email (por modelo/segmento) ──
        new AdvertisingProduct
        {
            Id = PackAlertasEmailId,
            Slug = "pack-alertas-email",
            Name = "Pack Alertas Email",
            Description = "Vehículos del dealer incluidos en alertas automáticas a compradores por modelo.",
            Category = AdProductCategory.DirectMarketing,
            PricePerDay = null,
            PricePerWeek = null,
            PricePerMonth = 35.00m,
            CoinsPricePerMonth = 3500,
            EstimatedCost = 0.10m,
            Scope = AdProductScope.PerDealer,
            IsBundle = false,
            DisplayOrder = 6,
            IsActive = true
        },

        // ── 7. PAQUETE VISIBILIDAD TOTAL (bundle) ──
        new AdvertisingProduct
        {
            Id = PaqueteVisibilidadId,
            Slug = "paquete-visibilidad-total",
            Name = "Paquete Visibilidad Total",
            Description = "Banner + Showcase + 10 destacados + Pack alertas. Ahorro vs. individual: $82. Margen: $174.90.",
            Category = AdProductCategory.Bundle,
            PricePerDay = null,
            PricePerWeek = null,
            PricePerMonth = 175.00m,
            CoinsPricePerMonth = 17500,
            EstimatedCost = 0.10m,
            Scope = AdProductScope.PerDealer,
            IsBundle = true,
            BundleSavings = 82.00m,
            DisplayOrder = 7,
            IsActive = true
        }
    };

    /// <summary>
    /// Get a product by its slug
    /// </summary>
    public static AdvertisingProduct? GetBySlug(string slug)
        => GetAll().FirstOrDefault(p => p.Slug == slug);

    /// <summary>
    /// Get a product by its ID
    /// </summary>
    public static AdvertisingProduct? GetById(Guid id)
        => GetAll().FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// Get products by category
    /// </summary>
    public static List<AdvertisingProduct> GetByCategory(AdProductCategory category)
        => GetAll().Where(p => p.Category == category).ToList();
}
