using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Commands;

public record BulkUpdateStatusCommand : IRequest<int>
{
    public List<Guid> ItemIds { get; init; } = new();
    public string Status { get; init; } = string.Empty;
}

public class BulkUpdateStatusHandler : IRequestHandler<BulkUpdateStatusCommand, int>
{
    private readonly IInventoryItemRepository _repository;

    public BulkUpdateStatusHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(BulkUpdateStatusCommand request, CancellationToken cancellationToken)
    {
        var status = Enum.Parse<InventoryStatus>(request.Status);
        await _repository.BulkUpdateStatusAsync(request.ItemIds, status);
        return request.ItemIds.Count;
    }
}
