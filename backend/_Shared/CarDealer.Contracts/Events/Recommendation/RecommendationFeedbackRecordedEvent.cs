using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Recommendation;

/// <summary>
/// Event published when a user provides feedback on a vehicle recommendation.
/// Consumed by DealerAnalyticsService for engagement metrics and by RecoAgent's
/// own learning pipeline to improve future recommendations.
/// </summary>
public class RecommendationFeedbackRecordedEvent : EventBase
{
    public override string EventType => "recommendation.feedback.recorded";

    /// <summary>User who gave the feedback.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Vehicle that received the feedback.</summary>
    public string VehicleId { get; set; } = string.Empty;

    /// <summary>Feedback type: thumbs_up, thumbs_down, dismiss, click.</summary>
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>Recommendation session (correlates with the AI generation request).</summary>
    public string? SessionId { get; set; }

    /// <summary>Position of the vehicle in the recommendation list (1-based).</summary>
    public int? Position { get; set; }

    /// <summary>Whether this was a positive signal (thumbs_up, click) vs negative (thumbs_down, dismiss).</summary>
    public bool IsPositive { get; set; }

    /// <summary>UTC timestamp when the feedback was recorded.</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
