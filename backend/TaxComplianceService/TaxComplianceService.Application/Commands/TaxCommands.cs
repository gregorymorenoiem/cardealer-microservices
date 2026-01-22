// =====================================================
// TaxComplianceService - Commands
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using MediatR;
using TaxComplianceService.Application.DTOs;

namespace TaxComplianceService.Application.Commands;

public record CreateTaxpayerCommand(CreateTaxpayerDto Taxpayer) : IRequest<TaxpayerDto>;

public record UpdateTaxpayerCommand(Guid Id, CreateTaxpayerDto Taxpayer) : IRequest<TaxpayerDto?>;

public record CreateTaxDeclarationCommand(CreateTaxDeclarationDto Declaration) : IRequest<TaxDeclarationDto>;

public record SubmitDeclarationToDgiiCommand(Guid DeclarationId) : IRequest<DgiiSubmissionResultDto>;

public record CreateTaxPaymentCommand(CreateTaxPaymentDto Payment) : IRequest<TaxPaymentDto>;

public record ConfirmPaymentCommand(Guid PaymentId, string DgiiReceiptNumber) : IRequest<bool>;

public record CreateNcfSequenceCommand(CreateNcfSequenceDto Sequence) : IRequest<NcfSequenceDto>;

public record GetNextNcfCommand(Guid TaxpayerId, string NcfType) : IRequest<string>;

public record AddReporte606ItemsCommand(Guid DeclarationId, List<Reporte606ItemDto> Items) : IRequest<bool>;

public record AddReporte607ItemsCommand(Guid DeclarationId, List<Reporte607ItemDto> Items) : IRequest<bool>;

public record GenerateITBISDeclarationCommand(Guid TaxpayerId, string Period) : IRequest<TaxDeclarationDto>;
