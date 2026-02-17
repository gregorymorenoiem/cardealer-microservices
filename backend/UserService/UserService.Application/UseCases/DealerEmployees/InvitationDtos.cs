using MediatR;

namespace UserService.Application.UseCases.DealerEmployees;

/// <summary>
/// Invitation details shown before acceptance
/// </summary>
public class InvitationDetailsDto
{
    public string Email { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public string DealerLogo { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public bool IsExpired { get; set; }
    public bool UserExists { get; set; }
}

// ============================================================================
// QUERIES & COMMANDS
// ============================================================================

/// <summary>
/// Query para obtener detalles de invitación
/// </summary>
public record GetInvitationDetailsQuery(string Token) : IRequest<InvitationDetailsDto?>;

/// <summary>
/// Command para rechazar una invitación
/// </summary>
public record DeclineInvitationCommand(string Token) : IRequest<Unit>;
