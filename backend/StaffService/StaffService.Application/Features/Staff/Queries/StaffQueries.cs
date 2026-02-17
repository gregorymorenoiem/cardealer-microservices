using MediatR;
using StaffService.Application.DTOs;
using StaffService.Domain.Entities;

namespace StaffService.Application.Features.Staff.Queries;

// ============================================
// Get Staff By Id
// ============================================
public record GetStaffByIdQuery(Guid Id) : IRequest<StaffDto?>;

// ============================================
// Get Staff By Auth User Id
// ============================================
public record GetStaffByAuthUserIdQuery(Guid AuthUserId) : IRequest<StaffDto?>;

// ============================================
// Get Staff By Email
// ============================================
public record GetStaffByEmailQuery(string Email) : IRequest<StaffDto?>;

// ============================================
// Search Staff
// ============================================
public record SearchStaffQuery(
    string? SearchTerm,
    StaffStatus? Status,
    StaffRole? Role,
    Guid? DepartmentId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedResponse<StaffListItemDto>>;

// ============================================
// Get Staff Summary
// ============================================
public record GetStaffSummaryQuery : IRequest<StaffSummaryDto>;

// ============================================
// Get Direct Reports
// ============================================
public record GetDirectReportsQuery(Guid SupervisorId) : IRequest<IEnumerable<StaffListItemDto>>;

// ============================================
// Get Organization Chart
// ============================================
public record GetOrganizationChartQuery(Guid? RootStaffId = null) : IRequest<object>;
