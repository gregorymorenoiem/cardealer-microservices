using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;
using InventoryManagementService.Application.DTOs;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

public record GetInventoryItemsQuery : IRequest<PagedResultDto<InventoryItemDto>>
{
    public Guid DealerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
}

public class GetInventoryItemsHandler : IRequestHandler<GetInventoryItemsQuery, PagedResultDto<InventoryItemDto>>
{
    private readonly IInventoryItemRepository _repository;

    public GetInventoryItemsHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<InventoryItemDto>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        InventoryStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status))
        {
            status = Enum.Parse<InventoryStatus>(request.Status);
        }

        var (items, totalCount) = await _repository.GetPagedByDealerIdAsync(
            request.DealerId,
            request.Page,
            request.PageSize,
            status,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<InventoryItemDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    private static InventoryItemDto MapToDto(InventoryItem item)
    {
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
