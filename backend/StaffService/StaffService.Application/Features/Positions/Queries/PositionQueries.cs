using MediatR;
using StaffService.Application.DTOs;

namespace StaffService.Application.Features.Positions.Queries;

// ============================================
// Get Position By Id
// ============================================
public record GetPositionByIdQuery(Guid Id) : IRequest<PositionDto?>;

// ============================================
// Get All Positions
// ============================================
public record GetAllPositionsQuery : IRequest<IEnumerable<PositionDto>>;

// ============================================
// Get Positions By Department
// ============================================
public record GetPositionsByDepartmentQuery(Guid DepartmentId) : IRequest<IEnumerable<PositionDto>>;
