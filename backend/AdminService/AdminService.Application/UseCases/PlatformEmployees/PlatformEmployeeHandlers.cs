using MediatR;
using Microsoft.Extensions.Logging;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Application.Exceptions;

namespace AdminService.Application.UseCases.PlatformEmployees;

/// <summary>
/// Handler para invitar un nuevo empleado de plataforma
/// </summary>
public class InvitePlatformEmployeeCommandHandler : IRequestHandler<InvitePlatformEmployeeCommand, PlatformEmployeeInvitationDto>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly ILogger<InvitePlatformEmployeeCommandHandler> _logger;

    public InvitePlatformEmployeeCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        IAdminUserRepository adminUserRepository,
        ILogger<InvitePlatformEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _adminUserRepository = adminUserRepository;
        _logger = logger;
    }

    public async Task<PlatformEmployeeInvitationDto> Handle(InvitePlatformEmployeeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inviting platform employee {Email} with role {Role}", request.Email, request.Role);

        // Check if email already has an employee record
        var existingEmployee = await _employeeRepository.GetByEmailAsync(request.Email);
        if (existingEmployee != null)
        {
            throw new BadRequestException("Este email ya tiene una cuenta de empleado de plataforma");
        }

        // Check for existing pending invitation
        var existingInvitation = await _employeeRepository.GetPendingInvitationByEmailAsync(request.Email);
        if (existingInvitation != null)
        {
            throw new BadRequestException("Ya existe una invitación pendiente para este email");
        }

        // Parse role
        if (!Enum.TryParse<AdminRole>(request.Role, out var adminRole))
        {
            throw new BadRequestException($"Rol inválido: {request.Role}");
        }

        // Get inviter info
        var inviter = await _adminUserRepository.GetByIdAsync(request.InvitedBy);
        var inviterName = inviter != null ? inviter.FullName : "Administrador";

        // Create invitation
        var invitation = new PlatformEmployeeInvitation
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PlatformRole = adminRole,
            Permissions = request.Permissions != null ? string.Join(",", request.Permissions) : "[]",
            Department = request.Department,
            InvitedBy = request.InvitedBy,
            Status = InvitationStatus.Pending,
            InvitationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            Token = Guid.NewGuid().ToString("N")
        };

        await _employeeRepository.AddInvitationAsync(invitation);

        _logger.LogInformation("Platform employee invitation {InvitationId} created for {Email}", invitation.Id, request.Email);

        // TODO: Send email notification via NotificationService

        return new PlatformEmployeeInvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.PlatformRole.ToString(),
            Permissions = request.Permissions ?? Array.Empty<string>(),
            Department = invitation.Department,
            Status = invitation.Status.ToString(),
            InvitationDate = invitation.InvitationDate,
            ExpirationDate = invitation.ExpirationDate,
            InvitedByName = inviterName
        };
    }
}

/// <summary>
/// Handler para obtener lista de empleados de plataforma
/// </summary>
public class GetPlatformEmployeesQueryHandler : IRequestHandler<GetPlatformEmployeesQuery, PaginatedResult<PlatformEmployeeDto>>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<GetPlatformEmployeesQueryHandler> _logger;

    public GetPlatformEmployeesQueryHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<GetPlatformEmployeesQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<PlatformEmployeeDto>> Handle(GetPlatformEmployeesQuery request, CancellationToken cancellationToken)
    {
        var (employees, totalCount) = await _employeeRepository.GetAllAsync(
            status: request.Status,
            role: request.Role,
            department: request.Department,
            page: request.Page,
            pageSize: request.PageSize
        );

        var dtos = employees.Select(e => new PlatformEmployeeDto
        {
            Id = e.Id,
            UserId = e.UserId,
            Email = e.User?.Email ?? string.Empty,
            FirstName = e.User?.FirstName ?? string.Empty,
            LastName = e.User?.LastName ?? string.Empty,
            Role = e.PlatformRole.ToString(),
            Permissions = e.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            Department = e.Department,
            Status = e.Status.ToString(),
            HireDate = e.HireDate,
            LastActiveAt = e.User?.LastLoginAt,
            AvatarUrl = e.User?.AvatarUrl
        }).ToList();

        return new PaginatedResult<PlatformEmployeeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler para obtener detalle de un empleado de plataforma
/// </summary>
public class GetPlatformEmployeeQueryHandler : IRequestHandler<GetPlatformEmployeeQuery, PlatformEmployeeDetailDto?>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly ILogger<GetPlatformEmployeeQueryHandler> _logger;

    public GetPlatformEmployeeQueryHandler(
        IPlatformEmployeeRepository employeeRepository,
        IAdminUserRepository adminUserRepository,
        ILogger<GetPlatformEmployeeQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _adminUserRepository = adminUserRepository;
        _logger = logger;
    }

    public async Task<PlatformEmployeeDetailDto?> Handle(GetPlatformEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            return null;
        }

        var assignedBy = await _adminUserRepository.GetByIdAsync(employee.AssignedBy);

        return new PlatformEmployeeDetailDto
        {
            Id = employee.Id,
            UserId = employee.UserId,
            Email = employee.User?.Email ?? string.Empty,
            FirstName = employee.User?.FirstName ?? string.Empty,
            LastName = employee.User?.LastName ?? string.Empty,
            Role = employee.PlatformRole.ToString(),
            Permissions = employee.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            Department = employee.Department,
            Status = employee.Status.ToString(),
            HireDate = employee.HireDate,
            LastActiveAt = employee.User?.LastLoginAt,
            AvatarUrl = employee.User?.AvatarUrl,
            Notes = employee.Notes,
            AssignedBy = employee.AssignedBy,
            AssignedByName = assignedBy?.FullName ?? "Sistema",
            CreatedAt = employee.HireDate,
            UpdatedAt = employee.User?.UpdatedAt
        };
    }
}

/// <summary>
/// Handler para obtener invitaciones pendientes
/// </summary>
public class GetPlatformInvitationsQueryHandler : IRequestHandler<GetPlatformInvitationsQuery, IEnumerable<PlatformEmployeeInvitationDto>>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly ILogger<GetPlatformInvitationsQueryHandler> _logger;

    public GetPlatformInvitationsQueryHandler(
        IPlatformEmployeeRepository employeeRepository,
        IAdminUserRepository adminUserRepository,
        ILogger<GetPlatformInvitationsQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _adminUserRepository = adminUserRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PlatformEmployeeInvitationDto>> Handle(GetPlatformInvitationsQuery request, CancellationToken cancellationToken)
    {
        InvitationStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<InvitationStatus>(request.Status, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var invitations = await _employeeRepository.GetInvitationsAsync(status);

        var dtos = new List<PlatformEmployeeInvitationDto>();
        foreach (var invitation in invitations)
        {
            var inviter = await _adminUserRepository.GetByIdAsync(invitation.InvitedBy);
            dtos.Add(new PlatformEmployeeInvitationDto
            {
                Id = invitation.Id,
                Email = invitation.Email,
                Role = invitation.PlatformRole.ToString(),
                Permissions = invitation.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                Department = invitation.Department,
                Status = invitation.Status.ToString(),
                InvitationDate = invitation.InvitationDate,
                ExpirationDate = invitation.ExpirationDate,
                InvitedByName = inviter?.FullName ?? "Sistema"
            });
        }

        return dtos;
    }
}

/// <summary>
/// Handler para suspender un empleado de plataforma
/// </summary>
public class SuspendPlatformEmployeeCommandHandler : IRequestHandler<SuspendPlatformEmployeeCommand, Unit>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<SuspendPlatformEmployeeCommandHandler> _logger;

    public SuspendPlatformEmployeeCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<SuspendPlatformEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(SuspendPlatformEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new NotFoundException($"Empleado {request.EmployeeId} no encontrado");
        }

        employee.Status = EmployeeStatus.Suspended;
        if (!string.IsNullOrEmpty(request.Reason))
        {
            employee.Notes = $"{employee.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Suspendido: {request.Reason}";
        }

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation("Platform employee {EmployeeId} suspended", request.EmployeeId);

        return Unit.Value;
    }
}

/// <summary>
/// Handler para reactivar un empleado de plataforma
/// </summary>
public class ReactivatePlatformEmployeeCommandHandler : IRequestHandler<ReactivatePlatformEmployeeCommand, Unit>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<ReactivatePlatformEmployeeCommandHandler> _logger;

    public ReactivatePlatformEmployeeCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<ReactivatePlatformEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ReactivatePlatformEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new NotFoundException($"Empleado {request.EmployeeId} no encontrado");
        }

        employee.Status = EmployeeStatus.Active;
        employee.Notes = $"{employee.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Reactivado";

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation("Platform employee {EmployeeId} reactivated", request.EmployeeId);

        return Unit.Value;
    }
}

/// <summary>
/// Handler para remover un empleado de plataforma (soft delete)
/// </summary>
public class RemovePlatformEmployeeCommandHandler : IRequestHandler<RemovePlatformEmployeeCommand, Unit>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<RemovePlatformEmployeeCommandHandler> _logger;

    public RemovePlatformEmployeeCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<RemovePlatformEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemovePlatformEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new NotFoundException($"Empleado {request.EmployeeId} no encontrado");
        }

        // Soft delete - just mark as removed
        await _employeeRepository.SoftDeleteAsync(request.EmployeeId);

        _logger.LogInformation("Platform employee {EmployeeId} removed", request.EmployeeId);

        return Unit.Value;
    }
}

/// <summary>
/// Handler para cancelar una invitación
/// </summary>
public class CancelPlatformInvitationCommandHandler : IRequestHandler<CancelPlatformInvitationCommand, Unit>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<CancelPlatformInvitationCommandHandler> _logger;

    public CancelPlatformInvitationCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<CancelPlatformInvitationCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelPlatformInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _employeeRepository.GetInvitationByIdAsync(request.InvitationId);
        if (invitation == null)
        {
            throw new NotFoundException($"Invitación {request.InvitationId} no encontrada");
        }

        invitation.Status = InvitationStatus.Revoked;
        await _employeeRepository.UpdateInvitationAsync(invitation);

        _logger.LogInformation("Platform invitation {InvitationId} cancelled", request.InvitationId);

        return Unit.Value;
    }
}

/// <summary>
/// Handler para reenviar una invitación
/// </summary>
public class ResendPlatformInvitationCommandHandler : IRequestHandler<ResendPlatformInvitationCommand, PlatformEmployeeInvitationDto>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly ILogger<ResendPlatformInvitationCommandHandler> _logger;

    public ResendPlatformInvitationCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        IAdminUserRepository adminUserRepository,
        ILogger<ResendPlatformInvitationCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _adminUserRepository = adminUserRepository;
        _logger = logger;
    }

    public async Task<PlatformEmployeeInvitationDto> Handle(ResendPlatformInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _employeeRepository.GetInvitationByIdAsync(request.InvitationId);
        if (invitation == null)
        {
            throw new NotFoundException($"Invitación {request.InvitationId} no encontrada");
        }

        if (invitation.Status != InvitationStatus.Pending && invitation.Status != InvitationStatus.Expired)
        {
            throw new BadRequestException($"No se puede reenviar una invitación con estado: {invitation.Status}");
        }

        // Update invitation
        invitation.Token = Guid.NewGuid().ToString("N");
        invitation.ExpirationDate = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        invitation.InvitationDate = DateTime.UtcNow;

        await _employeeRepository.UpdateInvitationAsync(invitation);

        var inviter = await _adminUserRepository.GetByIdAsync(invitation.InvitedBy);

        _logger.LogInformation("Platform invitation {InvitationId} resent", request.InvitationId);

        // TODO: Send email notification via NotificationService

        return new PlatformEmployeeInvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.PlatformRole.ToString(),
            Permissions = invitation.Permissions?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            Department = invitation.Department,
            Status = invitation.Status.ToString(),
            InvitationDate = invitation.InvitationDate,
            ExpirationDate = invitation.ExpirationDate,
            InvitedByName = inviter?.FullName ?? "Sistema"
        };
    }
}

/// <summary>
/// Handler para actualizar un empleado de plataforma
/// </summary>
public class UpdatePlatformEmployeeCommandHandler : IRequestHandler<UpdatePlatformEmployeeCommand, Unit>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<UpdatePlatformEmployeeCommandHandler> _logger;

    public UpdatePlatformEmployeeCommandHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<UpdatePlatformEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdatePlatformEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new NotFoundException($"Empleado {request.EmployeeId} no encontrado");
        }

        if (!string.IsNullOrEmpty(request.Role) && Enum.TryParse<AdminRole>(request.Role, out var role))
        {
            employee.PlatformRole = role;
        }

        if (request.Permissions != null)
        {
            employee.Permissions = string.Join(",", request.Permissions);
        }

        if (request.Department != null)
        {
            employee.Department = request.Department;
        }

        if (request.Notes != null)
        {
            employee.Notes = request.Notes;
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<EmployeeStatus>(request.Status, out var status))
        {
            employee.Status = status;
        }

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation("Platform employee {EmployeeId} updated", request.EmployeeId);

        return Unit.Value;
    }
}

/// <summary>
/// Handler para obtener actividad de un empleado
/// </summary>
public class GetPlatformEmployeeActivityQueryHandler : IRequestHandler<GetPlatformEmployeeActivityQuery, PaginatedResult<EmployeeActivityDto>>
{
    private readonly IPlatformEmployeeRepository _employeeRepository;
    private readonly ILogger<GetPlatformEmployeeActivityQueryHandler> _logger;

    public GetPlatformEmployeeActivityQueryHandler(
        IPlatformEmployeeRepository employeeRepository,
        ILogger<GetPlatformEmployeeActivityQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<EmployeeActivityDto>> Handle(GetPlatformEmployeeActivityQuery request, CancellationToken cancellationToken)
    {
        var (activities, totalCount) = await _employeeRepository.GetActivityAsync(
            request.EmployeeId,
            request.From ?? DateTime.UtcNow.AddMonths(-1),
            request.To ?? DateTime.UtcNow,
            request.Page,
            request.PageSize
        );

        var dtos = activities.Select(a => new EmployeeActivityDto
        {
            Id = a.Id,
            Action = a.Action,
            Description = a.Description,
            TargetType = a.TargetType,
            TargetId = a.TargetId,
            Timestamp = a.Timestamp,
            IpAddress = a.IpAddress
        }).ToList();

        return new PaginatedResult<EmployeeActivityDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
