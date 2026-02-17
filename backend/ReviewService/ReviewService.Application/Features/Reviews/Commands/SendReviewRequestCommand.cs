using MediatR;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Sprint 15 - Enviar solicitud automática de review post-compra
/// Se envía 7 días después de la compra verificada
/// </summary>
public record SendReviewRequestCommand : IRequest<Result<ReviewRequestResultDto>>
{
    public Guid BuyerId { get; init; }
    public Guid SellerId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid OrderId { get; init; }
    public string BuyerEmail { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string VehicleTitle { get; init; } = string.Empty;
    public string SellerName { get; init; } = string.Empty;
    public DateTime PurchaseDate { get; init; }
}

/// <summary>
/// Resultado de la solicitud de review
/// </summary>
public record ReviewRequestResultDto
{
    public Guid RequestId { get; init; }
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool EmailSent { get; init; }
    public string Message { get; init; } = string.Empty;
}
