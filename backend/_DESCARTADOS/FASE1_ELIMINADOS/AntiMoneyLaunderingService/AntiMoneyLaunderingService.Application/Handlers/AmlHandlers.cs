// =====================================================
// AntiMoneyLaunderingService - Handlers
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using MediatR;
using AntiMoneyLaunderingService.Application.Commands;
using AntiMoneyLaunderingService.Application.Queries;
using AntiMoneyLaunderingService.Application.DTOs;
using AntiMoneyLaunderingService.Domain.Interfaces;
using AntiMoneyLaunderingService.Domain.Entities;
using AntiMoneyLaunderingService.Domain.Enums;

namespace AntiMoneyLaunderingService.Application.Handlers;

// Customer Handlers
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = request.Customer.UserId,
            FullName = request.Customer.FullName,
            IdentificationType = Enum.Parse<IdentificationType>(request.Customer.IdentificationType),
            IdentificationNumber = request.Customer.IdentificationNumber,
            DateOfBirth = request.Customer.DateOfBirth,
            Nationality = request.Customer.Nationality,
            Occupation = request.Customer.Occupation,
            SourceOfFunds = request.Customer.SourceOfFunds,
            EstimatedMonthlyIncome = request.Customer.EstimatedMonthlyIncome,
            RiskLevel = RiskLevel.Low,
            KycStatus = KycStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(customer);
        return MapToDto(created);
    }

    private static CustomerDto MapToDto(Customer c) => new(
        c.Id, c.UserId, c.FullName, c.IdentificationType.ToString(), c.IdentificationNumber,
        c.DateOfBirth, c.Nationality, c.Occupation, c.SourceOfFunds, c.EstimatedMonthlyIncome,
        c.RiskLevel.ToString(), c.KycStatus.ToString(), c.LastKycReviewDate, c.NextKycReviewDate,
        c.IsPep, c.PepCategory?.ToString(), c.PepPosition, c.IsOnSanctionsList, c.CreatedAt
    );
}

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id);
        if (customer == null) return null;

        return new CustomerDto(
            customer.Id, customer.UserId, customer.FullName, customer.IdentificationType.ToString(),
            customer.IdentificationNumber, customer.DateOfBirth, customer.Nationality, customer.Occupation,
            customer.SourceOfFunds, customer.EstimatedMonthlyIncome, customer.RiskLevel.ToString(),
            customer.KycStatus.ToString(), customer.LastKycReviewDate, customer.NextKycReviewDate,
            customer.IsPep, customer.PepCategory?.ToString(), customer.PepPosition, customer.IsOnSanctionsList,
            customer.CreatedAt
        );
    }
}

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly ICustomerRepository _repository;

    public GetAllCustomersHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _repository.GetAllAsync(request.Page, request.PageSize);
        return customers.Select(c => new CustomerDto(
            c.Id, c.UserId, c.FullName, c.IdentificationType.ToString(), c.IdentificationNumber,
            c.DateOfBirth, c.Nationality, c.Occupation, c.SourceOfFunds, c.EstimatedMonthlyIncome,
            c.RiskLevel.ToString(), c.KycStatus.ToString(), c.LastKycReviewDate, c.NextKycReviewDate,
            c.IsPep, c.PepCategory?.ToString(), c.PepPosition, c.IsOnSanctionsList, c.CreatedAt
        ));
    }
}

public class GetAmlStatisticsHandler : IRequestHandler<GetAmlStatisticsQuery, AmlStatisticsDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ISuspiciousActivityReportRepository _reportRepository;
    private readonly IAmlAlertRepository _alertRepository;

    public GetAmlStatisticsHandler(
        ICustomerRepository customerRepository,
        ISuspiciousActivityReportRepository reportRepository,
        IAmlAlertRepository alertRepository)
    {
        _customerRepository = customerRepository;
        _reportRepository = reportRepository;
        _alertRepository = alertRepository;
    }

    public async Task<AmlStatisticsDto> Handle(GetAmlStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalCustomers = await _customerRepository.GetCountAsync();
        var highRisk = (await _customerRepository.GetByRiskLevelAsync("High")).Count();
        var peps = (await _customerRepository.GetPepsAsync()).Count();
        var pendingReports = (await _reportRepository.GetPendingSubmissionAsync()).Count();
        var activeAlerts = await _alertRepository.GetActiveCountAsync();

        return new AmlStatisticsDto(
            totalCustomers, highRisk, peps, pendingReports, 0, activeAlerts, 0, DateTime.UtcNow
        );
    }
}
