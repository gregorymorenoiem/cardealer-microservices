using MediatR;
using StaffService.Application.DTOs;
using StaffService.Domain.Entities;

namespace StaffService.Application.Features.Invitations.Queries;

// ============================================
// Get Invitation By Id
// ============================================
public record GetInvitationByIdQuery(Guid Id) : IRequest<InvitationDto?>;

// ============================================
// Validate Invitation Token
// ============================================
public record ValidateInvitationTokenQuery(string Token) : IRequest<InvitationValidationDto?>;

// ============================================
// Search Invitations
// ============================================
public record SearchInvitationsQuery(
    InvitationStatus? Status,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedResponse<InvitationDto>>;

// ============================================
// Get Pending Invitations Count
// ============================================
public record GetPendingInvitationsCountQuery : IRequest<int>;
