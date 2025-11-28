<#
.SYNOPSIS
  Agrega los archivos especificados en un único documento,
  incluyendo la ruta relativa de cada uno y su contenido.

.DESCRIPTION
  - Procesa únicamente los archivos listados en el array `$filesToInclude`.
  - Escribe un encabezado y luego el contenido completo de cada archivo en 'code_bundle.txt'.
  - No recorre el árbol de directorios; usa la lista explícita.
#>

# 1. Define la raíz del proyecto y el archivo de salida
$rootPath   = 'C:\Users\gmoreno\source\repos\cardealer'
$outputFile = Join-Path $rootPath 'code_bundle.txt'

# 2. Lista de rutas relativas a incluir (solo archivos; sin dll; sin carpetas)
$filesToInclude = @(
    # # --- AuthService.Api ---
    # 'backend\AuthService\AuthService.Api\appsettings.Development.json',
    # 'backend\AuthService\AuthService.Api\appsettings.json',
    # 'backend\AuthService\AuthService.Api\appsettings.Production.json',
    # 'backend\AuthService\AuthService.Api\AuthService.Api.csproj',
    # 'backend\AuthService\AuthService.Api\AuthService.Api.http',
    # 'backend\AuthService\AuthService.Api\Dockerfile.dev',
    # 'backend\AuthService\AuthService.Api\Dockerfile.prod',
    # 'backend\AuthService\AuthService.Api\Program.cs',

    # # --- AuthService.Application ---
    # 'backend\AuthService\AuthService.Application\Services\AuthService.cs',
    # 'backend\AuthService\AuthService.Application\Services\IAuthService.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\ForgotPassword\ForgotPasswordCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\ForgotPassword\ForgotPasswordCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Login\LoginCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Login\LoginCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Login\LoginCommandValidator.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Logout\LogoutCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Logout\LogoutCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\RefreshToken\RefreshTokenCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\RefreshToken\RefreshTokenCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Register\RegisterCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Register\RegisterCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\Register\RegisterCommandValidator.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\ResetPassword\ResetPasswordCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\ResetPassword\ResetPasswordCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\VerifyEmail\VerifyEmailCommand.cs',
    # 'backend\AuthService\AuthService.Application\UseCases\VerifyEmail\VerifyEmailCommandHandler.cs',
    # 'backend\AuthService\AuthService.Application\AuthService.Application.csproj',

    # # --- AuthService.Domain ---
    # 'backend\AuthService\AuthService.Domain\Entities\ApplicationUser.cs',
    # 'backend\AuthService\AuthService.Domain\Entities\RefreshToken.cs',
    # 'backend\AuthService\AuthService.Domain\Entities\VerificationToken.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IEmailSender.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IEmailVerificationService.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IJwtGenerator.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IPasswordHasher.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IPasswordResetService.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IRefreshTokenRepository.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IUserRepository.cs',
    # 'backend\AuthService\AuthService.Domain\Interfaces\IVerificationTokenRepository.cs',
    # 'backend\AuthService\AuthService.Domain\AuthService.Domain.csproj',

    # # --- AuthService.Infrastructure ---
    # 'backend\AuthService\AuthService.Infrastructure\Caching\EFSecondLevelCacheConfig.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Migrations\20250618160312_InitAuthService.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Migrations\20250618160312_InitAuthService.Designer.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Migrations\ApplicationDbContextModelSnapshot.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Persistence\ApplicationDbContext.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Persistence\EfRefreshTokenRepository.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Persistence\EfUserRepository.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Persistence\EfVerificationTokenRepository.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Security\BcryptPasswordHasher.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Security\EmailPasswordResetService.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Security\EmailVerificationService.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Security\JwtGenerator.cs',
    # 'backend\AuthService\AuthService.Infrastructure\Security\SmtpEmailSender.cs',
    # 'backend\AuthService\AuthService.Infrastructure\AuthService.Infrastructure.csproj',

    # # --- AuthService.Shared ---
    # 'backend\AuthService\AuthService.Shared\ApiResponse.cs',
    # 'backend\AuthService\AuthService.Shared\AuthService.Shared.csproj',
    # 'backend\AuthService\AuthService.Shared\EmailSettings.cs',
    # 'backend\AuthService\AuthService.Shared\JwtSettings.cs',

    # # --- Solución AuthService ---
    # 'backend\AuthService\AuthService.sln',

    # # --- ErrorService.Api ---
    # 'backend\ErrorService\ErrorService.Api\Controllers\HealthController.cs',
    # 'backend\ErrorService\ErrorService.Api\appsettings.Development.json',
    # 'backend\ErrorService\ErrorService.Api\appsettings.json',
    # 'backend\ErrorService\ErrorService.Api\Dockerfile.dev',
    # 'backend\ErrorService\ErrorService.Api\Dockerfile.prod',
    # 'backend\ErrorService\ErrorService.Api\ErrorService.Api.csproj',
    # 'backend\ErrorService\ErrorService.Api\ErrorService.Api.http',
    # 'backend\ErrorService\ErrorService.Api\Program.cs',

    # # --- ErrorService.Shared ---
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\AppException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\BadGatewayException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\BadRequestException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\ConflictException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\ForbiddenException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\GatewayTimeoutException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\LengthRequiredException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\MethodNotAllowedException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\NotAcceptableException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\NotFoundException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\NotImplementedServiceException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\PreconditionFailedException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\ServiceUnavailableException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\TooManyRequestsException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\UnauthorizedException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\UnprocessableEntityException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Exceptions\UnsupportedMediaTypeException.cs',
    # 'backend\ErrorService\ErrorService.Shared\Middleware\ErrorHandlingMiddleware.cs',
    # 'backend\ErrorService\ErrorService.Shared\ApiResponse.cs',
    # 'backend\ErrorService\ErrorService.Shared\ErrorService.Shared.csproj',

    # --- Solución ErrorService ---
    # 'backend\ErrorService\ErrorService.sln',

    # --- NotificationService.Api ---
    'backend\NotificationService\NotificationService.Api\Controllers\NotificationsController.cs',
    'backend\NotificationService\NotificationService.Api\Controllers\WebhooksController.cs',
    'backend\NotificationService\NotificationService.Api\appsettings.Development.json',
    'backend\NotificationService\NotificationService.Api\appsettings.json',
    'backend\NotificationService\NotificationService.Api\appsettings.Production.json',
    'backend\NotificationService\NotificationService.Api\Dockerfile.dev',
    'backend\NotificationService\NotificationService.Api\Dockerfile.prod',
    'backend\NotificationService\NotificationService.Api\NotificationService.Api.csproj',
    'backend\NotificationService\NotificationService.Api\NotificationService.Api.http',
    'backend\NotificationService\NotificationService.Api\Program.cs',

    # --- NotificationService.Application ---
    'backend\NotificationService\NotificationService.Application\DTOs\EmailNotificationDto.cs',
    'backend\NotificationService\NotificationService.Application\DTOs\NotificationResponse.cs',
    'backend\NotificationService\NotificationService.Application\DTOs\NotificationStatusDto.cs',
    'backend\NotificationService\NotificationService.Application\DTOs\PushNotificationDto.cs',
    'backend\NotificationService\NotificationService.Application\DTOs\SmsNotificationDto.cs',
    'backend\NotificationService\NotificationService.Application\Interfaces\External\IEmailProvider.cs',
    'backend\NotificationService\NotificationService.Application\Interfaces\External\IPushNotificationProvider.cs',
    'backend\NotificationService\NotificationService.Application\Interfaces\External\ISmsProvider.cs',
    'backend\NotificationService\NotificationService.Application\Interfaces\External\ITemplateEngine.cs',
    'backend\NotificationService\NotificationService.Application\Services\EmailService.cs',
    'backend\NotificationService\NotificationService.Application\Services\IEmailService.cs',
    'backend\NotificationService\NotificationService.Application\Services\INotificationService.cs',
    'backend\NotificationService\NotificationService.Application\Services\IPushNotificationService.cs',
    'backend\NotificationService\NotificationService.Application\Services\ISmsService.cs',
    'backend\NotificationService\NotificationService.Application\Services\NotificationService.cs',
    'backend\NotificationService\NotificationService.Application\Services\PushNotificationService.cs',
    'backend\NotificationService\NotificationService.Application\Services\SmsService.cs',
    'backend\NotificationService\NotificationService.Application\NotificationService.Application.csproj',

    # --- NotificationService.Domain ---
    'backend\NotificationService\NotificationService.Domain\Entities\Notification.cs',
    'backend\NotificationService\NotificationService.Domain\Entities\NotificationLog.cs',
    'backend\NotificationService\NotificationService.Domain\Entities\NotificationQueue.cs',
    'backend\NotificationService\NotificationService.Domain\Entities\NotificationTemplate.cs',
    'backend\NotificationService\NotificationService.Domain\Enums\NotificationProvider.cs',
    'backend\NotificationService\NotificationService.Domain\Enums\NotificationStatus.cs',
    'backend\NotificationService\NotificationService.Domain\Enums\NotificationType.cs',
    'backend\NotificationService\NotificationService.Domain\Enums\PriorityLevel.cs',
    'backend\NotificationService\NotificationService.Domain\Enums\QueueStatus.cs',
    'backend\NotificationService\NotificationService.Domain\Events\NotificationSentEvent.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\IMessageBus.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\INotificationLogRepository.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\INotificationQueueRepository.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\INotificationRepository.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\INotificationTemplateRepository.cs',
    'backend\NotificationService\NotificationService.Domain\Interfaces\IUnitOfWork.cs',
    'backend\NotificationService\NotificationService.Domain\ValueObjects\EmailAddress.cs',
    'backend\NotificationService\NotificationService.Domain\ValueObjects\MessageContent.cs',
    'backend\NotificationService\NotificationService.Domain\ValueObjects\NotificationSubject.cs',
    'backend\NotificationService\NotificationService.Domain\ValueObjects\PhoneNumber.cs',
    'backend\NotificationService\NotificationService.Domain\NotificationService.Domain.csproj',

    # --- NotificationService.Infrastructure ---
    'backend\NotificationService\NotificationService.Infrastructure\External\FirebasePushService.cs',
    'backend\NotificationService\NotificationService.Infrastructure\External\SendGridEmailService.cs',
    'backend\NotificationService\NotificationService.Infrastructure\External\TwilioSmsService.cs',
    'backend\NotificationService\NotificationService.Infrastructure\MessageBus\AzureServiceBus.cs',
    'backend\NotificationService\NotificationService.Infrastructure\MessageBus\InMemoryMessageBus.cs',
    'backend\NotificationService\NotificationService.Infrastructure\MessageBus\RabbitMQMessageBus.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Migrations\ApplicationDbContextModelSnapshot.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\ApplicationDbContext.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\Configurations\*',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\EfNotificationLogRepository.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\EfNotificationQueueRepository.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\EfNotificationRepository.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Persistence\EfNotificationTemplateRepository.cs',
    'backend\NotificationService\NotificationService.Infrastructure\Templates\EmailTemplates\*',
    'backend\NotificationService\NotificationService.Infrastructure\Templates\TemplateEngine.cs',
    'backend\NotificationService\NotificationService.Infrastructure\NotificationService.Infrastructure.csproj',

    # --- NotificationService.Shared ---
    'backend\NotificationService\NotificationService.Shared\Constants\CacheConstants.cs',
    'backend\NotificationService\NotificationService.Shared\Constants\NotificationConstants.cs',
    'backend\NotificationService\NotificationService.Shared\ApiResponse.cs',
    'backend\NotificationService\NotificationService.Shared\NotificationService.Shared.csproj',
    'backend\NotificationService\NotificationService.Shared\NotificationSettings.cs',

    # --- Solución NotificationService ---
    'backend\NotificationService\NotificationService.sln',

    # # --- Gateway.Api ---
    # 'backend\Gateway\Gateway.Api\appsettings.Development.json',
    # 'backend\Gateway\Gateway.Api\appsettings.json',
    # 'backend\Gateway\Gateway.Api\appsettings.Production.json',
    # 'backend\Gateway\Gateway.Api\Dockerfile.dev',
    # 'backend\Gateway\Gateway.Api\Dockerfile.prod',
    # 'backend\Gateway\Gateway.Api\Gateway.Api.csproj',
    # 'backend\Gateway\Gateway.Api\ocelot.dev.json',
    # 'backend\Gateway\Gateway.Api\ocelot.prod.json',
    # 'backend\Gateway\Gateway.Api\Program.cs',

    # # --- Solución Gateway ---
    # 'backend\Gateway\Gateway.sln',

    # --- Docker Compose ---
    # 'backend\docker-compose.production.yml',
    'backend\docker-compose.yml'
)

# 3. Eliminar bundle existente si lo hay
if (Test-Path $outputFile) {
    Remove-Item $outputFile -Force
}

# 4. Escribir encabezado en el archivo
'## Code Bundle - Aggregated Files' | Out-File -FilePath $outputFile -Encoding UTF8
'' | Out-File -Append -FilePath $outputFile -Encoding UTF8

# 5. Iterar y agregar cada archivo
foreach ($relativePath in $filesToInclude) {
    $fullPath = Join-Path $rootPath $relativePath

    if (Test-Path $fullPath) {
        "---- File: $relativePath ----" | Out-File -Append -FilePath $outputFile -Encoding UTF8
        Get-Content -Path $fullPath | Out-File -Append -FilePath $outputFile -Encoding UTF8
        '' | Out-File -Append -FilePath $outputFile -Encoding UTF8
    } else {
        "## WARNING: File not found -> $relativePath" | Out-File -Append -FilePath $outputFile -Encoding UTF8
        '' | Out-File -Append -FilePath $outputFile -Encoding UTF8
    }
}

# 6. Mensaje de finalización
Write-Host "✅ Proceso completado. Bundle generado en: $outputFile"