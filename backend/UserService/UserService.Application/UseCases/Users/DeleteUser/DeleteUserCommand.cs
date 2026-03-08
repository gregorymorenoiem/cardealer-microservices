using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DeleteUserCommandHandler> _logger;

        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            IAuditServiceClient auditClient,
            ILogger<DeleteUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _auditClient = auditClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User {request.UserId} not found");
            }

            await _userRepository.DeleteAsync(request.UserId);

            // Auditoría (fire-and-forget with error logging)
            _ = SafeFireAndForgetAsync(
                _auditClient.LogUserDeletedAsync(user.Id, user.Email, "system"),
                user.Id);

            return Unit.Value;
        }

        private async Task SafeFireAndForgetAsync(Task task, Guid userId)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log audit for user deletion {UserId}", userId);
            }
        }
    }
}
