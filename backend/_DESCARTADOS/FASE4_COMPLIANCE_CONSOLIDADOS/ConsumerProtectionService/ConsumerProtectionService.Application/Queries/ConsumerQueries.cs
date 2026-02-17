// =====================================================
// ConsumerProtectionService - Queries
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using MediatR;
using ConsumerProtectionService.Application.DTOs;

namespace ConsumerProtectionService.Application.Queries;

public record GetWarrantyByIdQuery(Guid Id) : IRequest<WarrantyDto?>;

public record GetWarrantiesByProductQuery(Guid ProductId) : IRequest<IEnumerable<WarrantyDto>>;

public record GetWarrantiesBySellerQuery(Guid SellerId) : IRequest<IEnumerable<WarrantyDto>>;

public record GetWarrantiesByConsumerQuery(Guid ConsumerId) : IRequest<IEnumerable<WarrantyDto>>;

public record GetActiveWarrantiesQuery() : IRequest<IEnumerable<WarrantyDto>>;

public record GetExpiringWarrantiesQuery(int DaysAhead = 30) : IRequest<IEnumerable<WarrantyDto>>;

public record GetWarrantyClaimsByWarrantyQuery(Guid WarrantyId) : IRequest<IEnumerable<WarrantyClaimDto>>;

public record GetWarrantyClaimsByConsumerQuery(Guid ConsumerId) : IRequest<IEnumerable<WarrantyClaimDto>>;

public record GetPendingWarrantyClaimsQuery() : IRequest<IEnumerable<WarrantyClaimDto>>;

public record GetComplaintByIdQuery(Guid Id) : IRequest<ComplaintDto?>;

public record GetComplaintsByConsumerQuery(Guid ConsumerId) : IRequest<IEnumerable<ComplaintDto>>;

public record GetComplaintsBySellerQuery(Guid SellerId) : IRequest<IEnumerable<ComplaintDto>>;

public record GetPendingComplaintsQuery() : IRequest<IEnumerable<ComplaintDto>>;

public record GetEscalatedComplaintsQuery() : IRequest<IEnumerable<ComplaintDto>>;

public record GetComplaintEvidencesQuery(Guid ComplaintId) : IRequest<IEnumerable<ComplaintEvidenceDto>>;

public record GetMediationsByComplaintQuery(Guid ComplaintId) : IRequest<IEnumerable<MediationDto>>;

public record GetScheduledMediationsQuery() : IRequest<IEnumerable<MediationDto>>;

public record GetConsumerProtectionStatisticsQuery() : IRequest<ConsumerProtectionStatisticsDto>;
