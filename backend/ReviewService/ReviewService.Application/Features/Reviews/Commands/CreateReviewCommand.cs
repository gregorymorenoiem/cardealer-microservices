using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// &lt;summary&gt;
/// Command para crear una nueva review
/// &lt;/summary&gt;
public record CreateReviewCommand : IRequest&lt;Result&lt;ReviewDto&gt;&gt;
{
    public Guid BuyerId { get; init; }
    public Guid SellerId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? OrderId { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    
    // Info del buyer para cache
    public string BuyerName { get; init; } = string.Empty;
    public string? BuyerPhotoUrl { get; init; }
}