using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Application.Services;
using MediatR;
using AuthService.Application.DTOs.Auth;
using AuthService.Domain.Interfaces;
using CarDealer.Contracts.Events.Auth;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Commands.Login;

/// <summary>
/// US-18.3: Login handler with CAPTCHA verification after 2 failed attempts.
/// AUTH-SEC-005: Revoked device detection and verification.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IRequestContext _requestContext;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ICaptchaService _captchaService;
    private readonly IDistributedCache _cache;
    private readonly IAuthNotificationService _notificationService;
    private readonly IRevokedDeviceService _revokedDeviceService;
    private readonly ILogger<LoginCommandHandler> _logger;

    private const int CAPTCHA_REQUIRED_AFTER_ATTEMPTS = 2;
    private const int SECURITY_ALERT_THRESHOLD = 3;
    private const int MAX_FAILED_ATTEMPTS = 5;
    private const int LOCKOUT_MINUTES = 30;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository sessionRepository,
        IEventPublisher eventPublisher,
        IRequestContext requestContext,
        ITwoFactorService twoFactorService,
        ICaptchaService captchaService,
        IDistributedCache cache,
        IAuthNotificationService notificationService,
        IRevokedDeviceService revokedDeviceService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _eventPublisher = eventPublisher;
        _requestContext = requestContext;
        _twoFactorService = twoFactorService;
        _captchaService = captchaService;
        _cache = cache;
        _notificationService = notificationService;
        _revokedDeviceService = revokedDeviceService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // US-18.3: Check if CAPTCHA is required based on failed attempts
        var failedAttemptsKey = $"login_failed:{request.Email.ToLowerInvariant()}";
        var failedAttemptsStr = await _cache.GetStringAsync(failedAttemptsKey, cancellationToken);
        var failedAttempts = 0;
        if (!string.IsNullOrEmpty(failedAttemptsStr))
        {
            int.TryParse(failedAttemptsStr, out failedAttempts);
        }

        if (_captchaService.IsCaptchaRequired(failedAttempts))
        {
            if (string.IsNullOrEmpty(request.CaptchaToken))
            {
                throw new BadRequestException("CAPTCHA verification required. Please complete the CAPTCHA challenge.");
            }

            var captchaValid = await _captchaService.VerifyAsync(
                request.CaptchaToken, 
                "login", 
                _requestContext.IpAddress);

            if (!captchaValid)
            {
                _logger.LogWarning("CAPTCHA verification failed for {Email}. Score: {Score}", 
                    request.Email, _captchaService.LastScore);
                throw new BadRequestException("CAPTCHA verification failed. Please try again.");
            }
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null)
        {
            await TrackFailedLoginAttemptAsync(request.Email, null, cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // Verificar que PasswordHash no sea nulo
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            await TrackFailedLoginAttemptAsync(request.Email, user.Email, cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await TrackFailedLoginAttemptAsync(request.Email, user.Email, cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
            throw new UnauthorizedException("Please verify your email before logging in.");

        if (user.IsLockedOut())
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");

        // Clear failed attempts on successful password verification
        await _cache.RemoveAsync(failedAttemptsKey, cancellationToken);

        // AUTH-SEC-005: Check if this device was previously revoked
        var revokedDeviceCheck = await _revokedDeviceService.CheckIfDeviceIsRevokedAsync(
            user.Id,
            _requestContext.IpAddress,
            _requestContext.UserAgent,
            cancellationToken);

        if (revokedDeviceCheck.IsRevoked)
        {
            _logger.LogWarning(
                "AUTH-SEC-005: Login attempt from revoked device. User: {UserId}, Fingerprint: {Fingerprint}",
                user.Id, revokedDeviceCheck.DeviceFingerprint);

            // Return a response indicating revoked device verification is required
            // The frontend will need to handle this and call the revoked device verification flow
            return new LoginResponse(
                user.Id,
                user.Email!,
                string.Empty, // No access token
                string.Empty, // No refresh token
                DateTime.UtcNow.AddMinutes(10), // Short expiration
                false, // requiresTwoFactor
                null, // No temp token (reserved for 2FA)
                null, // No 2FA type
                true, // requiresRevokedDeviceVerification
                revokedDeviceCheck.DeviceFingerprint // Device fingerprint for verification
            );
        }

        // Verificar si requiere 2FA
        if (user.IsTwoFactorEnabled)
        {
            // Generar token temporal para 2FA
            var tempToken = _jwtGenerator.GenerateTempToken(user.Id);
            
            // Determinar el tipo de 2FA y obtener el string para el frontend
            string twoFactorTypeString = "authenticator"; // default
            
            if (user.TwoFactorAuth != null)
            {
                var twoFactorType = user.TwoFactorAuth.PrimaryMethod;
                
                // Map enum to string for frontend
                twoFactorTypeString = twoFactorType switch
                {
                    TwoFactorAuthType.SMS => "sms",
                    TwoFactorAuthType.Email => "email",
                    _ => "authenticator"
                };
                
                // Enviar código 2FA automáticamente para SMS y Email
                if (twoFactorType == TwoFactorAuthType.SMS || twoFactorType == TwoFactorAuthType.Email)
                {
                    await _twoFactorService.SendTwoFactorCodeAsync(user.Id, twoFactorType);
                }
            }

            return new LoginResponse(
                user.Id,
                user.Email!,
                string.Empty, // No access token yet
                string.Empty, // No refresh token yet
                DateTime.UtcNow.AddMinutes(5), // Short expiration for 2FA
                true, // requiresTwoFactor
                tempToken,
                twoFactorTypeString // Include 2FA type for frontend
            );
        }

        // Flujo normal sin 2FA
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var sessionExpiresAt = DateTime.UtcNow.AddDays(7);

        var refreshTokenEntity = new RefreshTokenEntity(
            user.Id,
            refreshTokenValue,
            sessionExpiresAt,
            _requestContext.IpAddress
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        // Parse device info once
        var deviceInfo = ParseDeviceInfo(_requestContext.UserAgent);
        var browser = ParseBrowser(_requestContext.UserAgent);
        var operatingSystem = ParseOperatingSystem(_requestContext.UserAgent);

        // Check if there's an existing active session for the same device/browser/IP
        var existingSession = await _sessionRepository.GetActiveSessionByDeviceAsync(
            user.Id,
            deviceInfo,
            browser,
            _requestContext.IpAddress,
            cancellationToken);

        if (existingSession != null)
        {
            // Reuse existing session - update with new refresh token
            existingSession.RenewSession(refreshTokenEntity.Id.ToString(), sessionExpiresAt);
            await _sessionRepository.UpdateAsync(existingSession, cancellationToken);
            _logger.LogInformation("Renewed existing session {SessionId} for user {UserId}", existingSession.Id, user.Id);
        }
        else
        {
            // Create new session only if no matching session exists
            var userSession = new UserSession(
                userId: user.Id,
                refreshTokenId: refreshTokenEntity.Id.ToString(),
                deviceInfo: deviceInfo,
                browser: browser,
                operatingSystem: operatingSystem,
                ipAddress: _requestContext.IpAddress,
                expiresAt: sessionExpiresAt
            );
            await _sessionRepository.AddAsync(userSession, cancellationToken);
            _logger.LogInformation("Created new session {SessionId} for user {UserId}", userSession.Id, user.Id);
        }

        user.ResetAccessFailedCount();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Publish UserLoggedInEvent
        var userLoggedInEvent = new UserLoggedInEvent
        {
            UserId = Guid.Parse(user.Id),
            Email = user.Email!,
            LoggedInAt = DateTime.UtcNow,
            IpAddress = _requestContext.IpAddress,
            UserAgent = _requestContext.UserAgent
        };
        await _eventPublisher.PublishAsync(userLoggedInEvent, cancellationToken);

        return new LoginResponse(
            user.Id,
            user.Email!,
            accessToken,
            refreshTokenValue,
            expiresAt,
            false // requiresTwoFactor
        );
    }

    /// <summary>
    /// US-18.2/18.3: Track failed login attempts for CAPTCHA requirement and security alerts.
    /// Sends security alerts after SECURITY_ALERT_THRESHOLD attempts.
    /// </summary>
    private async Task TrackFailedLoginAttemptAsync(string email, string? userEmail, CancellationToken cancellationToken)
    {
        var failedAttemptsKey = $"login_failed:{email.ToLowerInvariant()}";
        var attemptsStr = await _cache.GetStringAsync(failedAttemptsKey, cancellationToken);
        
        int attempts = 1;
        if (!string.IsNullOrEmpty(attemptsStr) && int.TryParse(attemptsStr, out var existing))
        {
            attempts = existing + 1;
        }

        // Update failed attempts counter
        await _cache.SetStringAsync(failedAttemptsKey, attempts.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LOCKOUT_MINUTES)
        }, cancellationToken);

        _logger.LogWarning("Failed login attempt {Attempts} for {Email} from IP {IP}", 
            attempts, email, _requestContext.IpAddress);

        // US-18.2: Send security alert after 3+ failed attempts
        if (attempts >= SECURITY_ALERT_THRESHOLD && !string.IsNullOrEmpty(userEmail))
        {
            try
            {
                var alert = new SecurityAlertDto(
                    AlertType: "FailedLoginAttempts",
                    IpAddress: _requestContext.IpAddress,
                    AttemptCount: attempts,
                    Timestamp: DateTime.UtcNow,
                    DeviceInfo: _requestContext.UserAgent
                );
                await _notificationService.SendSecurityAlertAsync(userEmail, alert);
                _logger.LogInformation("Security alert sent to {Email} after {Attempts} failed login attempts", 
                    userEmail, attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security alert for {Email}", email);
            }
        }

        // Log when CAPTCHA will be required
        if (attempts == CAPTCHA_REQUIRED_AFTER_ATTEMPTS)
        {
            _logger.LogInformation("CAPTCHA will be required for next login attempt from {Email}", email);
        }
    }

    /// <summary>
    /// Parse device info from User-Agent string
    /// </summary>
    private static string ParseDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";
        
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            return "Mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
            return "Tablet";
        return "Desktop";
    }

    /// <summary>
    /// Parse browser name from User-Agent string
    /// </summary>
    private static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        
        if (userAgent.Contains("Edg/")) return "Microsoft Edge";
        if (userAgent.Contains("Chrome/") && !userAgent.Contains("Chromium")) return "Chrome";
        if (userAgent.Contains("Firefox/")) return "Firefox";
        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
        return "Unknown Browser";
    }

    /// <summary>
    /// Parse operating system from User-Agent string
    /// </summary>
    private static string ParseOperatingSystem(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        
        if (userAgent.Contains("Windows NT 10")) return "Windows 10/11";
        if (userAgent.Contains("Windows NT")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Linux") && !userAgent.Contains("Android")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        return "Unknown OS";
    }
}
