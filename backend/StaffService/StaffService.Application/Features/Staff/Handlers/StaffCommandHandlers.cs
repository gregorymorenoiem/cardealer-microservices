using MediatR;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Application.Features.Staff.Handlers;

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, Result<StaffDto>>
{
    private readonly IStaffRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<UpdateStaffCommandHandler> _logger;

    public UpdateStaffCommandHandler(
        IStaffRepository repository,
        IAuditServiceClient auditClient,
        ILogger<UpdateStaffCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<StaffDto>> Handle(UpdateStaffCommand request, CancellationToken ct)
    {
        var staff = await _repository.GetByIdAsync(request.Id, ct);
        if (staff == null)
            return Result<StaffDto>.Failure("Staff member not found", 404);

        // Update fields
        staff.FirstName = request.FirstName;
        staff.LastName = request.LastName;
        staff.PhoneNumber = request.PhoneNumber;
        staff.EmployeeCode = request.EmployeeCode;
        staff.DepartmentId = request.DepartmentId;
        staff.PositionId = request.PositionId;
        staff.SupervisorId = request.SupervisorId;
        staff.Notes = request.Notes;
        
        if (request.Role.HasValue)
            staff.Role = request.Role.Value;

        await _repository.UpdateAsync(staff, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "UPDATE_STAFF",
            "Staff",
            staff.Id.ToString(),
            request,
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff {StaffId} updated", staff.Id);

        // Reload with relationships
        staff = await _repository.GetByIdAsync(request.Id, ct);

        return Result<StaffDto>.Success(MapToDto(staff!));
    }

    private static StaffDto MapToDto(Domain.Entities.Staff staff) => new(
        staff.Id,
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
    );
}

public class ChangeStaffStatusCommandHandler : IRequestHandler<ChangeStaffStatusCommand, Result<bool>>
{
    private readonly IStaffRepository _repository;
    private readonly IAuthServiceClient _authClient;
    private readonly INotificationClient _notificationClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<ChangeStaffStatusCommandHandler> _logger;

    public ChangeStaffStatusCommandHandler(
        IStaffRepository repository,
        IAuthServiceClient authClient,
        INotificationClient notificationClient,
        IAuditServiceClient auditClient,
        ILogger<ChangeStaffStatusCommandHandler> logger)
    {
        _repository = repository;
        _authClient = authClient;
        _notificationClient = notificationClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ChangeStaffStatusCommand request, CancellationToken ct)
    {
        var staff = await _repository.GetByIdAsync(request.Id, ct);
        if (staff == null)
            return Result<bool>.Failure("Staff member not found", 404);

        var oldStatus = staff.Status;
        staff.Status = request.NewStatus;

        // Sync with AuthService
        if (request.NewStatus == StaffStatus.Suspended || request.NewStatus == StaffStatus.Terminated)
        {
            await _authClient.DisableUserAsync(staff.AuthUserId, ct);
        }
        else if (request.NewStatus == StaffStatus.Active && oldStatus == StaffStatus.Suspended)
        {
            await _authClient.EnableUserAsync(staff.AuthUserId, ct);
        }

        await _repository.UpdateAsync(staff, ct);

        // Send notification
        await _notificationClient.SendStatusChangeEmailAsync(
            staff.Email,
            staff.FullName,
            oldStatus.ToString(),
            request.NewStatus.ToString(),
            request.Reason,
            ct
        );

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "CHANGE_STAFF_STATUS",
            "Staff",
            staff.Id.ToString(),
            new { OldStatus = oldStatus, NewStatus = request.NewStatus, Reason = request.Reason },
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff {StaffId} status changed from {OldStatus} to {NewStatus}",
            staff.Id, oldStatus, request.NewStatus);

        return Result<bool>.Success(true);
    }
}

public class TerminateStaffCommandHandler : IRequestHandler<TerminateStaffCommand, Result<bool>>
{
    private readonly IStaffRepository _repository;
    private readonly IAuthServiceClient _authClient;
    private readonly INotificationClient _notificationClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<TerminateStaffCommandHandler> _logger;

    public TerminateStaffCommandHandler(
        IStaffRepository repository,
        IAuthServiceClient authClient,
        INotificationClient notificationClient,
        IAuditServiceClient auditClient,
        ILogger<TerminateStaffCommandHandler> logger)
    {
        _repository = repository;
        _authClient = authClient;
        _notificationClient = notificationClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(TerminateStaffCommand request, CancellationToken ct)
    {
        var staff = await _repository.GetByIdAsync(request.Id, ct);
        if (staff == null)
            return Result<bool>.Failure("Staff member not found", 404);

        if (staff.Status == StaffStatus.Terminated)
            return Result<bool>.Failure("Staff member is already terminated");

        var oldStatus = staff.Status;
        staff.Status = StaffStatus.Terminated;
        staff.TerminationDate = DateTime.UtcNow;
        staff.TerminationReason = request.Reason;

        // Disable in AuthService
        await _authClient.DisableUserAsync(staff.AuthUserId, ct);

        await _repository.UpdateAsync(staff, ct);

        // Send notification
        await _notificationClient.SendStatusChangeEmailAsync(
            staff.Email,
            staff.FullName,
            oldStatus.ToString(),
            "Terminated",
            request.Reason,
            ct
        );

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "TERMINATE_STAFF",
            "Staff",
            staff.Id.ToString(),
            new { Reason = request.Reason },
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff {StaffId} terminated. Reason: {Reason}", staff.Id, request.Reason);

        return Result<bool>.Success(true);
    }
}

public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, Result<bool>>
{
    private readonly IStaffRepository _repository;
    private readonly IAuthServiceClient _authClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<DeleteStaffCommandHandler> _logger;

    public DeleteStaffCommandHandler(
        IStaffRepository repository,
        IAuthServiceClient authClient,
        IAuditServiceClient auditClient,
        ILogger<DeleteStaffCommandHandler> logger)
    {
        _repository = repository;
        _authClient = authClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteStaffCommand request, CancellationToken ct)
    {
        var staff = await _repository.GetByIdAsync(request.Id, ct);
        if (staff == null)
            return Result<bool>.Failure("Staff member not found", 404);

        // Delete from AuthService
        await _authClient.DeleteUserAsync(staff.AuthUserId, ct);

        // Delete from StaffService
        await _repository.DeleteAsync(request.Id, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "DELETE_STAFF",
            "Staff",
            staff.Id.ToString(),
            new { Email = staff.Email },
            null,
            null,
            ct
        );

        _logger.LogInformation("Staff {StaffId} ({Email}) deleted", staff.Id, staff.Email);

        return Result<bool>.Success(true);
    }
}
