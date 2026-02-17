using MediatR;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;

namespace StaffService.Application.Features.Positions.Commands;

// ============================================
// Create Position
// ============================================
public record CreatePositionCommand(
    string Title,
    string? Description,
    string? Code,
    Guid? DepartmentId,
    StaffRole DefaultRole,
    int Level
) : IRequest<Result<PositionDto>>;

// ============================================
// Update Position
// ============================================
public record UpdatePositionCommand(
    Guid Id,
    string Title,
    string? Description,
    string? Code,
    Guid? DepartmentId,
    StaffRole DefaultRole,
    int Level,
    bool IsActive
) : IRequest<Result<PositionDto>>;

// ============================================
// Delete Position
// ============================================
public record DeletePositionCommand(Guid Id) : IRequest<Result<bool>>;
