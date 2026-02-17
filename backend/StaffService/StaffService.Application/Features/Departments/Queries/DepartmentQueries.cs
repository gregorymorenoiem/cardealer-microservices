using MediatR;
using StaffService.Application.DTOs;

namespace StaffService.Application.Features.Departments.Queries;

// ============================================
// Get Department By Id
// ============================================
public record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto?>;

// ============================================
// Get All Departments
// ============================================
public record GetAllDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>;

// ============================================
// Get Department Tree
// ============================================
public record GetDepartmentTreeQuery : IRequest<IEnumerable<DepartmentTreeDto>>;
