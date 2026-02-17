// =====================================================
// ConsumerProtectionService - Handlers
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using MediatR;
using ConsumerProtectionService.Application.Commands;
using ConsumerProtectionService.Application.Queries;
using ConsumerProtectionService.Application.DTOs;
using ConsumerProtectionService.Domain.Interfaces;
using ConsumerProtectionService.Domain.Entities;
using ConsumerProtectionService.Domain.Enums;

namespace ConsumerProtectionService.Application.Handlers;

public class CreateWarrantyHandler : IRequestHandler<CreateWarrantyCommand, WarrantyDto>
{
    private readonly IWarrantyRepository _repository;

    public CreateWarrantyHandler(IWarrantyRepository repository) => _repository = repository;

    public async Task<WarrantyDto> Handle(CreateWarrantyCommand request, CancellationToken cancellationToken)
    {
        var warranty = new Warranty
        {
            Id = Guid.NewGuid(),
            ProductId = request.Warranty.ProductId,
            SellerId = request.Warranty.SellerId,
            ConsumerId = request.Warranty.ConsumerId,
            WarrantyNumber = $"WRN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            WarrantyType = Enum.Parse<WarrantyType>(request.Warranty.WarrantyType),
            StartDate = request.Warranty.StartDate,
            EndDate = request.Warranty.EndDate,
            Status = WarrantyStatus.Active,
            CoverageDescription = request.Warranty.CoverageDescription,
            Exclusions = request.Warranty.Exclusions,
            PurchasePrice = request.Warranty.PurchasePrice,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(warranty);
        return MapToDto(created);
    }

    private static WarrantyDto MapToDto(Warranty w) => new(
        w.Id, w.ProductId, w.SellerId, w.ConsumerId, w.WarrantyNumber,
        w.WarrantyType.ToString(), w.StartDate, w.EndDate, w.Status.ToString(),
        w.CoverageDescription, w.Exclusions, w.PurchasePrice, w.CreatedAt
    );
}

public class GetWarrantyByIdHandler : IRequestHandler<GetWarrantyByIdQuery, WarrantyDto?>
{
    private readonly IWarrantyRepository _repository;

    public GetWarrantyByIdHandler(IWarrantyRepository repository) => _repository = repository;

    public async Task<WarrantyDto?> Handle(GetWarrantyByIdQuery request, CancellationToken cancellationToken)
    {
        var warranty = await _repository.GetByIdAsync(request.Id);
        if (warranty == null) return null;
        return new WarrantyDto(
            warranty.Id, warranty.ProductId, warranty.SellerId, warranty.ConsumerId,
            warranty.WarrantyNumber, warranty.WarrantyType.ToString(), warranty.StartDate,
            warranty.EndDate, warranty.Status.ToString(), warranty.CoverageDescription,
            warranty.Exclusions, warranty.PurchasePrice, warranty.CreatedAt
        );
    }
}

public class CreateComplaintHandler : IRequestHandler<CreateComplaintCommand, ComplaintDto>
{
    private readonly IComplaintRepository _repository;

    public CreateComplaintHandler(IComplaintRepository repository) => _repository = repository;

    public async Task<ComplaintDto> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        // Según Ley 358-05, el vendedor tiene 15 días para responder
        var responseDueDate = DateTime.UtcNow.AddDays(15);

        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ConsumerId = request.Complaint.ConsumerId,
            SellerId = request.Complaint.SellerId,
            ProductId = request.Complaint.ProductId,
            ComplaintNumber = $"CMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            ComplaintType = Enum.Parse<ComplaintType>(request.Complaint.ComplaintType),
            Description = request.Complaint.Description,
            Status = ComplaintStatus.Pending,
            Priority = Enum.Parse<ComplaintPriority>(request.Complaint.Priority),
            ResponseDueDate = responseDueDate,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(complaint);
        return MapToDto(created);
    }

    private static ComplaintDto MapToDto(Complaint c) => new(
        c.Id, c.ConsumerId, c.SellerId, c.ProductId, c.ComplaintNumber,
        c.ComplaintType.ToString(), c.Description, c.Status.ToString(),
        c.Priority.ToString(), c.ResponseDueDate, c.ResponseDate, c.ResponseNotes,
        c.IsEscalatedToProConsumidor, c.ProConsumidorCaseNumber, c.CreatedAt
    );
}

public class GetConsumerProtectionStatisticsHandler : IRequestHandler<GetConsumerProtectionStatisticsQuery, ConsumerProtectionStatisticsDto>
{
    private readonly IWarrantyRepository _warrantyRepository;
    private readonly IWarrantyClaimRepository _claimRepository;
    private readonly IComplaintRepository _complaintRepository;

    public GetConsumerProtectionStatisticsHandler(
        IWarrantyRepository warrantyRepository,
        IWarrantyClaimRepository claimRepository,
        IComplaintRepository complaintRepository)
    {
        _warrantyRepository = warrantyRepository;
        _claimRepository = claimRepository;
        _complaintRepository = complaintRepository;
    }

    public async Task<ConsumerProtectionStatisticsDto> Handle(GetConsumerProtectionStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalWarranties = await _warrantyRepository.GetCountAsync();
        var activeWarranties = (await _warrantyRepository.GetActiveWarrantiesAsync()).Count();
        var pendingClaims = (await _claimRepository.GetPendingClaimsAsync()).Count();
        var totalComplaints = await _complaintRepository.GetCountAsync();
        var pendingComplaints = (await _complaintRepository.GetPendingComplaintsAsync()).Count();
        var escalated = (await _complaintRepository.GetEscalatedToProConsumidorAsync()).Count();

        return new ConsumerProtectionStatisticsDto(
            totalWarranties, activeWarranties, 0, pendingClaims,
            totalComplaints, pendingComplaints, escalated, 0, DateTime.UtcNow
        );
    }
}
