using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.DealerModules;

/// <summary>
/// Query to get details of all available modules
/// </summary>
public record GetModulesDetailsQuery() : IRequest<List<ModuleDetailsDto>>;

public class GetModulesDetailsQueryHandler : IRequestHandler<GetModulesDetailsQuery, List<ModuleDetailsDto>>
{
    private readonly IModuleRepository _moduleRepository;
    private readonly ILogger<GetModulesDetailsQueryHandler> _logger;

    public GetModulesDetailsQueryHandler(
        IModuleRepository moduleRepository,
        ILogger<GetModulesDetailsQueryHandler> logger)
    {
        _moduleRepository = moduleRepository;
        _logger = logger;
    }

    public async Task<List<ModuleDetailsDto>> Handle(GetModulesDetailsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all available modules");

        var modules = await _moduleRepository.GetAllActiveAsync();

        var moduleDtos = modules.Select(m => new ModuleDetailsDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description ?? "Sin descripción",
            Icon = m.Icon,
            Price = m.Price,
            Features = m.Features ?? new List<string>(),
            IsActive = m.IsActive
        }).ToList();

        _logger.LogInformation("Found {Count} available modules", moduleDtos.Count);

        return moduleDtos;
    }
}
