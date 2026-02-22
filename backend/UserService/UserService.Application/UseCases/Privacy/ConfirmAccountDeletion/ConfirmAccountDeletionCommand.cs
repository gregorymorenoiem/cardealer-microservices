using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.ConfirmAccountDeletion;

/// <summary>
/// Command para confirmar eliminación de cuenta
/// </summary>
public record ConfirmAccountDeletionCommand(
    Guid UserId,
    string ConfirmationCode,
    string Password,
    string? Email = null
) : IRequest<bool>;

/// <summary>
/// Handler para ConfirmAccountDeletionCommand
/// </summary>
public class ConfirmAccountDeletionCommandHandler : IRequestHandler<ConfirmAccountDeletionCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPrivacyRequestRepository _privacyRepository;

    public ConfirmAccountDeletionCommandHandler(
        IUserRepository userRepository,
        IPrivacyRequestRepository privacyRepository)
    {
        _userRepository = userRepository;
        _privacyRepository = privacyRepository;
    }

    public async Task<bool> Handle(ConfirmAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve user (UserService ID may differ from AuthService ID)
        Domain.Entities.User? user = null;
        if (!string.IsNullOrEmpty(request.Email))
            user = await _userRepository.GetByEmailAsync(request.Email);
        user ??= await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado.");

        // 2. Find pending deletion request matching the confirmation code
        var privacyRequest = await _privacyRepository.GetByConfirmationCodeAsync(user.Id, request.ConfirmationCode);

        if (privacyRequest == null)
            throw new InvalidOperationException("Código de confirmación inválido o no existe una solicitud de eliminación pendiente.");

        // 3. Verify code has not expired (24-hour window from creation)
        if (privacyRequest.ExpiresAt.HasValue && privacyRequest.ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("El código de confirmación ha expirado. Por favor, inicia una nueva solicitud de eliminación.");

        // 4. Verify password — skip for OAuth users whose hash is a sentinel value
        var isOAuthUser = string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash == "SYNCED_FROM_AUTH";
        if (!isOAuthUser && !string.IsNullOrEmpty(request.Password))
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new InvalidOperationException("Contraseña incorrecta.");
        }

        // 5. Mark as confirmed — background worker will anonymize after grace period
        privacyRequest.IsConfirmed = true;
        privacyRequest.Status = PrivacyRequestStatus.Processing;

        await _privacyRepository.UpdateAsync(privacyRequest);
        return true;
    }
}
