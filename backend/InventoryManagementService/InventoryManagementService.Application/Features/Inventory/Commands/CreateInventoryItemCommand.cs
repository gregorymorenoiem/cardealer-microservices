using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;
using InventoryManagementService.Application.DTOs;

namespace InventoryManagementService.Application.Features.Inventory.Commands;

public record CreateInventoryItemCommand : IRequest<InventoryItemDto>
{
    public Guid DealerId { get; init; }
    public Guid VehicleId { get; init; }
    public string? InternalNotes { get; init; }
    public string? Location { get; init; }
    public int? StockNumber { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal ListPrice { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? MinAcceptablePrice { get; init; }
    public bool IsNegotiable { get; init; } = true;
    public DateTime? AcquiredDate { get; init; }
    public string? AcquisitionSource { get; init; }
    public string? AcquisitionDetails { get; init; }
    public List<string>? Tags { get; init; }
}

public class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, InventoryItemDto>
{
    private readonly IInventoryItemRepository _repository;

    public CreateInventoryItemHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItemDto> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem
        {
            DealerId = request.DealerId,
            VehicleId = request.VehicleId,
            InternalNotes = request.InternalNotes,
            Location = request.Location,
            StockNumber = request.StockNumber,
            CostPrice = request.CostPrice,
            ListPrice = request.ListPrice,
            TargetPrice = request.TargetPrice,
            MinAcceptablePrice = request.MinAcceptablePrice,
            IsNegotiable = request.IsNegotiable,
            AcquiredDate = request.AcquiredDate,
            AcquisitionDetails = request.AcquisitionDetails,
            Tags = request.Tags ?? new List<string>()
        };

        if (!string.IsNullOrEmpty(request.AcquisitionSource))
        {
            item.AcquisitionSource = Enum.Parse<AcquisitionSource>(request.AcquisitionSource);
        }

        var created = await _repository.CreateAsync(item);

        return MapToDto(created);
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
