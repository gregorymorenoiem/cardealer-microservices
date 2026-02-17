using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

/// <summary>
/// Query to get a single inventory item by ID
/// </summary>
public record GetInventoryItemByIdQuery : IRequest<InventoryItemDto?>
{
    public Guid Id { get; init; }
}

public class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto?>
{
    private readonly IInventoryItemRepository _repository;

    public GetInventoryItemByIdQueryHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItemDto?> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        
        if (item == null)
            return null;

        return new InventoryItemDto
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
        };
    }
}
