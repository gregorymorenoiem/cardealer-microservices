using MassTransit;
using CarDealer.Shared.Sagas.Contracts;

namespace CarDealer.Shared.Sagas.StateMachines;

/// <summary>
/// Estado de la Saga de procesamiento de orden
/// </summary>
public class OrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }
    public int Version { get; set; } // Requerido por ISagaVersion para Redis
    
    // Datos de la orden
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    
    // Estado del proceso
    public Guid? OrderId { get; set; }
    public Guid? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    
    // Reserva
    public DateTime? ReservedUntil { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Error handling
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// State Machine para procesamiento de órdenes de vehículos
/// Implementa el patrón Saga con compensaciones
/// </summary>
public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    // Estados
    public State Submitted { get; private set; } = null!;
    public State VehicleReservedState { get; private set; } = null!;
    public State PaymentPending { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;
    public State Faulted { get; private set; } = null!;

    // Eventos
    public Event<SubmitOrder> SubmitOrder { get; private set; } = null!;
    public Event<VehicleReserved> VehicleReservedEvent { get; private set; } = null!;
    public Event<VehicleReservationFailed> VehicleReservationFailed { get; private set; } = null!;
    public Event<PaymentCompleted> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;
    public Event<VehicleReleased> VehicleReleased { get; private set; } = null!;

    public OrderStateMachine()
    {
        // Configurar estado inicial y final
        InstanceState(x => x.CurrentState, Submitted, VehicleReservedState, PaymentPending, Completed, Cancelled, Faulted);

        // Configurar eventos
        Event(() => SubmitOrder, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => VehicleReservedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => VehicleReservationFailed, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentCompleted, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentFailed, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => VehicleReleased, x => x.CorrelateById(m => m.Message.CorrelationId));

        // =============================================
        // FLUJO PRINCIPAL
        // =============================================

        // Estado Inicial -> Submitted
        Initially(
            When(SubmitOrder)
                .Then(context =>
                {
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.VehicleId = context.Message.VehicleId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.PaymentMethod = context.Message.PaymentMethod;
                    context.Saga.OrderId = Guid.NewGuid();
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                // Publicar evento de orden aceptada
                .PublishAsync(context => context.Init<OrderAccepted>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId
                }))
                // Enviar comando para reservar vehículo
                .SendAsync(new Uri("queue:reserve-vehicle"), context => context.Init<ReserveVehicle>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId,
                    context.Saga.VehicleId
                }))
                .TransitionTo(Submitted)
        );

        // Submitted -> VehicleReservedState (vehículo reservado exitosamente)
        During(Submitted,
            When(VehicleReservedEvent)
                .Then(context =>
                {
                    context.Saga.ReservedUntil = context.Message.ReservedUntil;
                })
                // Enviar comando para procesar pago
                .SendAsync(new Uri("queue:process-payment"), context => context.Init<ProcessPayment>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId,
                    context.Saga.Amount,
                    context.Saga.PaymentMethod
                }))
                .TransitionTo(PaymentPending),

            // Reserva fallida -> Cancelar orden
            When(VehicleReservationFailed)
                .Then(context =>
                {
                    context.Saga.ErrorCode = context.Message.ErrorCode;
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                })
                .PublishAsync(context => context.Init<OrderCancelled>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId,
                    Reason = $"Vehicle reservation failed: {context.Message.ErrorMessage}"
                }))
                .TransitionTo(Cancelled)
        );

        // PaymentPending -> Completed o Compensación
        During(PaymentPending,
            When(PaymentCompleted)
                .Then(context =>
                {
                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.TransactionId = context.Message.TransactionId;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .PublishAsync(context => context.Init<OrderCompleted>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId,
                    context.Saga.VehicleId,
                    context.Saga.PaymentId
                }))
                .TransitionTo(Completed),

            // Pago fallido -> COMPENSACIÓN: liberar vehículo
            When(PaymentFailed)
                .Then(context =>
                {
                    context.Saga.ErrorCode = context.Message.ErrorCode;
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                })
                // Compensación: liberar el vehículo reservado
                .SendAsync(new Uri("queue:release-vehicle"), context => context.Init<ReleaseVehicle>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.VehicleId,
                    Reason = $"Payment failed: {context.Message.ErrorMessage}"
                }))
                .TransitionTo(Faulted)
        );

        // Faulted -> Esperando compensación
        During(Faulted,
            When(VehicleReleased)
                .PublishAsync(context => context.Init<OrderCancelled>(new
                {
                    context.Saga.CorrelationId,
                    Timestamp = DateTime.UtcNow,
                    context.Saga.OrderId,
                    Reason = $"Order cancelled due to: {context.Saga.ErrorMessage}"
                }))
                .TransitionTo(Cancelled)
        );

        // Estados finales
        SetCompletedWhenFinalized();
    }
}
