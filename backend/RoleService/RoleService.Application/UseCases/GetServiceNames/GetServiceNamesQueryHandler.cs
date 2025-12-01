using RoleService.Application.DTOs;
using RoleService.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoleService.Application.UseCases.GetServiceNames
{
    public class GetServiceNamesQueryHandler : IRequestHandler<GetServiceNamesQuery, GetServiceNamesResponse>
    {
        private readonly IRoleRepository _RoleRepository;

        public GetServiceNamesQueryHandler(IRoleRepository RoleRepository)
        {
            _RoleRepository = RoleRepository;
        }

        public async Task<GetServiceNamesResponse> Handle(GetServiceNamesQuery query, CancellationToken cancellationToken)
        {
            var serviceNames = await _RoleRepository.GetServiceNamesAsync();
            return new GetServiceNamesResponse(serviceNames.ToList());
        }
    }
}
