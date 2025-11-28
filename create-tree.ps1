<#
.SYNOPSIS
    Crea una estructura completa de soluciones y proyectos de .NET Core
    utilizando el CLI de 'dotnet' basado en una definición de árbol de texto.
.DESCRIPTION
    Este script analiza una cadena de texto (here-string) que define una 
    jerarquía de microservicios.
    - Usa 'dotnet new sln' para archivos .sln
    - Usa 'dotnet new webapi' para proyectos en carpetas '.Api'.
    - Usa 'dotnet new classlib' para otros proyectos (.Application, .Domain, etc.).
    - Usa 'dotnet sln add' para vincular los proyectos a sus soluciones.
    - Crea archivos vacíos para todo lo demás (.cs, .json, etc.).
.NOTES
    Autor: Gemini
    Fecha: 20-10-2025
    Versión: 3.3 (Versión limpia final con todas las correcciones)
#>

# 1. Definición de la estructura del árbol
# ASEGÚRATE DE QUE ESTE BLOQUE ESTÉ LIMPIO Y NO CONTENGA ERRORES PEGADOS.
$TreeDefinition = @"
backend/
└── MediaService/
    ├── MediaService.Api/
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── MediaService.Api.csproj
    │   ├── MediaService.Api.http
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── Program.cs
    │   └── Controllers/
    │       ├── MediaController.cs
    │       └── AdminMediaController.cs
    ├── MediaService.Application/
    │   ├── Services/
    │   │   ├── IMediaService.cs
    │   │   ├── MediaService.cs
    │   │   ├── IImageProcessor.cs
    │   │   └── ImageProcessor.cs
    │   ├── UseCases/
    │   │   ├── UploadMedia/
    │   │   │   ├── UploadMediaCommand.cs
    │   │   │   ├── UploadMediaCommandHandler.cs
    │   │   │   └── UploadMediaCommandValidator.cs
    │   │   ├── DeleteMedia/
    │   │   │   ├── DeleteMediaCommand.cs
    │   │   │   └── DeleteMediaCommandHandler.cs
    │   │   ├── GetMedia/
    │   │   │   ├── GetMediaQuery.cs
    │   │   │   └── GetMediaQueryHandler.cs
    │   │   ├── GetMediaByOwner/
    │   │   │   ├── GetMediaByOwnerQuery.cs
    │   │   │   └── GetMediaByOwnerQueryHandler.cs
    │   │   └── GeneratePresignedUrl/
    │   │       ├── GeneratePresignedUrlQuery.cs
    │   │       └── GeneratePresignedUrlQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── UploadMediaRequest.cs
    │   │   ├── UploadMediaResponse.cs
    │   │   ├── MediaResponse.cs
    │   │   ├── PresignedUrlResponse.cs
    │   │   └── MediaOwnerResponse.cs
    │   └── MediaService.Application.csproj
    ├── MediaService.Domain/
    │   ├── Entities/
    │   │   ├── MediaFile.cs
    │   │   ├── MediaVersion.cs
    │   │   └── MediaMetadata.cs
    │   ├── Interfaces/
    │   │   ├── IMediaRepository.cs
    │   │   ├── IMediaStorage.cs
    │   │   ├── IMediaVersionRepository.cs
    │   │   └── IImageOptimizer.cs
    │   ├── ValueObjects/
    │   │   ├── FileSize.cs
    │   │   ├── ImageDimensions.cs
    │   │   └── StoragePath.cs
    │   ├── Enums/
    │   │   ├── MediaType.cs
    │   │   ├── MediaStatus.cs
    │   │   ├── MediaVersionType.cs
    │   │   └── StorageProvider.cs
    │   ├── Events/
    │   │   └── MediaUploadedEvent.cs
    │   └── MediaService.Domain.csproj
    ├── MediaService.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfMediaRepository.cs
    │   │   ├── EfMediaVersionRepository.cs
    │   │   └── Configurations/
    │   │       ├── MediaFileConfiguration.cs
    │   │       └── MediaVersionConfiguration.cs
    │   ├── Migrations/
    │   │   ├── [MigrationFiles...]
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── Storage/
    │   │   ├── LocalStorageService.cs
    │   │   ├── AzureBlobStorageService.cs
    │   │   ├── CloudinaryStorageService.cs
    │   │   ├── S3StorageService.cs
    │   │   └── IMediaStorage.cs
    │   ├── ImageProcessing/
    │   │   ├── ImageOptimizer.cs
    │   │   ├── ImageResizer.cs
    │   │   ├── WebPConverter.cs
    │   │   └── ThumbnailGenerator.cs
    │   ├── External/
    │   │   └── CloudinaryService.cs
    │   └── MediaService.Infrastructure.csproj
    ├── MediaService.Shared/
    │   ├── ApiResponse.cs
    │   ├── MediaSettings.cs
    │   ├── StorageSettings.cs
    │   ├── Constants/
    │   │   ├── MediaConstants.cs
    │   │   └── CacheConstants.cs
    │   └── MediaService.Shared.csproj
    └── MediaService.sln
==================
backend/
└── VehicleService/
    ├── VehicleService.Api/
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── VehicleService.Api.csproj
    │   ├── VehicleService.Api.http
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── Program.cs
    │   └── Controllers/
    │       ├── VehiclesController.cs
    │       └── VehicleSearchController.cs
    ├── VehicleService.Application/
    │   ├── Services/
    │   │   ├── IVehicleService.cs
    │   │   ├── VehicleService.cs
    │   │   ├── IMediaIntegrationService.cs
    │   │   └── MediaIntegrationService.cs
    │   ├── UseCases/
    │   │   ├── CreateVehicle/
    │   │   │   ├── CreateVehicleCommand.cs
    │   │   │   ├── CreateVehicleCommandHandler.cs
    │   │   │   └── CreateVehicleCommandValidator.cs
    │   │   ├── UpdateVehicle/
    │   │   │   ├── UpdateVehicleCommand.cs
    │   │   │   ├── UpdateVehicleCommandHandler.cs
    │   │   │   └── UpdateVehicleCommandValidator.cs
    │   │   ├── DeleteVehicle/
    │   │   │   ├── DeleteVehicleCommand.cs
    │   │   │   └── DeleteVehicleCommandHandler.cs
    │   │   ├── GetVehicle/
    │   │   │   ├── GetVehicleQuery.cs
    │   │   │   └── GetVehicleQueryHandler.cs
    │   │   ├── SearchVehicles/
    │   │   │   ├── SearchVehiclesQuery.cs
    │   │   │   └── SearchVehiclesQueryHandler.cs
    │   │   ├── GetUserVehicles/
    │   │   │   ├── GetUserVehiclesQuery.cs
    │   │   │   └── GetUserVehiclesQueryHandler.cs
    │   │   ├── AddVehicleImages/
    │   │   │   ├── AddVehicleImagesCommand.cs
    │   │   │   ├── AddVehicleImagesCommandHandler.cs
    │   │   │   └── AddVehicleImagesCommandValidator.cs
    │   │   ├── RemoveVehicleImage/
    │   │   │   ├── RemoveVehicleImageCommand.cs
    │   │   │   └── RemoveVehicleImageCommandHandler.cs
    │   │   └── SetPrimaryImage/
    │   │       ├── SetPrimaryImageCommand.cs
    │   │       └── SetPrimaryImageCommandHandler.cs
    │   ├── DTOs/
    │   │   ├── CreateVehicleRequest.cs
    │   │   ├── UpdateVehicleRequest.cs
    │   │   ├── VehicleResponse.cs
    │   │   ├── VehicleSearchResponse.cs
    │   │   ├── VehicleSearchCriteria.cs
    │   │   ├── VehicleImageDto.cs
    │   │   └── AddVehicleImagesRequest.cs
    │   └── VehicleService.Application.csproj
    ├── VehicleService.Domain/
    │   ├── Entities/
    │   │   ├── Vehicle.cs
    │   │   ├── VehicleImage.cs
    │   │   ├── VehicleFeature.cs
    │   │   └── Enums/
    │   │       ├── VehicleStatus.cs
    │   │       ├── FuelType.cs
    │   │       ├── TransmissionType.cs
    │   │       ├── BodyType.cs
    │   │       └── ConditionType.cs
    │   ├── Interfaces/
    │   │   ├── IVehicleRepository.cs
    │   │   ├── IVehicleImageRepository.cs
    │   │   └── IVehicleSearchRepository.cs
    │   ├── ValueObjects/
    │   │   ├── Price.cs
    │   │   ├── Address.cs
    │   │   ├── VehicleSpecifications.cs
    │   │   └── ContactInfo.cs
    │   ├── Events/
    │   │   ├── VehicleCreatedEvent.cs
    │   │   ├── VehicleUpdatedEvent.cs
    │   │   └── VehicleDeletedEvent.cs
    │   └── VehicleService.Domain.csproj
    ├── VehicleService.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfVehicleRepository.cs
    │   │   ├── EfVehicleImageRepository.cs
    │   │   ├── EfVehicleSearchRepository.cs
    │   │   └── Configurations/
    │   │       ├── VehicleConfiguration.cs
    │   │       └── VehicleImageConfiguration.cs
    │   ├── Migrations/
    │   │   ├── [MigrationFiles...]
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── Search/
    │   │   ├── VehicleSearchService.cs
    │   │   └── IVehicleSearchService.cs
    │   ├── External/
    │   │   ├── MediaServiceClient.cs
    │   │   ├── IMediaServiceClient.cs
    │   │   └── DTOs/
    │   │       ├── MediaFileResponse.cs
    │   │       ├── UploadMediaRequest.cs
    │   │       └── PresignedUrlResponse.cs
    │   ├── Integrations/
    │   │   └── NotificationServiceIntegration.cs
    │   └── VehicleService.Infrastructure.csproj
    ├── VehicleService.Shared/
    │   ├── ApiResponse.cs
    │   ├── PagingOptions.cs
    │   ├── SearchResult.cs
    │   ├── VehicleSettings.cs
    │   ├── Constants/
    │   │   ├── VehicleConstants.cs
    │   │   ├── CacheConstants.cs
    │   │   └── SearchConstants.cs
    │   └── VehicleService.Shared.csproj
    └── VehicleService.sln
================
backend/
└── AdminService/
    ├── AdminService.Api/
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── AdminService.Api.csproj
    │   ├── AdminService.Api.http
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── Program.cs
    │   └── Controllers/
    │       ├── AdminController.cs
    │       ├── ModerationController.cs
    │       ├── ReportsController.cs
    │       └── StatisticsController.cs
    ├── AdminService.Application/
    │   ├── Services/
    │   │   ├── IAdminService.cs
    │   │   ├── AdminService.cs
    │   │   ├── IModerationService.cs
    │   │   ├── ModerationService.cs
    │   │   ├── IReportService.cs
    │   │   ├── ReportService.cs
    │   │   ├── IStatisticsService.cs
    │   │   └── StatisticsService.cs
    │   ├── UseCases/
    │   │   ├── Users/
    │   │   │   ├── GetUsersQuery.cs
    │   │   │   ├── GetUsersQueryHandler.cs
    │   │   │   ├── GetUserDetailsQuery.cs
    │   │   │   └── GetUserDetailsQueryHandler.cs
    │   │   ├── Vehicles/
    │   │   │   ├── GetVehiclesForModerationQuery.cs
    │   │   │   ├── GetVehiclesForModerationQueryHandler.cs
    │   │   │   ├── ApproveVehicleCommand.cs
    │   │   │   ├── ApproveVehicleCommandHandler.cs
    │   │   │   ├── RejectVehicleCommand.cs
    │   │   │   └── RejectVehicleCommandHandler.cs
    │   │   ├── Reports/
    │   │   │   ├── GetReportsQuery.cs
    │   │   │   ├── GetReportsQueryHandler.cs
    │   │   │   ├── ResolveReportCommand.cs
    │   │   │   └── ResolveReportCommandHandler.cs
    │   │   ├── Statistics/
    │   │   │   ├── GetPlatformStatisticsQuery.cs
    │   │   │   └── GetPlatformStatisticsQueryHandler.cs
    │   │   └── System/
    │   │       ├── GetSystemHealthQuery.cs
    │   │       └── GetSystemHealthQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── UserDto.cs
    │   │   ├── VehicleModerationDto.cs
    │   │   ├── ReportDto.cs
    │   │   ├── PlatformStatisticsDto.cs
    │   │   ├── SystemHealthDto.cs
    │   │   └── ModerationActionDto.cs
    │   └── AdminService.Application.csproj
    ├── AdminService.Domain/
    │   ├── Entities/
    │   │   ├── ModerationItem.cs
    │   │   ├── Report.cs
    │   │   ├── AdminActionLog.cs
    │   │   └── SystemAudit.cs
    │   ├── Interfaces/
    │   │   ├── IModerationRepository.cs
    │   │   ├── IReportRepository.cs
    │   │   ├── IAdminActionLogRepository.cs
    │   │   ├── IStatisticsRepository.cs
    │   │   └── ISystemAuditRepository.cs
    │   ├── Enums/
    │   │   ├── ModerationStatus.cs
    │   │   ├── ReportType.cs
    │   │   ├── ReportStatus.cs
    │   │   ├── AdminActionType.cs
    │   │   ├── ContentType.cs
    │   │   └── SystemStatus.cs
    │   ├── ValueObjects/
    │   │   ├── ModerationReason.cs
    │   │   ├── ReportDetails.cs
    │   │   └── SystemMetrics.cs
    │   └── AdminService.Domain.csproj
    ├── AdminService.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfModerationRepository.cs
    │   │   ├── EfReportRepository.cs
    │   │   ├── EfAdminActionLogRepository.cs
    │   │   ├── EfStatisticsRepository.cs
    │   │   ├── EfSystemAuditRepository.cs
    │   │   └── Configurations/
    │   │       ├── ModerationItemConfiguration.cs
    │   │       ├── ReportConfiguration.cs
    │   │       ├── AdminActionLogConfiguration.cs
    │   │       └── SystemAuditConfiguration.cs
    │   ├── Migrations/
    │   │   ├── [MigrationFiles...]
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── External/
    │   │   ├── AuthServiceClient.cs
    │   │   ├── VehicleServiceClient.cs
    │   │   ├── MediaServiceClient.cs
    │   │   ├── IAuthServiceClient.cs
    │   │   ├── IVehicleServiceClient.cs
    │   │   └── IMediaServiceClient.cs
    │   ├── Integrations/
    │   │   ├── ServiceHealthChecker.cs
    │   │   └── IServiceHealthChecker.cs
    │   └── AdminService.Infrastructure.csproj
    ├── AdminService.Shared/
    │   ├── ApiResponse.cs
    │   ├── AdminSettings.cs
    │   ├── Constants/
    │   │   ├── AdminConstants.cs
    │   │   ├── CacheConstants.cs
    │   │   └── RoleConstants.cs
    │   └── AdminService.Shared.csproj
    └── AdminService.sln
=======================
backend/
└── NotificationService/
    ├── NotificationService.Api/
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── NotificationService.Api.csproj
    │   ├── NotificationService.Api.http
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── Program.cs
    │   └── Controllers/
    │       ├── NotificationsController.cs
    │       └── WebhooksController.cs
    ├── NotificationService.Application/
    │   ├── Services/
    │   │   ├── INotificationService.cs
    │   │   ├── NotificationService.cs
    │   │   ├── IEmailService.cs
    │   │   ├── EmailService.cs
    │   │   ├── ISmsService.cs
    │   │   ├── SmsService.cs
    │   │   ├── IPushNotificationService.cs
    │   │   └── PushNotificationService.cs
    │   ├── UseCases/
    │   │   ├── SendEmailNotification/
    │   │   │   ├── SendEmailNotificationCommand.cs
    │   │   │   ├── SendEmailNotificationCommandHandler.cs
    │   │   │   └── SendEmailNotificationCommandValidator.cs
    │   │   ├── SendSmsNotification/
    │   │   │   ├── SendSmsNotificationCommand.cs
    │   │   │   ├── SendSmsNotificationCommandHandler.cs
    │   │   │   └── SendSmsNotificationCommandValidator.cs
    │   │   ├── SendPushNotification/
    │   │   │   ├── SendPushNotificationCommand.cs
    │   │   │   ├── SendPushNotificationCommandHandler.cs
    │   │   │   └── SendPushNotificationCommandValidator.cs
    │   │   ├── ProcessNotificationQueue/
    │   │   │   ├── ProcessNotificationQueueCommand.cs
    │   │   │   └── ProcessNotificationQueueCommandHandler.cs
    │   │   └── GetNotificationStatus/
    │   │       ├── GetNotificationStatusQuery.cs
    │   │       └── GetNotificationStatusQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── EmailNotificationDto.cs
    │   │   ├── SmsNotificationDto.cs
    │   │   ├── PushNotificationDto.cs
    │   │   ├── NotificationResponse.cs
    │   │   └── NotificationStatusDto.cs
    │   └── NotificationService.Application.csproj
    ├── NotificationService.Domain/
    │   ├── Entities/
    │   │   ├── Notification.cs
    │   │   ├── NotificationTemplate.cs
    │   │   ├── NotificationQueue.cs
    │   │   └── NotificationLog.cs
    │   ├── Interfaces/
    │   │   ├── INotificationRepository.cs
    │   │   ├── INotificationTemplateRepository.cs
    │   │   ├── INotificationQueueRepository.cs
    │   │   ├── INotificationLogRepository.cs
    │   │   └── IMessageBus.cs
    │   ├── Enums/
    │   │   ├── NotificationType.cs
    │   │   ├── NotificationStatus.cs
    │   │   ├── NotificationProvider.cs
    │   │   └── PriorityLevel.cs
    │   ├── ValueObjects/
    │   │   ├── EmailAddress.cs
    │   │   ├── PhoneNumber.cs
    │   │   ├── MessageContent.cs
    │   │   └── NotificationSubject.cs
    │   ├── Events/
    │   │   └── NotificationSentEvent.cs
    │   └── NotificationService.Domain.csproj
    ├── NotificationService.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfNotificationRepository.cs
    │   │   ├── EfNotificationTemplateRepository.cs
    │   │   ├── EfNotificationQueueRepository.cs
    │   │   ├── EfNotificationLogRepository.cs
    │   │   └── Configurations/
    │   │       ├── NotificationConfiguration.cs
    │   │       ├── NotificationTemplateConfiguration.cs
    │   │       ├── NotificationQueueConfiguration.cs
    │   │       └── NotificationLogConfiguration.cs
    │   ├── Migrations/
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── External/
    │   │   ├── SendGridEmailService.cs
    │   │   ├── TwilioSmsService.cs
    │   │   ├── FirebasePushService.cs
    │   │   ├── IEmailProvider.cs
    │   │   ├── ISmsProvider.cs
    │   │   └── IPushNotificationProvider.cs
    │   ├── MessageBus/
    │   │   ├── RabbitMQMessageBus.cs
    │   │   ├── AzureServiceBus.cs
    │   │   └── InMemoryMessageBus.cs
    │   ├── Templates/
    │   │   ├── EmailTemplates/
    │   │   │   ├── ContactNotification.html
    │   │   │   ├── WelcomeEmail.html
    │   │   │   └── PasswordReset.html
    │   │   └── TemplateEngine.cs
    │   └── NotificationService.Infrastructure.csproj
    ├── NotificationService.Shared/
    │   ├── ApiResponse.cs
    │   ├── NotificationSettings.cs
    │   ├── Constants/
    │   │   ├── NotificationConstants.cs
    │   │   └── CacheConstants.cs
    │   └── NotificationService.Shared.csproj
    └── NotificationService.sln
================================================
backend/
└── ContactService/
    ├── ContactService.Api/
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── ContactService.Api.csproj
    │   ├── ContactService.Api.http
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── Program.cs
    │   └── Controllers/
    │       ├── ContactController.cs
    │       └── ContactHistoryController.cs
    ├── ContactService.Application/
    │   ├── Services/
    │   │   ├── IContactService.cs
    │   │   ├── ContactService.cs
    │   │   ├── IContactNotificationService.cs
    │   │   └── ContactNotificationService.cs
    │   ├── UseCases/
    │   │   ├── CreateContactRequest/
    │   │   │   ├── CreateContactRequestCommand.cs
    │   │   │   ├── CreateContactRequestCommandHandler.cs
    │   │   │   └── CreateContactRequestCommandValidator.cs
    │   │   ├── GetContactRequestsBySeller/
    │   │   │   ├── GetContactRequestsBySellerQuery.cs
    │   │   │   └── GetContactRequestsBySellerQueryHandler.cs
    │   │   ├── GetContactRequestsByBuyer/
    │   │   │   ├── GetContactRequestsByBuyerQuery.cs
    │   │   │   └── GetContactRequestsByBuyerQueryHandler.cs
    │   │   ├── UpdateContactRequestStatus/
    │   │   │   ├── UpdateContactRequestStatusCommand.cs
    │   │   │   └── UpdateContactRequestStatusCommandHandler.cs
    │   │   └── GetContactRequestDetail/
    │   │       ├── GetContactRequestDetailQuery.cs
    │   │       └── GetContactRequestDetailQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── CreateContactRequestDto.cs
    │   │   ├── ContactRequestDto.cs
    │   │   ├── ContactRequestDetailDto.cs
    │   │   ├── ContactRequestStatusDto.cs
    │   │   └── ContactRequestResponse.cs
    │   └── ContactService.Application.csproj
    ├── ContactService.Domain/
    │   ├── Entities/
    │   │   ├── ContactRequest.cs
    │   │   ├── ContactMessage.cs
    │   │   └── ContactHistory.cs
    │   ├── Interfaces/
    │   │   ├── IContactRequestRepository.cs
    │   │   ├── IContactMessageRepository.cs
    │   │   └── IContactHistoryRepository.cs
    │   ├── Enums/
    │   │   ├── ContactRequestStatus.cs
    │   │   ├── ContactMethod.cs
    │   │   └── ContactType.cs
    │   ├── ValueObjects/
    │   │   ├── ContactInfo.cs
    │   │   ├── MessageContent.cs
    │   │   └── VehicleInfo.cs
    │   ├── Events/
    │   │   └── ContactRequestCreatedEvent.cs
    │   └── ContactService.Domain.csproj
    ├── ContactService.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfContactRequestRepository.cs
    │   │   ├── EfContactMessageRepository.cs
    │   │   ├── EfContactHistoryRepository.cs
    │   │   └── Configurations/
    │   │       ├── ContactRequestConfiguration.cs
    │   │       ├── ContactMessageConfiguration.cs
    │   │       └── ContactHistoryConfiguration.cs
    │   ├── Migrations/
    │   │   ├── [MigrationFiles...]
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── External/
    │   │   ├── VehicleServiceClient.cs
    │   │   ├── NotificationServiceClient.cs
    │   │   ├── IVehicleServiceClient.cs
    │   │   └── INotificationServiceClient.cs
    │ 	├── Integrations/
    │   │   └── NotificationIntegrationService.cs
    │   └── ContactService.Infrastructure.csproj
    ├── ContactService.Shared/
    │   ├── ApiResponse.cs
    │   ├── ContactSettings.cs
    │   ├── Constants/
    │   │   ├── ContactConstants.cs
    │   │   └── CacheConstants.cs
    │   └── ContactService.Shared.csproj
    └── ContactService.sln
===========================
backend/
└── ErrorService/
    ├── ErrorService.Api/
    │   ├── Controllers/
    │   │   ├── HealthController.cs
    │   │   └── ErrorsController.cs
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── appsettings.Production.json
    │   ├── Dockerfile.dev
    │   ├── Dockerfile.prod
    │   ├── ErrorService.Api.csproj
    │   ├── ErrorService.Api.http
    │   └── Program.cs
    ├── ErrorService.Shared/
    │   ├── ErrorService.Shared.csproj
    │   ├── Exceptions/
    │   │   ├── AppException.cs
    │   │   ├── BadRequestException.cs
    │   │   ├── UnauthorizedException.cs
    │   │   ├── ForbiddenException.cs
    │   │   ├── NotFoundException.cs
    │   │   ├── ConflictException.cs
    │   │   ├── ValidationException.cs
    │   │   ├── ServiceUnavailableException.cs
    │   │   ├── TooManyRequestsException.cs
    │   │   ├── NotImplementedServiceException.cs
    │   │   ├── BadGatewayException.cs
    │   │   ├── GatewayTimeoutException.cs
    │   │   ├── LengthRequiredException.cs
    │   │   ├── MethodNotAllowedException.cs
    │   │   ├── NotAcceptableException.cs
    │   │   ├── PreconditionFailedException.cs
    │   │   ├── UnprocessableEntityException.cs
    │   │   └── UnsupportedMediaTypeException.cs
    │   ├── Middleware/
    │   │   └── ErrorHandlingMiddleware.cs
    │   ├── Models/
    │   │   └── ApiResponse.cs
    │   └── Extensions/
    │       └── ServiceCollectionExtensions.cs
    ├── ErrorService.Application/
    │   ├── Services/
    │   │   ├── IErrorService.cs
    │   │   └── ErrorService.cs
    │   ├── DTOs/
    │   │   ├── LogErrorRequest.cs
    │   │   ├── ErrorResponse.cs
    │   │   └── ErrorQuery.cs
    │   └── ErrorService.Application.csproj
    ├── ErrorService.Domain/  
    │   ├── Entities/
    │   │   └── ErrorLog.cs
    │   ├── Interfaces/
    │   │   ├── IErrorLogRepository.cs
    │   │   └── IErrorReportingService.cs
    │   └── ErrorService.Domain.csproj
    ├── ErrorService.Infrastructure/ 
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── EfErrorLogRepository.cs
    │   │   └── Configurations/
    │   │       └── ErrorLogConfiguration.cs
    │   ├── Migrations/
    │   │   ├── [MigrationFiles...]
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   ├── External/
    │   │   └── ElasticSearchService.cs
    │   └── ErrorService.Infrastructure.csproj
    └── ErrorService.sln
==============================================================
"@

# 2. Lógica del Script

$RootPath = $PWD 
Write-Host "Iniciando la creación de la estructura del proyecto en: $RootPath" -ForegroundColor Green

$pathStack = New-Object System.Collections.Stack
$pathStack.Push($RootPath)

$levelStack = New-Object System.Collections.Stack
$levelStack.Push(-1) 

$nameFinderRegex = '([a-zA-Z0-9\[\]._].*$)'

# Almacenará los proyectos que se deben agregar a las soluciones
$projectsToAdd = @()

$lines = $TreeDefinition.Split([Environment]::NewLine)

try {
    foreach ($line in $lines) {
        $trimmedLine = $line.Trim()

        # Omitir líneas vacías, separadores o comentarios/endpoints de API
        if ([string]::IsNullOrWhiteSpace($trimmedLine) -or `
            $trimmedLine.StartsWith("===") -or `
            $trimmedLine.StartsWith("GET ") -or `
            $trimmedLine.StartsWith("POST ") -or `
            $trimmedLine.StartsWith("PUT ") -or `
            $trimmedLine.StartsWith("DELETE ") -or `
            $trimmedLine.StartsWith("#")) {
            continue
        }

        $nameMatch = [regex]::Match($line, $nameFinderRegex)
        if (-not $nameMatch.Success) {
            continue
        }

        $name = $nameMatch.Groups[1].Value.Trim()
        $currentIndent = $nameMatch.Groups[1].Index

        if ([string]::IsNullOrWhiteSpace($name)) {
            continue
        }

        # Ajustar la pila para encontrar al padre correcto
        while ($currentIndent -le $levelStack.Peek()) {
            $pathStack.Pop() | Out-Null
            $levelStack.Pop() | Out-Null
        }

        $parentPath = $pathStack.Peek()

        # --- Lógica de 'dotnet' ---

        if ($name.EndsWith('.sln')) {
            # 1. ES UNA SOLUCIÓN (.sln)
            $solutionName = $name.Replace('.sln', '')
            $solutionDir = $parentPath # El .sln está en la carpeta raíz del servicio
            $solutionFile = Join-Path -Path $solutionDir -ChildPath $name
            
            Write-Host "  [SLN]  Creando solución: $solutionFile" -ForegroundColor Yellow
            
            if (-not (Test-Path $solutionFile)) {
                # Usamos -o (output) para colocar el .sln dentro de su carpeta de servicio
                dotnet new sln -n $solutionName -o $solutionDir | Out-Null
            } else {
                Write-Host "    -> La solución ya existe. Omitiendo." -ForegroundColor Gray
            }
            
            # No agregamos la solución a la pila de carpetas

        } elseif ($name.EndsWith('.csproj')) {
            # 2. ES UN PROYECTO (.csproj)
            $projectName = $name.Replace('.csproj', '')
            $projectDir = $parentPath # El .csproj está en su propia carpeta (ej: .../MediaService/Api)
            $projectFile = Join-Path -Path $projectDir -ChildPath $name
            
            # Determinar tipo de proyecto
            $projectType = "classlib"
            if ($projectName.EndsWith('.Api')) {
                $projectType = "webapi"
            }
            
            Write-Host "  [CSPROJ] Creando proyecto '$projectType': $projectFile" -ForegroundColor Magenta
            
            if (-not (Test-Path $projectFile)) {
                # Usamos -o (output) para crear el proyecto en su carpeta y --force para sobrescribir
                dotnet new $projectType -n $projectName -o $projectDir --force | Out-Null
            } else {
                 Write-Host "    -> El proyecto ya existe. Omitiendo." -ForegroundColor Gray
            }
            
            # Buscar la solución a la que pertenece
            # Asumimos que la solución está en la raíz del servicio (un nivel arriba del proyecto)
            if ($pathStack.Count -gt 1) {
                # $pathStack[0] es $RootPath
                
                # --- CORRECCIÓN ANTERIOR ---
                # Convertimos la pila a un array para poder acceder por índice
                $serviceRootPath = $pathStack.ToArray()[1] 
                # ---------------------------------

                $serviceName = Split-Path -Leaf $serviceRootPath
                $solutionFile = Join-Path -Path $serviceRootPath -ChildPath "$serviceName.sln"
                
                # Guardar para agregar después (en caso de que el .sln aún no se haya creado)
                $projectsToAdd += [PSCustomObject]@{
                    Solution = $solutionFile
                    Project = $projectFile
                }
            }
            # No agregamos el proyecto a la pila de carpetas

        } elseif ($name.EndsWith('/')) {
            # 3. ES UN DIRECTORIO
            $dirName = $name.TrimEnd('/')
            $currentPath = Join-Path -Path $parentPath -ChildPath $dirName
            
            Write-Host "  [DIR]  Creando: $currentPath" -ForegroundColor Cyan
            New-Item -ItemType Directory -Path $currentPath -Force -ErrorAction SilentlyContinue | Out-Null
            
            $pathStack.Push($currentPath)
            $levelStack.Push($currentIndent)

        } else {
            # 4. ES UN ARCHIVO (ej: .cs, .json, Dockerfile)
            $currentPath = Join-Path -Path $parentPath -ChildPath $name
            Write-Host "  [FILE] Creando: $currentPath"
            
            # No sobrescribir archivos que 'dotnet new' ya creó (como Program.cs)
            New-Item -ItemType File -Path $currentPath -Force:$false -ErrorAction SilentlyContinue | Out-Null
        }
    }

    # --- Fase de Post-Procesamiento ---
    # Ahora que todos los archivos .sln y .csproj existen, los vinculamos.
    
    Write-Host "-----------------------------------------------------" -ForegroundColor Green
    Write-Host "Vinculando proyectos a las soluciones..." -ForegroundColor Green
    
    $groupedProjects = $projectsToAdd | Group-Object Solution

    foreach ($group in $groupedProjects) {
        $solutionFile = $group.Name
        if (Test-Path $solutionFile) {
            Write-Host "  Agregando a la solución: $solutionFile" -ForegroundColor Yellow
            
            # Preparamos una lista de proyectos para agregar
            $projectPaths = $group.Group | ForEach-Object { $_.Project }
            
            # Usamos 'dotnet sln add' con todos los proyectos a la vez (más rápido)
            try {
                dotnet sln $solutionFile add $projectPaths | Out-Null
                foreach($proj in $projectPaths) {
                    Write-Host "    -> [OK] $proj"
                }
            } catch {
                 # --- CORRECCIÓN ANTERIOR ---
                 Write-Error "Error agregando proyectos a ${solutionFile}: $_"
            }

        } else {
            Write-Warning "No se encontró la solución '$solutionFile'. No se pudieron agregar los siguientes proyectos:"
            $group.Group | ForEach-Object { Write-Warning "  - $($_.Project)" }
        }
    }


    Write-Host "-----------------------------------------------------" -ForegroundColor Green
    Write-Host "Estructura del proyecto 'dotnet' creada exitosamente." -ForegroundColor Green
}
catch {
    Write-Error "Ocurrió un error: $_"
}