using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagementService.Domain.Interfaces;
using InventoryManagementService.Application.DTOs;

namespace InventoryManagementService.Application.Features.Inventory.Commands;

public record UpdateInventoryItemCommand : IRequest<InventoryItemDto>
{
    public Guid Id { get; init; }
    public string? InternalNotes { get; init; }
    public string? Location { get; init; }
    public int? StockNumber { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal? ListPrice { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? MinAcceptablePrice { get; init; }
    public bool? IsNegotiable { get; init; }
    public bool? IsFeatured { get; init; }
    public int? Priority { get; init; }
    public List<string>? Tags { get; init; }
}

public class UpdateInventoryItemHandler : IRequestHandler<UpdateInventoryItemCommand, InventoryItemDto>
{
    private readonly IInventoryItemRepository _repository;

    public UpdateInventoryItemHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItemDto> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) throw new Exception($"Inventory item {request.Id} not found");

        if (request.InternalNotes != null) item.InternalNotes = request.InternalNotes;
        if (request.Location != null) item.Location = request.Location;
        if (request.StockNumber.HasValue) item.StockNumber = request.StockNumber;
        if (request.CostPrice.HasValue) item.CostPrice = request.CostPrice;
        if (request.ListPrice.HasValue) item.ListPrice = request.ListPrice.Value;
        if (request.TargetPrice.HasValue) item.TargetPrice = request.TargetPrice;
        if (request.MinAcceptablePrice.HasValue) item.MinAcceptablePrice = request.MinAcceptablePrice;
        if (request.IsNegotiable.HasValue) item.IsNegotiable = request.IsNegotiable.Value;
        if (request.IsFeatured.HasValue) item.IsFeatured = request.IsFeatured.Value;
        if (request.Priority.HasValue) item.Priority = request.Priority.Value;
        if (request.Tags != null) item.Tags = request.Tags;

        item.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(item);

        return MapToDto(updated);
    }

    private static InventoryItemDto MapToDto(InventoryManagementService.Domain.Entities.InventoryItem item)
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
