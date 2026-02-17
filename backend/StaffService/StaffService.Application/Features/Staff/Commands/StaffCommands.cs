using MediatR;
using StaffService.Application.DTOs;
using StaffService.Domain.Entities;

namespace StaffService.Application.Features.Staff.Commands;

// ============================================
// Create Staff (from accepted invitation)
// ============================================
public record CreateStaffFromInvitationCommand(
    string Token,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result<StaffDto>>;

// ============================================
// Update Staff
// ============================================
public record UpdateStaffCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? EmployeeCode,
    Guid? DepartmentId,
    Guid? PositionId,
    Guid? SupervisorId,
    StaffRole? Role,
    string? Notes
) : IRequest<Result<StaffDto>>;

// ============================================
// Change Staff Status
// ============================================
public record ChangeStaffStatusCommand(
    Guid Id,
    StaffStatus NewStatus,
    string? Reason
) : IRequest<Result<bool>>;

// ============================================
// Terminate Staff
// ============================================
public record TerminateStaffCommand(
    Guid Id,
    string Reason
) : IRequest<Result<bool>>;

// ============================================
// Delete Staff
// ============================================
public record DeleteStaffCommand(Guid Id) : IRequest<Result<bool>>;

// ============================================
// Result Pattern
// ============================================
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public int? StatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error, int statusCode = 400) => new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}
