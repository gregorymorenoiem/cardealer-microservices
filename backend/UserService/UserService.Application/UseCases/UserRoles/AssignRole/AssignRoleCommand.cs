using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Application.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.UserRoles.AssignRole
{
    public record AssignRoleToUserCommand(
        Guid UserId,
        Guid RoleId,
        string AssignedBy
    ) : IRequest<Guid>;

    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleServiceClient _roleServiceClient;
        private readonly IAuditServiceClient _auditClient;
        private readonly INotificationServiceClient _notificationClient;

        public AssignRoleToUserCommandHandler(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleServiceClient roleServiceClient,
            IAuditServiceClient auditClient,
            INotificationServiceClient notificationClient)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleServiceClient = roleServiceClient;
            _auditClient = auditClient;
            _notificationClient = notificationClient;
        }

        public async Task<Guid> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar que el usuario existe
            if (!await _userRepository.ExistsAsync(request.UserId))
            {
                throw new NotFoundException($"User {request.UserId} not found");
            }

            // 2. Verificar que el rol existe en RoleService
            var roleExists = await _roleServiceClient.RoleExistsAsync(request.RoleId);
            if (!roleExists)
            {
                throw new NotFoundException($"Role {request.RoleId} not found in RoleService");
            }

            // 3. Verificar si el usuario ya tiene este rol asignado
            var existingAssignment = await _userRoleRepository.GetByUserAndRoleAsync(
                request.UserId,
                request.RoleId);

            if (existingAssignment != null && existingAssignment.IsActive)
            {
                throw new BadRequestException("User already has this role assigned");
            }

            // 4. Si existía pero estaba revocado, reactivar
            if (existingAssignment != null && !existingAssignment.IsActive)
            {
                existingAssignment.IsActive = true;
                existingAssignment.AssignedAt = DateTime.UtcNow;
                existingAssignment.AssignedBy = request.AssignedBy;
                existingAssignment.RevokedAt = null;
                existingAssignment.RevokedBy = null;

                await _userRoleRepository.UpdateAsync(existingAssignment);
                return existingAssignment.Id;
            }

            // 5. Crear nueva asignación
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RoleId = request.RoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = request.AssignedBy,
                IsActive = true
            };

            await _userRoleRepository.AddAsync(userRole);

            // Auditoría
            _ = _auditClient.LogRoleAssignedAsync(request.UserId, request.RoleId, request.AssignedBy);

            // Notificación (obtener info del rol y usuario)
            var user = await _userRepository.GetByIdAsync(request.UserId);
            var role = await _roleServiceClient.GetRoleByIdAsync(request.RoleId);
            if (user != null && role != null)
            {
                _ = _notificationClient.SendRoleAssignedNotificationAsync(user.Email, role.Name);
            }

            return userRole.Id;
        }
    }
}
