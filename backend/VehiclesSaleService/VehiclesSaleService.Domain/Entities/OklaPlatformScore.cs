namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// OKLA Platform Score — Puntuación de engagement del vehículo en la plataforma.
/// A diferencia del OKLA Quality Score (VIN history, mechanical, price vs market),
/// este score mide la ACTIVIDAD y TRANSPARENCIA acumulada del vehículo en OKLA.
/// 
/// SWITCHING COST: Este score es único a OKLA. Si el dealer se lleva el vehículo
/// a otra plataforma, pierde todo este historial acumulado, creando un costo real
/// de abandono.
/// 
/// Dimensiones:
/// D1: Antigüedad en plataforma (días publicado)
/// D2: Actividad de compradores (views, favorites, inquiries)
/// D3: Transparencia de precios (historial de cambios)
/// D4: Reputación del vendedor (reviews, response rate)
/// D5: Compromiso del dealer (fotos, descripción, actualización)
/// </summary>
public class OklaPlatformScore
{
    // ========================================
    // D1: ANTIGÜEDAD EN PLATAFORMA (25 pts max)
    // ========================================

    /// <summary>Días que el vehículo lleva publicado en OKLA</summary>
    public int DaysOnPlatform { get; set; }

    /// <summary>Puntos por antigüedad: 0-7d=5, 8-30d=15, 31-90d=25, >90d=25</summary>
    public int AntiquityPoints { get; set; }

    // ========================================
    // D2: ACTIVIDAD DE COMPRADORES (30 pts max)
    // ========================================

    /// <summary>Total de vistas del listado</summary>
    public int TotalViews { get; set; }

    /// <summary>Total de favoritos</summary>
    public int TotalFavorites { get; set; }

    /// <summary>Total de consultas/leads recibidos</summary>
    public int TotalInquiries { get; set; }

    /// <summary>Puntos por engagement de compradores</summary>
    public int BuyerEngagementPoints { get; set; }

    // ========================================
    // D3: TRANSPARENCIA DE PRECIOS (20 pts max)
    // ========================================

    /// <summary>Total de cambios de precio registrados</summary>
    public int PriceChangeCount { get; set; }

    /// <summary>Dirección general del precio (bajó, subió, estable)</summary>
    public PriceTrend PriceTrend { get; set; } = PriceTrend.Stable;

    /// <summary>Porcentaje total de variación desde el primer precio</summary>
    public decimal TotalPriceVariation { get; set; }

    /// <summary>Puntos por transparencia de precios</summary>
    public int PriceTransparencyPoints { get; set; }

    // ========================================
    // D4: REPUTACIÓN DEL VENDEDOR (15 pts max)
    // ========================================

    /// <summary>Rating del vendedor (0-5)</summary>
    public decimal? SellerRating { get; set; }

    /// <summary>Total de reviews del vendedor</summary>
    public int? SellerReviewCount { get; set; }

    /// <summary>Vendedor está verificado</summary>
    public bool SellerVerified { get; set; }

    /// <summary>Puntos por reputación del vendedor</summary>
    public int SellerReputationPoints { get; set; }

    // ========================================
    // D5: COMPROMISO / COMPLETITUD (10 pts max)
    // ========================================

    /// <summary>Número de fotos del vehículo</summary>
    public int PhotoCount { get; set; }

    /// <summary>Largo de la descripción en caracteres</summary>
    public int DescriptionLength { get; set; }

    /// <summary>Tiene VIN verificado</summary>
    public bool HasVerifiedVin { get; set; }

    /// <summary>Tiene historial CARFAX/VinAudit</summary>
    public bool HasExternalHistory { get; set; }

    /// <summary>Puntos por completitud del listado</summary>
    public int CompletenessPoints { get; set; }

    // ========================================
    // SCORE TOTAL
    // ========================================

    /// <summary>
    /// Score total de la plataforma (0-100).
    /// AntiquityPoints + BuyerEngagementPoints + PriceTransparencyPoints
    /// + SellerReputationPoints + CompletenessPoints
    /// </summary>
    public int TotalScore => AntiquityPoints + BuyerEngagementPoints
        + PriceTransparencyPoints + SellerReputationPoints + CompletenessPoints;

    /// <summary>Nivel cualitativo del score</summary>
    public PlatformScoreLevel Level => TotalScore switch
    {
        >= 85 => PlatformScoreLevel.Platinum,
        >= 65 => PlatformScoreLevel.Gold,
        >= 45 => PlatformScoreLevel.Silver,
        >= 25 => PlatformScoreLevel.Bronze,
        _ => PlatformScoreLevel.New
    };

    /// <summary>Descripción legible para el comprador</summary>
    public string LevelDescription => Level switch
    {
        PlatformScoreLevel.Platinum => "Vehículo con historial excepcional en OKLA — máxima transparencia",
        PlatformScoreLevel.Gold => "Vehículo con historial sólido en OKLA — alta confianza",
        PlatformScoreLevel.Silver => "Vehículo con buen historial en OKLA — confianza media",
        PlatformScoreLevel.Bronze => "Vehículo reciente en OKLA — historial en construcción",
        _ => "Vehículo nuevo en OKLA — aún sin historial de plataforma"
    };

    // ========================================
    // SWITCHING COST DATA
    // ========================================

    /// <summary>
    /// Resumen de lo que el dealer perdería si retira este vehículo de OKLA.
    /// Usado en la pantalla de confirmación de unpublish/delete.
    /// </summary>
    public SwitchingCostSummary SwitchingCost { get; set; } = new();
}

/// <summary>
/// Resumen de "costo de abandono" — lo que pierde el dealer si retira el vehículo.
/// Se muestra como warning antes de unpublish/delete.
/// </summary>
public class SwitchingCostSummary
{
    /// <summary>Días de presencia en la plataforma que se perderían</summary>
    public int DaysAccumulated { get; set; }

    /// <summary>Total de vistas acumuladas que se perderían</summary>
    public int ViewsAccumulated { get; set; }

    /// <summary>Total de consultas acumuladas que se perderían</summary>
    public int InquiriesAccumulated { get; set; }

    /// <summary>Total de favoritos acumulados que se perderían</summary>
    public int FavoritesAccumulated { get; set; }

    /// <summary>Historial de precios que se perdería (registros)</summary>
    public int PriceHistoryRecords { get; set; }

    /// <summary>Platform Score actual que se perdería</summary>
    public int CurrentPlatformScore { get; set; }

    /// <summary>Nivel que se perdería</summary>
    public PlatformScoreLevel CurrentLevel { get; set; }

    /// <summary>Mensaje de advertencia para el dealer</summary>
    public string WarningMessage => DaysAccumulated > 0
        ? $"⚠️ Este vehículo lleva {DaysAccumulated} días en OKLA con {ViewsAccumulated} vistas, " +
          $"{InquiriesAccumulated} consultas y {FavoritesAccumulated} favoritos. " +
          $"Si lo retiras, perderás este historial acumulado y el Platform Score nivel {CurrentLevel}. " +
          $"Esta información NO es transferible a otra plataforma."
        : "Este vehículo aún no tiene historial acumulado en OKLA.";

    /// <summary>¿El vehículo tiene switching cost significativo?</summary>
    public bool HasSignificantCost =>
        DaysAccumulated > 7 || ViewsAccumulated > 10 || InquiriesAccumulated > 0;
}

/// <summary>
/// Niveles del Platform Score
/// </summary>
public enum PlatformScoreLevel
{
    New = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4
}

/// <summary>
/// Tendencia del precio
/// </summary>
public enum PriceTrend
{
    Stable = 0,
    Decreasing = 1,
    Increasing = 2,
    Volatile = 3
}
