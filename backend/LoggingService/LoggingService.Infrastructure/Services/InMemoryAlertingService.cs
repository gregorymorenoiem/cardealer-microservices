using LoggingService.Application.Interfaces;
using LoggingService.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LoggingService.Infrastructure.Services;

/// <summary>
/// In-memory implementation of alerting service
/// </summary>
public class InMemoryAlertingService : IAlertingService
{
    private readonly ConcurrentDictionary<string, AlertRule> _rules = new();
    private readonly ConcurrentDictionary<string, Alert> _alerts = new();
    private readonly ILogAggregator _logAggregator;
    private readonly ILogger<InMemoryAlertingService> _logger;

    public InMemoryAlertingService(ILogAggregator logAggregator, ILogger<InMemoryAlertingService> logger)
    {
        _logAggregator = logAggregator;
        _logger = logger;
    }

    // Alert Rule Management

    public Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        rule.CreatedAt = DateTime.UtcNow;
        _rules[rule.Id] = rule;
        _logger.LogInformation("Created alert rule: {RuleName} ({RuleId})", rule.Name, rule.Id);
        return Task.FromResult(rule);
    }

    public Task<AlertRule?> GetRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        _rules.TryGetValue(ruleId, out var rule);
        return Task.FromResult(rule);
    }

    public Task<IEnumerable<AlertRule>> GetAllRulesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<AlertRule>>(_rules.Values.ToList());
    }

    public Task<AlertRule> UpdateRuleAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _rules[rule.Id] = rule;
        _logger.LogInformation("Updated alert rule: {RuleName} ({RuleId})", rule.Name, rule.Id);
        return Task.FromResult(rule);
    }

    public Task<bool> DeleteRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        var removed = _rules.TryRemove(ruleId, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted alert rule: {RuleId}", ruleId);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> EnableRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        if (_rules.TryGetValue(ruleId, out var rule))
        {
            rule.IsEnabled = true;
            _logger.LogInformation("Enabled alert rule: {RuleId}", ruleId);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DisableRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        if (_rules.TryGetValue(ruleId, out var rule))
        {
            rule.IsEnabled = false;
            _logger.LogInformation("Disabled alert rule: {RuleId}", ruleId);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    // Alert Evaluation

    public async Task EvaluateRulesAsync(CancellationToken cancellationToken = default)
    {
        var enabledRules = _rules.Values.Where(r => r.IsEnabled && r.CanTrigger()).ToList();

        _logger.LogInformation("Evaluating {Count} enabled alert rules", enabledRules.Count);

        foreach (var rule in enabledRules)
        {
            try
            {
                await EvaluateRuleAsync(rule.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating rule {RuleId}: {RuleName}", rule.Id, rule.Name);
            }
        }
    }

    public async Task<bool> EvaluateRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        if (!_rules.TryGetValue(ruleId, out var rule))
        {
            return false;
        }

        if (!rule.IsEnabled || !rule.CanTrigger())
        {
            return false;
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - rule.EvaluationWindow;

        var filter = new LogFilter
        {
            StartDate = startTime,
            EndDate = endTime,
            ServiceName = rule.Condition.ServiceName,
            MinLevel = rule.Condition.MinLevel
        };

        var logs = (await _logAggregator.QueryLogsAsync(filter, cancellationToken)).ToList();

        bool shouldTrigger = false;
        var context = new Dictionary<string, object>();

        switch (rule.Condition.Type)
        {
            case ConditionType.ErrorCount:
                var errorCount = logs.Count(l => l.IsError());
                shouldTrigger = errorCount >= (rule.Condition.ErrorCountThreshold ?? 0);
                context["ErrorCount"] = errorCount;
                context["Threshold"] = rule.Condition.ErrorCountThreshold ?? 0;
                break;

            case ConditionType.ErrorRate:
                var totalLogs = logs.Count;
                var errors = logs.Count(l => l.IsError());
                var errorRate = totalLogs > 0 ? (double)errors / totalLogs * 100 : 0;
                shouldTrigger = errorRate >= (rule.Condition.ErrorRateThreshold ?? 0);
                context["ErrorRate"] = errorRate;
                context["Threshold"] = rule.Condition.ErrorRateThreshold ?? 0;
                context["ErrorCount"] = errors;
                context["TotalLogs"] = totalLogs;
                break;

            case ConditionType.SpecificError:
                if (!string.IsNullOrEmpty(rule.Condition.MessagePattern))
                {
                    var matchingLogs = logs.Where(l => l.Message.Contains(rule.Condition.MessagePattern, StringComparison.OrdinalIgnoreCase)).ToList();
                    shouldTrigger = matchingLogs.Any();
                    context["MatchCount"] = matchingLogs.Count;
                    context["Pattern"] = rule.Condition.MessagePattern;
                }
                break;

            case ConditionType.ServiceDown:
                shouldTrigger = logs.Count == 0 || logs.All(l => l.IsError());
                context["LogCount"] = logs.Count;
                break;
        }

        if (shouldTrigger)
        {
            var alert = new Alert
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                Title = rule.Name,
                Message = BuildAlertMessage(rule, context),
                Severity = DetermineAlertSeverity(rule.Condition.Type),
                Context = context,
                RelatedLogIds = logs.Take(10).Select(l => l.Id).ToList()
            };

            await CreateAlertAsync(alert, cancellationToken);

            // Execute actions
            var actionResults = await ExecuteActionsAsync(alert, rule.Actions, cancellationToken);
            alert.ActionResults = actionResults;

            rule.MarkTriggered();

            _logger.LogWarning("Alert triggered: {AlertTitle} (Rule: {RuleName})", alert.Title, rule.Name);

            return true;
        }

        return false;
    }

    // Alert Management

    public Task<Alert> CreateAlertAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _alerts[alert.Id] = alert;
        _logger.LogInformation("Created alert: {AlertTitle} ({AlertId})", alert.Title, alert.Id);
        return Task.FromResult(alert);
    }

    public Task<Alert?> GetAlertAsync(string alertId, CancellationToken cancellationToken = default)
    {
        _alerts.TryGetValue(alertId, out var alert);
        return Task.FromResult(alert);
    }

    public Task<IEnumerable<Alert>> GetAlertsAsync(AlertStatus? status = null, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = _alerts.Values.AsEnumerable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(a => a.TriggeredAt >= since.Value);
        }

        return Task.FromResult(query.OrderByDescending(a => a.TriggeredAt).AsEnumerable());
    }

    public Task<Alert> AcknowledgeAlertAsync(string alertId, string userId, CancellationToken cancellationToken = default)
    {
        if (!_alerts.TryGetValue(alertId, out var alert))
        {
            throw new KeyNotFoundException($"Alert {alertId} not found");
        }

        alert.Acknowledge(userId);
        _logger.LogInformation("Alert {AlertId} acknowledged by {UserId}", alertId, userId);

        return Task.FromResult(alert);
    }

    public Task<Alert> ResolveAlertAsync(string alertId, string userId, string? notes = null, CancellationToken cancellationToken = default)
    {
        if (!_alerts.TryGetValue(alertId, out var alert))
        {
            throw new KeyNotFoundException($"Alert {alertId} not found");
        }

        alert.Resolve(userId, notes);
        _logger.LogInformation("Alert {AlertId} resolved by {UserId}", alertId, userId);

        return Task.FromResult(alert);
    }

    // Alert Actions

    public async Task<List<AlertActionResult>> ExecuteActionsAsync(Alert alert, List<AlertAction> actions, CancellationToken cancellationToken = default)
    {
        var results = new List<AlertActionResult>();

        foreach (var action in actions.OrderByDescending(a => a.Priority))
        {
            try
            {
                var result = await ExecuteActionAsync(alert, action, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action {ActionType} for alert {AlertId}", action.Type, alert.Id);
                results.Add(new AlertActionResult
                {
                    ActionId = action.Id,
                    ActionType = action.Type,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results;
    }

    private async Task<AlertActionResult> ExecuteActionAsync(Alert alert, AlertAction action, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing action {ActionType} for alert {AlertId}", action.Type, alert.Id);

        // Simulate action execution
        await Task.Delay(100, cancellationToken);

        var result = new AlertActionResult
        {
            ActionId = action.Id,
            ActionType = action.Type,
            Success = true,
            Metadata = new Dictionary<string, object>
            {
                ["AlertId"] = alert.Id,
                ["AlertTitle"] = alert.Title,
                ["Severity"] = alert.Severity.ToString()
            }
        };

        switch (action.Type)
        {
            case ActionType.Email:
                _logger.LogInformation("Would send email to: {Recipients}",
                    action.Configuration.GetValueOrDefault("Recipients", "N/A"));
                result.Metadata["Recipients"] = action.Configuration.GetValueOrDefault("Recipients", "");
                break;

            case ActionType.Webhook:
                _logger.LogInformation("Would call webhook: {Url}",
                    action.Configuration.GetValueOrDefault("Url", "N/A"));
                result.Metadata["Url"] = action.Configuration.GetValueOrDefault("Url", "");
                break;

            case ActionType.Slack:
                _logger.LogInformation("Would send Slack message to: {Channel}",
                    action.Configuration.GetValueOrDefault("Channel", "N/A"));
                result.Metadata["Channel"] = action.Configuration.GetValueOrDefault("Channel", "");
                break;

            case ActionType.Teams:
                _logger.LogInformation("Would send Teams message to: {WebhookUrl}",
                    action.Configuration.GetValueOrDefault("WebhookUrl", "N/A"));
                break;

            case ActionType.PagerDuty:
                _logger.LogInformation("Would create PagerDuty incident");
                break;

            case ActionType.CreateTicket:
                _logger.LogInformation("Would create ticket in: {System}",
                    action.Configuration.GetValueOrDefault("System", "N/A"));
                break;
        }

        return result;
    }

    // Statistics

    public Task<AlertStatistics> GetAlertStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _alerts.Values.AsEnumerable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.TriggeredAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.TriggeredAt <= endDate.Value);
        }

        var alertList = query.ToList();

        var acknowledgedAlerts = alertList.Where(a => a.IsAcknowledged() || a.IsResolved()).ToList();
        var resolvedAlerts = alertList.Where(a => a.IsResolved()).ToList();

        var stats = new AlertStatistics
        {
            TotalAlerts = alertList.Count,
            OpenAlerts = alertList.Count(a => a.IsOpen()),
            AcknowledgedAlerts = alertList.Count(a => a.IsAcknowledged()),
            ResolvedAlerts = resolvedAlerts.Count,
            AlertsByRule = alertList.GroupBy(a => a.RuleName).ToDictionary(g => g.Key, g => g.Count()),
            AlertsBySeverity = alertList.GroupBy(a => a.Severity).ToDictionary(g => g.Key, g => g.Count()),
            AverageTimeToAcknowledge = acknowledgedAlerts.Any()
                ? acknowledgedAlerts.Average(a => a.GetTimeToAcknowledge()?.TotalMinutes ?? 0)
                : 0,
            AverageTimeToResolve = resolvedAlerts.Any()
                ? resolvedAlerts.Average(a => a.GetTimeToResolve()?.TotalMinutes ?? 0)
                : 0,
            MostTriggeredRules = alertList.GroupBy(a => a.RuleName)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList()
        };

        return Task.FromResult(stats);
    }

    // Helper methods

    private string BuildAlertMessage(AlertRule rule, Dictionary<string, object> context)
    {
        return rule.Condition.Type switch
        {
            ConditionType.ErrorCount => $"Error count {context.GetValueOrDefault("ErrorCount", 0)} exceeds threshold {context.GetValueOrDefault("Threshold", 0)}",
            ConditionType.ErrorRate => $"Error rate {context.GetValueOrDefault("ErrorRate", 0):F2}% exceeds threshold {context.GetValueOrDefault("Threshold", 0)}%",
            ConditionType.SpecificError => $"Detected {context.GetValueOrDefault("MatchCount", 0)} occurrences of pattern '{context.GetValueOrDefault("Pattern", "")}'",
            ConditionType.ServiceDown => $"Service appears to be down (log count: {context.GetValueOrDefault("LogCount", 0)})",
            _ => rule.Description
        };
    }

    private AlertSeverity DetermineAlertSeverity(ConditionType conditionType)
    {
        return conditionType switch
        {
            ConditionType.ServiceDown => AlertSeverity.Critical,
            ConditionType.ErrorRate => AlertSeverity.Error,
            ConditionType.ErrorCount => AlertSeverity.Warning,
            ConditionType.SpecificError => AlertSeverity.Warning,
            _ => AlertSeverity.Info
        };
    }
}
