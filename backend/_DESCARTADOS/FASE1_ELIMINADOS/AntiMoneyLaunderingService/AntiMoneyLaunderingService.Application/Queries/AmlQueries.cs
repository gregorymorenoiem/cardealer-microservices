// =====================================================
// AntiMoneyLaunderingService - Queries
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using MediatR;
using AntiMoneyLaunderingService.Application.DTOs;

namespace AntiMoneyLaunderingService.Application.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

public record GetCustomerByUserIdQuery(Guid UserId) : IRequest<CustomerDto?>;

public record GetAllCustomersQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<CustomerDto>>;

public record GetHighRiskCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;

public record GetPepCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;

public record GetTransactionsByCustomerQuery(Guid CustomerId) : IRequest<IEnumerable<TransactionDto>>;

public record GetSuspiciousTransactionsQuery() : IRequest<IEnumerable<TransactionDto>>;

public record GetAboveThresholdTransactionsQuery(decimal Threshold = 50000) : IRequest<IEnumerable<TransactionDto>>;

public record GetSuspiciousActivityReportByIdQuery(Guid Id) : IRequest<SuspiciousActivityReportDto?>;

public record GetAllSuspiciousActivityReportsQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<SuspiciousActivityReportDto>>;

public record GetPendingReportsQuery() : IRequest<IEnumerable<SuspiciousActivityReportDto>>;

public record GetActiveAlertsQuery() : IRequest<IEnumerable<AmlAlertDto>>;

public record GetCustomerAlertsQuery(Guid CustomerId) : IRequest<IEnumerable<AmlAlertDto>>;

public record GetAmlStatisticsQuery() : IRequest<AmlStatisticsDto>;
