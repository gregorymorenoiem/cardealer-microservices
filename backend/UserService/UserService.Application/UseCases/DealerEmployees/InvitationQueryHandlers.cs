using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.DealerEmployees;

/// <summary>
/// Handler para obtener detalles de una invitación por token
/// </summary>
public class GetInvitationDetailsQueryHandler : IRequestHandler<GetInvitationDetailsQuery, InvitationDetailsDto?>
{
    private readonly IDealerEmployeeRepository _employeeRepository;
    private readonly IDealerRepository _dealerRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetInvitationDetailsQueryHandler> _logger;

    public GetInvitationDetailsQueryHandler(
        IDealerEmployeeRepository employeeRepository,
        IDealerRepository dealerRepository,
        IUserRepository userRepository,
        ILogger<GetInvitationDetailsQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _dealerRepository = dealerRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<InvitationDetailsDto?> Handle(GetInvitationDetailsQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _employeeRepository.GetInvitationByTokenAsync(request.Token);
        if (invitation == null)
        {
            return null;
        }

        var dealer = await _dealerRepository.GetByIdAsync(invitation.DealerId);
        var invitedByUser = await _userRepository.GetByIdAsync(invitation.InvitedBy);
        var existingUser = await _userRepository.GetByEmailAsync(invitation.Email);

        var roleDescription = GetRoleDescription(invitation.DealerRole);

        return new InvitationDetailsDto
        {
            Email = invitation.Email,
            DealerName = dealer?.BusinessName ?? dealer?.TradeName ?? "Dealer",
            DealerLogo = dealer?.LogoUrl ?? string.Empty,
            Role = invitation.DealerRole.ToString(),
            RoleDescription = roleDescription,
            InvitedByName = invitedByUser != null ? $"{invitedByUser.FirstName} {invitedByUser.LastName}" : "Administrador",
            ExpirationDate = invitation.ExpirationDate,
            IsExpired = invitation.ExpirationDate < DateTime.UtcNow || invitation.Status == InvitationStatus.Expired,
            UserExists = existingUser != null
        };
    }

    private static string GetRoleDescription(DealerRole role) => role switch
    {
        DealerRole.Owner => "Control total del dealer, incluyendo facturación y empleados",
        DealerRole.Manager => "Gestión completa del negocio excepto facturación",
        DealerRole.SalesManager => "Gestión de ventas, leads y equipo de vendedores",
        DealerRole.InventoryManager => "Gestión del inventario de vehículos",
        DealerRole.Salesperson => "Gestión de leads asignados y vehículos",
        DealerRole.Viewer => "Acceso de solo lectura a información del dealer",
        _ => "Rol de empleado"
    };
}

/// <summary>
/// Handler para rechazar una invitación
/// </summary>
public class DeclineInvitationCommandHandler : IRequestHandler<DeclineInvitationCommand, Unit>
{
    private readonly IDealerEmployeeRepository _employeeRepository;
    private readonly ILogger<DeclineInvitationCommandHandler> _logger;

    public DeclineInvitationCommandHandler(
        IDealerEmployeeRepository employeeRepository,
        ILogger<DeclineInvitationCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _employeeRepository.GetInvitationByTokenAsync(request.Token);
        if (invitation == null)
        {
            _logger.LogWarning("Attempted to decline non-existent invitation with token {Token}", request.Token);
            return Unit.Value; // Silently ignore
        }

        if (invitation.Status == InvitationStatus.Pending)
        {
            invitation.Status = InvitationStatus.Revoked;
            await _employeeRepository.UpdateInvitationAsync(invitation);
            _logger.LogInformation("Invitation {InvitationId} was declined", invitation.Id);
        }

        return Unit.Value;
    }
}
