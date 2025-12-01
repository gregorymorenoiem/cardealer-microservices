using MediatR;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases.Users.UpdateUser
{
    public record UpdateUserCommand(
        Guid UserId,
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        bool? IsActive
    ) : IRequest<Unit>;

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditServiceClient _auditClient;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IAuditServiceClient auditClient)
        {
            _userRepository = userRepository;
            _auditClient = auditClient;
        }

        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User {request.UserId} not found");
            }

            var changes = new StringBuilder();

            // Actualizar solo los campos provistos
            if (!string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName.Trim())
            {
                changes.Append($"FirstName: {user.FirstName} → {request.FirstName.Trim()}; ");
                user.FirstName = request.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName.Trim())
            {
                changes.Append($"LastName: {user.LastName} → {request.LastName.Trim()}; ");
                user.LastName = request.LastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber.Trim())
            {
                changes.Append($"PhoneNumber: {user.PhoneNumber} → {request.PhoneNumber.Trim()}; ");
                user.PhoneNumber = request.PhoneNumber.Trim();
            }

            if (request.IsActive.HasValue && user.IsActive != request.IsActive.Value)
            {
                changes.Append($"IsActive: {user.IsActive} → {request.IsActive.Value}; ");
                user.IsActive = request.IsActive.Value;
            }

            await _userRepository.UpdateAsync(user);

            // Auditoría
            if (changes.Length > 0)
            {
                _ = _auditClient.LogUserUpdatedAsync(user.Id, changes.ToString(), "system");
            }

            return Unit.Value;
        }
    }
}
