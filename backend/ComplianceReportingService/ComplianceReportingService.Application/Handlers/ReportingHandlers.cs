// =====================================================
// ComplianceReportingService - Handlers
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using MediatR;
using ComplianceReportingService.Application.Commands;
using ComplianceReportingService.Application.Queries;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Interfaces;
using ComplianceReportingService.Domain.Enums;
using System.Text.Json;

namespace ComplianceReportingService.Application.Handlers;

// ==================== Report Handlers ====================

public class GenerateReportHandler : IRequestHandler<GenerateReportCommand, ComplianceReportDto>
{
    private readonly IComplianceReportRepository _repository;

    public GenerateReportHandler(IComplianceReportRepository repository) => _repository = repository;

    public async Task<ComplianceReportDto> Handle(GenerateReportCommand request, CancellationToken ct)
    {
        var reportNumber = $"RPT-{request.Data.RegulatoryBody}-{request.Data.Period}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var report = new ComplianceReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = reportNumber,
            ReportType = request.Data.ReportType,
            RegulatoryBody = request.Data.RegulatoryBody,
            Period = request.Data.Period,
            StartDate = request.Data.StartDate,
            EndDate = request.Data.EndDate,
            Status = ReportStatus.Generated,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "System",
            CreatedAt = DateTime.UtcNow
        };

        // En producción, aquí se recopilan datos de otros servicios
        report.ReportContent = JsonSerializer.Serialize(new { GeneratedAt = DateTime.UtcNow, Period = request.Data.Period });

        await _repository.AddAsync(report);

        return new ComplianceReportDto(
            report.Id, report.ReportNumber, report.ReportType, report.RegulatoryBody,
            report.Period, report.StartDate, report.EndDate, report.Status,
            report.GeneratedAt, report.GeneratedBy, report.SubmittedAt, 0
        );
    }
}

public class GetReportByIdHandler : IRequestHandler<GetReportByIdQuery, ComplianceReportDetailDto?>
{
    private readonly IComplianceReportRepository _reportRepo;
    private readonly IReportItemRepository _itemRepo;
    private readonly IReportSubmissionRepository _submissionRepo;

    public GetReportByIdHandler(
        IComplianceReportRepository reportRepo,
        IReportItemRepository itemRepo,
        IReportSubmissionRepository submissionRepo)
    {
        _reportRepo = reportRepo;
        _itemRepo = itemRepo;
        _submissionRepo = submissionRepo;
    }

    public async Task<ComplianceReportDetailDto?> Handle(GetReportByIdQuery request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.Id);
        if (report == null) return null;

        var items = await _itemRepo.GetByReportIdAsync(report.Id);
        var submissions = await _submissionRepo.GetByReportIdAsync(report.Id);

        return new ComplianceReportDetailDto(
            report.Id, report.ReportNumber, report.ReportType, report.RegulatoryBody,
            report.Period, report.StartDate, report.EndDate, report.Status,
            report.ReportContent, report.FilePath, report.SubmissionReference,
            report.GeneratedAt, report.GeneratedBy, report.SubmittedAt, report.RejectionReason,
            items.Select(i => new ReportItemDto(i.Id, i.ItemType, i.ItemData, i.Amount, i.ReferenceNumber, i.ItemDate)),
            submissions.Select(s => new ReportSubmissionDto(s.Id, s.SubmittedAt, s.SubmissionMethod, s.SubmissionReference, s.IsSuccessful, s.ResponseMessage, s.SubmittedBy))
        );
    }
}

public class GetReportsByBodyHandler : IRequestHandler<GetReportsByBodyQuery, IEnumerable<ComplianceReportDto>>
{
    private readonly IComplianceReportRepository _repository;

    public GetReportsByBodyHandler(IComplianceReportRepository repository) => _repository = repository;

    public async Task<IEnumerable<ComplianceReportDto>> Handle(GetReportsByBodyQuery request, CancellationToken ct)
    {
        var reports = await _repository.GetByRegulatoryBodyAsync(request.Body);
        return reports.Select(r => new ComplianceReportDto(
            r.Id, r.ReportNumber, r.ReportType, r.RegulatoryBody, r.Period,
            r.StartDate, r.EndDate, r.Status, r.GeneratedAt, r.GeneratedBy,
            r.SubmittedAt, r.Items?.Count ?? 0
        ));
    }
}

public class SubmitReportHandler : IRequestHandler<SubmitReportCommand, ReportSubmissionDto>
{
    private readonly IComplianceReportRepository _reportRepo;
    private readonly IReportSubmissionRepository _submissionRepo;

    public SubmitReportHandler(IComplianceReportRepository reportRepo, IReportSubmissionRepository submissionRepo)
    {
        _reportRepo = reportRepo;
        _submissionRepo = submissionRepo;
    }

    public async Task<ReportSubmissionDto> Handle(SubmitReportCommand request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.Data.ReportId);
        if (report == null) throw new KeyNotFoundException("Reporte no encontrado");

        var submission = new ReportSubmission
        {
            Id = Guid.NewGuid(),
            ComplianceReportId = report.Id,
            SubmittedAt = DateTime.UtcNow,
            SubmissionMethod = request.Data.SubmissionMethod,
            SubmissionReference = $"SUB-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            IsSuccessful = true, // En producción, resultado real del envío
            ResponseMessage = "Reporte recibido correctamente",
            SubmittedBy = "System",
            CreatedAt = DateTime.UtcNow
        };

        await _submissionRepo.AddAsync(submission);

        report.Status = ReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;
        report.SubmissionReference = submission.SubmissionReference;
        report.UpdatedAt = DateTime.UtcNow;
        await _reportRepo.UpdateAsync(report);

        return new ReportSubmissionDto(
            submission.Id, submission.SubmittedAt, submission.SubmissionMethod,
            submission.SubmissionReference, submission.IsSuccessful,
            submission.ResponseMessage, submission.SubmittedBy
        );
    }
}

// ==================== Schedule Handlers ====================

public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, ReportScheduleDto>
{
    private readonly IReportScheduleRepository _repository;

    public CreateScheduleHandler(IReportScheduleRepository repository) => _repository = repository;

    public async Task<ReportScheduleDto> Handle(CreateScheduleCommand request, CancellationToken ct)
    {
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ReportType = request.Data.ReportType,
            RegulatoryBody = request.Data.RegulatoryBody,
            CronExpression = request.Data.CronExpression,
            IsActive = true,
            Description = request.Data.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(schedule);

        return new ReportScheduleDto(
            schedule.Id, schedule.ReportType, schedule.RegulatoryBody,
            schedule.CronExpression, schedule.IsActive, schedule.Description,
            schedule.LastRunAt, schedule.NextRunAt
        );
    }
}

public class GetActiveSchedulesHandler : IRequestHandler<GetActiveSchedulesQuery, IEnumerable<ReportScheduleDto>>
{
    private readonly IReportScheduleRepository _repository;

    public GetActiveSchedulesHandler(IReportScheduleRepository repository) => _repository = repository;

    public async Task<IEnumerable<ReportScheduleDto>> Handle(GetActiveSchedulesQuery request, CancellationToken ct)
    {
        var schedules = await _repository.GetActiveSchedulesAsync();
        return schedules.Select(s => new ReportScheduleDto(
            s.Id, s.ReportType, s.RegulatoryBody, s.CronExpression,
            s.IsActive, s.Description, s.LastRunAt, s.NextRunAt
        ));
    }
}

// ==================== Statistics Handler ====================

public class GetReportingStatisticsHandler : IRequestHandler<GetReportingStatisticsQuery, ReportingStatisticsDto>
{
    private readonly IComplianceReportRepository _reportRepo;
    private readonly IReportScheduleRepository _scheduleRepo;

    public GetReportingStatisticsHandler(IComplianceReportRepository reportRepo, IReportScheduleRepository scheduleRepo)
    {
        _reportRepo = reportRepo;
        _scheduleRepo = scheduleRepo;
    }

    public async Task<ReportingStatisticsDto> Handle(GetReportingStatisticsQuery request, CancellationToken ct)
    {
        var total = await _reportRepo.GetCountAsync();
        var pending = await _reportRepo.GetPendingSubmissionAsync();
        var submitted = await _reportRepo.GetByStatusAsync(ReportStatus.Submitted);
        var accepted = await _reportRepo.GetByStatusAsync(ReportStatus.Accepted);
        var rejected = await _reportRepo.GetByStatusAsync(ReportStatus.Rejected);
        var schedules = await _scheduleRepo.GetActiveSchedulesAsync();

        return new ReportingStatisticsDto(
            TotalReports: total,
            PendingReports: pending.Count(),
            SubmittedReports: submitted.Count(),
            AcceptedReports: accepted.Count(),
            RejectedReports: rejected.Count(),
            ActiveSchedules: schedules.Count(),
            ReportsByBody: new Dictionary<string, int>()
        );
    }
}
