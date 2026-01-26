using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

/// <summary>
/// Query to get hot inventory items (high activity) for a dealer
/// Criteria: ViewCount > 50 AND InquiryCount > 5 AND DaysOnMarket < 30
/// </summary>
public record GetHotItemsQuery : IRequest<List<InventoryItemDto>>
{
    public Guid DealerId { get; init; }
    public int MaxItems { get; init; } = 20;
}

public class GetHotItemsQueryHandler : IRequestHandler<GetHotItemsQuery, List<InventoryItemDto>>
{
    private readonly IInventoryItemRepository _repository;

    public GetHotItemsQueryHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InventoryItemDto>> Handle(GetHotItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetHotItemsByDealerIdAsync(request.DealerId);
        
        return items
            .Take(request.MaxItems)
            .Select(item => new InventoryItemDto
            {
                Id = item.Id,
                DealerId = item.DealerId,
                VehicleId = item.VehicleId,
                Status = item.Status.ToString(),
                Visibility = item.Visibility.ToString(),
                InternalNotes = item.InternalNotes,
                Location = item.Location,
                StockNumber = item.StockNumber,
                VIN = item.VIN,
                CostPrice = item.CostPrice,
                ListPrice = item.ListPrice,
                TargetPrice = item.TargetPrice,
                MinAcceptablePrice = item.MinAcceptablePrice,
                IsNegotiable = item.IsNegotiable,
                AcquiredDate = item.AcquiredDate,
                AcquisitionSource = item.AcquisitionSource?.ToString(),
                AcquisitionDetails = item.AcquisitionDetails,
                DaysOnMarket = item.DaysOnMarket,
                ViewCount = item.ViewCount,
                InquiryCount = item.InquiryCount,
                TestDriveCount = item.TestDriveCount,
                OfferCount = item.OfferCount,
                HighestOffer = item.HighestOffer,
                LastViewedAt = item.LastViewedAt,
                LastInquiryAt = item.LastInquiryAt,
                IsFeatured = item.IsFeatured,
                FeaturedUntil = item.FeaturedUntil,
                Priority = item.Priority,
                Tags = item.Tags,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                PublishedAt = item.PublishedAt,
                SoldAt = item.SoldAt,
                SoldPrice = item.SoldPrice,
                SoldTo = item.SoldTo,
                ExpectedProfit = item.ExpectedProfit,
                IsOverdue = item.IsOverdue,
                IsHot = item.IsHot
            })
            .ToList();
    }
}
