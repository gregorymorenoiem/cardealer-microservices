using MassTransit;

namespace CarDealer.Shared.Sagas.Contracts;

/// <summary>
/// Interface base para todos los comandos de Saga
/// </summary>
public interface ISagaCommand : CorrelatedBy<Guid>
{
    /// <summary>
    /// Timestamp del comando
    /// </summary>
    DateTime Timestamp { get; }
}

/// <summary>
/// Interface base para todos los eventos de Saga
/// </summary>
public interface ISagaEvent : CorrelatedBy<Guid>
{
    /// <summary>
    /// Timestamp del evento
    /// </summary>
    DateTime Timestamp { get; }
}

/// <summary>
/// Interface para eventos de fallo
/// </summary>
public interface ISagaFaultEvent : ISagaEvent
{
    /// <summary>
    /// Código de error
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Mensaje de error
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Detalles adicionales del error
    /// </summary>
    string? ErrorDetails { get; }
}

/// <summary>
/// Evento base para fallas en Sagas
/// </summary>
public record SagaFaulted : ISagaFaultEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string ErrorCode { get; init; } = "SAGA_FAULT";
    public string ErrorMessage { get; init; } = string.Empty;
    public string? ErrorDetails { get; init; }
    public string SagaType { get; init; } = string.Empty;
    public string CurrentState { get; init; } = string.Empty;
}

// ============================================================
// SAGA: Order Processing (Ejemplo de referencia)
// ============================================================

/// <summary>
/// Comando para iniciar una orden
/// </summary>
public record SubmitOrder : ISagaCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid VehicleId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}

/// <summary>
/// Evento: Orden aceptada
/// </summary>
public record OrderAccepted : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
}

/// <summary>
/// Comando: Reservar vehículo
/// </summary>
public record ReserveVehicle : ISagaCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Evento: Vehículo reservado
/// </summary>
public record VehicleReserved : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid VehicleId { get; init; }
    public DateTime ReservedUntil { get; init; }
}

/// <summary>
/// Evento: Reserva de vehículo fallida
/// </summary>
public record VehicleReservationFailed : ISagaFaultEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string ErrorCode { get; init; } = "VEHICLE_RESERVATION_FAILED";
    public string ErrorMessage { get; init; } = string.Empty;
    public string? ErrorDetails { get; init; }
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Comando: Procesar pago
/// </summary>
public record ProcessPayment : ISagaCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}

/// <summary>
/// Evento: Pago completado
/// </summary>
public record PaymentCompleted : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid PaymentId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

/// <summary>
/// Evento: Pago fallido
/// </summary>
public record PaymentFailed : ISagaFaultEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string ErrorCode { get; init; } = "PAYMENT_FAILED";
    public string ErrorMessage { get; init; } = string.Empty;
    public string? ErrorDetails { get; init; }
}

/// <summary>
/// Comando: Compensar reserva de vehículo
/// </summary>
public record ReleaseVehicle : ISagaCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid VehicleId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Evento: Vehículo liberado
/// </summary>
public record VehicleReleased : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Evento: Orden completada exitosamente
/// </summary>
public record OrderCompleted : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid PaymentId { get; init; }
}

/// <summary>
/// Evento: Orden cancelada
/// </summary>
public record OrderCancelled : ISagaEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
