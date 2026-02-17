// =====================================================
// AntiMoneyLaunderingService - Commands
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using MediatR;
using AntiMoneyLaunderingService.Application.DTOs;

namespace AntiMoneyLaunderingService.Application.Commands;

public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<CustomerDto>;

public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Customer) : IRequest<CustomerDto?>;

public record UpdateRiskLevelCommand(Guid CustomerId, string RiskLevel, string Reason) : IRequest<bool>;

public record CreateTransactionCommand(CreateTransactionDto Transaction) : IRequest<TransactionDto>;

public record FlagTransactionAsSuspiciousCommand(Guid TransactionId, string Reason) : IRequest<bool>;

public record CreateSuspiciousActivityReportCommand(CreateSuspiciousActivityReportDto Report) : IRequest<SuspiciousActivityReportDto>;

public record SubmitReportToUafCommand(Guid ReportId, Guid SubmittedByUserId) : IRequest<bool>;

public record AcknowledgeAlertCommand(Guid AlertId, Guid AcknowledgedByUserId, string Notes) : IRequest<bool>;

public record PerformKycReviewCommand(Guid CustomerId, Guid ReviewerUserId, string ReviewNotes, bool Approved) : IRequest<bool>;
