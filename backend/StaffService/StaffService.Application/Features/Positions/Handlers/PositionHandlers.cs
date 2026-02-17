using MediatR;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Positions.Commands;
using StaffService.Application.Features.Positions.Queries;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Application.Features.Positions.Handlers;

// Query Handlers

public class GetPositionByIdQueryHandler : IRequestHandler<GetPositionByIdQuery, PositionDto?>
{
    private readonly IPositionRepository _repository;

    public GetPositionByIdQueryHandler(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PositionDto?> Handle(GetPositionByIdQuery request, CancellationToken ct)
    {
        var pos = await _repository.GetByIdAsync(request.Id, ct);
        if (pos == null) return null;
        
        return new PositionDto(
            pos.Id,
            pos.Title,
            pos.Description,
            pos.Code,
            pos.DepartmentId,
            pos.Department?.Name,
            pos.DefaultRole,
            pos.Level,
            pos.StaffMembers?.Count ?? 0,
            pos.IsActive
        );
    }
}

public class GetAllPositionsQueryHandler : IRequestHandler<GetAllPositionsQuery, IEnumerable<PositionDto>>
{
    private readonly IPositionRepository _repository;

    public GetAllPositionsQueryHandler(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PositionDto>> Handle(GetAllPositionsQuery request, CancellationToken ct)
    {
        var positions = await _repository.GetAllAsync(ct);
        
        return positions.Select(pos => new PositionDto(
            pos.Id,
            pos.Title,
            pos.Description,
            pos.Code,
            pos.DepartmentId,
            pos.Department?.Name,
            pos.DefaultRole,
            pos.Level,
            pos.StaffMembers?.Count ?? 0,
            pos.IsActive
        ));
    }
}

public class GetPositionsByDepartmentQueryHandler : IRequestHandler<GetPositionsByDepartmentQuery, IEnumerable<PositionDto>>
{
    private readonly IPositionRepository _repository;

    public GetPositionsByDepartmentQueryHandler(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PositionDto>> Handle(GetPositionsByDepartmentQuery request, CancellationToken ct)
    {
        var positions = await _repository.GetByDepartmentAsync(request.DepartmentId, ct);
        
        return positions.Select(pos => new PositionDto(
            pos.Id,
            pos.Title,
            pos.Description,
            pos.Code,
            pos.DepartmentId,
            pos.Department?.Name,
            pos.DefaultRole,
            pos.Level,
            pos.StaffMembers?.Count ?? 0,
            pos.IsActive
        ));
    }
}

// Command Handlers

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Result<PositionDto>>
{
    private readonly IPositionRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<CreatePositionCommandHandler> _logger;

    public CreatePositionCommandHandler(
        IPositionRepository repository,
        IAuditServiceClient auditClient,
        ILogger<CreatePositionCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<PositionDto>> Handle(CreatePositionCommand request, CancellationToken ct)
    {
        // Check for duplicate title
        var existing = await _repository.GetByTitleAsync(request.Title, ct);
        if (existing != null)
            return Result<PositionDto>.Failure("A position with this title already exists");

        var position = new Position
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Code = request.Code,
            DepartmentId = request.DepartmentId,
            DefaultRole = request.DefaultRole,
            Level = request.Level,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(position, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "CREATE_POSITION",
            "Position",
            position.Id.ToString(),
            request,
            null,
            null,
            ct
        );

        _logger.LogInformation("Position {PositionId} ({Title}) created", position.Id, position.Title);

        // Reload with relationships
        position = await _repository.GetByIdAsync(position.Id, ct);

        return Result<PositionDto>.Success(new PositionDto(
            position!.Id,
            position.Title,
            position.Description,
            position.Code,
            position.DepartmentId,
            position.Department?.Name,
            position.DefaultRole,
            position.Level,
            0,
            position.IsActive
        ));
    }
}

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Result<PositionDto>>
{
    private readonly IPositionRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<UpdatePositionCommandHandler> _logger;

    public UpdatePositionCommandHandler(
        IPositionRepository repository,
        IAuditServiceClient auditClient,
        ILogger<UpdatePositionCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<PositionDto>> Handle(UpdatePositionCommand request, CancellationToken ct)
    {
        var position = await _repository.GetByIdAsync(request.Id, ct);
        if (position == null)
            return Result<PositionDto>.Failure("Position not found", 404);

        position.Title = request.Title;
        position.Description = request.Description;
        position.Code = request.Code;
        position.DepartmentId = request.DepartmentId;
        position.DefaultRole = request.DefaultRole;
        position.Level = request.Level;
        position.IsActive = request.IsActive;

        await _repository.UpdateAsync(position, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "UPDATE_POSITION",
            "Position",
            position.Id.ToString(),
            request,
            null,
            null,
            ct
        );

        _logger.LogInformation("Position {PositionId} updated", position.Id);

        // Reload with relationships
        position = await _repository.GetByIdAsync(request.Id, ct);

        return Result<PositionDto>.Success(new PositionDto(
            position!.Id,
            position.Title,
            position.Description,
            position.Code,
            position.DepartmentId,
            position.Department?.Name,
            position.DefaultRole,
            position.Level,
            position.StaffMembers?.Count ?? 0,
            position.IsActive
        ));
    }
}

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, Result<bool>>
{
    private readonly IPositionRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<DeletePositionCommandHandler> _logger;

    public DeletePositionCommandHandler(
        IPositionRepository repository,
        IAuditServiceClient auditClient,
        ILogger<DeletePositionCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeletePositionCommand request, CancellationToken ct)
    {
        var position = await _repository.GetByIdAsync(request.Id, ct);
        if (position == null)
            return Result<bool>.Failure("Position not found", 404);

        // Check for staff members
        if (await _repository.HasStaffAsync(request.Id, ct))
            return Result<bool>.Failure("Cannot delete position with assigned staff members");

        await _repository.DeleteAsync(request.Id, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "DELETE_POSITION",
            "Position",
            position.Id.ToString(),
            new { Title = position.Title },
            null,
            null,
            ct
        );

        _logger.LogInformation("Position {PositionId} ({Title}) deleted", position.Id, position.Title);

        return Result<bool>.Success(true);
    }
}
