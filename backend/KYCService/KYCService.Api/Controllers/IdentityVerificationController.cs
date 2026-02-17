using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KYCService.Application.Commands;
using KYCService.Application.Queries;
using KYCService.Application.DTOs;
using KYCService.Application.Handlers;
using KYCService.Domain.Entities;
using System.Security.Claims;

namespace KYCService.Api.Controllers;

/// <summary>
/// Controlador para verificación de identidad biométrica
/// Proceso estilo Qik (Banco Popular) con captura de documento y selfie
/// </summary>
[ApiController]
[Route("api/kyc/identity-verification")]
[Authorize]
public class IdentityVerificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IdentityVerificationController> _logger;

    public IdentityVerificationController(IMediator mediator, ILogger<IdentityVerificationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar sesión de verificación de identidad
    /// </summary>
    /// <remarks>
    /// Crea una nueva sesión de verificación biométrica.
    /// La sesión expira en 30 minutos.
    /// </remarks>
    /// <param name="request">Datos iniciales de la verificación</param>
    /// <returns>Sesión creada con instrucciones</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StartVerificationResponse>> Start([FromBody] StartVerificationRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        // Verificar si puede iniciar
        var canStart = await _mediator.Send(new CanStartVerificationQuery(userId));
        if (!canStart.CanStart)
        {
            return BadRequest(new { 
                error = "CannotStart", 
                message = canStart.Reason,
                canRetryAfter = canStart.CanRetryAfter,
                hasActiveSession = canStart.HasActiveSession,
                activeSessionId = canStart.ActiveSessionId
            });
        }

        var command = new StartIdentityVerificationCommand
        {
            UserId = userId,
            DocumentType = request.DocumentType,
            DeviceInfo = request.DeviceInfo,
            Location = request.Location,
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Identity verification session {SessionId} started for user {UserId}", 
            result.SessionId, userId);
        
        return Ok(result);
    }

    /// <summary>
    /// Subir foto del documento (frente o reverso)
    /// </summary>
    /// <remarks>
    /// Procesa la imagen del documento usando OCR.
    /// Para cédula dominicana, valida formato y dígito verificador.
    /// </remarks>
    /// <param name="sessionId">ID de la sesión</param>
    /// <param name="side">Lado del documento (Front/Back)</param>
    /// <param name="image">Imagen del documento</param>
    /// <returns>Resultado del procesamiento OCR</returns>
    [HttpPost("document")]
    [ProducesResponseType(typeof(DocumentProcessedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentProcessedResponse>> UploadDocument(
        [FromForm] Guid sessionId,
        [FromForm] string side,
        [FromForm] IFormFile image)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        if (image == null || image.Length == 0)
            return BadRequest("La imagen es requerida");

        if (image.Length > 10 * 1024 * 1024) // 10MB max
            return BadRequest("La imagen excede el tamaño máximo de 10MB");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/heic" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
            return BadRequest("Tipo de archivo no permitido. Use JPG, PNG o HEIC");

        // SECURITY: Validate file magic bytes to prevent spoofed content types
        using var headerStream = new MemoryStream();
        await image.CopyToAsync(headerStream);
        headerStream.Position = 0;
        var header = new byte[8];
        await headerStream.ReadAsync(header, 0, Math.Min(8, (int)headerStream.Length));
        headerStream.Position = 0;

        bool isValidMagicBytes = false;
        // JPEG: FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            isValidMagicBytes = true;
        // PNG: 89 50 4E 47
        else if (header.Length >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            isValidMagicBytes = true;
        // HEIC: starts with ftyp after 4 bytes
        else if (header.Length >= 8 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70)
            isValidMagicBytes = true;

        if (!isValidMagicBytes)
            return BadRequest("El archivo no es una imagen válida");

        if (!Enum.TryParse<DocumentSide>(side, true, out var documentSide))
            return BadRequest("Lado de documento inválido. Use 'Front' o 'Back'");

        var command = new ProcessDocumentCommand
        {
            SessionId = sessionId,
            UserId = userId,
            Side = documentSide,
            ImageData = headerStream.ToArray(),
            FileName = image.FileName,
            ContentType = image.ContentType
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "SessionError", message = ex.Message });
        }
    }

    /// <summary>
    /// Subir selfie con datos de liveness
    /// </summary>
    /// <remarks>
    /// Procesa la selfie y compara con la foto del documento.
    /// Incluye verificación de liveness (anti-spoofing).
    /// </remarks>
    /// <param name="sessionId">ID de la sesión</param>
    /// <param name="selfie">Imagen de la selfie</param>
    /// <param name="livenessDataJson">Datos de liveness en JSON</param>
    /// <returns>Resultado de la verificación</returns>
    [HttpPost("selfie")]
    [ProducesResponseType(typeof(VerificationCompletedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(VerificationFailedResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadSelfie(
        [FromForm] Guid sessionId,
        [FromForm] IFormFile selfie,
        [FromForm] string? livenessDataJson)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        if (selfie == null || selfie.Length == 0)
            return BadRequest("La selfie es requerida");

        if (selfie.Length > 10 * 1024 * 1024)
            return BadRequest("La imagen excede el tamaño máximo de 10MB");

        LivenessDataDto? livenessData = null;
        if (!string.IsNullOrEmpty(livenessDataJson))
        {
            try
            {
                livenessData = System.Text.Json.JsonSerializer.Deserialize<LivenessDataDto>(
                    livenessDataJson, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to parse liveness data: {Error}", ex.Message);
            }
        }

        using var memoryStream = new MemoryStream();
        await selfie.CopyToAsync(memoryStream);

        var command = new ProcessSelfieCommand
        {
            SessionId = sessionId,
            UserId = userId,
            SelfieImageData = memoryStream.ToArray(),
            FileName = selfie.FileName,
            ContentType = selfie.ContentType,
            LivenessData = livenessData
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (VerificationFailedException ex)
        {
            return BadRequest(ex.Response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "SessionError", message = ex.Message });
        }
    }

    /// <summary>
    /// Completar la verificación manualmente
    /// </summary>
    /// <remarks>
    /// Finaliza el proceso de verificación y crea/actualiza el perfil KYC.
    /// </remarks>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(VerificationCompletedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationCompletedResponse>> Complete([FromBody] CompleteVerificationRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var command = new CompleteVerificationCommand
        {
            SessionId = request.SessionId,
            UserId = userId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (VerificationFailedException ex)
        {
            return BadRequest(ex.Response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "SessionError", message = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estado de una sesión de verificación
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(VerificationSessionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationSessionStatusDto>> GetSession(Guid sessionId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var result = await _mediator.Send(new GetVerificationSessionQuery(sessionId, userId));
        if (result == null)
            return NotFound("Sesión no encontrada");

        return Ok(result);
    }

    /// <summary>
    /// Obtener sesión activa del usuario
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(VerificationSessionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationSessionStatusDto>> GetActiveSession()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var result = await _mediator.Send(new GetActiveVerificationSessionQuery(userId));
        if (result == null)
            return NotFound("No hay sesión activa");

        return Ok(result);
    }

    /// <summary>
    /// Reintentar verificación fallida
    /// </summary>
    [HttpPost("retry")]
    [ProducesResponseType(typeof(StartVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StartVerificationResponse>> Retry([FromBody] RetryRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var command = new RetryVerificationCommand
        {
            SessionId = request.SessionId,
            UserId = userId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "CannotRetry", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancelar sesión de verificación
    /// </summary>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(Guid sessionId, [FromQuery] string? reason)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var command = new CancelVerificationCommand
        {
            SessionId = sessionId,
            UserId = userId,
            Reason = reason
        };

        var result = await _mediator.Send(command);
        if (!result)
            return NotFound("Sesión no encontrada");

        return NoContent();
    }

    /// <summary>
    /// Obtener historial de verificaciones
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<VerificationSessionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VerificationSessionSummaryDto>>> GetHistory([FromQuery] int limit = 10)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var result = await _mediator.Send(new GetVerificationHistoryQuery(userId, limit));
        return Ok(result);
    }

    /// <summary>
    /// Verificar si el usuario puede iniciar una nueva verificación
    /// </summary>
    [HttpGet("can-start")]
    [ProducesResponseType(typeof(CanStartVerificationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<CanStartVerificationResult>> CanStart()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        var result = await _mediator.Send(new CanStartVerificationQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// Verificar identidad usando perfil KYC existente (endpoint simplificado)
    /// </summary>
    /// <remarks>
    /// Este endpoint permite verificar la identidad usando el profileId en lugar de sessionId.
    /// Es útil cuando ya se ha creado un perfil KYC y se quiere procesar la verificación facial.
    /// </remarks>
    /// <param name="profileId">ID del perfil KYC</param>
    /// <param name="selfie">Imagen de la selfie</param>
    /// <param name="livenessData">Datos de liveness en JSON (opcional)</param>
    /// <returns>Resultado de la verificación</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyIdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerifyIdentityResponse>> Verify(
        [FromForm] Guid profileId,
        [FromForm] IFormFile selfie,
        [FromForm] string? livenessData)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized("Usuario no autenticado");

        if (selfie == null || selfie.Length == 0)
            return BadRequest(new { error = "ValidationError", message = "La selfie es requerida" });

        if (selfie.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "ValidationError", message = "La imagen excede el tamaño máximo de 10MB" });

        LivenessDataDto? livenessDto = null;
        if (!string.IsNullOrEmpty(livenessData))
        {
            try
            {
                livenessDto = System.Text.Json.JsonSerializer.Deserialize<LivenessDataDto>(
                    livenessData, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to parse liveness data: {Error}", ex.Message);
            }
        }

        using var memoryStream = new MemoryStream();
        await selfie.CopyToAsync(memoryStream);

        var command = new VerifyIdentityByProfileCommand
        {
            ProfileId = profileId,
            UserId = userId,
            SelfieImageData = memoryStream.ToArray(),
            FileName = selfie.FileName,
            ContentType = selfie.ContentType,
            LivenessData = livenessDto
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "VerificationError", message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NotFound", message = "Perfil KYC no encontrado" });
        }
    }

    #region Helper Methods

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst("userId")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    #endregion
}

/// <summary>
/// Request para completar verificación
/// </summary>
public class CompleteVerificationRequest
{
    public Guid SessionId { get; set; }
}

/// <summary>
/// Request para reintentar verificación
/// </summary>
public class RetryRequest
{
    public Guid SessionId { get; set; }
}
