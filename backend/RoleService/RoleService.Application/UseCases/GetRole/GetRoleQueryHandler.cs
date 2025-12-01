using RoleService.Application.DTOs;
using RoleService.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace RoleService.Application.UseCases.GetError
{
    public class GetErrorQueryHandler : IRequestHandler<GetErrorQuery, GetErrorResponse?>
    {
        private readonly IRoleRepository _RoleRepository;

        public GetErrorQueryHandler(IRoleRepository RoleRepository)
        {
            _RoleRepository = RoleRepository;
        }

        public async Task<GetErrorResponse?> Handle(GetErrorQuery query, CancellationToken cancellationToken)
        {
            var Role = await _RoleRepository.GetByIdAsync(query.Request.Id);
            if (Role == null) 
                return null; // Esto está bien con el tipo nullable

            return new GetErrorResponse(
                Role.Id,
                Role.ServiceName,
                Role.ExceptionType,
                Role.Message,
                Role.StackTrace,
                Role.OccurredAt,
                Role.Endpoint,
                Role.HttpMethod,
                Role.StatusCode,
                Role.UserId,
                Role.Metadata
            );
        }
    }
}
