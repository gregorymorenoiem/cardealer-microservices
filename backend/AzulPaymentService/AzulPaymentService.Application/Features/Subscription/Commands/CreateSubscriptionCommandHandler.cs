using MediatR;
using Microsoft.Extensions.Logging;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;

namespace AzulPaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Handler para crear una suscripción
/// </summary>
public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionResponseDto>
{
    private readonly IAzulSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

    public CreateSubscriptionCommandHandler(
        IAzulSubscriptionRepository subscriptionRepository,
        ILogger<CreateSubscriptionCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Crea la suscripción
    /// </summary>
    public async Task<SubscriptionResponseDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando suscripción para usuario {UserId}", request.SubscriptionRequest.UserId);

        try
        {
            // Parsear frecuencia
            var frequency = Enum.Parse<SubscriptionFrequency>(request.SubscriptionRequest.Frequency);
            
            // Crear entidad de suscripción
            var subscription = new AzulSubscription
            {
                Id = Guid.NewGuid(),
                UserId = request.SubscriptionRequest.UserId,
                Amount = request.SubscriptionRequest.Amount,
                Currency = request.SubscriptionRequest.Currency,
                Frequency = frequency,
                Status = "Active",
                StartDate = request.SubscriptionRequest.StartDate,
                EndDate = request.SubscriptionRequest.EndDate,
                PaymentMethod = Enum.Parse<PaymentMethod>(request.SubscriptionRequest.PaymentMethod),
                CardToken = request.SubscriptionRequest.CardToken,
                PlanName = request.SubscriptionRequest.PlanName,
                InvoiceReference = request.SubscriptionRequest.InvoiceReference,
                CreatedAt = DateTime.UtcNow,
                NextChargeDate = CalculateNextChargeDate(
                    request.SubscriptionRequest.StartDate, 
                    frequency),
                ChargeCount = 0,
                TotalAmountCharged = 0
            };

            // En este punto, se llamaría a AZUL API
            // TODO: Implementar integración con AZUL en Infrastructure layer
            
            // Simular respuesta exitosa
            subscription.AzulSubscriptionId = Guid.NewGuid().ToString();

            // Guardar suscripción
            await _subscriptionRepository.CreateAsync(subscription, cancellationToken);

            _logger.LogInformation("Suscripción creada exitosamente. ID: {SubscriptionId}", subscription.Id);

            return new SubscriptionResponseDto
            {
                SubscriptionId = subscription.Id,
                AzulSubscriptionId = subscription.AzulSubscriptionId,
                UserId = subscription.UserId,
                Amount = subscription.Amount,
                Currency = subscription.Currency,
                Frequency = subscription.Frequency.ToString(),
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextChargeDate = subscription.NextChargeDate,
                ChargeCount = subscription.ChargeCount,
                TotalAmountCharged = subscription.TotalAmountCharged,
                PlanName = subscription.PlanName,
                IsSuccessful = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear suscripción para usuario {UserId}", request.SubscriptionRequest.UserId);
            throw;
        }
    }

    private static DateTime CalculateNextChargeDate(DateTime startDate, SubscriptionFrequency frequency)
    {
        return frequency switch
        {
            SubscriptionFrequency.Daily => startDate.AddDays(1),
            SubscriptionFrequency.Weekly => startDate.AddDays(7),
            SubscriptionFrequency.BiWeekly => startDate.AddDays(14),
            SubscriptionFrequency.Monthly => startDate.AddMonths(1),
            SubscriptionFrequency.Quarterly => startDate.AddMonths(3),
            SubscriptionFrequency.SemiAnnual => startDate.AddMonths(6),
            SubscriptionFrequency.Annual => startDate.AddYears(1),
            _ => startDate.AddMonths(1)
        };
    }
}
