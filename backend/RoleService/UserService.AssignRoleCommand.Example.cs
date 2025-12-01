using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.UserRoles.AssignRole
{
    /// <summary>
    /// Comando para asignar un rol a un usuario
    /// </summary>
    public record AssignRoleToUserCommand(Guid UserId, Guid RoleId, string AssignedBy) : IRequest<Guid>;

    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Guid>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleServiceClient _roleServiceClient; // HTTP client para RoleService

        public AssignRoleToUserCommandHandler(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IRoleServiceClient roleServiceClient)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _roleServiceClient = roleServiceClient;
        }

        public async Task<Guid> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException($"User {request.UserId} not found");

            // 2. Verificar que el rol existe en RoleService (llamada HTTP)
            var roleExists = await _roleServiceClient.RoleExistsAsync(request.RoleId);
            if (!roleExists)
                throw new NotFoundException($"Role {request.RoleId} not found in RoleService");

            // 3. Verificar que el usuario no tenga ya este rol asignado
            var existingAssignment = await _userRoleRepository.GetByUserAndRoleAsync(
                request.UserId,
                request.RoleId);

            if (existingAssignment != null && existingAssignment.IsActive)
                throw new BadRequestException("User already has this role assigned");

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
            return userRole.Id;
        }
    }
}
