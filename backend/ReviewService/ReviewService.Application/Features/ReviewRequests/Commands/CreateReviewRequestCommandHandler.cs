using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.ReviewRequests.Commands;

/// <summary>
/// Handler para crear solicitudes de review
/// </summary>
public class CreateReviewRequestCommandHandler : IRequestHandler<CreateReviewRequestCommand, ReviewRequestDto>
{
    private readonly IReviewRequestRepository _reviewRequestRepository;
    private readonly ILogger<CreateReviewRequestCommandHandler> _logger;

    public CreateReviewRequestCommandHandler(
        IReviewRequestRepository reviewRequestRepository,
        ILogger<CreateReviewRequestCommandHandler> logger)
    {
        _reviewRequestRepository = reviewRequestRepository;
        _logger = logger;
    }

    public async Task<ReviewRequestDto> Handle(CreateReviewRequestCommand request, CancellationToken cancellationToken)
    {
        // Verificar que los IDs requeridos no sean vacíos
        if (request.SellerId == Guid.Empty || request.BuyerId == Guid.Empty)
        {
            throw new ArgumentException("SellerId and BuyerId are required");
        }

        // Crear directamente la solicitud (asumiendo que el negocio permite múltiples solicitudes)
        _logger.LogInformation("Creating review request for Seller {SellerId}, Buyer {BuyerId}",
            request.SellerId, request.BuyerId);

        // Crear nueva solicitud
        var reviewRequest = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = request.SellerId,
            BuyerId = request.BuyerId,
            VehicleId = request.VehicleId ?? Guid.Empty,
            OrderId = request.OrderId ?? Guid.Empty,
            BuyerEmail = request.BuyerEmail,
            BuyerName = "Usuario", // TODO: obtener de UserService
            PurchaseDate = DateTime.UtcNow.AddDays(-7), // Asumimos que la compra fue hace 7 días
            Token = Guid.NewGuid().ToString("N")[..16], // 16 caracteres
            RequestSentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Expira en 30 días
            Status = ReviewRequestStatus.Sent,
            RemindersSent = 0
        };

        await _reviewRequestRepository.AddAsync(reviewRequest);

        _logger.LogInformation("Created review request {RequestId} for Seller {SellerId}, Buyer {BuyerId}",
            reviewRequest.Id, request.SellerId, request.BuyerId);

        return new ReviewRequestDto
        {
            Id = reviewRequest.Id,
            SellerId = reviewRequest.SellerId,
            BuyerId = reviewRequest.BuyerId,
            VehicleId = reviewRequest.VehicleId,
            OrderId = reviewRequest.OrderId,
            BuyerEmail = reviewRequest.BuyerEmail,
            BuyerName = reviewRequest.BuyerName,
            Token = reviewRequest.Token,
            RequestSentAt = reviewRequest.RequestSentAt,
            ExpiresAt = reviewRequest.ExpiresAt,
            Status = reviewRequest.Status.ToString(),
            ReviewCreatedAt = reviewRequest.ReviewCreatedAt,
            RemindersSent = reviewRequest.RemindersSent,
            LastReminderAt = reviewRequest.LastReminderAt
        };
    }
}