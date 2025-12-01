using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases.Users.DeleteUser
{
    public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditServiceClient _auditClient;

        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            IAuditServiceClient auditClient)
        {
            _userRepository = userRepository;
            _auditClient = auditClient;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User {request.UserId} not found");
            }

            await _userRepository.DeleteAsync(request.UserId);

            // Auditoría
            _ = _auditClient.LogUserDeletedAsync(user.Id, user.Email, "system");

            return Unit.Value;
        }
    }
}
