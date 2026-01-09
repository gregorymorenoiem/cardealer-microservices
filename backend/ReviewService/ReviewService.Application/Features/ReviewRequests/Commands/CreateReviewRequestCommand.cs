using MediatR;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.ReviewRequests.Commands;

/// <summary>
/// Comando para crear solicitud automática de review
/// </summary>
public record CreateReviewRequestCommand(
    Guid SellerId,
    Guid BuyerId,
    string BuyerEmail,
    Guid? VehicleId = null,
    Guid? OrderId = null,
    string? VehicleMake = null,
    string? VehicleModel = null) : IRequest<ReviewRequestDto>;