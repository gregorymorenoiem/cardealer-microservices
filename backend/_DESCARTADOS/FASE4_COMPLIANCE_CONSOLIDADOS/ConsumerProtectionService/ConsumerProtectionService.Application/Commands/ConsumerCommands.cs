// =====================================================
// ConsumerProtectionService - Commands
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using MediatR;
using ConsumerProtectionService.Application.DTOs;

namespace ConsumerProtectionService.Application.Commands;

public record CreateWarrantyCommand(CreateWarrantyDto Warranty) : IRequest<WarrantyDto>;

public record ActivateWarrantyCommand(Guid WarrantyId, Guid ConsumerId) : IRequest<bool>;

public record CreateWarrantyClaimCommand(CreateWarrantyClaimDto Claim) : IRequest<WarrantyClaimDto>;

public record ResolveWarrantyClaimCommand(Guid ClaimId, ResolveClaimDto Resolution) : IRequest<bool>;

public record CreateComplaintCommand(CreateComplaintDto Complaint) : IRequest<ComplaintDto>;

public record RespondToComplaintCommand(Guid ComplaintId, RespondToComplaintDto Response) : IRequest<bool>;

public record EscalateToProConsumidorCommand(Guid ComplaintId, EscalateToProConsumidorDto Escalation) : IRequest<bool>;

public record CloseComplaintCommand(Guid ComplaintId, string Resolution) : IRequest<bool>;

public record AddComplaintEvidenceCommand(Guid ComplaintId, string FileName, string FileType, string FilePath, string? Description) : IRequest<ComplaintEvidenceDto>;

public record ScheduleMediationCommand(CreateMediationDto Mediation) : IRequest<MediationDto>;

public record CompleteMediationCommand(Guid MediationId, string Outcome, string? AgreementSummary) : IRequest<bool>;
