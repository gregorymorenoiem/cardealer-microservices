using MediatR;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;

namespace StaffService.Application.Features.Invitations.Commands;

// ============================================
// Create Invitation
// ============================================
public record CreateInvitationCommand(
    string Email,
    StaffRole Role,
    Guid? DepartmentId,
    Guid? PositionId,
    Guid? SupervisorId,
    string? Message,
    Guid? InvitedBy
) : IRequest<Result<InvitationDto>>;

// ============================================
// Resend Invitation
// ============================================
public record ResendInvitationCommand(Guid Id) : IRequest<Result<bool>>;

// ============================================
// Revoke Invitation
// ============================================
public record RevokeInvitationCommand(Guid Id) : IRequest<Result<bool>>;

// ============================================
// Accept Invitation
// ============================================
public record AcceptInvitationCommand(
    string Token,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result<StaffDto>>;
