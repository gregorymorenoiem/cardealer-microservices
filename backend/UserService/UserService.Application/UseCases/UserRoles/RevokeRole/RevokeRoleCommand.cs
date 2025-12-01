using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases.UserRoles.RevokeRole
{
    public record RevokeRoleFromUserCommand(
        Guid UserId,
        Guid RoleId,
        string RevokedBy
    ) : IRequest<Unit>;

    public class RevokeRoleFromUserCommandHandler : IRequestHandler<RevokeRoleFromUserCommand, Unit>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditServiceClient _auditClient;

        public RevokeRoleFromUserCommandHandler(
            IUserRoleRepository userRoleRepository,
            IAuditServiceClient auditClient)
        {
            _userRoleRepository = userRoleRepository;
            _auditClient = auditClient;
        }

        public async Task<Unit> Handle(RevokeRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            var userRole = await _userRoleRepository.GetByUserAndRoleAsync(
                request.UserId,
                request.RoleId);

            if (userRole == null || !userRole.IsActive)
            {
                throw new NotFoundException($"Active role assignment not found for user {request.UserId} and role {request.RoleId}");
            }

            // Soft delete - marcar como inactivo
            userRole.IsActive = false;
            userRole.RevokedAt = DateTime.UtcNow;
            userRole.RevokedBy = request.RevokedBy;

            await _userRoleRepository.UpdateAsync(userRole);

            // Auditoría
            _ = _auditClient.LogRoleRevokedAsync(request.UserId, request.RoleId, request.RevokedBy);

            return Unit.Value;
        }
    }
}
