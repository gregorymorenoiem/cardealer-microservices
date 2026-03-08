using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Application.UseCases.Users.CreateUser
{
    public record CreateUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string PhoneNumber
    ) : IRequest<Guid>;

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditServiceClient _auditClient;
        private readonly INotificationServiceClient _notificationClient;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IAuditServiceClient auditClient,
            INotificationServiceClient notificationClient,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _auditClient = auditClient;
            _notificationClient = notificationClient;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Validar que el email no exista
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new BadRequestException("An account with this email already exists");
            }

            // Hash del password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Crear usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Auditoría (fire-and-forget with error logging)
            _ = SafeFireAndForgetAsync(
                _auditClient.LogUserCreatedAsync(user.Id, user.Email, "system"),
                "audit log user creation", user.Id);

            // Notificación de bienvenida (fire-and-forget with error logging)
            _ = SafeFireAndForgetAsync(
                _notificationClient.SendWelcomeEmailAsync(user.Email, user.FirstName, user.LastName),
                "send welcome email", user.Id);

            _logger.LogInformation("User created successfully: {UserId}", user.Id);

            return user.Id;
        }

        private async Task SafeFireAndForgetAsync(Task task, string operation, Guid userId)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to {Operation} for user {UserId}", operation, userId);
            }
        }
    }
}
