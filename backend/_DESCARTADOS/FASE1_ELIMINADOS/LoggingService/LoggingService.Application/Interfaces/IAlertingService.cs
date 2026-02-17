using LoggingService.Domain;

namespace LoggingService.Application.Interfaces;

/// <summary>
/// Service for managing and evaluating alert rules
/// </summary>
public interface IAlertingService
{
    // Alert Rule Management
    Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task<AlertRule?> GetRuleAsync(string ruleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AlertRule>> GetAllRulesAsync(CancellationToken cancellationToken = default);
    Task<AlertRule> UpdateRuleAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(string ruleId, CancellationToken cancellationToken = default);
    Task<bool> EnableRuleAsync(string ruleId, CancellationToken cancellationToken = default);
    Task<bool> DisableRuleAsync(string ruleId, CancellationToken cancellationToken = default);

    // Alert Evaluation
    Task EvaluateRulesAsync(CancellationToken cancellationToken = default);
    Task<bool> EvaluateRuleAsync(string ruleId, CancellationToken cancellationToken = default);

    // Alert Management
    Task<Alert> CreateAlertAsync(Alert alert, CancellationToken cancellationToken = default);
    Task<Alert?> GetAlertAsync(string alertId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alert>> GetAlertsAsync(AlertStatus? status = null, DateTime? since = null, CancellationToken cancellationToken = default);
    Task<Alert> AcknowledgeAlertAsync(string alertId, string userId, CancellationToken cancellationToken = default);
    Task<Alert> ResolveAlertAsync(string alertId, string userId, string? notes = null, CancellationToken cancellationToken = default);

    // Alert Actions
    Task<List<AlertActionResult>> ExecuteActionsAsync(Alert alert, List<AlertAction> actions, CancellationToken cancellationToken = default);

    // Statistics
    Task<AlertStatistics> GetAlertStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about alerts
/// </summary>
public class AlertStatistics
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int AcknowledgedAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public Dictionary<string, int> AlertsByRule { get; set; } = new();
    public Dictionary<AlertSeverity, int> AlertsBySeverity { get; set; } = new();
    public double AverageTimeToAcknowledge { get; set; } // in minutes
    public double AverageTimeToResolve { get; set; } // in minutes
    public List<string> MostTriggeredRules { get; set; } = new();
}
