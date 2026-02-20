using System.Diagnostics.Metrics;

namespace UserService.Application.Metrics;

/// <summary>
/// Métricas personalizadas para UserService con OpenTelemetry
/// </summary>
public class UserServiceMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _errorsLoggedCounter;
    private readonly Counter<long> _criticalErrorsCounter;
    private readonly Histogram<double> _errorProcessingDuration;
    private readonly ObservableGauge<int> _circuitBreakerStateGauge;

    // Seller conversion metrics
    private readonly Counter<long> _sellerConversionsRequested;
    private readonly Counter<long> _sellerConversionsApproved;
    private readonly Counter<long> _sellerConversionsFailed;

    // Dealer registration metrics
    private readonly Counter<long> _dealerRegistrationsRequested;
    private readonly Counter<long> _dealerRegistrationsCreated;
    private readonly Counter<long> _dealerRegistrationsFailed;
    private readonly Counter<long> _dealerApproved;
    private readonly Counter<long> _dealerRejected;

    // Estado del Circuit Breaker (0 = CLOSED, 1 = HALF-OPEN, 2 = OPEN)
    private int _circuitBreakerState = 0;

    public UserServiceMetrics()
    {
        _meter = new Meter("UserService.Metrics", "1.0.0");

        // Contador: Total de errores registrados
        _errorsLoggedCounter = _meter.CreateCounter<long>(
            name: "UserService.errors.logged",
            unit: "errors",
            description: "Total number of errors logged by UserService");

        // Contador: Errores críticos (status code >= 500)
        _criticalErrorsCounter = _meter.CreateCounter<long>(
            name: "UserService.errors.critical",
            unit: "errors",
            description: "Total number of critical errors (status code >= 500)");

        // Histograma: Duración del procesamiento de errores
        _errorProcessingDuration = _meter.CreateHistogram<double>(
            name: "UserService.error.processing.duration",
            unit: "ms",
            description: "Duration of error processing in milliseconds");

        // Gauge observable: Estado del Circuit Breaker
        _circuitBreakerStateGauge = _meter.CreateObservableGauge<int>(
            name: "UserService.circuitbreaker.state",
            observeValue: () => _circuitBreakerState,
            unit: "state",
            description: "Circuit Breaker state: 0=CLOSED, 1=HALF-OPEN, 2=OPEN");

        // Seller conversion metrics
        _sellerConversionsRequested = _meter.CreateCounter<long>(
            name: "seller.conversions.requested_total",
            unit: "conversions",
            description: "Total seller conversion requests");

        _sellerConversionsApproved = _meter.CreateCounter<long>(
            name: "seller.conversions.approved_total",
            unit: "conversions",
            description: "Total approved seller conversions");

        _sellerConversionsFailed = _meter.CreateCounter<long>(
            name: "seller.conversions.failed_total",
            unit: "conversions",
            description: "Total failed seller conversions");

        // Dealer registration metrics
        _dealerRegistrationsRequested = _meter.CreateCounter<long>(
            name: "dealer.registrations.requested_total",
            unit: "registrations",
            description: "Total dealer registration requests");

        _dealerRegistrationsCreated = _meter.CreateCounter<long>(
            name: "dealer.registrations.created_total",
            unit: "registrations",
            description: "Total dealer registrations created (pending approval)");

        _dealerRegistrationsFailed = _meter.CreateCounter<long>(
            name: "dealer.registrations.failed_total",
            unit: "registrations",
            description: "Total failed dealer registrations");

        _dealerApproved = _meter.CreateCounter<long>(
            name: "dealer.approved_total",
            unit: "dealers",
            description: "Total dealers approved by admin");

        _dealerRejected = _meter.CreateCounter<long>(
            name: "dealer.rejected_total",
            unit: "dealers",
            description: "Total dealers rejected by admin");
    }

    /// <summary>
    /// Incrementa el contador de errores registrados
    /// </summary>
    public void RecordRoleged(string serviceName, int statusCode, string exceptionType)
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
    /// Records a seller conversion request.
    /// </summary>
    public void RecordSellerConversionRequested()
    {
        _sellerConversionsRequested.Add(1);
    }

    /// <summary>
    /// Records an approved seller conversion.
    /// </summary>
    public void RecordSellerConversionApproved()
    {
        _sellerConversionsApproved.Add(1);
    }

    /// <summary>
    /// Records a failed seller conversion.
    /// </summary>
    public void RecordSellerConversionFailed(string reason)
    {
        _sellerConversionsFailed.Add(1, new KeyValuePair<string, object?>("reason", reason));
    }

    /// <summary>
    /// Records a dealer registration request.
    /// </summary>
    public void RecordDealerRegistrationRequested()
    {
        _dealerRegistrationsRequested.Add(1);
    }

    /// <summary>
    /// Records a dealer registration created (pending admin approval).
    /// </summary>
    public void RecordDealerRegistrationCreated()
    {
        _dealerRegistrationsCreated.Add(1);
    }

    /// <summary>
    /// Records a failed dealer registration.
    /// </summary>
    public void RecordDealerRegistrationFailed(string reason)
    {
        _dealerRegistrationsFailed.Add(1, new KeyValuePair<string, object?>("reason", reason));
    }

    /// <summary>
    /// Records a dealer approved by admin.
    /// </summary>
    public void RecordDealerApproved()
    {
        _dealerApproved.Add(1);
    }

    /// <summary>
    /// Records a dealer rejected by admin.
    /// </summary>
    public void RecordDealerRejected()
    {
        _dealerRejected.Add(1);
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
