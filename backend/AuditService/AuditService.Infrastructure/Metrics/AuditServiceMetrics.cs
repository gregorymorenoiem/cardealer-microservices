using System.Diagnostics.Metrics;
using AuditService.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AuditService.Infrastructure.Metrics;

/// <summary>
/// Métricas personalizadas para AuditService usando OpenTelemetry
/// </summary>
public class AuditServiceMetrics
{
    private readonly Meter _meter;
    private readonly IServiceProvider _serviceProvider;
    private int _activeSessions;

    // Counters
    private readonly Counter<long> _auditLogsCreatedCounter;
    private readonly Counter<long> _auditLogsQueriedCounter;
    private readonly Counter<long> _auditLogsByActionCounter;
    private readonly Counter<long> _auditLogsByEntityCounter;
    private readonly Counter<long> _securityEventsCounter;
    private readonly Counter<long> _complianceEventsCounter;

    // Histograms
    private readonly Histogram<double> _queryDurationHistogram;
    private readonly Histogram<double> _eventProcessingDurationHistogram;

    // Observable Gauges
    private readonly ObservableGauge<int> _activeAuditSessionsGauge;
    private readonly ObservableGauge<long> _totalAuditLogsGauge;

    public AuditServiceMetrics(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _meter = new Meter("AuditService", "1.0.0");

        // Counters - Incrementan con cada operación
        _auditLogsCreatedCounter = _meter.CreateCounter<long>(
            "auditservice_audit_logs_created_total",
            description: "Total de audit logs creados");

        _auditLogsQueriedCounter = _meter.CreateCounter<long>(
            "auditservice_audit_logs_queried_total",
            description: "Total de queries a audit logs");

        _auditLogsByActionCounter = _meter.CreateCounter<long>(
            "auditservice_audit_logs_by_action_total",
            description: "Audit logs por tipo de acción");

        _auditLogsByEntityCounter = _meter.CreateCounter<long>(
            "auditservice_audit_logs_by_entity_total",
            description: "Audit logs por tipo de entidad");

        _securityEventsCounter = _meter.CreateCounter<long>(
            "auditservice_security_events_total",
            description: "Eventos de seguridad detectados");

        _complianceEventsCounter = _meter.CreateCounter<long>(
            "auditservice_compliance_events_total",
            description: "Eventos de compliance registrados");

        // Histograms - Distribución de valores
        _queryDurationHistogram = _meter.CreateHistogram<double>(
            "auditservice_query_duration_ms",
            unit: "ms",
            description: "Duración de queries a audit logs");

        _eventProcessingDurationHistogram = _meter.CreateHistogram<double>(
            "auditservice_event_processing_duration_ms",
            unit: "ms",
            description: "Duración del procesamiento de eventos");

        // Observable Gauges - Valores observables
        _activeAuditSessionsGauge = _meter.CreateObservableGauge<int>(
            "auditservice_active_audit_sessions",
            () => GetActiveAuditSessions(),
            description: "Número de sesiones de auditoría activas");

        _totalAuditLogsGauge = _meter.CreateObservableGauge<long>(
            "auditservice_total_audit_logs",
            () => GetTotalAuditLogs(),
            description: "Total de audit logs en el sistema");
    }

    // Métodos para registrar métricas

    public void RecordAuditLogCreated(string action, string entityType, string serviceName)
    {
        _auditLogsCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("action", action),
            new KeyValuePair<string, object?>("entity_type", entityType),
            new KeyValuePair<string, object?>("service", serviceName));

        _auditLogsByActionCounter.Add(1,
            new KeyValuePair<string, object?>("action", action));

        _auditLogsByEntityCounter.Add(1,
            new KeyValuePair<string, object?>("entity_type", entityType));
    }

    public void RecordAuditLogQueried(string queryType, bool success)
    {
        _auditLogsQueriedCounter.Add(1,
            new KeyValuePair<string, object?>("query_type", queryType),
            new KeyValuePair<string, object?>("success", success.ToString()));
    }

    public void RecordSecurityEvent(string eventType, string severity)
    {
        _securityEventsCounter.Add(1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("severity", severity));
    }

    public void RecordComplianceEvent(string complianceType, string regulation)
    {
        _complianceEventsCounter.Add(1,
            new KeyValuePair<string, object?>("compliance_type", complianceType),
            new KeyValuePair<string, object?>("regulation", regulation));
    }

    public void RecordQueryDuration(double durationMs, string queryType)
    {
        _queryDurationHistogram.Record(durationMs,
            new KeyValuePair<string, object?>("query_type", queryType));
    }

    public void RecordEventProcessingDuration(double durationMs, string eventType)
    {
        _eventProcessingDurationHistogram.Record(durationMs,
            new KeyValuePair<string, object?>("event_type", eventType));
    }

    // Métodos auxiliares para gauges observables
    private int GetActiveAuditSessions()
    {
        // Return the current count of active sessions
        // This is incremented/decremented when sessions start/end
        return _activeSessions;
    }

    private long GetTotalAuditLogs()
    {
        try
        {
            // Create a scope to get the repository
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<IAuditLogRepository>();

            if (repository == null)
                return 0;

            // Get total count from repository (synchronously for the gauge callback)
            return repository.GetTotalCountAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Return 0 if unable to query (e.g., during startup)
            return 0;
        }
    }

    /// <summary>
    /// Increment active sessions count
    /// </summary>
    public void IncrementActiveSessions()
    {
        Interlocked.Increment(ref _activeSessions);
    }

    /// <summary>
    /// Decrement active sessions count
    /// </summary>
    public void DecrementActiveSessions()
    {
        Interlocked.Decrement(ref _activeSessions);
    }
}
