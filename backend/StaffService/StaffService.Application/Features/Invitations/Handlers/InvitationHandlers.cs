using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Invitations.Commands;
using StaffService.Application.Features.Invitations.Queries;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Application.Features.Invitations.Handlers;

// Query Handlers

public class GetInvitationByIdQueryHandler : IRequestHandler<GetInvitationByIdQuery, InvitationDto?>
{
    private readonly IStaffInvitationRepository _repository;

    public GetInvitationByIdQueryHandler(IStaffInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvitationDto?> Handle(GetInvitationByIdQuery request, CancellationToken ct)
    {
        var invitation = await _repository.GetByIdAsync(request.Id, ct);
        if (invitation == null) return null;
        
        return MapToDto(invitation);
    }

    private static InvitationDto MapToDto(StaffInvitation inv) => new(
        inv.Id,
        inv.Email,
        inv.AssignedRole,
        inv.Department?.Name,
        inv.Position?.Title,
        inv.Status,
        inv.ExpiresAt,
        inv.AcceptedAt,
        inv.InvitedByStaff != null ? $"{inv.InvitedByStaff.FirstName} {inv.InvitedByStaff.LastName}" : "System",
        inv.CreatedAt,
        inv.IsExpired,
        inv.IsValid
    );
}

public class ValidateInvitationTokenQueryHandler : IRequestHandler<ValidateInvitationTokenQuery, InvitationValidationDto?>
{
    private readonly IStaffInvitationRepository _repository;

    public ValidateInvitationTokenQueryHandler(IStaffInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvitationValidationDto?> Handle(ValidateInvitationTokenQuery request, CancellationToken ct)
    {
        var invitation = await _repository.GetByTokenAsync(request.Token, ct);
        if (invitation == null || !invitation.IsValid)
            return null;
        
        return new InvitationValidationDto(
            true,
            invitation.Email,
            invitation.AssignedRole,
            invitation.Department?.Name,
            invitation.Position?.Title,
            invitation.Message,
            invitation.ExpiresAt
        );
    }
}

public class SearchInvitationsQueryHandler : IRequestHandler<SearchInvitationsQuery, PaginatedResponse<InvitationDto>>
{
    private readonly IStaffInvitationRepository _repository;

    public SearchInvitationsQueryHandler(IStaffInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<InvitationDto>> Handle(SearchInvitationsQuery request, CancellationToken ct)
    {
        var invitations = await _repository.SearchAsync(request.Status, request.Page, request.PageSize, ct);
        var totalCount = await _repository.CountAsync(request.Status, ct);

        var items = invitations.Select(inv => new InvitationDto(
            inv.Id,
            inv.Email,
            inv.AssignedRole,
            inv.Department?.Name,
            inv.Position?.Title,
            inv.Status,
            inv.ExpiresAt,
            inv.AcceptedAt,
            inv.InvitedByStaff != null ? $"{inv.InvitedByStaff.FirstName} {inv.InvitedByStaff.LastName}" : "System",
            inv.CreatedAt,
            inv.IsExpired,
            inv.IsValid
        ));

        return new PaginatedResponse<InvitationDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        );
    }
}

public class GetPendingInvitationsCountQueryHandler : IRequestHandler<GetPendingInvitationsCountQuery, int>
{
    private readonly IStaffInvitationRepository _repository;

    public GetPendingInvitationsCountQueryHandler(IStaffInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetPendingInvitationsCountQuery request, CancellationToken ct)
    {
        return await _repository.CountAsync(InvitationStatus.Pending, ct);
    }
}

// Command Handlers

public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<InvitationDto>>
{
    private readonly IStaffInvitationRepository _invitationRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly INotificationClient _notificationClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<CreateInvitationCommandHandler> _logger;

    public CreateInvitationCommandHandler(
        IStaffInvitationRepository invitationRepository,
        IStaffRepository staffRepository,
        INotificationClient notificationClient,
        IAuditServiceClient auditClient,
        ILogger<CreateInvitationCommandHandler> logger)
    {
        _invitationRepository = invitationRepository;
        _staffRepository = staffRepository;
        _notificationClient = notificationClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<InvitationDto>> Handle(CreateInvitationCommand request, CancellationToken ct)
    {
        // Check if email already exists as staff
        if (await _staffRepository.EmailExistsAsync(request.Email, ct))
            return Result<InvitationDto>.Failure("A staff member with this email already exists");

        // Check for existing pending invitation
        var existingInvitation = await _invitationRepository.GetByEmailAsync(request.Email, ct);
        if (existingInvitation != null && existingInvitation.IsValid)
            return Result<InvitationDto>.Failure("A pending invitation already exists for this email");

        // Generate secure token
        var token = GenerateSecureToken();

        // Get inviter info (InvitedBy may be null if system admin not in staff table)
        Domain.Entities.Staff? inviter = request.InvitedBy.HasValue
            ? await _staffRepository.GetByIdAsync(request.InvitedBy.Value, ct)
            : null;
        var inviterName = inviter != null ? $"{inviter.FirstName} {inviter.LastName}" : "System Administrator";

        var invitation = new StaffInvitation
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Token = token,
            AssignedRole = request.Role,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            SupervisorId = request.SupervisorId,
            Message = request.Message,
            InvitedBy = inviter != null ? request.InvitedBy : null,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            EmailSentCount = 1,
            LastEmailSentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _invitationRepository.AddAsync(invitation, ct);

        // Send invitation email
        await _notificationClient.SendInvitationEmailAsync(
            request.Email,
            token,
            inviterName,
            request.Role.ToString(),
            request.Message,
            ct
        );

        // Audit
        await _auditClient.LogActionAsync(
            request.InvitedBy,
            "CREATE_STAFF_INVITATION",
            "StaffInvitation",
            invitation.Id.ToString(),
            new { Email = request.Email, Role = request.Role },
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff invitation created for {Email} by {InvitedBy}", request.Email, request.InvitedBy);

        // Reload with relationships
        invitation = await _invitationRepository.GetByIdAsync(invitation.Id, ct);

        return Result<InvitationDto>.Success(new InvitationDto(
            invitation!.Id,
            invitation.Email,
            invitation.AssignedRole,
            invitation.Department?.Name,
            invitation.Position?.Title,
            invitation.Status,
            invitation.ExpiresAt,
            invitation.AcceptedAt,
            inviterName,
            invitation.CreatedAt,
            invitation.IsExpired,
            invitation.IsValid
        ));
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<StaffDto>>
{
    private readonly IStaffInvitationRepository _invitationRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAuthServiceClient _authClient;
    private readonly INotificationClient _notificationClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        IStaffInvitationRepository invitationRepository,
        IStaffRepository staffRepository,
        IAuthServiceClient authClient,
        INotificationClient notificationClient,
        IAuditServiceClient auditClient,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _invitationRepository = invitationRepository;
        _staffRepository = staffRepository;
        _authClient = authClient;
        _notificationClient = notificationClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<StaffDto>> Handle(AcceptInvitationCommand request, CancellationToken ct)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(request.Token, ct);
        if (invitation == null)
            return Result<StaffDto>.Failure("Invalid invitation token", 404);

        if (!invitation.IsValid)
            return Result<StaffDto>.Failure(invitation.IsExpired ? "Invitation has expired" : "Invitation is no longer valid");

        // Create user in AuthService
        var authResult = await _authClient.CreateStaffUserAsync(new CreateStaffUserRequest(
            invitation.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            invitation.AssignedRole.ToString()
        ), ct);

        if (!authResult.Success || !authResult.UserId.HasValue)
            return Result<StaffDto>.Failure($"Failed to create authentication account: {authResult.Error}");

        // Create staff record
        var staff = new Domain.Entities.Staff
        {
            Id = Guid.NewGuid(),
            AuthUserId = authResult.UserId.Value,
            Email = invitation.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DepartmentId = invitation.DepartmentId,
            PositionId = invitation.PositionId,
            SupervisorId = invitation.SupervisorId,
            Role = invitation.AssignedRole,
            Status = StaffStatus.Active,
            InvitationId = invitation.Id,
            HireDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "self"
        };

        await _staffRepository.AddAsync(staff, ct);

        // Update invitation
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.StaffId = staff.Id;
        await _invitationRepository.UpdateAsync(invitation, ct);

        // Send welcome email
        await _notificationClient.SendWelcomeEmailAsync(
            staff.Email,
            staff.FullName,
            staff.Role.ToString(),
            ct
        );

        // Audit
        await _auditClient.LogActionAsync(
            staff.Id,
            "ACCEPT_STAFF_INVITATION",
            "Staff",
            staff.Id.ToString(),
            new { Email = staff.Email, Role = staff.Role },
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff invitation accepted. New staff {StaffId} ({Email}) created", staff.Id, staff.Email);

        // Reload with relationships
        staff = await _staffRepository.GetByIdAsync(staff.Id, ct);

        return Result<StaffDto>.Success(new StaffDto(
            staff!.Id,
            staff.AuthUserId,
            staff.Email,
            staff.FirstName,
            staff.LastName,
            staff.FullName,
            staff.PhoneNumber,
            staff.AvatarUrl,
            staff.EmployeeCode,
            staff.DepartmentId,
            staff.Department?.Name,
            staff.PositionId,
            staff.Position?.Title,
            staff.SupervisorId,
            staff.Supervisor != null ? $"{staff.Supervisor.FirstName} {staff.Supervisor.LastName}" : null,
            staff.Status,
            staff.Role,
            staff.HireDate,
            staff.LastLoginAt,
            staff.TwoFactorEnabled,
            staff.CreatedAt
        ));
    }
}

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand, Result<bool>>
{
    private readonly IStaffInvitationRepository _invitationRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly INotificationClient _notificationClient;
    private readonly ILogger<ResendInvitationCommandHandler> _logger;

    public ResendInvitationCommandHandler(
        IStaffInvitationRepository invitationRepository,
        IStaffRepository staffRepository,
        INotificationClient notificationClient,
        ILogger<ResendInvitationCommandHandler> logger)
    {
        _invitationRepository = invitationRepository;
        _staffRepository = staffRepository;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResendInvitationCommand request, CancellationToken ct)
    {
        var invitation = await _invitationRepository.GetByIdAsync(request.Id, ct);
        if (invitation == null)
            return Result<bool>.Failure("Invitation not found", 404);

        if (invitation.Status != InvitationStatus.Pending)
            return Result<bool>.Failure("Can only resend pending invitations");

        // Extend expiration
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        invitation.EmailSentCount++;
        invitation.LastEmailSentAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation, ct);

        // Get inviter name
        Domain.Entities.Staff? inviter = invitation.InvitedBy.HasValue
            ? await _staffRepository.GetByIdAsync(invitation.InvitedBy.Value, ct)
            : null;
        var inviterName = inviter != null ? $"{inviter.FirstName} {inviter.LastName}" : "System Administrator";

        // Resend email
        await _notificationClient.SendInvitationEmailAsync(
            invitation.Email,
            invitation.Token,
            inviterName,
            invitation.AssignedRole.ToString(),
            invitation.Message,
            ct
        );

        _logger.LogInformation("Invitation {InvitationId} resent to {Email}", invitation.Id, invitation.Email);

        return Result<bool>.Success(true);
    }
}

public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand, Result<bool>>
{
    private readonly IStaffInvitationRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<RevokeInvitationCommandHandler> _logger;

    public RevokeInvitationCommandHandler(
        IStaffInvitationRepository repository,
        IAuditServiceClient auditClient,
        ILogger<RevokeInvitationCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RevokeInvitationCommand request, CancellationToken ct)
    {
        var invitation = await _repository.GetByIdAsync(request.Id, ct);
        if (invitation == null)
            return Result<bool>.Failure("Invitation not found", 404);

        if (invitation.Status != InvitationStatus.Pending)
            return Result<bool>.Failure("Can only revoke pending invitations");

        invitation.Status = InvitationStatus.Revoked;
        await _repository.UpdateAsync(invitation, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "REVOKE_STAFF_INVITATION",
            "StaffInvitation",
            invitation.Id.ToString(),
            new { Email = invitation.Email },
            null,
            null,
            ct
        );

        _logger.LogInformation("Invitation {InvitationId} revoked", invitation.Id);

        return Result<bool>.Success(true);
    }
}
