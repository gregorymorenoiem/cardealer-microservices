using System.Diagnostics.Metrics;

namespace ErrorService.Application.Metrics;

/// <summary>
/// Métricas personalizadas para ErrorService con OpenTelemetry
/// </summary>
public class ErrorServiceMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _errorsLoggedCounter;
    private readonly Counter<long> _criticalErrorsCounter;
    private readonly Histogram<double> _errorProcessingDuration;
    private readonly ObservableGauge<int> _circuitBreakerStateGauge;

    // Estado del Circuit Breaker (0 = CLOSED, 1 = HALF-OPEN, 2 = OPEN)
    private int _circuitBreakerState = 0;

    public ErrorServiceMetrics()
    {
        _meter = new Meter("ErrorService.Metrics", "1.0.0");

        // Contador: Total de errores registrados
        _errorsLoggedCounter = _meter.CreateCounter<long>(
            name: "errorservice.errors.logged",
            unit: "errors",
            description: "Total number of errors logged by ErrorService");

        // Contador: Errores críticos (status code >= 500)
        _criticalErrorsCounter = _meter.CreateCounter<long>(
            name: "errorservice.errors.critical",
            unit: "errors",
            description: "Total number of critical errors (status code >= 500)");

        // Histograma: Duración del procesamiento de errores
        _errorProcessingDuration = _meter.CreateHistogram<double>(
            name: "errorservice.error.processing.duration",
            unit: "ms",
            description: "Duration of error processing in milliseconds");

        // Gauge observable: Estado del Circuit Breaker
        _circuitBreakerStateGauge = _meter.CreateObservableGauge<int>(
            name: "errorservice.circuitbreaker.state",
            observeValue: () => _circuitBreakerState,
            unit: "state",
            description: "Circuit Breaker state: 0=CLOSED, 1=HALF-OPEN, 2=OPEN");
    }

    /// <summary>
    /// Incrementa el contador de errores registrados
    /// </summary>
    public void RecordErrorLogged(string serviceName, int statusCode, string exceptionType)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("service.name", serviceName),
            new("status.code", statusCode),
            new("exception.type", exceptionType)
        };

        _errorsLoggedCounter.Add(1, tags);

        // Si es crítico (status code >= 500), también incrementar contador crítico
        if (statusCode >= 500)
        {
            _criticalErrorsCounter.Add(1, tags);
        }
    }

    /// <summary>
    /// Registra la duración del procesamiento de un error
    /// </summary>
    public void RecordProcessingDuration(double durationMs, string serviceName, bool success)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("service.name", serviceName),
            new("success", success)
        };

        _errorProcessingDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Actualiza el estado del Circuit Breaker
    /// </summary>
    /// <param name="state">0 = CLOSED, 1 = HALF-OPEN, 2 = OPEN</param>
    public void SetCircuitBreakerState(CircuitBreakerState state)
    {
        _circuitBreakerState = (int)state;
    }
}

public enum CircuitBreakerState
{
    Closed = 0,
    HalfOpen = 1,
    Open = 2
}
