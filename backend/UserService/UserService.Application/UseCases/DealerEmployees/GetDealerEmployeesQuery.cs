using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.DealerEmployees;

/// <summary>
/// Query to get all employees for a dealer
/// </summary>
public record GetDealerEmployeesQuery(Guid DealerId) : IRequest<List<DealerEmployeeDto>>;

public class GetDealerEmployeesQueryHandler : IRequestHandler<GetDealerEmployeesQuery, List<DealerEmployeeDto>>
{
    private readonly IDealerEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetDealerEmployeesQueryHandler> _logger;

    public GetDealerEmployeesQueryHandler(
        IDealerEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        ILogger<GetDealerEmployeesQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<DealerEmployeeDto>> Handle(GetDealerEmployeesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employees for dealer {DealerId}", request.DealerId);

        var employees = await _employeeRepository.GetByDealerIdAsync(request.DealerId);

        var result = new List<DealerEmployeeDto>();
        foreach (var employee in employees)
        {
            var user = await _userRepository.GetByIdAsync(employee.UserId);
            result.Add(new DealerEmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                DealerId = employee.DealerId,
                UserFullName = user?.FullName ?? "Sin nombre",
                UserEmail = user?.Email ?? "Sin email",
                Role = employee.DealerRole.ToString(),
                Status = employee.Status.ToString(),
                InvitationDate = employee.InvitationDate,
                ActivationDate = employee.ActivationDate,
                Permissions = "[]" // TODO: implement permissions
            });
        }

        _logger.LogInformation("Found {Count} employees for dealer {DealerId}", result.Count, request.DealerId);

        return result;
    }
}
