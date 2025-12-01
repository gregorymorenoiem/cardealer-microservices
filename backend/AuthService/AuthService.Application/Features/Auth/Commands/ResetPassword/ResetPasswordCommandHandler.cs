using MediatR;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using AuthService.Shared.Exceptions;
using AuthService.Domain.Exceptions;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
      IUserRepository userRepository,
      IPasswordHasher passwordHasher,
      IPasswordResetTokenService tokenService,
      ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar token
            if (!_tokenService.ValidateResetToken(request.Token, out var email))
            {
                throw new BadRequestException("Invalid or expired reset token.");
            }

            // Verificar que el token es válido para el email
            if (!await _tokenService.IsTokenValidAsync(email, request.Token))
            {
                throw new BadRequestException("Invalid or expired reset token.");
            }

            // Buscar usuario
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            // <--- CORRECCIÓN: Pasa la contraseña en texto plano y el hasher.
            // La entidad ApplicationUser se encarga de hashear y verificar.
            user.UpdatePassword(request.NewPassword, _passwordHasher);
            await _userRepository.UpdateAsync(user);

            // Invalidar token
            await _tokenService.InvalidateTokenAsync(email);

            _logger.LogInformation("Password reset successfully for user {Email}", email);

            return new ResetPasswordResponse(true, "Password reset successfully.");
        }
        catch (AppException) // Excepciones como BadRequest, NotFound
        {
            throw;
        }
        catch (DomainException ex) // Ej: "La nueva contraseña no puede ser igual a la anterior"
        {
            throw new BadRequestException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token");
            throw new ServiceUnavailableException("An error occurred while resetting your password.");
        }
    }
}
