using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Entities;

/// <summary>
/// Tracks usage statistics per provider
/// </summary>
public class ProviderUsageStats
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The provider these stats are for
    /// </summary>
    public BackgroundRemovalProvider Provider { get; set; }
    
    /// <summary>
    /// Date for these stats (UTC)
    /// </summary>
    public DateOnly Date { get; set; }
    
    /// <summary>
    /// Total requests made
    /// </summary>
    public int TotalRequests { get; set; }
    
    /// <summary>
    /// Successful requests
    /// </summary>
    public int SuccessfulRequests { get; set; }
    
    /// <summary>
    /// Failed requests
    /// </summary>
    public int FailedRequests { get; set; }
    
    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public long TotalProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Total bytes processed (input)
    /// </summary>
    public long TotalBytesProcessed { get; set; }
    
    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTimeMs => TotalRequests > 0 
        ? (double)TotalProcessingTimeMs / TotalRequests 
        : 0;
    
    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate => TotalRequests > 0 
        ? (double)SuccessfulRequests / TotalRequests * 100 
        : 0;
    
    /// <summary>
    /// Estimated API cost (in USD)
    /// </summary>
    public decimal EstimatedCostUsd { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
