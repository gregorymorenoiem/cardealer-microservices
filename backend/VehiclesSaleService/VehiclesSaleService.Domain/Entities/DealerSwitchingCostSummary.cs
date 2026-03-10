namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Costo de migración a nivel dealer — agrega TODAS las métricas
/// que el dealer perdería si cancela su cuenta OKLA.
/// Combina: vehículos, reviews, badges, engagement, platform score.
/// 
/// Se muestra en:
/// 1. Dashboard del dealer como "valor acumulado en OKLA"
/// 2. Pantalla de cancelación de cuenta como advertencia
/// 3. API para que el frontend construya la UI de retención
/// 
/// SWITCHING COST: Esta información es exclusiva de OKLA y
/// NO es transferible a ninguna otra plataforma.
/// </summary>
public class DealerSwitchingCostSummary
{
    /// <summary>ID del dealer</summary>
    public Guid DealerId { get; set; }

    /// <summary>Nombre del dealer</summary>
    public string DealerName { get; set; } = string.Empty;

    // ========================================
    // REVIEWS & REPUTACIÓN (no portables)
    // ========================================

    /// <summary>Total de reviews acumuladas en OKLA</summary>
    public int TotalReviews { get; set; }

    /// <summary>Reviews de compra verificada</summary>
    public int VerifiedPurchaseReviews { get; set; }

    /// <summary>Rating promedio del dealer</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Distribución de estrellas (5→N, 4→N, 3→N, 2→N, 1→N)</summary>
    public Dictionary<int, int> RatingDistribution { get; set; } = new();

    /// <summary>Total de badges/insignias ganadas</summary>
    public int TotalBadges { get; set; }

    /// <summary>Nombres de los badges activos</summary>
    public List<string> ActiveBadges { get; set; } = new();

    /// <summary>Porcentaje de reviews positivas (4-5 estrellas)</summary>
    public decimal PositiveReviewPercentage { get; set; }

    // ========================================
    // VEHÍCULOS & PLATFORM SCORE
    // ========================================

    /// <summary>Total de vehículos activos en OKLA</summary>
    public int ActiveVehicles { get; set; }

    /// <summary>Total de vehículos alguna vez publicados</summary>
    public int TotalVehiclesEverPublished { get; set; }

    /// <summary>Sumatoria de vistas en todos los vehículos</summary>
    public int TotalViewsAcrossVehicles { get; set; }

    /// <summary>Sumatoria de consultas en todos los vehículos</summary>
    public int TotalInquiriesAcrossVehicles { get; set; }

    /// <summary>Sumatoria de favoritos en todos los vehículos</summary>
    public int TotalFavoritesAcrossVehicles { get; set; }

    /// <summary>Total de registros de historial de precio</summary>
    public int TotalPriceHistoryRecords { get; set; }

    /// <summary>Promedio del Platform Score de sus vehículos activos</summary>
    public decimal AveragePlatformScore { get; set; }

    // ========================================
    // LEADS & VENTAS
    // ========================================

    /// <summary>Total de leads recibidos en OKLA</summary>
    public int TotalLeadsReceived { get; set; }

    /// <summary>Total de ventas completadas en OKLA</summary>
    public int TotalSalesCompleted { get; set; }

    // ========================================
    // TIEMPO EN PLATAFORMA
    // ========================================

    /// <summary>Fecha de registro del dealer en OKLA</summary>
    public DateTime? MemberSince { get; set; }

    /// <summary>Días como miembro de OKLA</summary>
    public int DaysAsMember { get; set; }

    // ========================================
    // MENSAJES DE ADVERTENCIA
    // ========================================

    /// <summary>Mensaje principal de advertencia al cancelar</summary>
    public string CancellationWarning => GenerateWarning();

    /// <summary>¿Tiene costo significativo de migración?</summary>
    public bool HasSignificantSwitchingCost =>
        TotalReviews >= 3 || DaysAsMember >= 30 || TotalViewsAcrossVehicles >= 50
        || TotalSalesCompleted >= 1;

    /// <summary>Nivel de retención (cuánto esfuerzo poner en retener)</summary>
    public RetentionLevel RetentionPriority => TotalReviews switch
    {
        >= 50 => RetentionLevel.Critical,     // Dealer con mucha reputación
        >= 20 => RetentionLevel.High,
        >= 5 => RetentionLevel.Medium,
        _ => HasSignificantSwitchingCost ? RetentionLevel.Low : RetentionLevel.None
    };

    private string GenerateWarning()
    {
        if (!HasSignificantSwitchingCost)
            return "Tu cuenta OKLA aún no tiene historial significativo acumulado.";

        var parts = new List<string>();

        if (TotalReviews > 0)
            parts.Add($"⭐ {TotalReviews} reseñas verificadas (rating {AverageRating:F1}★)");

        if (TotalBadges > 0)
            parts.Add($"🏆 {TotalBadges} insignias ganadas ({string.Join(", ", ActiveBadges.Take(3))})");

        if (TotalViewsAcrossVehicles > 0)
            parts.Add($"👀 {TotalViewsAcrossVehicles:N0} vistas acumuladas");

        if (TotalInquiriesAcrossVehicles > 0)
            parts.Add($"💬 {TotalInquiriesAcrossVehicles:N0} consultas de compradores");

        if (TotalSalesCompleted > 0)
            parts.Add($"🤝 {TotalSalesCompleted} ventas completadas");

        if (DaysAsMember > 0)
            parts.Add($"📅 {DaysAsMember} días como miembro");

        return "⚠️ Si cancelas tu cuenta OKLA, perderás:\n\n"
            + string.Join("\n", parts)
            + "\n\n🚫 Esta información es EXCLUSIVA de OKLA y NO es transferible a otra plataforma."
            + "\n💡 Tu reputación acumulada es tu mayor ventaja competitiva en el marketplace.";
    }
}

/// <summary>
/// Nivel de prioridad de retención del dealer
/// </summary>
public enum RetentionLevel
{
    /// <summary>Sin historial significativo</summary>
    None = 0,

    /// <summary>Algo de historial — retención básica</summary>
    Low = 1,

    /// <summary>Historial medio — retención activa</summary>
    Medium = 2,

    /// <summary>Buen historial — retención prioritaria</summary>
    High = 3,

    /// <summary>Historial excepcional — retención crítica (llamar al dealer)</summary>
    Critical = 4
}
