using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Domain.Interfaces;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller para el proceso completo de onboarding de dealers
/// Maneja desde registro inicial hasta activación de cuenta
/// Integra con Azul (Banco Popular) para pagos
/// </summary>
[ApiController]
[Route("api/dealer-onboarding")]
public class DealerOnboardingV2Controller : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DealerOnboardingV2Controller> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificationServiceClient _notificationClient;

    public DealerOnboardingV2Controller(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<DealerOnboardingV2Controller> logger,
        IConfiguration configuration,
        IEventPublisher eventPublisher,
        INotificationServiceClient notificationClient)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _eventPublisher = eventPublisher;
        _notificationClient = notificationClient;
    }

    // ========================================
    // ONBOARD-001: REGISTRO INICIAL
    // ========================================

    /// <summary>
    /// Registra un nuevo dealer (inicio del onboarding)
    /// </summary>
    /// <remarks>
    /// Este endpoint es público y crea:
    /// 1. Un registro de DealerOnboarding con status Pending
    /// 2. Envía email de verificación (via NotificationService)
    /// 3. Publica evento dealer.registered en RabbitMQ
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterDealer([FromBody] RegisterDealerOnboardingRequest request)
    {
        try
        {
            // Validar que el RNC no exista
            var existingRnc = await _context.DealerOnboardings
                .AnyAsync(d => d.RNC == request.RNC);

            if (existingRnc)
            {
                return Conflict(new { error = "Ya existe un dealer con este RNC" });
            }

            // Validar que el email no exista
            var existingEmail = await _context.DealerOnboardings
                .AnyAsync(d => d.Email == request.Email);

            if (existingEmail)
            {
                return Conflict(new { error = "Ya existe un dealer con este email" });
            }

            // Calcular eligibilidad de Early Bird
            var isEarlyBirdEligible = DealerOnboarding.CalculateEarlyBirdEligibility(DateTime.UtcNow);

            // Crear el registro de onboarding
            var onboarding = new DealerOnboarding
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty, // Se asignará cuando AuthService cree el usuario
                Status = DealerOnboardingStatus.Pending,
                
                // Información del negocio
                BusinessName = request.BusinessName,
                BusinessLegalName = request.BusinessLegalName,
                RNC = request.RNC,
                Type = Enum.Parse<DealerOnboardingType>(request.Type, true),
                Description = request.Description,
                
                // Contacto
                Email = request.Email,
                Phone = request.Phone,
                MobilePhone = request.MobilePhone,
                Website = request.Website,
                
                // Ubicación
                Address = request.Address,
                City = request.City,
                Province = request.Province,
                PostalCode = request.PostalCode,
                
                // Representante Legal
                LegalRepName = request.LegalRepName,
                LegalRepCedula = request.LegalRepCedula,
                LegalRepPosition = request.LegalRepPosition,
                
                // Plan
                RequestedPlan = Enum.Parse<DealerOnboardingPlan>(request.RequestedPlan, true),
                IsEarlyBirdEligible = isEarlyBirdEligible,
                
                // Token de verificación de email
                EmailVerificationToken = GenerateVerificationToken(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                
                CreatedAt = DateTime.UtcNow
            };

            _context.DealerOnboardings.Add(onboarding);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Dealer onboarding created: {DealerId} - {BusinessName}", 
                onboarding.Id, onboarding.BusinessName);

            // Enviar email de verificación via NotificationService
            await _notificationClient.SendDealerVerificationEmailAsync(
                onboarding.Email,
                onboarding.BusinessName,
                onboarding.EmailVerificationToken!,
                onboarding.EmailVerificationTokenExpiry!.Value);

            // Publicar evento dealer.registered en RabbitMQ
            var registeredEvent = DealerRegisteredEvent.Create(
                onboarding.Id,
                onboarding.UserId,
                onboarding.BusinessName,
                onboarding.Email,
                onboarding.RNC,
                onboarding.RequestedPlan.ToString(),
                onboarding.IsEarlyBirdEligible);
            
            await _eventPublisher.PublishAsync(registeredEvent);

            return Ok(new RegisterDealerOnboardingResponse(
                DealerId: onboarding.Id,
                UserId: onboarding.UserId,
                Status: onboarding.Status.ToString(),
                Message: "Registro exitoso. Por favor verifica tu email.",
                NextStep: "Verificar email",
                IsEarlyBirdEligible: isEarlyBirdEligible,
                EmailVerificationTokenExpiry: onboarding.EmailVerificationTokenExpiry
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering dealer");
            return StatusCode(500, new { error = "Error al registrar dealer" });
        }
    }

    // ========================================
    // ONBOARD-002: VERIFICACIÓN DE EMAIL
    // ========================================

    /// <summary>
    /// Verifica el email del dealer usando el token
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.EmailVerificationToken == request.Token);

        if (onboarding == null)
        {
            return NotFound(new { error = "Token de verificación inválido" });
        }

        if (onboarding.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return BadRequest(new { error = "Token de verificación expirado" });
        }

        if (onboarding.Status != DealerOnboardingStatus.Pending)
        {
            return BadRequest(new { error = "Email ya verificado" });
        }

        // Actualizar status
        onboarding.Status = DealerOnboardingStatus.EmailVerified;
        onboarding.EmailVerifiedAt = DateTime.UtcNow;
        onboarding.EmailVerificationToken = null; // Limpiar token usado
        onboarding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email verified for dealer {DealerId}", onboarding.Id);

        // Publicar evento dealer.email_verified en RabbitMQ
        var emailVerifiedEvent = DealerEmailVerifiedEvent.Create(
            onboarding.Id,
            onboarding.Email,
            onboarding.EmailVerifiedAt!.Value);
        
        await _eventPublisher.PublishAsync(emailVerifiedEvent);

        return Ok(new
        {
            success = true,
            dealerId = onboarding.Id,
            status = onboarding.Status.ToString(),
            nextStep = "Subir documentos",
            redirectUrl = "/dealer/documents"
        });
    }

    /// <summary>
    /// Reenvía el email de verificación
    /// </summary>
    [HttpPost("{dealerId:guid}/resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerificationEmail(Guid dealerId)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (onboarding.Status != DealerOnboardingStatus.Pending)
        {
            return BadRequest(new { error = "Email ya verificado" });
        }

        // Generar nuevo token
        onboarding.EmailVerificationToken = GenerateVerificationToken();
        onboarding.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        onboarding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Enviar email de verificación via NotificationService
        await _notificationClient.SendDealerVerificationEmailAsync(
            onboarding.Email,
            onboarding.BusinessName,
            onboarding.EmailVerificationToken!,
            onboarding.EmailVerificationTokenExpiry!.Value);

        return Ok(new { success = true, message = "Email de verificación reenviado" });
    }

    // ========================================
    // ONBOARD-003: SUBIDA DE DOCUMENTOS
    // ========================================

    /// <summary>
    /// Actualiza los IDs de documentos subidos a MediaService
    /// </summary>
    [HttpPut("{dealerId:guid}/documents")]
    [Authorize]
    public async Task<IActionResult> UpdateDocuments(Guid dealerId, [FromBody] UpdateDocumentsRequest request)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (!onboarding.CanUploadDocuments)
        {
            return BadRequest(new { 
                error = "No se pueden subir documentos en este estado", 
                currentStatus = onboarding.Status.ToString() 
            });
        }

        // Actualizar IDs de documentos
        if (request.RncDocumentId.HasValue)
            onboarding.RncDocumentId = request.RncDocumentId;
        
        if (request.BusinessLicenseDocumentId.HasValue)
            onboarding.BusinessLicenseDocumentId = request.BusinessLicenseDocumentId;
        
        if (request.LegalRepCedulaDocumentId.HasValue)
            onboarding.LegalRepCedulaDocumentId = request.LegalRepCedulaDocumentId;
        
        if (request.SocialContractDocumentId.HasValue)
            onboarding.SocialContractDocumentId = request.SocialContractDocumentId;
        
        if (request.LegalPowerDocumentId.HasValue)
            onboarding.LegalPowerDocumentId = request.LegalPowerDocumentId;
        
        if (request.AddressProofDocumentId.HasValue)
            onboarding.AddressProofDocumentId = request.AddressProofDocumentId;

        onboarding.UpdatedAt = DateTime.UtcNow;

        // Verificar si todos los documentos obligatorios están
        if (onboarding.HasAllRequiredDocuments)
        {
            onboarding.Status = DealerOnboardingStatus.DocumentsSubmitted;
            onboarding.DocumentsSubmittedAt = DateTime.UtcNow;
            
            _logger.LogInformation("All documents submitted for dealer {DealerId}", dealerId);
            
            // Notificar a admins via NotificationService
            await _notificationClient.NotifyAdminsNewDealerApplicationAsync(
                onboarding.BusinessName,
                onboarding.RNC,
                onboarding.Email,
                dealerId);
            
            // Publicar evento dealer.documents_submitted en RabbitMQ
            var documentsEvent = DealerDocumentsSubmittedEvent.Create(
                onboarding.Id,
                onboarding.BusinessName,
                CountDocuments(onboarding),
                onboarding.DocumentsSubmittedAt.Value);
            
            await _eventPublisher.PublishAsync(documentsEvent);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            status = onboarding.Status.ToString(),
            hasAllRequiredDocuments = onboarding.HasAllRequiredDocuments,
            nextStep = onboarding.HasAllRequiredDocuments ? "Esperar revisión de admin" : "Completar documentos faltantes"
        });
    }

    // ========================================
    // GET STATUS
    // ========================================

    /// <summary>
    /// Obtiene el estado actual del onboarding
    /// </summary>
    [HttpGet("{dealerId:guid}/status")]
    [Authorize]
    public async Task<IActionResult> GetOnboardingStatus(Guid dealerId)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        return Ok(new DealerOnboardingStatusResponse(
            DealerId: onboarding.Id,
            Status: onboarding.Status.ToString(),
            StatusCode: (int)onboarding.Status,
            BusinessName: onboarding.BusinessName,
            Email: onboarding.Email,
            RequestedPlan: onboarding.RequestedPlan.ToString(),
            IsEarlyBirdEligible: onboarding.IsEarlyBirdEligible,
            IsEarlyBirdEnrolled: onboarding.IsEarlyBirdEnrolled,
            HasAllRequiredDocuments: onboarding.HasAllRequiredDocuments,
            CreatedAt: onboarding.CreatedAt,
            EmailVerifiedAt: onboarding.EmailVerifiedAt,
            DocumentsSubmittedAt: onboarding.DocumentsSubmittedAt,
            ApprovedAt: onboarding.ApprovedAt,
            ActivatedAt: onboarding.ActivatedAt,
            RejectedAt: onboarding.RejectedAt,
            RejectionReason: onboarding.RejectionReason
        ));
    }

    // ========================================
    // ONBOARD-004: APROBACIÓN/RECHAZO (ADMIN)
    // ========================================

    /// <summary>
    /// Aprueba la solicitud de dealer (solo Admin)
    /// </summary>
    [HttpPost("{dealerId:guid}/approve")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ApproveDealer(Guid dealerId, [FromBody] ApproveDealerRequest request)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (!onboarding.IsPendingReview)
        {
            return BadRequest(new { 
                error = "El dealer no está pendiente de revisión",
                currentStatus = onboarding.Status.ToString()
            });
        }

        // Obtener ID del admin desde claims
        var adminIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        var adminId = adminIdClaim != null ? Guid.Parse(adminIdClaim.Value) : Guid.Empty;

        onboarding.Status = DealerOnboardingStatus.Approved;
        onboarding.ApprovedAt = DateTime.UtcNow;
        onboarding.ApprovedBy = adminId;
        onboarding.ApprovalNotes = request.Notes;
        onboarding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Dealer {DealerId} approved by admin {AdminId}", dealerId, adminId);

        // Enviar email de aprobación via NotificationService
        await _notificationClient.SendDealerApprovalEmailAsync(
            onboarding.Email,
            onboarding.BusinessName,
            onboarding.RequestedPlan.ToString());

        // Publicar evento dealer.approved en RabbitMQ
        var approvedEvent = DealerApprovedEvent.Create(
            onboarding.Id,
            onboarding.BusinessName,
            onboarding.Email,
            adminId,
            onboarding.ApprovedAt!.Value,
            onboarding.RequestedPlan.ToString());
        
        await _eventPublisher.PublishAsync(approvedEvent);

        return Ok(new
        {
            success = true,
            dealerId = onboarding.Id,
            status = onboarding.Status.ToString(),
            nextStep = "Configurar pago",
            message = "Dealer aprobado exitosamente"
        });
    }

    /// <summary>
    /// Rechaza la solicitud de dealer (solo Admin)
    /// </summary>
    [HttpPost("{dealerId:guid}/reject")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RejectDealer(Guid dealerId, [FromBody] RejectDealerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { error = "Debe proporcionar una razón para el rechazo" });
        }

        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (!onboarding.IsPendingReview)
        {
            return BadRequest(new { 
                error = "El dealer no está pendiente de revisión",
                currentStatus = onboarding.Status.ToString()
            });
        }

        // Obtener ID del admin desde claims
        var adminIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        var adminId = adminIdClaim != null ? Guid.Parse(adminIdClaim.Value) : Guid.Empty;

        onboarding.Status = DealerOnboardingStatus.Rejected;
        onboarding.RejectedAt = DateTime.UtcNow;
        onboarding.RejectedBy = adminId;
        onboarding.RejectionReason = request.Reason;
        onboarding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Dealer {DealerId} rejected by admin {AdminId}: {Reason}", 
            dealerId, adminId, request.Reason);

        // Enviar email de rechazo via NotificationService
        await _notificationClient.SendDealerRejectionEmailAsync(
            onboarding.Email,
            onboarding.BusinessName,
            request.Reason);

        // Publicar evento dealer.rejected en RabbitMQ
        var rejectedEvent = DealerRejectedEvent.Create(
            onboarding.Id,
            onboarding.BusinessName,
            onboarding.Email,
            adminId,
            request.Reason,
            onboarding.RejectedAt!.Value);
        
        await _eventPublisher.PublishAsync(rejectedEvent);

        return Ok(new
        {
            success = true,
            dealerId = onboarding.Id,
            status = onboarding.Status.ToString(),
            message = "Dealer rechazado"
        });
    }

    // ========================================
    // ONBOARD-005: CONFIGURACIÓN DE PAGO (AZUL)
    // ========================================

    /// <summary>
    /// Actualiza los IDs de Azul (llamado después del checkout)
    /// </summary>
    [HttpPut("{dealerId:guid}/azul-ids")]
    [Authorize]
    public async Task<IActionResult> UpdateAzulIds(Guid dealerId, [FromBody] UpdateAzulIdsRequest request)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (!onboarding.CanSetupPayment)
        {
            return BadRequest(new { 
                error = "El dealer no está en estado para configurar pago",
                currentStatus = onboarding.Status.ToString()
            });
        }

        onboarding.AzulCustomerId = request.AzulCustomerId;
        onboarding.AzulSubscriptionId = request.AzulSubscriptionId;
        onboarding.AzulCardToken = request.AzulCardToken;
        onboarding.Status = DealerOnboardingStatus.PaymentSetup;
        onboarding.PaymentSetupAt = DateTime.UtcNow;
        onboarding.UpdatedAt = DateTime.UtcNow;

        // Si es Early Bird, marcarlo
        if (onboarding.IsEarlyBirdEligible && request.EnrollEarlyBird)
        {
            onboarding.IsEarlyBirdEnrolled = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Azul payment configured for dealer {DealerId}", dealerId);

        // Publicar evento dealer.payment_setup en RabbitMQ
        var paymentEvent = DealerPaymentSetupEvent.Create(
            onboarding.Id,
            onboarding.AzulCustomerId!,
            onboarding.AzulSubscriptionId,
            onboarding.RequestedPlan.ToString(),
            onboarding.IsEarlyBirdEnrolled,
            onboarding.PaymentSetupAt!.Value);
        
        await _eventPublisher.PublishAsync(paymentEvent);

        return Ok(new
        {
            success = true,
            dealerId = onboarding.Id,
            status = onboarding.Status.ToString(),
            nextStep = "Activación automática"
        });
    }

    // ========================================
    // ONBOARD-006: ACTIVACIÓN FINAL
    // ========================================

    /// <summary>
    /// Completa el onboarding y activa el dealer
    /// Llamado después de confirmar pago exitoso (webhook de Azul)
    /// </summary>
    [HttpPost("{dealerId:guid}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin,System")]
    public async Task<IActionResult> ActivateDealer(Guid dealerId)
    {
        var onboarding = await _context.DealerOnboardings
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Dealer no encontrado" });
        }

        if (onboarding.Status != DealerOnboardingStatus.PaymentSetup)
        {
            return BadRequest(new { 
                error = "El dealer no tiene pago configurado",
                currentStatus = onboarding.Status.ToString()
            });
        }

        onboarding.Status = DealerOnboardingStatus.Active;
        onboarding.ActivatedAt = DateTime.UtcNow;
        onboarding.UpdatedAt = DateTime.UtcNow;

        // Crear el Dealer real
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            BusinessName = onboarding.BusinessName,
            TradeName = onboarding.BusinessName,
            Description = onboarding.Description,
            DealerType = MapOnboardingTypeToDealer(onboarding.Type),
            Email = onboarding.Email,
            Phone = onboarding.Phone,
            WhatsApp = onboarding.MobilePhone,
            Website = onboarding.Website,
            Address = onboarding.Address,
            City = onboarding.City,
            State = onboarding.Province,
            Country = "DO",
            Latitude = onboarding.Latitude,
            Longitude = onboarding.Longitude,
            BusinessRegistrationNumber = onboarding.RNC,
            VerificationStatus = DealerVerificationStatus.Verified,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Dealers.Add(dealer);

        // Crear suscripción de dealer
        var subscription = new DealerSubscription
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            Plan = MapOnboardingPlanToSubscription(onboarding.RequestedPlan),
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            // Si es Early Bird, trial de 90 días
            TrialEndDate = onboarding.IsEarlyBirdEnrolled ? DateTime.UtcNow.AddDays(90) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.DealerSubscriptions.Add(subscription);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Dealer {DealerId} activated. Created Dealer entity {NewDealerId}", 
            dealerId, dealer.Id);

        // Enviar email de bienvenida via NotificationService
        await _notificationClient.SendDealerWelcomeEmailAsync(
            onboarding.Email,
            onboarding.BusinessName,
            onboarding.RequestedPlan.ToString(),
            onboarding.IsEarlyBirdEnrolled);

        // Publicar evento dealer.activated en RabbitMQ
        var activatedEvent = DealerActivatedEvent.Create(
            onboarding.Id,
            dealer.Id,
            subscription.Id,
            onboarding.BusinessName,
            onboarding.Email,
            onboarding.RequestedPlan.ToString(),
            onboarding.IsEarlyBirdEnrolled,
            onboarding.ActivatedAt!.Value);
        
        await _eventPublisher.PublishAsync(activatedEvent);

        return Ok(new
        {
            success = true,
            onboardingId = onboarding.Id,
            dealerId = dealer.Id,
            subscriptionId = subscription.Id,
            status = onboarding.Status.ToString(),
            isEarlyBird = onboarding.IsEarlyBirdEnrolled,
            message = "¡Bienvenido a OKLA! Tu cuenta de dealer está activa.",
            redirectUrl = "/dealer/dashboard"
        });
    }

    // ========================================
    // ADMIN: LISTAR ONBOARDINGS PENDIENTES
    // ========================================

    /// <summary>
    /// Lista onboardings pendientes de revisión (para Admin)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetPendingOnboardings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.DealerOnboardings
            .Where(d => d.Status == DealerOnboardingStatus.DocumentsSubmitted ||
                        d.Status == DealerOnboardingStatus.UnderReview)
            .OrderBy(d => d.DocumentsSubmittedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.BusinessName,
                d.RNC,
                d.Email,
                Status = d.Status.ToString(),
                d.RequestedPlan,
                d.IsEarlyBirdEligible,
                d.CreatedAt,
                d.DocumentsSubmittedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    // ========================================
    // HELPERS
    // ========================================

    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }

    private static DealerType MapOnboardingTypeToDealer(DealerOnboardingType type)
    {
        return type switch
        {
            DealerOnboardingType.Independent => DealerType.Independent,
            DealerOnboardingType.Chain => DealerType.MultiLocation,
            DealerOnboardingType.MultipleStore => DealerType.MultiLocation,
            DealerOnboardingType.Franchise => DealerType.Franchise,
            _ => DealerType.Independent
        };
    }

    private static DealerPlan MapOnboardingPlanToSubscription(DealerOnboardingPlan plan)
    {
        return plan switch
        {
            DealerOnboardingPlan.Starter => DealerPlan.Basic,
            DealerOnboardingPlan.Pro => DealerPlan.Pro,
            DealerOnboardingPlan.Enterprise => DealerPlan.Enterprise,
            _ => DealerPlan.Free
        };
    }

    private static int CountDocuments(DealerOnboarding onboarding)
    {
        var count = 0;
        if (onboarding.RncDocumentId.HasValue) count++;
        if (onboarding.BusinessLicenseDocumentId.HasValue) count++;
        if (onboarding.LegalRepCedulaDocumentId.HasValue) count++;
        if (onboarding.SocialContractDocumentId.HasValue) count++;
        if (onboarding.LegalPowerDocumentId.HasValue) count++;
        if (onboarding.AddressProofDocumentId.HasValue) count++;
        return count;
    }
}

// ========================================
// REQUEST/RESPONSE DTOs
// ========================================

public record RegisterDealerOnboardingRequest(
    // Credenciales (se enviarán a AuthService)
    string Email,
    string Password,
    string ConfirmPassword,
    
    // Información del negocio
    string BusinessName,
    string BusinessLegalName,
    string RNC,
    string Type, // Independent, Chain, MultipleStore, Franchise
    string? Description,
    
    // Contacto
    string Phone,
    string? MobilePhone,
    string? Website,
    
    // Ubicación
    string Address,
    string City,
    string Province,
    string? PostalCode,
    
    // Representante Legal
    string LegalRepName,
    string LegalRepCedula,
    string LegalRepPosition,
    
    // Plan
    string RequestedPlan, // Starter, Pro, Enterprise
    bool AcceptedTerms
);

public record RegisterDealerOnboardingResponse(
    Guid DealerId,
    Guid UserId,
    string Status,
    string Message,
    string NextStep,
    bool IsEarlyBirdEligible,
    DateTime? EmailVerificationTokenExpiry
);

public record VerifyEmailRequest(string Token);

public record UpdateDocumentsRequest(
    Guid? RncDocumentId,
    Guid? BusinessLicenseDocumentId,
    Guid? LegalRepCedulaDocumentId,
    Guid? SocialContractDocumentId,
    Guid? LegalPowerDocumentId,
    Guid? AddressProofDocumentId
);

public record DealerOnboardingStatusResponse(
    Guid DealerId,
    string Status,
    int StatusCode,
    string BusinessName,
    string Email,
    string RequestedPlan,
    bool IsEarlyBirdEligible,
    bool IsEarlyBirdEnrolled,
    bool HasAllRequiredDocuments,
    DateTime CreatedAt,
    DateTime? EmailVerifiedAt,
    DateTime? DocumentsSubmittedAt,
    DateTime? ApprovedAt,
    DateTime? ActivatedAt,
    DateTime? RejectedAt,
    string? RejectionReason
);

public record ApproveDealerRequest(string? Notes);

public record RejectDealerRequest(string Reason);

public record UpdateAzulIdsRequest(
    string AzulCustomerId,
    string? AzulSubscriptionId,
    string? AzulCardToken,
    bool EnrollEarlyBird = true
);
