// =====================================================
// TaxComplianceService - Queries
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using MediatR;
using TaxComplianceService.Application.DTOs;

namespace TaxComplianceService.Application.Queries;

public record GetTaxpayerByIdQuery(Guid Id) : IRequest<TaxpayerDto?>;

public record GetTaxpayerByRncQuery(string Rnc) : IRequest<TaxpayerDto?>;

public record GetAllTaxpayersQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<TaxpayerDto>>;

public record GetTaxDeclarationByIdQuery(Guid Id) : IRequest<TaxDeclarationDto?>;

public record GetDeclarationsByTaxpayerQuery(Guid TaxpayerId) : IRequest<IEnumerable<TaxDeclarationDto>>;

public record GetDeclarationsByPeriodQuery(string Period) : IRequest<IEnumerable<TaxDeclarationDto>>;

public record GetPendingDeclarationsQuery() : IRequest<IEnumerable<TaxDeclarationDto>>;

public record GetOverdueDeclarationsQuery() : IRequest<IEnumerable<TaxDeclarationDto>>;

public record GetNcfSequencesByTaxpayerQuery(Guid TaxpayerId) : IRequest<IEnumerable<NcfSequenceDto>>;

public record GetActiveNcfSequenceQuery(Guid TaxpayerId, string NcfType) : IRequest<NcfSequenceDto?>;

public record GetReporte606ItemsQuery(Guid DeclarationId) : IRequest<IEnumerable<Reporte606ItemDto>>;

public record GetReporte607ItemsQuery(Guid DeclarationId) : IRequest<IEnumerable<Reporte607ItemDto>>;

public record GetTaxStatisticsQuery() : IRequest<TaxStatisticsDto>;

public record GetPendingPaymentsQuery() : IRequest<IEnumerable<TaxPaymentDto>>;
