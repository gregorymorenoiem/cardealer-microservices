namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Statistical analysis results for an experiment variant
/// </summary>
public class ExperimentResult
{
    public Guid VariantId { get; set; }
    public string VariantKey { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public bool IsControl { get; set; }

    // Sample size
    public int TotalAssignments { get; set; }
    public int TotalExposures { get; set; }
    public int TotalConversions { get; set; }

    // Primary metric statistics
    public double ConversionRate { get; set; }
    public double StandardError { get; set; }
    public double ConfidenceIntervalLower { get; set; }
    public double ConfidenceIntervalUpper { get; set; }

    // Comparison with control
    public double? RelativeLift { get; set; } // % improvement vs control
    public double? AbsoluteDifference { get; set; }
    public double? PValue { get; set; }
    public double? ZScore { get; set; }
    public bool? IsStatisticallySignificant { get; set; }

    // Secondary metrics
    public Dictionary<string, double> SecondaryMetrics { get; set; } = new();

    // Revenue metrics (if applicable)
    public double? AverageRevenuePerUser { get; set; }
    public double? TotalRevenue { get; set; }

    // Engagement metrics
    public double? AverageTimeSpent { get; set; }
    public double? BounceRate { get; set; }

    /// <summary>
    /// Check if variant is a winner (better than control with statistical significance)
    /// </summary>
    public bool IsWinner()
    {
        return IsStatisticallySignificant == true && RelativeLift > 0;
    }

    /// <summary>
    /// Get confidence level as percentage
    /// </summary>
    public double GetConfidencePercent()
    {
        if (!PValue.HasValue) return 0;
        return (1 - PValue.Value) * 100;
    }
}

/// <summary>
/// Complete analysis results for an experiment
/// </summary>
public class ExperimentAnalysis
{
    public Guid ExperimentId { get; set; }
    public string ExperimentKey { get; set; } = string.Empty;
    public string ExperimentName { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    // Experiment status
    public int TotalParticipants { get; set; }
    public int TotalExposures { get; set; }
    public int DaysRunning { get; set; }
    public bool HasReachedMinSampleSize { get; set; }

    // Results by variant
    public List<ExperimentResult> VariantResults { get; set; } = new();

    // Overall analysis
    public Guid? RecommendedWinnerId { get; set; }
    public double? WinnerConfidence { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Insights { get; set; } = new();

    // Statistical power
    public double? StatisticalPower { get; set; }
    public int? EstimatedDaysToSignificance { get; set; }

    /// <summary>
    /// Get the variant with the best conversion rate
    /// </summary>
    public ExperimentResult? GetBestVariant()
    {
        return VariantResults
            .Where(v => v.IsStatisticallySignificant == true)
            .OrderByDescending(v => v.ConversionRate)
            .FirstOrDefault();
    }

    /// <summary>
    /// Check if experiment has a clear winner
    /// </summary>
    public bool HasClearWinner()
    {
        var bestVariant = GetBestVariant();
        return bestVariant != null &&
               bestVariant.IsStatisticallySignificant == true &&
               bestVariant.RelativeLift > 0;
    }
}
