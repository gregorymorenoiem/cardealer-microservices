using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;
using InventoryManagementService.Application.DTOs;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

public record GetInventoryStatsQuery : IRequest<InventoryStatsDto>
{
    public Guid DealerId { get; init; }
}

public class GetInventoryStatsHandler : IRequestHandler<GetInventoryStatsQuery, InventoryStatsDto>
{
    private readonly IInventoryItemRepository _repository;

    public GetInventoryStatsHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryStatsDto> Handle(GetInventoryStatsQuery request, CancellationToken cancellationToken)
    {
        var allItems = await _repository.GetByDealerIdAsync(request.DealerId);

        var activeItems = allItems.Where(i => i.Status == InventoryStatus.Active).ToList();
        var soldItems = allItems.Where(i => i.Status == InventoryStatus.Sold).ToList();
        var pausedItems = allItems.Where(i => i.Status == InventoryStatus.Paused).ToList();
        var featuredItems = allItems.Where(i => i.IsFeatured).ToList();
        var hotItems = allItems.Where(i => i.IsHot).ToList();
        var overdueItems = allItems.Where(i => i.IsOverdue && i.Status == InventoryStatus.Active).ToList();

        var totalValue = activeItems.Sum(i => i.ListPrice);
        var avgListPrice = activeItems.Any() ? activeItems.Average(i => i.ListPrice) : 0;
        var avgDaysOnMarket = activeItems.Any() ? (int)activeItems.Average(i => i.DaysOnMarket) : 0;
        var totalProjectedProfit = activeItems.Sum(i => i.ExpectedProfit ?? 0);

        return new InventoryStatsDto
        {
            TotalItems = allItems.Count,
            ActiveItems = activeItems.Count,
            SoldItems = soldItems.Count,
            PausedItems = pausedItems.Count,
            FeaturedItems = featuredItems.Count,
            HotItems = hotItems.Count,
            OverdueItems = overdueItems.Count,
            TotalInventoryValue = totalValue,
            AverageListPrice = avgListPrice,
            AverageDaysOnMarket = avgDaysOnMarket,
            TotalProjectedProfit = totalProjectedProfit
        };
    }
}
