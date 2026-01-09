using CarDealer.Shared.Domain.Base;

namespace ReviewService.Domain.Entities;

/// &lt;summary&gt;
/// Estadísticas agregadas de reviews para un vendedor
/// Para performance y evitar cálculos en tiempo real
/// &lt;/summary&gt;
public class ReviewSummary : BaseEntity&lt;Guid&gt;
{
    /// &lt;summary&gt;
    /// ID del vendedor
    /// &lt;/summary&gt;
    public Guid SellerId { get; set; }

    /// &lt;summary&gt;
    /// Total de reviews recibidas
    /// &lt;/summary&gt;
    public int TotalReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Rating promedio (1.0 - 5.0)
    /// &lt;/summary&gt;
    public decimal AverageRating { get; set; } = 0;

    /// &lt;summary&gt;
    /// Cantidad de reviews de 5 estrellas
    /// &lt;/summary&gt;
    public int FiveStarReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Cantidad de reviews de 4 estrellas
    /// &lt;/summary&gt;
    public int FourStarReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Cantidad de reviews de 3 estrellas
    /// &lt;/summary&gt;
    public int ThreeStarReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Cantidad de reviews de 2 estrellas
    /// &lt;/summary&gt;
    public int TwoStarReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Cantidad de reviews de 1 estrella
    /// &lt;/summary&gt;
    public int OneStarReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Fecha de la última review recibida
    /// &lt;/summary&gt;
    public DateTime? LastReviewDate { get; set; }

    /// &lt;summary&gt;
    /// Porcentaje de reviews positivas (4-5 estrellas)
    /// &lt;/summary&gt;
    public decimal PositivePercentage { get; set; } = 0;

    /// &lt;summary&gt;
    /// Total de compras verificadas que tienen review
    /// &lt;/summary&gt;
    public int VerifiedPurchaseReviews { get; set; } = 0;

    /// &lt;summary&gt;
    /// Calcular distribución de ratings
    /// &lt;/summary&gt;
    public Dictionary&lt;int, int&gt; GetRatingDistribution()
    {
        return new Dictionary&lt;int, int&gt;
        {
            { 5, FiveStarReviews },
            { 4, FourStarReviews },
            { 3, ThreeStarReviews },
            { 2, TwoStarReviews },
            { 1, OneStarReviews }
        };
    }

    /// &lt;summary&gt;
    /// Recalcular todas las métricas basadas en las reviews actuales
    /// &lt;/summary&gt;
    public void RecalculateMetrics(IEnumerable&lt;Review&gt; reviews)
    {
        var reviewList = reviews.Where(r =&gt; r.IsApproved).ToList();
        
        TotalReviews = reviewList.Count;
        
        if (TotalReviews &gt; 0)
        {
            AverageRating = (decimal)reviewList.Average(r =&gt; r.Rating);
            
            FiveStarReviews = reviewList.Count(r =&gt; r.Rating == 5);
            FourStarReviews = reviewList.Count(r =&gt; r.Rating == 4);
            ThreeStarReviews = reviewList.Count(r =&gt; r.Rating == 3);
            TwoStarReviews = reviewList.Count(r =&gt; r.Rating == 2);
            OneStarReviews = reviewList.Count(r =&gt; r.Rating == 1);
            
            PositivePercentage = (decimal)(FiveStarReviews + FourStarReviews) / TotalReviews * 100;
            VerifiedPurchaseReviews = reviewList.Count(r =&gt; r.IsVerifiedPurchase);
            LastReviewDate = reviewList.Max(r =&gt; r.CreatedAt);
        }
        else
        {
            // Reset valores
            AverageRating = 0;
            FiveStarReviews = FourStarReviews = ThreeStarReviews = TwoStarReviews = OneStarReviews = 0;
            PositivePercentage = 0;
            VerifiedPurchaseReviews = 0;
            LastReviewDate = null;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
}