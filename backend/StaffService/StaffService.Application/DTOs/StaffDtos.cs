using System;
using StaffService.Domain.Entities;

namespace StaffService.Application.DTOs;

// ============================================
// Staff DTOs
// ============================================

public record StaffDto(
    Guid Id,
    Guid AuthUserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    string? EmployeeCode,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? PositionId,
    string? PositionTitle,
    Guid? SupervisorId,
    string? SupervisorName,
    StaffStatus Status,
    StaffRole Role,
    DateTime HireDate,
    DateTime? LastLoginAt,
    bool TwoFactorEnabled,
    DateTime CreatedAt
);

public record StaffListItemDto(
    Guid Id,
    string Email,
    string FullName,
    string? AvatarUrl,
    string? DepartmentName,
    string? PositionTitle,
    StaffStatus Status,
    StaffRole Role,
    DateTime? LastLoginAt
);

public record StaffSummaryDto(
    int Total,
    int Active,
    int Pending,
    int Suspended,
    int OnLeave,
    int Terminated,
    Dictionary<string, int> ByRole,
    Dictionary<string, int> ByDepartment
);

// ============================================
// Invitation DTOs
// ============================================

public record InvitationDto(
    Guid Id,
    string Email,
    StaffRole AssignedRole,
    string? DepartmentName,
    string? PositionTitle,
    InvitationStatus Status,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    string InvitedByName,
    DateTime CreatedAt,
    bool IsExpired,
    bool IsValid
);

public record InvitationValidationDto(
    bool IsValid,
    string Email,
    StaffRole AssignedRole,
    string? DepartmentName,
    string? PositionTitle,
    string? Message,
    DateTime ExpiresAt
);

// ============================================
// Department DTOs
// ============================================

public record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    string? Code,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    Guid? HeadId,
    string? HeadName,
    int StaffCount,
    bool IsActive
);

public record DepartmentTreeDto(
    Guid Id,
    string Name,
    string? Code,
    int StaffCount,
    List<DepartmentTreeDto> Children
);

// ============================================
// Position DTOs
// ============================================

public record PositionDto(
    Guid Id,
    string Title,
    string? Description,
    string? Code,
    Guid? DepartmentId,
    string? DepartmentName,
    StaffRole DefaultRole,
    int Level,
    int StaffCount,
    bool IsActive
);

// ============================================
// Permission DTOs
// ============================================

public record PermissionDto(
    Guid Id,
    string Permission,
    bool IsGranted,
    string? Reason,
    DateTime? ExpiresAt,
    bool IsActive
);

// ============================================
// Paginated Response
// ============================================

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
