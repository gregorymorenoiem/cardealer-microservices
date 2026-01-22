using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KYCService.Application.Commands;
using KYCService.Application.Queries;
using KYCService.Application.DTOs;

namespace KYCService.Api.Controllers;

/// <summary>
/// Controlador para documentos y verificaciones KYC
/// </summary>
[ApiController]
[Route("api/kyc")]
public class KYCDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<KYCDocumentsController> _logger;

    public KYCDocumentsController(IMediator mediator, ILogger<KYCDocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener documentos de un perfil KYC
    /// </summary>
    [HttpGet("profiles/{profileId:guid}/documents")]
    [Authorize]
    public async Task<ActionResult<List<KYCDocumentDto>>> GetDocuments(Guid profileId)
    {
        var result = await _mediator.Send(new GetKYCDocumentsQuery(profileId));
        return Ok(result);
    }

    /// <summary>
    /// Subir documento KYC
    /// </summary>
    [HttpPost("profiles/{profileId:guid}/documents")]
    [Authorize]
    public async Task<ActionResult<KYCDocumentDto>> UploadDocument(
        Guid profileId, 
        [FromBody] UploadKYCDocumentCommand command)
    {
        if (profileId != command.KYCProfileId)
            return BadRequest("Profile ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("Document uploaded for KYC Profile {ProfileId}", profileId);
        return CreatedAtAction(nameof(GetDocuments), new { profileId }, result);
    }

    /// <summary>
    /// Verificar documento KYC
    /// </summary>
    [HttpPost("documents/{documentId:guid}/verify")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCDocumentDto>> VerifyDocument(
        Guid documentId, 
        [FromBody] VerifyKYCDocumentCommand command)
    {
        if (documentId != command.Id)
            return BadRequest("Document ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("Document {DocumentId} verification: {Status}", 
            documentId, command.Approved ? "Approved" : "Rejected");
        return Ok(result);
    }

    /// <summary>
    /// Obtener verificaciones de un perfil KYC
    /// </summary>
    [HttpGet("profiles/{profileId:guid}/verifications")]
    [Authorize]
    public async Task<ActionResult<List<KYCVerificationDto>>> GetVerifications(Guid profileId)
    {
        var result = await _mediator.Send(new GetKYCVerificationsQuery(profileId));
        return Ok(result);
    }

    /// <summary>
    /// Crear verificación KYC
    /// </summary>
    [HttpPost("profiles/{profileId:guid}/verifications")]
    [Authorize(Roles = "Admin,Compliance,System")]
    public async Task<ActionResult<KYCVerificationDto>> CreateVerification(
        Guid profileId,
        [FromBody] CreateKYCVerificationCommand command)
    {
        if (profileId != command.KYCProfileId)
            return BadRequest("Profile ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("Verification {Type} created for KYC Profile {ProfileId}: {Passed}",
            command.VerificationType, profileId, command.Passed);
        return Ok(result);
    }

    /// <summary>
    /// Obtener historial de evaluaciones de riesgo
    /// </summary>
    [HttpGet("profiles/{profileId:guid}/risk-assessments")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<KYCRiskAssessmentDto>>> GetRiskAssessments(Guid profileId)
    {
        var result = await _mediator.Send(new GetKYCRiskAssessmentsQuery(profileId));
        return Ok(result);
    }

    /// <summary>
    /// Crear evaluación de riesgo KYC
    /// </summary>
    [HttpPost("profiles/{profileId:guid}/risk-assessments")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCRiskAssessmentDto>> AssessRisk(
        Guid profileId,
        [FromBody] AssessKYCRiskCommand command)
    {
        if (profileId != command.KYCProfileId)
            return BadRequest("Profile ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("Risk assessment for KYC Profile {ProfileId}: {OldLevel} -> {NewLevel}",
            profileId, result.PreviousLevel, result.NewLevel);
        return Ok(result);
    }
}
