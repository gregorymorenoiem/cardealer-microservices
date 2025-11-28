// AuthService.Application/UseCases/Register/RegisterCommandHandler.cs
using ErrorService.Shared.Exceptions;
using AuthService.Domain.Entities;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IAuthNotificationService _notificationService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IVerificationTokenRepository verificationTokenRepository,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _notificationService = notificationService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists.");

        // Hash password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create user
        var user = new ApplicationUser(request.UserName, request.Email, passwordHash);
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        // Save refresh token
        var refreshTokenEntity = new RefreshTokenEntity(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1" // TODO: Get actual IP from context
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        // Create verification token and send email
        var verificationToken = new VerificationToken(
            user.Id,
            VerificationTokenType.EmailVerification,
            TimeSpan.FromHours(24)
        );

        await _verificationTokenRepository.AddAsync(verificationToken);

        // CORRECCIÓN: Usar operador de supresión nula (!) ya que sabemos que no son nulos
        await _notificationService.SendEmailConfirmationAsync(user.Email!, verificationToken.Token);

        // Send welcome email
        await _notificationService.SendWelcomeEmailAsync(user.Email!, user.UserName!);

        // CORRECCIÓN: Usar operador de supresión nula (!) para evitar warnings
        return new RegisterResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            accessToken,
            refreshTokenValue,
            expiresAt
        );
    }
}