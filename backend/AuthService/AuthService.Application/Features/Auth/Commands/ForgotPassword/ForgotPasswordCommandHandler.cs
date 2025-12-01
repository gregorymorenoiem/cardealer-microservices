using MediatR;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using AuthService.Shared.Exceptions;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthNotificationService _notificationService;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
      IUserRepository userRepository,
      IAuthNotificationService notificationService,
      IPasswordResetTokenService tokenService,
      ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Por seguridad, siempre devolvemos éxito incluso si el email no existe
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
                return new ForgotPasswordResponse(true, "If the email exists, a reset link has been sent.");
            }

            // Generar token de reset
            var resetToken = _tokenService.GenerateResetToken(request.Email);

            // Enviar email
            await _notificationService.SendPasswordResetEmailAsync(request.Email, resetToken);

            _logger.LogInformation("Password reset email sent to {Email}", request.Email);

            return new ForgotPasswordResponse(true, "Password reset email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset for {Email}", request.Email);
            throw new ServiceUnavailableException("An error occurred while processing your request.");
        }
    }
}
