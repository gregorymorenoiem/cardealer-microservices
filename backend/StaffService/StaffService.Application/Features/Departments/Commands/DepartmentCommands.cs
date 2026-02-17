using MediatR;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Commands;

namespace StaffService.Application.Features.Departments.Commands;

// ============================================
// Create Department
// ============================================
public record CreateDepartmentCommand(
    string Name,
    string? Description,
    string? Code,
    Guid? ParentDepartmentId,
    Guid? HeadId
) : IRequest<Result<DepartmentDto>>;

// ============================================
// Update Department
// ============================================
public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Code,
    Guid? ParentDepartmentId,
    Guid? HeadId,
    bool IsActive
) : IRequest<Result<DepartmentDto>>;

// ============================================
// Delete Department
// ============================================
public record DeleteDepartmentCommand(Guid Id) : IRequest<Result<bool>>;
