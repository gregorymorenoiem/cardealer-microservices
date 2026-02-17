// =====================================================
// TaxComplianceService - Handlers
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using MediatR;
using TaxComplianceService.Application.Commands;
using TaxComplianceService.Application.Queries;
using TaxComplianceService.Application.DTOs;
using TaxComplianceService.Domain.Interfaces;
using TaxComplianceService.Domain.Entities;
using TaxComplianceService.Domain.Enums;

namespace TaxComplianceService.Application.Handlers;

public class CreateTaxpayerHandler : IRequestHandler<CreateTaxpayerCommand, TaxpayerDto>
{
    private readonly ITaxpayerRepository _repository;

    public CreateTaxpayerHandler(ITaxpayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaxpayerDto> Handle(CreateTaxpayerCommand request, CancellationToken cancellationToken)
    {
        var taxpayer = new Taxpayer
        {
            Id = Guid.NewGuid(),
            Rnc = request.Taxpayer.Rnc,
            BusinessName = request.Taxpayer.BusinessName,
            TradeName = request.Taxpayer.TradeName,
            TaxpayerType = Enum.Parse<TaxpayerType>(request.Taxpayer.TaxpayerType),
            Email = request.Taxpayer.Email,
            Phone = request.Taxpayer.Phone,
            Address = request.Taxpayer.Address,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(taxpayer);
        return MapToDto(created);
    }

    private static TaxpayerDto MapToDto(Taxpayer t) => new(
        t.Id, t.Rnc, t.BusinessName, t.TradeName, t.TaxpayerType.ToString(),
        t.Email, t.Phone, t.Address, t.IsActive, t.RegisteredAt, t.CreatedAt
    );
}

public class GetTaxpayerByIdHandler : IRequestHandler<GetTaxpayerByIdQuery, TaxpayerDto?>
{
    private readonly ITaxpayerRepository _repository;

    public GetTaxpayerByIdHandler(ITaxpayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaxpayerDto?> Handle(GetTaxpayerByIdQuery request, CancellationToken cancellationToken)
    {
        var taxpayer = await _repository.GetByIdAsync(request.Id);
        if (taxpayer == null) return null;
        return new TaxpayerDto(
            taxpayer.Id, taxpayer.Rnc, taxpayer.BusinessName, taxpayer.TradeName,
            taxpayer.TaxpayerType.ToString(), taxpayer.Email, taxpayer.Phone,
            taxpayer.Address, taxpayer.IsActive, taxpayer.RegisteredAt, taxpayer.CreatedAt
        );
    }
}

public class GetTaxpayerByRncHandler : IRequestHandler<GetTaxpayerByRncQuery, TaxpayerDto?>
{
    private readonly ITaxpayerRepository _repository;

    public GetTaxpayerByRncHandler(ITaxpayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaxpayerDto?> Handle(GetTaxpayerByRncQuery request, CancellationToken cancellationToken)
    {
        var taxpayer = await _repository.GetByRncAsync(request.Rnc);
        if (taxpayer == null) return null;
        return new TaxpayerDto(
            taxpayer.Id, taxpayer.Rnc, taxpayer.BusinessName, taxpayer.TradeName,
            taxpayer.TaxpayerType.ToString(), taxpayer.Email, taxpayer.Phone,
            taxpayer.Address, taxpayer.IsActive, taxpayer.RegisteredAt, taxpayer.CreatedAt
        );
    }
}

public class GetAllTaxpayersHandler : IRequestHandler<GetAllTaxpayersQuery, IEnumerable<TaxpayerDto>>
{
    private readonly ITaxpayerRepository _repository;

    public GetAllTaxpayersHandler(ITaxpayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaxpayerDto>> Handle(GetAllTaxpayersQuery request, CancellationToken cancellationToken)
    {
        var taxpayers = await _repository.GetAllAsync(request.Page, request.PageSize);
        return taxpayers.Select(t => new TaxpayerDto(
            t.Id, t.Rnc, t.BusinessName, t.TradeName, t.TaxpayerType.ToString(),
            t.Email, t.Phone, t.Address, t.IsActive, t.RegisteredAt, t.CreatedAt
        ));
    }
}

public class CreateTaxDeclarationHandler : IRequestHandler<CreateTaxDeclarationCommand, TaxDeclarationDto>
{
    private readonly ITaxDeclarationRepository _repository;
    private readonly ITaxpayerRepository _taxpayerRepository;

    public CreateTaxDeclarationHandler(ITaxDeclarationRepository repository, ITaxpayerRepository taxpayerRepository)
    {
        _repository = repository;
        _taxpayerRepository = taxpayerRepository;
    }

    public async Task<TaxDeclarationDto> Handle(CreateTaxDeclarationCommand request, CancellationToken cancellationToken)
    {
        var taxpayer = await _taxpayerRepository.GetByIdAsync(request.Declaration.TaxpayerId);
        if (taxpayer == null)
            throw new InvalidOperationException("Contribuyente no encontrado");

        var netPayable = request.Declaration.TaxAmount - request.Declaration.WithholdingAmount;
        var dueDate = CalculateDueDate(request.Declaration.Period, request.Declaration.DeclarationType);

        var declaration = new TaxDeclaration
        {
            Id = Guid.NewGuid(),
            TaxpayerId = request.Declaration.TaxpayerId,
            Rnc = taxpayer.Rnc,
            DeclarationType = Enum.Parse<DeclarationType>(request.Declaration.DeclarationType),
            Period = request.Declaration.Period,
            GrossAmount = request.Declaration.GrossAmount,
            TaxableAmount = request.Declaration.TaxableAmount,
            TaxAmount = request.Declaration.TaxAmount,
            WithholdingAmount = request.Declaration.WithholdingAmount,
            NetPayable = netPayable,
            Status = DeclarationStatus.Draft,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(declaration);
        return MapToDto(created);
    }

    private static DateTime CalculateDueDate(string period, string declarationType)
    {
        // YYYYMM format
        var year = int.Parse(period[..4]);
        var month = int.Parse(period[4..]);
        
        // ITBIS y retenciones vencen el día 20 del mes siguiente
        var nextMonth = month == 12 ? 1 : month + 1;
        var nextYear = month == 12 ? year + 1 : year;
        
        return new DateTime(nextYear, nextMonth, 20);
    }

    private static TaxDeclarationDto MapToDto(TaxDeclaration d) => new(
        d.Id, d.TaxpayerId, d.Rnc, d.DeclarationType.ToString(), d.Period,
        d.GrossAmount, d.TaxableAmount, d.TaxAmount, d.WithholdingAmount,
        d.NetPayable, d.Status.ToString(), d.DgiiConfirmationNumber,
        d.SubmittedAt, d.DueDate, d.CreatedAt
    );
}

public class GetTaxStatisticsHandler : IRequestHandler<GetTaxStatisticsQuery, TaxStatisticsDto>
{
    private readonly ITaxpayerRepository _taxpayerRepository;
    private readonly ITaxDeclarationRepository _declarationRepository;

    public GetTaxStatisticsHandler(
        ITaxpayerRepository taxpayerRepository,
        ITaxDeclarationRepository declarationRepository)
    {
        _taxpayerRepository = taxpayerRepository;
        _declarationRepository = declarationRepository;
    }

    public async Task<TaxStatisticsDto> Handle(GetTaxStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalTaxpayers = await _taxpayerRepository.GetCountAsync();
        var totalDeclarations = await _declarationRepository.GetCountAsync();
        var pending = (await _declarationRepository.GetPendingDeclarationsAsync()).Count();
        var overdue = (await _declarationRepository.GetOverdueDeclarationsAsync()).Count();

        return new TaxStatisticsDto(
            totalTaxpayers, totalDeclarations, pending, overdue, 0, 0, DateTime.UtcNow
        );
    }
}
