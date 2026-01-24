using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.DealerEmployees;

/// <summary>
/// Query para obtener invitaciones pendientes de un dealer
/// </summary>
public record GetInvitationsQuery(Guid DealerId) : IRequest<List<DealerEmployeeInvitationDto>>;

public class GetInvitationsQueryHandler : IRequestHandler<GetInvitationsQuery, List<DealerEmployeeInvitationDto>>
{
    private readonly IDealerEmployeeRepository _employeeRepository;
    private readonly ILogger<GetInvitationsQueryHandler> _logger;

    public GetInvitationsQueryHandler(
        IDealerEmployeeRepository employeeRepository,
        ILogger<GetInvitationsQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<List<DealerEmployeeInvitationDto>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting pending invitations for dealer {DealerId}", request.DealerId);

        var invitations = await _employeeRepository.GetPendingInvitationsAsync(request.DealerId);

        return invitations
            .OrderByDescending(i => i.InvitationDate)
            .Select(i => new DealerEmployeeInvitationDto
            {
                Id = i.Id,
                Email = i.Email,
                Role = i.DealerRole.ToString(),
                Status = i.Status.ToString(),
                InvitationDate = i.InvitationDate,
                ExpirationDate = i.ExpirationDate
            })
            .ToList();
    }
}
