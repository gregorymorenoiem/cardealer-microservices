using MediatR;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Commands;

/// <summary>
/// Command to delete an inventory item
/// </summary>
public record DeleteInventoryItemCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public Guid DealerId { get; init; } // For ownership validation
}

public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand, bool>
{
    private readonly IInventoryItemRepository _repository;

    public DeleteInventoryItemCommandHandler(IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        
        if (item == null)
            return false;

        // Validate ownership if dealerId provided
        if (request.DealerId != Guid.Empty && item.DealerId != request.DealerId)
            throw new UnauthorizedAccessException("No autorizado para eliminar este item");

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}
