using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

/// <summary>
/// Query to get overdue inventory items (90+ days on market) for a dealer
/// </summary>
public record GetOverdueItemsQuery : IRequest<OverdueItemsResultDto>
{
    public Guid DealerId { get; init; }
}

public record OverdueItemsResultDto
{
    public List<OverdueItemDto> Items { get; init; } = new();
    public int TotalOverdueCount { get; init; }
    public decimal TotalInventoryValue { get; init; }
    public decimal AverageDaysOnMarket { get; init; }
    public decimal PotentialLoss { get; init; }
}

public record OverdueItemDto : InventoryItemDto
{
    public string Recommendation { get; init; } = string.Empty;
    public int SuggestedPriceReduction { get; init; }
}

public class GetOverdueItemsQueryHandler : IRequestHandler<GetOverdueItemsQuery, OverdueItemsResultDto>
{
    private readonly IInventoryItemRepository _repository;

    public GetOverdueItemsQueryHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<OverdueItemsResultDto> Handle(GetOverdueItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetOverdueByDealerIdAsync(request.DealerId);
        
        var overdueItems = items.Select(item => new OverdueItemDto
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
            IsHot = item.IsHot,
            Recommendation = GetRecommendation(item.DaysOnMarket),
            SuggestedPriceReduction = GetSuggestedPriceReduction(item.DaysOnMarket)
        }).ToList();

        var totalValue = items.Sum(i => i.ListPrice);
        var avgDays = items.Any() ? items.Average(i => i.DaysOnMarket) : 0;
        // Potential loss: estimate 5% value loss per 30 days overdue
        var potentialLoss = items.Sum(i => i.ListPrice * 0.05m * ((i.DaysOnMarket - 90) / 30m));

        return new OverdueItemsResultDto
        {
            Items = overdueItems,
            TotalOverdueCount = items.Count,
            TotalInventoryValue = totalValue,
            AverageDaysOnMarket = (decimal)avgDays,
            PotentialLoss = potentialLoss
        };
    }

    private static string GetRecommendation(int daysOnMarket)
    {
        return daysOnMarket switch
        {
            >= 180 => "Considerar subasta o liquidación inmediata",
            >= 150 => "Reducir precio 15-20% urgentemente",
            >= 120 => "Reducir precio 10-15% y destacar listing",
            >= 90 => "Reducir precio 5-10% para atraer compradores",
            _ => "Continuar monitoreo"
        };
    }

    private static int GetSuggestedPriceReduction(int daysOnMarket)
    {
        return daysOnMarket switch
        {
            >= 180 => 20,
            >= 150 => 15,
            >= 120 => 12,
            >= 90 => 8,
            _ => 0
        };
    }
}
